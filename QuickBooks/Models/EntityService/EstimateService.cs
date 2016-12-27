using QuickBooks.Models.ReportService;

namespace QuickBooks.Models.EntityService
{
    public class EstimateService: BaseService<Intuit.Ipp.Data.Estimate>, IEstimateService
    {
        public EstimateService(IReportService service) : base(service, "Estimate")
        {
        }
    }
}