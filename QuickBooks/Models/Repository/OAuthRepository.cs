using System;
using Common.Logging;
using QuickBooks.Models.DAL;
using Spring.Data.NHibernate;

namespace QuickBooks.Models.Repository
{
    public class OAuthRepository : BaseRepository<OAuth>, IOAuthRepository
    {
        private static readonly ILog Log = LogManager.GetLogger<OAuthRepository>();
        private const string NameEntity = "OAuth";
        private const int Id = 1;

        public OAuthRepository() : base(Log, NameEntity)
        {
        }

        public OAuth Get()
        {
            try
            {
                var ht = new HibernateTemplate(Sessionfactory);
                return (OAuth)ht.Get(typeof(OAuth), Id) ?? new OAuth();
            }
            catch (Exception e)
            {
                Log.Error($"Exception occured when system tried to get access token and access token secret from database", e);
                throw;
            }

        }
    }
}