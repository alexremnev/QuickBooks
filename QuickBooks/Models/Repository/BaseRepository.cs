using System;
using Common.Logging;
using NHibernate;
using Spring.Data.NHibernate;
using Spring.Transaction.Interceptor;

namespace QuickBooks.Models.Repository
{
    public class BaseRepository<T> : IRepository<T> where T : class
    {
        private readonly ILog _log;
        private readonly string _entityName;

        protected BaseRepository(ILog log, string entityName)
        {
            _log = log;
            _entityName = entityName;
        }

        public ISessionFactory Sessionfactory { get; set; }

        [Transaction]
        public void Create(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));
                var ht = new HibernateTemplate(Sessionfactory);
                ht.Save(entity);
            }
            catch (Exception e)
            {
                 _log.Error($"Exception occured when system tried to create an {_entityName}", e);
                throw;
            }
        }
    }
}