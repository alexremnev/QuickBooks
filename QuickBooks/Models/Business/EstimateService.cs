using System.Collections.Generic;
using Intuit.Ipp.Data;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class EstimateService : BaseService<Estimate>, IEstimateService
    {
        public EstimateService(IReportRepository reportRepository, ITaxRepository repository, IOAuthService oAuthService) : base(reportRepository, repository, oAuthService, "Estimate")
        {
        }

        public override void Save(IList<Estimate> list = null)
        {
        }
    }
}