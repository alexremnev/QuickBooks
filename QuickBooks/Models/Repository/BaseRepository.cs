using System;
using Common.Logging;
using Spring.Data.NHibernate.Generic.Support;
using Spring.Transaction.Interceptor;

namespace QuickBooks.Models.Repository
{
    public abstract class BaseRepository<T> : HibernateDaoSupport, IRepository<T> where T : class
    {
        protected BaseRepository()
        {
            Log = LogManager.GetLogger(GetType());
        }

        protected readonly ILog Log;

        [Transaction]
        public void Save(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));
                HibernateTemplate.Save(entity);
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to create an entity", e);
                throw;
            }
        }

        [Transaction(ReadOnly = true)]
        public T Get(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id)) throw new Exception("Id can not be null or empty");
                return HibernateTemplate.Get<T>(id);
            }
            catch (Exception e)
            {
                Log.Error(
                    $"Exception occured when application tried to get entity with id = {id} from database", e);
                throw;
            }
        }

        [Transaction]
        public void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                HibernateTemplate.Update(entity);
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when application tried to update entity", e);
                throw;
            }
        }

        [Transaction]
        public void Delete(string id)
        {
            var entity = Get(id);
            if (entity == null) return;
            try
            {
                HibernateTemplate.Delete(entity);
            }
            catch (Exception e)
            {
                Log.Error(
                    $"Exception occured when application tried to delete the object with id = {id}", e);
                throw;
            }
        }
    }
}