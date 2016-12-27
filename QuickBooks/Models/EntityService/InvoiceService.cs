using System.Collections.Generic;
using System.Linq;
using QuickBooks.Models.ReportService;
using Invoice = QuickBooks.Models.DAL.Invoice;

namespace QuickBooks.Models.EntityService
{
    public class InvoiceService : BaseService<Intuit.Ipp.Data.Invoice>, IInvoiceService
    {
        public InvoiceService(IReportService service) : base(service, "Invoice")
        {
        }

        public void Save(IList<Intuit.Ipp.Data.Invoice> entities, string accrualMethod = "Cash")
        {
            var baseEntity = new Invoice();
            if (accrualMethod == "Cash") Save(entities, baseEntity);
            var paidedInvoices = entities.Where(x => x.LinkedTxn != null && IsPaid(x)).ToList();
            Save(paidedInvoices, baseEntity);
        }

        private static bool IsPaid(Intuit.Ipp.Data.Invoice invoice)
        {
            var lines = invoice.LinkedTxn;
            foreach (var linkedTxn in lines)
            {
                if (linkedTxn.TxnType == "Payment" || (linkedTxn.TxnType == "ReimburseCharge")) return true;
            }
            return false;
        }
    }
}