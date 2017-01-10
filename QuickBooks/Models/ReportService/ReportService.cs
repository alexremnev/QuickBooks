using System;
using Common.Logging;
using QuickBooks.Models.DAL;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.ReportService
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;
        private readonly ILog _log = LogManager.GetLogger<ReportService>();

        public ReportService(IReportRepository repository)
        {
            _repository = repository;
        }

        public void Save(BaseEntity entity)
        {
            try
            {
                var report = new Report()
                {
                    Id = entity.Id,
                    ShipAddr = entity.ShipAddr,
                    DocNumber = entity.DocNumber,
                    LineItems = entity.LineItems,
                    NameAndId = entity.NameAndId,
                    TxnDate = entity.TxnDate
                };
                _repository.Create(report);
            }
            catch (Exception e)
            {
                _log.Error("Exception occured when you tried to save Report entity", e);
                throw;
            }
        }

        public Report Get(string id)
        {
            try
            {
                var report = _repository.Get(Convert.ToInt32(id));
                return report;
            }
            catch (Exception e)
            {
                _log.Error("Exception occured when you tried to find report by entityId", e);
                throw;
            }
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

    }
}