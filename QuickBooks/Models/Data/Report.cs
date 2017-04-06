using System;
using System.Collections.Generic;
using Intuit.Ipp.Data;

namespace QuickBooks.Models.Data
{
    public class Report
    {
        public virtual string Id { get; set; }
        public virtual string DocumentNumber { get; set; }
        public virtual DateTime SaleDate { get; set; }
        public virtual string CustomerName { get; set; }
        public virtual PhysicalAddress ShipToAddress { get; set; }
        public virtual IList<LineItem> LineItems { get; set; }
    }
}