using System;
using System.Collections.Generic;
using Common.Logging;
using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public class TaxRateRepository : BaseRepository<TaxRate>, ITaxRepository
    {
        private const string NameEntity = "TaxRate";
        private readonly ILog _log = LogManager.GetLogger<TaxRateRepository>();

        public TaxRateRepository() : base(NameEntity)
        {
        }

        public TaxRate GetByCountrySubDivisionCode(string state)
        {
            TaxRate taxRate;
            if (string.IsNullOrEmpty(state)) return null;
            using (var session = Sessionfactory.OpenSession())
            {
                try
                {
                    taxRate = session.QueryOver<TaxRate>().Where(m => m.CountrySubDivisionCode == state).SingleOrDefault();
                }
                catch (Exception e)
                {
                    _log.Error(
                        "Exception occured when system tried to get the product by name", e);
                    throw;
                }
            }
            return taxRate;
        }

        public IList<TaxRate> List()
        {
            try
            {
                using (var session = Sessionfactory.OpenSession())
                {
                    var list = session.QueryOver<TaxRate>().List();
                    return list;
                }
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when you tried to get the list of {NameEntity}s from database", e);
                throw;
            }
        }
    }
}
