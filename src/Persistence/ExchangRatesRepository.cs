using Common.Interfaces;
using Models;
using System;
using System.Collections.Generic;
using Common;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace Persistence
{
    public class ExchangRatesRepository : BaseRepository, IExchangRatesRepository
    {
        /// <summary>
        /// Gets exchange rates for a particular date
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<ExchangeRateModel> GetExchangeRatesForDate(DateTime date)
        {
            var parameters = new SqlParameter[]
            {
                 new SqlParameter("@pEffectiveDate", date)
            };
            var list = GetList<ExchangeRateModel>(Constants.spGetAEExchangeRatesByDate, parameters).ToList();
            return list;
        }

        /// <summary>
        /// Gets details of all server locations for exchange rate export
        /// </summary>
        /// <returns></returns>
        public List<FileServerDetail> GetServerLocations()
        {
            var list = GetList<FileServerDetail>(Constants.spGetAEFileServerDetails, null).ToList();
            return list;
        }

        #region AEApp

        /// <summary>
        /// Get AE app details
        /// </summary>
        /// <returns></returns>
        public List<AEApp> GetAEAppList()
        {
            var list = GetList<AEApp>(Constants.spGetAEApp, null).ToList();
            return list;
        }

        /// <summary>
        /// Get AE parameters for network connections
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public List<AEAppParameter> GetAEAppParameterList(int appId)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@pAppId", appId)
            };
            var list = GetList<AEAppParameter>(Constants.spGetAEAppParameter, parameters).ToList();
            return list;
        }

        #endregion
    }
}
