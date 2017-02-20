using System.Configuration;
using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;
using Intuit.Ipp.GlobalTaxService;
using Intuit.Ipp.QueryFilter;
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
        private ServiceContext context;

        public void Save(OAuth entity)
        {
            _oAuthRepository.Save(entity);
        }
        public OAuth Get()
        {
            return _oAuthRepository.Get();
        }
        public void Delete()
        {
            _oAuthRepository.Delete();
        }

        public ServiceContext GetContext()
        {
            if (context != null)
            {
                return context;
            }
            var permission = Get();
            var oauthValidator = new OAuthRequestValidator(permission.AccessToken,
                  permission.AccessTokenSecret, _consumerKey, _consumerSecret);
            context = new ServiceContext(_appToken, permission.RealmId, IntuitServicesType,
                 oauthValidator);
            context.IppConfiguration.Message.Request.SerializationFormat =
              Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
            context.IppConfiguration.Message.Response.SerializationFormat =
                Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
            return context;
        }

        public QueryService<T> GetQueryService<T>()
        {
            return new QueryService<T>(GetContext());
        }

        public DataService GetDataService()
        {
            return new DataService(GetContext());
        }

        public GlobalTaxService GetGlobalTaxService()
        {
            return new GlobalTaxService(GetContext());
        }

    }
}