using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public interface IOAuthRepository : IRepository<OAuth>
    {
        OAuth Get();
    }
}
