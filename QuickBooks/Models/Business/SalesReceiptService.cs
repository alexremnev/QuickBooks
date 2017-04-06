using System.Collections.Generic;
using System.Linq;
using Intuit.Ipp.Data;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class SalesReceiptService : BaseService<SalesReceipt>, ISalesReceiptService
    {
        public SalesReceiptService(IReportRepository reportRepository, ITaxRateProvider taxRateProvider,
            IOAuthService oAuthService, IQBApi qb)
            : base(reportRepository, taxRateProvider, oAuthService, qb, "SalesReceipt")
        {
        }

        protected override IList<SalesReceipt> Calculate(string realmId, params SalesReceipt[] list)
        {
            DeleteDepositedSalesReceipts(realmId, list);
            return base.Calculate(realmId, list);
        }

        private void DeleteDepositedSalesReceipts(string realmId, params SalesReceipt[] recalculateEntity)
        {
            var entities = recalculateEntity ?? _qb.List<SalesReceipt>(realmId);
            var deposits = _qb.FindAllDeposit(realmId);
            foreach (var salesReceipt in entities)
            {
                var deposit = FindDeposit(deposits, salesReceipt);
                if (deposit != null) _qb.DeleteDeposit(realmId, deposit);
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
                .SelectMany(deposit => deposit.Line, (deposit, line) => new {deposit, line})
                .Where(t => t.line.LinkedTxn != null)
                .Select(t => new {t, any = t.line.LinkedTxn.Any(linkedTxn => linkedTxn.TxnId == salesReceipt.Id)})
                .Where(t => t.any)
                .Select(t => t.t.deposit).FirstOrDefault();
        }
    }
}