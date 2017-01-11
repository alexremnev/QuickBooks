using System.Collections.Generic;
using System.Linq;
using Intuit.Ipp.Data;
using QuickBooks.Models.ReportService;
using QuickBooks.Models.Repository;
using Invoice = QuickBooks.Models.DAL.Invoice;

namespace QuickBooks.Models.EntityService
{
    public class InvoiceService : BaseService<Intuit.Ipp.Data.Invoice>, IInvoiceService
    {
        public InvoiceService(IReportService service, ITaxRepository repository) : base(service, repository, new Invoice(), "Invoice")
        {
        }

        public void Save(IList<Intuit.Ipp.Data.Invoice> entities, ReportBasisEnum accountingMethod = ReportBasisEnum.Accrual)
        {
            if (accountingMethod == ReportBasisEnum.Accrual) base.Save(entities);
            var paidedInvoices = entities.Where(x => x.LinkedTxn != null && IsPaid(x)).ToList();
            base.Save(paidedInvoices);
        }

        private static bool IsPaid(Transaction invoice)
        {
            var lines = invoice.LinkedTxn;
            return lines.Any(linkedTxn => linkedTxn.TxnType == "Payment" || (linkedTxn.TxnType == "ReimburseCharge"));
        }
    }
}