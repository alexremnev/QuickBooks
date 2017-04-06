namespace QuickBooks.Models.Data
{
    public class TaxRate 
    {
        public virtual int Id { get; set; }
        public virtual string CountrySubDivisionCode { get; set; }
        public virtual decimal Tax { get; set; }
    }
}