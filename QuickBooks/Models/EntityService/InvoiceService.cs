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
        public InvoiceService(IReportService service, ITaxRepository repository) : base(service, repository, "Invoice")
        {
        }

        public void Save(IList<Intuit.Ipp.Data.Invoice> entities, ReportBasisEnum accountingMethod = ReportBasisEnum.Accrual)
        {
            var baseEntity = new Invoice();
            if (accountingMethod == ReportBasisEnum.Accrual) Save(entities, baseEntity);
            var paidedInvoices = entities.Where(x => x.LinkedTxn != null && IsPaid(x)).ToList();
            Save(paidedInvoices, baseEntity);
        }

        private static bool IsPaid(Transaction invoice)
        {
            var lines = invoice.LinkedTxn;
            return lines.Any(linkedTxn => linkedTxn.TxnType == "Payment" || (linkedTxn.TxnType == "ReimburseCharge"));
        }
    }
}