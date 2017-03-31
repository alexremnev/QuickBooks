using System.Collections.Generic;
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

        private readonly string _appToken = ConfigurationManager.AppSettings["qb.appToken"];
        private readonly string _consumerKey = ConfigurationManager.AppSettings["qb.consumerKey"];
        private readonly string _consumerSecret = ConfigurationManager.AppSettings["qb.consumerSecret"];
        private const IntuitServicesType IntuitServicesType = Intuit.Ipp.Core.IntuitServicesType.QBO;
        private readonly IOAuthRepository _oAuthRepository;
        private ServiceContext context;

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

        public ServiceContext GetContext(string realmId)
        {
            if (context != null)
            {
                return context;
            }
            var permission = Get(realmId); 
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

        public QueryService<T> GetQueryService<T>(string realmId)
        {
            return new QueryService<T>(GetContext(realmId));
        }

        public DataService GetDataService(string realmId)
        {
            return new DataService(GetContext(realmId));
        }

        public GlobalTaxService GetGlobalTaxService(string realmId)
        {
            return new GlobalTaxService(GetContext(realmId));
        }
    }
}