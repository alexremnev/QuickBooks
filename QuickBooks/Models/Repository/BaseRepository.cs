using System;
using Common.Logging;
using NHibernate;
using Spring.Data.NHibernate;
using Spring.Transaction.Interceptor;

namespace QuickBooks.Models.Repository
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly ILog Log;
        private readonly string _entityName;
        public ISessionFactory Sessionfactory { get; set; }

        protected BaseRepository(string entityName)
        {
            Log = LogManager.GetLogger(GetType());
            _entityName = entityName;
        }

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
                Log.Error($"Exception occured when system tried to create an {_entityName}", e);
                throw;
            }
        }

        public T Get(int id)
        {
            using (var session = Sessionfactory.OpenSession())
            {
                try
                {
                    if (id <= 0) throw new Exception("Id can not be below 1");
                    return session.Get<T>(id);
                }
                catch (Exception e)
                {
                    Log.Error(
                        $"Exception occured when system tried to get entity by id ={id} from database", e);
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
                Log.Error($"Exception occured when system tried to update {_entityName}", e);
                throw;
            }
        }


        [Transaction]
        public void Delete(int id)
        {
            var entity = Get(id);

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
                        Log.Error($"Exception occured when system tried to delete the object by id= {id}", e);
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (HibernateException exception)
                        {
                            Log.Error("Exception occurred when system tried to roll back transaction", exception);
                        }
                        throw;
                    }
                }
            }
        }
    }
}