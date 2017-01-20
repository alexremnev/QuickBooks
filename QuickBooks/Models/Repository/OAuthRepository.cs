using QuickBooks.Models.DAL;

namespace QuickBooks.Models.Repository
{
    public class OAuthRepository : BaseRepository<OAuth>, IOAuthRepository
    {
        public OAuthRepository() : base("OAuth")
        {
        }
    }
}