using QuickBooks.Models.DAL;

namespace QuickBooks.Models.ReportService
{
   public interface IReportService
   {
       void Save(BaseEntity entities);
   }
}
