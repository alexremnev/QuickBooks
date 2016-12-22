using System;
using Common.Logging;
using NHibernate;
using QuickBooks.Models.DAL;
using Spring.Data.NHibernate;
using Spring.Transaction.Interceptor;

namespace QuickBooks.Models.Repository
{
    public class ReportRepository : IReportRepository
    {
        private readonly ILog _log = LogManager.GetLogger<ReportRepository>();
        public ISessionFactory Sessionfactory { get; set; }

        [Transaction]
        public void Create(Report entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));
                var ht = new HibernateTemplate(Sessionfactory);
                ht.Save(entity);
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when system tried to create an report", e);
                throw;
            }
        }
    }
}