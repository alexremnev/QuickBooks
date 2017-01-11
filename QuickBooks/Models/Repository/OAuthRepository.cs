using System;
using Common.Logging;
using QuickBooks.Models.DAL;
using Spring.Data.NHibernate;

namespace QuickBooks.Models.Repository
{
    public class OAuthRepository : BaseRepository<OAuth>, IOAuthRepository
    {
        private const string NameEntity = "OAuth";
        private const int Id = 1;
        private readonly ILog _log = LogManager.GetLogger<OAuthRepository>();

        public OAuthRepository() : base(NameEntity)
        {
        }

        public OAuth Get(string realmId)
        {
            try
            {
                var ht = new HibernateTemplate(Sessionfactory);
                return (OAuth)ht.Get(typeof(OAuth), realmId) ?? new OAuth();
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when system tried to get access token and access token secret from database", e);
                throw;
            }

        }
    }
}