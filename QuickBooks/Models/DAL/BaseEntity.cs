using System;
using System.Collections.Generic;

namespace QuickBooks.Models.DAL
{
    public abstract class BaseEntity
    {
        public virtual int Id { get; set; }
        public virtual string DocNumber { get; set; }
        public virtual DateTime TxnDate { get; set; }
        public virtual string NameAndId { get; set; }
        public virtual string ShipAddr { get; set; }
        public virtual IList<LineItem> LineItems { get; set; }
    }
}