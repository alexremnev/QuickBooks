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
        /// <summary>
        /// Get entity from database.
        /// </summary>
        /// <param name="id">id of entity.</param>
        /// <returns>founded entity or null.</returns>
       Report Get(string id);
   }
}
