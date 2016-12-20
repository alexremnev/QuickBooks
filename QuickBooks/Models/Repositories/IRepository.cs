using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repositories
{
    public interface IRepository<T> where T : BaseEntity
    {
        void Create(T entity);
    }
}
