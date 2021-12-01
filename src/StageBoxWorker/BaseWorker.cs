using Common;
using Common.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace StageBoxWorker
{
    public class ExchangeRateWorker : IHostedService, IDisposable
    {
        public static Guid sessionId = Guid.NewGuid();
        static Timer _timer;
        public static string basePath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// StartAsync
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var isDebug = Convert.ToBoolean(GetConfigValue("IsDebug"));

            var now = DateTime.Now;
            var firstRun = new DateTime(now.Year, now.Month, now.Day, 04, 30, 0, 0); //daily at 4:30 AM
            if (now > firstRun)
                firstRun = firstRun.AddDays(1);

            var timeToGo = firstRun - now; //TimeSpan.Zero
            if (timeToGo <= TimeSpan.Zero)
                timeToGo = TimeSpan.Zero;

            _timer = new Timer(async o => await SyncExchangeRate(0), null, isDebug ? TimeSpan.Zero : timeToGo, TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        /// <summary>
        /// StopAsync
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            Log(LogTypes.Information, $"{Constants.lblServiceName} ended on {DateTime.Now}.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets values from appsettings based on the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConfigValue(string key)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(basePath).AddJsonFile("appsettings.json", false).Build();
            return configuration.GetSection(key).Value;
        }

        /// <summary>
        /// Logger
        /// </summary>
        /// <param name="logTypes"></param>
        /// <param name="msg"></param>
        private void Log(LogTypes logTypes, string msg)
        {
            var logger = new LoggerService();
            ILoggerService _logger = logger;
            _logger.Log(logTypes, msg, sessionId);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _timer?.Dispose();
        }

        /// <summary>
        /// Generates network credential object from config
        /// </summary>
        /// <returns></returns>
        private NetworkCredential GetNetworkCredentials()
        {
            var username = GetConfigValue("username");
            var password = GetConfigValue("password");
            var domain = GetConfigValue("domain");
            return new NetworkCredential(username, password, domain);
        }

        /// <summary>
        /// Sync exchange rates from Netsuite to diffrent source systems 
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private async Task SyncExchangeRate(object state)
        {
            //for testing  
            //File.WriteAllText(@"C:\Log\ExRate" + DateTime.Now.ToString("yyyyMMdd.HHmmss") + ".txt", "test");

            //SN
            var snExchangeRateSyncEnabled = Convert.ToBoolean(GetConfigValue("ShipNetExchangeRateSyncEnabled"));
            if (snExchangeRateSyncEnabled)           
                await SyncExchangeRatesToShipnet();           

            //CN
            var cnExchangeRateSyncEnabled = Convert.ToBoolean(GetConfigValue("ConcurExchangeRateSyncEnabled"));
            if(cnExchangeRateSyncEnabled)
                await SyncExchangeRatesToConcurApp();
        }

        /// <summary>
        /// Sync exchange rates from Netsuite to Shipnet through excel export 
        /// </summary>
        private async Task SyncExchangeRatesToShipnet()
        {
            try
            {
                Log(LogTypes.Information, $"{Constants.lblServiceName} for {Constants.lbShipnet} started on {DateTime.Now}.");

                var exchangeRateService = new ExchangeRateService();
                IExchangeRateService _exchangeRateService = exchangeRateService;

                var dateForExchangeRateExport = GetConfigValue("FetchRatesForDate");
                var effectiveDate = string.IsNullOrEmpty(dateForExchangeRateExport) ? DateTime.Today.Date : Convert.ToDateTime(dateForExchangeRateExport);
                var exchangeRatesToExport = _exchangeRateService.GetExchangeRatesForDate(effectiveDate).Where(x => x.BaseCurrency == "USD").ToList();

                if (exchangeRatesToExport == null || exchangeRatesToExport.Count() < 1)
                {
                    Log(LogTypes.Error, $"No exchange rates to export for effective date - {effectiveDate}, application: {Constants.lbShipnet}.");
                    return;
                }

                var networkCredentials = GetNetworkCredentials();
                var fileServerLocations = _exchangeRateService.GetServerLocations();
                var fileGenerationResponse = ExportFileGeneratorService.GenerateCSVInLocalArchive("\t", exchangeRatesToExport, Constants.localArchivePath, Constants.localSNExRatesArchiveFolder);

                if (!fileGenerationResponse.Item1 || string.IsNullOrEmpty(fileGenerationResponse.Item2))
                {
                    Log(LogTypes.Error, $"File generation failed for {Constants.lbShipnet} for effective date - {effectiveDate}.");
                    return;
                }

                var localArchiveFilePath = Path.Combine(Constants.localArchivePath, Constants.localSNExRatesArchiveFolder, fileGenerationResponse.Item2);
                foreach (var location in fileServerLocations)
                {
                    try
                    {
                        await Task.Run(() => ExportFileGeneratorService.CopyFileFromLocalArchiveToSharedFolders(localArchiveFilePath, location.Path, networkCredentials));
                        continue; //IMPORTANT
                    }
                    catch (Exception ex)
                    {
                        Log(LogTypes.Error, $"Failed for {Constants.lbShipnet} path : {location.Path}, exception: {ex.Message}");
                    }
                }

                Log(LogTypes.Information, $"{Constants.lblServiceName} for {Constants.lbShipnet} ended on {DateTime.Now}.");
            }
            catch (Exception ex)
            {
                Log(LogTypes.Error, $"File generation failed for {Constants.lbShipnet}. Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Sync exchange rates from Netsuite to Concur App using SFTP
        /// </summary>
        private async Task SyncExchangeRatesToConcurApp()
        {
            try
            {
                Log(LogTypes.Information, $"{Constants.lblServiceName} for {Constants.lbConcur} started on {DateTime.Now}.");

                var exchangeRateService = new ExchangeRateService();
                IExchangeRateService _exchangeRateService = exchangeRateService;

                var cnParameters = _exchangeRateService.GetConcurConnectionDetails();               
                              
                var dateForExchangeRateExport = GetConfigValue("FetchRatesForDate");
                var effectiveDate = string.IsNullOrEmpty(dateForExchangeRateExport) ? DateTime.Today.Date : Convert.ToDateTime(dateForExchangeRateExport);
                var exchangeRatesBasedUSD = _exchangeRateService.GetExchangeRatesForDate(effectiveDate).Where(x => x.BaseCurrency == "USD").ToList();

                var separator = @"""";
                var lines = new List<string>();
                exchangeRatesBasedUSD.ForEach(x => {
                    lines.Add($"{separator}{x.SourceCurrency}{separator},{separator}{x.BaseCurrency}{separator},{separator}{x.ExchangeRate} {separator},{separator}{Convert.ToDateTime(x.EffectiveDate).ToString("yyyy/MM/dd")}{separator}");
                });
                
                if(lines.Count > 0)
                    await Task.Run(() => ExportFileGeneratorService.GenarateCSVInSFTP<string>(sftpClient: cnParameters.Item1, networkCredentials: cnParameters.Item2, networkPath: cnParameters.Item3, filePrefix: cnParameters.Item4, fileContent: lines, sftpPgpKey: cnParameters.Item5));
                else
                    Log(LogTypes.Error, $"No exchange rates to export for effective date - {effectiveDate}, application: {Constants.lbConcur}.");

                Log(LogTypes.Information, $"{Constants.lblServiceName} for {Constants.lbConcur} ended on {DateTime.Now}.");
            }
            catch (Exception ex)
            {
                Log(LogTypes.Error, ex.Message);
            }
        }       
    }
}
