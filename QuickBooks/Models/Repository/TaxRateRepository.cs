using System;
using System.Collections.Generic;
using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public class TaxRateRepository : BaseRepository<TaxRate>, ITaxRepository
    {
        public TaxRateRepository() : base(NameEntity)
        {
        }
        private const string NameEntity = "TaxRate";
     
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
                    Log.Error(
                        "Exception occured when application tried to get the product by name", e);
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
                Log.Error($"Exception occured when application tried to get the list of {NameEntity}s from database", e);
                throw;
            }
        }
    }
}
