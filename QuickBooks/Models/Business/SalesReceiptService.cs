using System.Collections.Generic;
using System.Linq;
using Intuit.Ipp.Data;
using Intuit.Ipp.LinqExtender;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class SalesReceiptService : BaseService<SalesReceipt>, ISalesReceiptService
    {
        public SalesReceiptService(IReportRepository reportRepository, ITaxRepository repository, IOAuthService oAuthService) : base(reportRepository, repository, oAuthService, "SalesReceipt")
        {
        }

        public override IList<SalesReceipt> Recalculate(IList<SalesReceipt> list = null)
        {
            DeleteDepositedSalesReceipts(list);
            return base.Recalculate(list);
        }

        private void DeleteDepositedSalesReceipts(IList<SalesReceipt> recalculateEntity)
        {
            var queryService = _oAuthService.GetQueryService<SalesReceipt>();
            var entities = recalculateEntity ?? queryService.Select(x => x).ToList();
            var dataService = _oAuthService.GetDataService();
            var deposits = dataService.FindAll(new Deposit());
            foreach (var salesReceipt in entities)
            {
                var deposit = FindDeposit(deposits, salesReceipt);
                if (deposit != null) dataService.Delete(deposit);
            }
        }
        /// <summary>
        /// Find deposit corresponding sales receipt or return null.
        /// </summary>
        /// <param name="deposits">collection of deposit.</param>
        /// <param name="salesReceipt">entity Sales Receipt.</param>
        /// <returns>found Deposit or null.</returns>
        private static Deposit FindDeposit(IEnumerable<Deposit> deposits, IntuitEntity salesReceipt)
        {
            return deposits.Where(deposit => deposit.Line != null)
                .SelectMany(deposit => deposit.Line, (deposit, line) => new { deposit, line })
                .Where(t => t.line.LinkedTxn != null)
                .Select(t => new { t, any = t.line.LinkedTxn.Any(linkedTxn => linkedTxn.TxnId == salesReceipt.Id) })
                .Where(t => t.any)
                .Select(t => t.t.deposit).FirstOrDefault();
        }
    }
}