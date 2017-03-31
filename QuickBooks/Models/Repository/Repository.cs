using System;
using Common.Logging;
using NHibernate;
using Spring.Data.NHibernate;
using Spring.Transaction.Interceptor;

namespace QuickBooks.Models.Repository
{
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        protected Repository(string entityName)
        {
            Log = LogManager.GetLogger(GetType());
            _entityName = entityName;
        }
        protected readonly ILog Log;
        private readonly string _entityName;
        public ISessionFactory Sessionfactory { get; set; }
        private string _entityId = "";

        [Transaction]
        public void Save(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));
                var ht = new HibernateTemplate(Sessionfactory);
                ht.Save(entity);
            }
            catch (Exception e)
            {
                Log.Error($"Exception occured when application tried to create an {_entityName}", e);
                throw;
            }
        }

        public T Get(string id)
        {
            using (var session = Sessionfactory.OpenSession())
            {
                try
                {
                    _entityId = id;
                    if (string.IsNullOrEmpty(id)) throw new Exception("Id can not be null or empty");
                    return session.Get<T>(id);
                }
                catch (Exception e)
                {
                    Log.Error(
                        $"Exception occured when application tried to get entity with id = {_entityId} from database", e);
                    throw;
                }
            }
        }

        [Transaction]
        public void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            try
            {
                var ht = new HibernateTemplate(Sessionfactory);
                ht.Update(entity);
            }
            catch (Exception e)
            {
                Log.Error($"Exception occured when application tried to update {_entityName}", e);
                throw;
            }
        }

        [Transaction]
        public void Delete(string id)
        {
            var entity = Get(id);
            if (entity == null) return;
            _entityId = id;
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
                        Log.Error($"Exception occured when application tried to delete the object with id = {_entityId}", e);
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