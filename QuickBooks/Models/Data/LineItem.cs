namespace QuickBooks.Models.Data
{
    public class LineItem
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual int Quantity { get; set; }
        public virtual decimal Amount { get; set; }
    }
}