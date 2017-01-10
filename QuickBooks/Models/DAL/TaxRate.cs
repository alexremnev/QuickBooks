using Intuit.Ipp.Data;

namespace QuickBooks.Models.DAL
{
    public class TaxRate 
    {
        public virtual int Id { get; set; }
        public virtual string CountrySubDivisionCode { get; set; }
        public virtual decimal Tax { get; set; }
    }
}