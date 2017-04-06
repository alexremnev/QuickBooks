using System.Collections.Generic;
using System.Linq;
using Intuit.Ipp.Data;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class InvoiceService : BaseService<Invoice>, IInvoiceService
    {
        public InvoiceService(IReportRepository reportRepository, ITaxRateProvider taxRateProvider,
            IOAuthService oAuthService, IQBApi qb)
            : base(reportRepository, taxRateProvider, oAuthService, qb, "Invoice")
        {
        }

        protected override void Save(string realmId, IList<Invoice> list)
        {
            var accountingMethod = _qb.GetAccountingMethod(realmId);
            if (accountingMethod == ReportBasisEnum.Accrual)
            {
                base.Save(realmId, list);
                return;
            }
            var entities = list ?? _qb.List<Invoice>(realmId);
            var paidedInvoices = entities.Where(x => x.LinkedTxn != null && IsPaid(x)).ToList();
            base.Save(realmId, paidedInvoices);
        }

        private static bool IsPaid(Transaction invoice)
        {
            var lines = invoice.LinkedTxn;
            return lines.Any(linkedTxn => linkedTxn.TxnType == "Payment" || (linkedTxn.TxnType == "ReimburseCharge"));
        }
    }
}