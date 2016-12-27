using QuickBooks.Models.DAL;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.EntityService
{
    public class OAuthService : IOAuthService
    {
        private readonly IOAuthRepository _baseRepository;
        public OAuthService(IOAuthRepository baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public void Save(OAuth entity)
        {
            _baseRepository.Create(entity);
        }

        public OAuth Get()
        {
            return _baseRepository.Get();
        }
    }
}