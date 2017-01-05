namespace QuickBooks.Models.Repository
{
    public interface IBaseRepository<T> where T : class
    {
        /// <summary>
        /// Save entity in database.
        /// </summary>
        /// <param name="entity">the entity which must be saved in database. if entity is null occurs ArgumentNullException.</param>
        void Create(T entity);
        /// <summary>
        /// Get entity by id.
        /// </summary>
        /// <param name="id">entity id.</param>
        /// <returns>found entity or null in case there's no entity with passed id found.</returns>
        T Get(int id);
        /// <summary>
        /// Update an entity.
        /// </summary>
        /// <param name="entity">persistant entity.</param>
        void Update(T entity);
        /// <summary>
        /// Delete an entity.
        /// </summary>
        /// <param name="id">entity id.</param>
        void Delete(int id);
    }
}
