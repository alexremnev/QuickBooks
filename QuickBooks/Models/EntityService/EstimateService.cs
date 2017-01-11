using System.Collections.Generic;
using QuickBooks.Models.DAL;
using QuickBooks.Models.ReportService;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.EntityService
{
    public class EstimateService: BaseService<Intuit.Ipp.Data.Estimate>, IEstimateService
    {
        public EstimateService(IReportService service, ITaxRepository repository) : base(service, repository,new Estimate(), "Estimate")
        {
        }

        public override void Save(IList<Intuit.Ipp.Data.Estimate> entities)
        {
            
        }
    }
}