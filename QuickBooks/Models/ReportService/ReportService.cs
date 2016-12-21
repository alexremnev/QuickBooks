using QuickBooks.Models.DAL;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.ReportService
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;

        public ReportService(IReportRepository repository)
        {
            _repository = repository;
        }

        public void Save(BaseEntity entity)
        {
            var report = new Report()
            {
                ShipAddr = entity.ShipAddr,
                DocNumber = entity.DocNumber,
                LineItems = entity.LineItems,
                NameAndId = entity.NameAndId,
                TxnDate = entity.TxnDate
            };
            _repository.Create(report);
        }
    }
}