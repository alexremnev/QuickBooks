using QuickBooks.Models.DAL;

namespace QuickBooks.Models.ReportService
{
   public interface IReportService
   {
        /// <summary>
        /// Save the list of entity in database.
        /// </summary>
        /// <param name="entities">the list of entities.</param>
        void Save(BaseEntity entities);

       /// <summary>
       /// Delete an entiry by id
       /// </summary>
       /// <param name="id">entity id</param>
       void Delete(int id);
   }
}
