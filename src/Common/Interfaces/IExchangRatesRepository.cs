using Models;
using System;
using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface IExchangRatesRepository
    {
        List<ExchangeRateModel> GetExchangeRatesForDate(DateTime date);
        List<FileServerDetail> GetServerLocations();
        List<AEApp> GetAEAppList();
        List<AEAppParameter> GetAEAppParameterList(int appId);
    }
}
