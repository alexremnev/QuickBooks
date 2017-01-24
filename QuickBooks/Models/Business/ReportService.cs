using System;
using Common.Logging;
using QuickBooks.Models.DAL;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class ReportService : IReportService
    {
        public ReportService(IReportRepository repository)
        {
            _repository = repository;
        }
        private readonly IReportRepository _repository;
        private readonly ILog _log = LogManager.GetLogger<ReportService>();
        private string _entityId = "";

        public void Save(BaseEntity entity)
        {
            try
            {
                var report = new Report
                {
                    Id = entity.Id,
                    ShipToAddress = entity.ShipToAddress,
                    DocumentNumber = entity.DocumentNumber,
                    LineItems = entity.LineItems,
                    CustomerName = entity.CustomerName,
                    SaleDate = entity.SaleDate
                };
                _entityId = entity.Id;
                _repository.Create(report);
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when application tried to save Report entity with id ={_entityId}", e);
                throw;
            }
        }

        public Report Get(string id)
        {
            try
            {
                _entityId = id;
                var report = _repository.Get(id);
                return report;
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when application tried to find report with id ={_entityId}", e);
                throw;
            }
        }

        public void Delete(string id)
        {
            try
            {
                _entityId = id;
                var entity = Get(id);
                if (entity!=null) _repository.Delete(id);
            }
            catch (Exception e)
            {
                _log.Error($"Exception occured when application tried to delete report with id ={_entityId}", e);
            }
        }
    }
}