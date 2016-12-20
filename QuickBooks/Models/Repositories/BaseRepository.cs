using System;
using NHibernate;
using QuickBooks.Models.DAL;
using Spring.Data.NHibernate;
using Spring.Transaction.Interceptor;


namespace QuickBooks.Models.Repositories
{
    public class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
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
                //  _log.Error($"Exception occured when system tried to create an {_entityName}", e);
                throw;
            }
        }
    }
}