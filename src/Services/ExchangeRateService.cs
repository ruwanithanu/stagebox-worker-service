using Common;
using Common.Interfaces;
using Models;
using Persistence;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Services
{
    public interface IExchangeRateService
    {
        List<ExchangeRateModel> GetExchangeRatesForDate(DateTime date);
        List<FileServerDetail> GetServerLocations();
        List<AEAppParameter> GetAppDetails(string appName);
        (SftpClient, NetworkCredential, string, string, string) GetConcurConnectionDetails();
        NetworkCredential GetAEConnection();
    }

    public class ExchangeRateService : IExchangeRateService
    {
        public ExchangeRateService()
        {
        }

        /// <summary>
        /// Gets exchange rates for a particular date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<ExchangeRateModel> GetExchangeRatesForDate(DateTime date)
        {
            ExchangRatesRepository exRateRepository = new ExchangRatesRepository();
            IExchangRatesRepository _exRateRepository = exRateRepository;
            return _exRateRepository.GetExchangeRatesForDate(date);
        }

        /// <summary>
        /// Gets details of all server locations for exchange rate export
        /// </summary>
        /// <returns></returns>
        public List<FileServerDetail> GetServerLocations()
        {
            ExchangRatesRepository exRateRepository = new ExchangRatesRepository();
            IExchangRatesRepository _exRateRepository = exRateRepository;
            return _exRateRepository.GetServerLocations();
        }

        /// <summary>
        /// Gets details of all concur app
        /// </summary>
        /// <returns></returns>
        public List<AEAppParameter> GetAppDetails(string appName)
        {
            ExchangRatesRepository exRateRepository = new ExchangRatesRepository();
            IExchangRatesRepository _exRateRepository = exRateRepository;

            var app = _exRateRepository.GetAEAppList().FirstOrDefault(x => x.Name.ToLower() == appName.ToString().ToLower());
            return _exRateRepository.GetAEAppParameterList(app.Id);
        }

        /// <summary>
        /// Get concur sftp connection
        /// </summary>
        /// <returns></returns>
        public NetworkCredential GetAEConnection()
        {
            var appParameters = GetAppDetails("StageBoxDBConnection");
            var networkClient = new NetworkCredential
                (
                     DecryptIdentityKey(appParameters.FirstOrDefault(x => x.Name == Constants.lblAEParameterAESPAdmin).Value),
                     DecryptIdentityKey(appParameters.FirstOrDefault(x => x.Name == Constants.lblAEParameterAESPPassword).Value),
                     DecryptIdentityKey(appParameters.FirstOrDefault(x => x.Name == Constants.lblAEParameterAEDomain).Value)
                );
            return networkClient;
        }

        /// <summary>
        /// Get concur sftp connection, networkcredentials, networkpath and filename
        /// </summary>
        /// <returns></returns>
        public (SftpClient, NetworkCredential, string, string, string) GetConcurConnectionDetails()
        {
            var appParameters = GetAppDetails("Concur");
            var sftpClient = new SftpClient
                (
                    DecryptIdentityKey(appParameters.FirstOrDefault(x => x.Name == Constants.lblCNXrateSFTPHost).Value),
                    Convert.ToInt32(DecryptIdentityKey(appParameters.FirstOrDefault(x => x.Name == Constants.lblCNXrateSFTPPort).Value)),
                    DecryptIdentityKey(appParameters.FirstOrDefault(x => x.Name == Constants.lblCNXrateSFTPUserId).Value),
                    DecryptIdentityKey(appParameters.FirstOrDefault(x => x.Name == Constants.lblCNXrateSFTPPassword).Value)
                );
            var sftpPgpKey = DecryptIdentityKey(appParameters.FirstOrDefault(x => x.Name == Constants.lblCNXratePgpKey).Value);
            var networkPath = DecryptIdentityKey(appParameters.FirstOrDefault(x => x.Name == Constants.lblCNXrateNetworkPath).Value);
            var fileNamePrefix = DecryptIdentityKey(appParameters.FirstOrDefault(x => x.Name == Constants.lblCNXrateFileName).Value);
            return (sftpClient, GetAEConnection(), networkPath, fileNamePrefix, sftpPgpKey);
        }

        /// <summary>
        ///  Decrypt APPs Identity keys
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public string DecryptIdentityKey(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
