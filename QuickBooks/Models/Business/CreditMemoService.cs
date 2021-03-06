﻿using Intuit.Ipp.Data;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class CreditMemoService : BaseService<CreditMemo>, ICreditMemoService
    {
        public CreditMemoService(IReportRepository reportRepository, ITaxRateProvider taxRateProvider,
            IOAuthService oAuthService, IQBApi qb)
            : base(reportRepository, taxRateProvider, oAuthService, qb, "CreditMemo")
        {
        }
    }
}