using System.Collections.Generic;

namespace QuickBooks.Models.Business
{
    public interface ITaxRateProvider
    {
        IDictionary<string, decimal> GetTaxRates();
    }
}