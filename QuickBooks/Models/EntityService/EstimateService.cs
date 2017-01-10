using QuickBooks.Models.ReportService;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.EntityService
{
    public class EstimateService: BaseService<Intuit.Ipp.Data.Estimate>, IEstimateService
    {
        public EstimateService(IReportService service, ITaxRepository repository) : base(service, repository, "Estimate")
        {
        }
    }
}