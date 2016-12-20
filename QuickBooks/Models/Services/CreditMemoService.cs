using System.Collections;
using System.Collections.Generic;
using NHibernate.Event.Default;
using QuickBooks.Models.DAL;
using QuickBooks.Models.Repositories;


namespace QuickBooks.Models.Services
{
    public class CreditMemoService : ICreditMemoService
    {
        private readonly ICreditMemoRepository _repository;
        public CreditMemoService(ICreditMemoRepository repository)
        {
            _repository = repository;
        }

        public void Save(IList<Intuit.Ipp.Data.CreditMemo> entities)
        {
            foreach (var entity in entities)
            {
                var lineItems = new List<LineItem>();
                var creditMemo = new CreditMemo();
                if (entity.DocNumber != null) creditMemo.DocNumber = entity.DocNumber;
                creditMemo.TxnDate = entity.TxnDate;
                if (entity.NameAndId != null) creditMemo.NameAndId = entity.NameAndId;
                if (entity.ShipAddr != null) creditMemo.ShipAddr = entity.ShipAddr.City;

                if (entity.Line == null)
                {
                    _repository.Create(creditMemo);
                    continue;

                }

                foreach (var line in entity.Line)
                {

                    var lineItem = new LineItem();

                    if (line.Description != null) lineItem.Name = line.Description;
                    if (line.Amount != 0) lineItem.Amount = line.Amount;

                    lineItems.Add(lineItem);
                }
                creditMemo.LineItems = lineItems;
                _repository.Create(creditMemo);
            }
        }
    }
}
