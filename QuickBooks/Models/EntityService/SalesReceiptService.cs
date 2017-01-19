using System.Collections.Generic;
using System.Linq;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.LinqExtender;
using Intuit.Ipp.QueryFilter;
using QuickBooks.Models.ReportService;
using QuickBooks.Models.Repository;
using SalesReceipt = QuickBooks.Models.DAL.SalesReceipt;

namespace QuickBooks.Models.EntityService
{
    public class SalesReceiptService : BaseService<Intuit.Ipp.Data.SalesReceipt>, ISalesReceiptService
    {
        public SalesReceiptService(IReportService service, ITaxRepository repository) : base(service, repository, new SalesReceipt(), "Sales Receipt")
        {
        }

        public override IList<Intuit.Ipp.Data.SalesReceipt> Recalculate(ServiceContext context,
            IList<Intuit.Ipp.Data.SalesReceipt> recalculateEntity = null)
        {
            DeleteDepositedSalesReceipts(context, recalculateEntity);
            return base.Recalculate(context, recalculateEntity);
        }

        private static void DeleteDepositedSalesReceipts(ServiceContext context, IList<Intuit.Ipp.Data.SalesReceipt> recalculateEntity = null)
        {
            var queryService = new QueryService<Intuit.Ipp.Data.SalesReceipt>(context);
            var entities = recalculateEntity ?? queryService.Select(x => x).ToList();
            var dataService = new DataService(context);
            var deposits = dataService.FindAll(new Deposit());

            var cancelationToken = false;
            foreach (var salesReceipt in entities)
            {
                foreach (var deposit in deposits)
                {
                    if (cancelationToken) break;
                    if (deposit.Line == null) continue;
                    foreach (var line in deposit.Line)
                    {
                        if (cancelationToken) break;
                        if (line.LinkedTxn?.Any(linkedTxn => linkedTxn.TxnId == salesReceipt.Id) != true) continue;
                        dataService.Delete(deposit);
                        cancelationToken = true;
                    }
                }
            }
        }


    }
}