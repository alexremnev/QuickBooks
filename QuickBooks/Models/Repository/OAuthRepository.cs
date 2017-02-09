using System;
using NHibernate;
using QuickBooks.Models.DAL;
using Spring.Transaction.Interceptor;

namespace QuickBooks.Models.Repository
{
    public class OAuthRepository : BaseRepository<OAuth>, IOAuthRepository
    {
        public OAuthRepository() : base("OAuth")
        {
        }

        public OAuth Get()
        {
            using (var session = Sessionfactory.OpenSession())
            {
                try
                {
                    return session.QueryOver<OAuth>().SingleOrDefault();
                }
                catch (Exception e)
                {
                    Log.Error("Exception occured when application tried to get entity from database", e);
                    throw;
                }
            }
        }
        [Transaction]
        public void Delete()
        {
            var entity = Get();
            if (entity == null) return;
            using (var session = Sessionfactory.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    try
                    {
                        session.Delete(entity);
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        Log.Error("Exception occured when application tried to delete the object", e);
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (HibernateException exception)
                        {
                            Log.Error("Exception occurred when application tried to roll back transaction", exception);
                        }
                        throw;
                    }
                }
            }
        }
    }
}