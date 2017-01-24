﻿using System.Collections.Generic;
using System.Linq;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.LinqExtender;
using Intuit.Ipp.QueryFilter;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class SalesReceiptService : BaseService<SalesReceipt>, ISalesReceiptService
    {
        public SalesReceiptService(IReportService service, ITaxRepository repository) : base(service, repository, new DAL.SalesReceipt(), "Sales Receipt")
        {
        }

        public override IList<SalesReceipt> Recalculate(ServiceContext context,
            IList<SalesReceipt> notCalculatedEntities = null)
        {
            DeleteDepositedSalesReceipts(context, notCalculatedEntities);
            return base.Recalculate(context, notCalculatedEntities);
        }

        private static void DeleteDepositedSalesReceipts(ServiceContext context, IList<SalesReceipt> recalculateEntity)
        {
            var queryService = new QueryService<SalesReceipt>(context);
            var entities = recalculateEntity ?? queryService.Select(x => x).ToList();
            var dataService = new DataService(context);
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
                .SelectMany(deposit => deposit.Line, (deposit, line) => new {deposit, line})
                .Where(t => t.line.LinkedTxn != null)
                .Select(t => new {t, any = t.line.LinkedTxn.Any(linkedTxn => linkedTxn.TxnId == salesReceipt.Id)})
                .Where(t => t.any)
                .Select(t => t.t.deposit).FirstOrDefault();
        }
    }
}