using System;
using System.Collections.Generic;
using QuickBooks.Models.DAL;
using Spring.Data.NHibernate;

namespace QuickBooks.Models.Repository
{
    public class OAuthRepository : Repository<OAuth>, IOAuthRepository
    {
        public OAuthRepository() : base("OAuth")
        {
        }

        public IList<OAuth> List()
        {
            try
            {
                using (var session = Sessionfactory.OpenSession())
                {
                    return session.QueryOver<OAuth>().List();
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application got all entities from database", e);
                throw;
            }
        }
    }
}