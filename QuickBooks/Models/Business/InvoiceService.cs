using System.Collections.Generic;
using System.Linq;
using Intuit.Ipp.Data;
using Intuit.Ipp.LinqExtender;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class InvoiceService : BaseService<Invoice>, IInvoiceService
    {
        public InvoiceService(IReportRepository reportRepository, ITaxRepository repository, IOAuthService oAuthService) : base(reportRepository, repository, oAuthService, "Invoice")
        {
        }

        public override void Save(string realmId, IList<Invoice> list = null)
        {
            if (GetAccountingMethod(realmId) == ReportBasisEnum.Accrual) { base.Save(realmId,list); return; }
            var queryService = _oAuthService.GetQueryService<Invoice>(realmId);
            var entities = list ?? queryService.Select(x => x).ToList();
            var paidedInvoices = entities.Where(x => x.LinkedTxn != null && IsPaid(x)).ToList();
            base.Save(realmId,paidedInvoices);
        }

        private static bool IsPaid(Transaction invoice)
        {
            var lines = invoice.LinkedTxn;
            return lines.Any(linkedTxn => linkedTxn.TxnType == "Payment" || (linkedTxn.TxnType == "ReimburseCharge"));
        }

        private ReportBasisEnum GetAccountingMethod(string realmId)
        {
            var dataService = _oAuthService.GetDataService(realmId);
            var preferences = dataService.FindAll(new Preferences()).ToList();
            var accountingMethod = preferences[0].ReportPrefs.ReportBasis;
            return accountingMethod;
        }
    }
}