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
                    ShipToAddress = entity.ShipToAddress,
                    DocumentNumber = entity.DocumentNumber,
                    LineItems = entity.LineItems,
                    CustomerName = entity.CustomerName,
                    SaleDate = entity.SaleDate
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
                var report = _repository.Get(id);
                return report;
            }
            catch (Exception e)
            {
                _log.Error("Exception occured when you tried to find report by entityId", e);
                throw;
            }
        }

        public void Delete(string id)
        {
            _repository.Delete(id);
        }
    }
}