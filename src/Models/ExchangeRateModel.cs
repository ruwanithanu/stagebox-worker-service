using Models.CustomAttributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class ExchangeRateModel
    {
        [Display(Name = "Source Currency")]
        public string SourceCurrency { get; set; }

        [Display(Name = "Base Currency")]
        public string BaseCurrency { get; set; }

        [SkipProperty]
        [Display(Name = "Effective Date")]
        public DateTime EffectiveDate { get; set; }

        [Display(Name = "Effective Date")]
        public string ShipNetFileEffectiveDateFormat => EffectiveDate.ToString("yyyyMMdd");

        [Display(Name = "Exchange Rate")]
        public decimal ExchangeRate { get; set; }

        [Display(Name = "")]
        public string ShipNetFileAppendText => $"Import";

        [SkipProperty]
        public string NSXRateRecordId { get; set; }
        [SkipProperty]
        public string NSXRateId { get; set; }
    }
}
