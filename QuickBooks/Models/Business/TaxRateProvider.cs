using System.Collections.Generic;
using System.Linq;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class TaxRateProvider : ITaxRateProvider
    {
        public TaxRateProvider(ITaxRepository taxRepository)
        {
            _taxRepository = taxRepository;
        }

        private readonly ITaxRepository _taxRepository;

        public IDictionary<string, decimal> GetTaxRates()
        {
            var taxRateList = _taxRepository.List();
            return taxRateList.ToDictionary(item => item.CountrySubDivisionCode.ToUpper(), item => item.Tax);
        }
    }
}