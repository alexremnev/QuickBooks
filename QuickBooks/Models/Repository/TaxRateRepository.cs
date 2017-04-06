using System;
using System.Collections.Generic;
using QuickBooks.Models.Data;
using Spring.Transaction.Interceptor;

namespace QuickBooks.Models.Repository
{
    public class TaxRateRepository : BaseRepository<TaxRate>, ITaxRepository
    {
        [Transaction(ReadOnly = true)]
        public IList<TaxRate> List()
        {
            try
            {
                return HibernateTemplate.LoadAll<TaxRate>();
            }
            catch (Exception e)
            {
                Log.Error($"Exception occured when application tried to get the list of entities from database", e);
                throw;
            }
        }
    }
}
