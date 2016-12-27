namespace QuickBooks.Models.Repository
{
    public interface IBaseRepository<T> where T : class
    {
        /// <summary>
        /// Save entity in database.
        /// </summary>
        /// <param name="entity">the entity which must be saved in database.</param>
        void Create(T entity);
    }
}
