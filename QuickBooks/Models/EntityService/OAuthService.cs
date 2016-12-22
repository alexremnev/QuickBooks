using QuickBooks.Models.DAL;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.EntityService
{
    public class OAuthService : IOAuthService
    {
        private readonly IOAuthRepository _repository;
        public OAuthService(IOAuthRepository repository)
        {
            _repository = repository;
        }
        public void Save(OAuth entity)
        {
            _repository.Create(entity);
        }

        public OAuth Get()
        {
            return _repository.Get();
        }
    }
}