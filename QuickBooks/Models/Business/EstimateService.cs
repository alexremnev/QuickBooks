using System.Collections.Generic;
using Intuit.Ipp.Data;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class EstimateService : BaseService<Estimate>, IEstimateService
    {
        public EstimateService(IReportRepository reportRepository, ITaxRateProvider taxRateProvider,
            IOAuthService oAuthService, IQBApi qb)
            : base(reportRepository, taxRateProvider, oAuthService, qb, "Estimate")
        {
        }

        protected override void Save(string realmId, IList<Estimate> list)
        {
        }
    }
}