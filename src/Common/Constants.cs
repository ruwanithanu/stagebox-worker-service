
using System;

namespace Common
{
    public class Constants
    {
        public const string lblServiceName = "Exchange Rate Sync Service";

        //DB objects
        public const string spGenerateAEExRateSyncLog = "spGenerateAEExRateSyncLog";
        public const string spGetAEExchangeRatesByDate = "spGetAEExchangeRatesByDate";
        public const string spGetAEFileServerDetails = "spGetAEFileServerDetails";
        public const string spGenerateExRateSyncLog = "spGenerateExRateSyncLog";
        public const string spGetDailyExchangeRatesForDate = "spGetExchangeRatesForDate";
        public const string spGetFileServerDetails = "spGetFileServerDetails"; 
        public const string spGetAEApp = "spGetAEApp";
        public const string spGetAEAppParameter = "spGetAEAppParameter";

        //Labels
        public const string lblAEParameterAEDomain = "AEDomain";
        public const string lblAEParameterAESPAdmin = "AESPAdmin";
        public const string lblAEParameterAESPPassword = "AESPPassword";
        public const string lblCNXrateSFTPHost = "CNXrateSFTPHost";
        public const string lblCNXrateSFTPPort = "CNXrateSFTPPort";
        public const string lblCNXrateSFTPUserId = "CNXrateSFTPUserId";
        public const string lblCNXrateSFTPPassword = "CNXrateSFTPPassword";
        public const string lblCNXrateNetworkPath = "CNXrateNetworkPath";
        public const string lblCNXrateFileName = "CNXrateFileName";
        public const string lblCNXratePgpKey = "CNXratePgpKey";
        public const string lbShipnet = "Shipnet";
        public const string lbConcur = "Concur";

        //File generation
        public static string csvFileName = "ExchangeRates.dat";
        public static string csvlogFileName = "EXCHANGERATES.log";
        public static string localArchivePath = @"C:\AEISResources";
        public static string localSNExRatesArchiveFolder = "ShipNetExchangeRateCSV";
        public static string excelFileName = "ExchangeRates.xlsx";
    }
}
