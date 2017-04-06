using System;
using System.Collections.Generic;
using QuickBooks.Models.Data;
using Spring.Transaction.Interceptor;

namespace QuickBooks.Models.Repository
{
    public class OAuthRepository : BaseRepository<OAuth>, IOAuthRepository
    {
        [Transaction(ReadOnly = true)]
        public IList<OAuth> List()
        {
            try
            {
                return HibernateTemplate.LoadAll<OAuth>();
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application got all entities from database", e);
                throw;
            }
        }
    }
}