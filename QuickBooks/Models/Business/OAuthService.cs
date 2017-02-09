using System.Configuration;
using Intuit.Ipp.Core;
using Intuit.Ipp.Security;
using QuickBooks.Models.DAL;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.Business
{
    public class OAuthService : IOAuthService
    {
        public OAuthService(IOAuthRepository oAuthRepository)
        {
            _oAuthRepository = oAuthRepository;
        }
        private readonly string _appToken = ConfigurationManager.AppSettings["appToken"];
        private readonly string _consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        private readonly string _consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        private const IntuitServicesType IntuitServicesType = Intuit.Ipp.Core.IntuitServicesType.QBO;
        private readonly IOAuthRepository _oAuthRepository;

        public void Save(OAuth entity)
        {
            _oAuthRepository.Save(entity);
        }
        public OAuth Get()
        {
            return _oAuthRepository.Get();
        }
        public ServiceContext GetServiceContext()
        {
            var permission = Get();
            var oauthValidator = new OAuthRequestValidator(permission.AccessToken,
                permission.AccessTokenSecret, _consumerKey, _consumerSecret);
            var context = new ServiceContext(_appToken, permission.RealmId, IntuitServicesType,
                oauthValidator);
            return context;
        }
        public void Delete()
        {
            _oAuthRepository.Delete();
        }
    }
}