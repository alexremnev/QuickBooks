using System.Collections.Generic;
using QuickBooks.Models.Data;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class OAuthService : IOAuthService
    {
        public OAuthService(IOAuthRepository oAuthRepository)
        {
            _oAuthRepository = oAuthRepository;
        }

        private readonly IOAuthRepository _oAuthRepository;

        public void Save(OAuth entity)
        {
            _oAuthRepository.Save(entity);
        }

        public IList<OAuth> List()
        {
            return _oAuthRepository.List();
        }

        public OAuth Get(string realmId)
        {
            return _oAuthRepository.Get(realmId);
        }

        public void Delete(string realmId)
        {
            _oAuthRepository.Delete(realmId);
        }
    }
}