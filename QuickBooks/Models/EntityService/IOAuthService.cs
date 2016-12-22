using QuickBooks.Models.DAL;

namespace QuickBooks.Models.EntityService
{
   public interface IOAuthService
   {
       void Save(OAuth entity);
       OAuth Get();
   }
}
