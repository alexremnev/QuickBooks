﻿using System.Configuration;
using Intuit.Ipp.Core;
using Intuit.Ipp.Security;
using QuickBooks.Models.DAL;
using QuickBooks.Models.Repository;

namespace QuickBooks.Models.EntityService
{
    public class OAuthService : IOAuthService
    {
        private readonly string _appToken = ConfigurationManager.AppSettings["appToken"];
        private readonly string _consumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        private readonly string _consumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        private const IntuitServicesType IntuitServicesType = Intuit.Ipp.Core.IntuitServicesType.QBO;
        private readonly string _realmId = ConfigurationManager.AppSettings["realmId"];
        private readonly IOAuthRepository _oAuthRepository;
        public OAuthService(IOAuthRepository oAuthRepository)
        {
            _oAuthRepository = oAuthRepository;
        }
        public void Save(OAuth entity)
        {
            _oAuthRepository.Create(entity);
        }
        public OAuth Get(string realmId)
        {
            return _oAuthRepository.Get(realmId);
        }
        public ServiceContext GetServiceContext()
        {
            var permission = Get(_realmId);
            var oauthValidator = new OAuthRequestValidator(permission.AccessToken,
                permission.AccessTokenSecret, _consumerKey, _consumerSecret);
            var context = new ServiceContext(_appToken, permission.RealmId, IntuitServicesType,
                oauthValidator);
           return context;
        }
        public void Delete(string id)
        {
            _oAuthRepository.Delete(id);
        }
    }
}