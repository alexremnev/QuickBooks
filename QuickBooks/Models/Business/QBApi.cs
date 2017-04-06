using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.GlobalTaxService;
using Intuit.Ipp.LinqExtender;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;

namespace QuickBooks.Models.Business
{
    public class QBApi : IQBApi
    {
        public QBApi(IOAuthService oAuthService)
        {
            _oAuthService = oAuthService;
        }

        private readonly IOAuthService _oAuthService;
        private readonly string _appToken = ConfigurationManager.AppSettings["qb.appToken"];
        private readonly string _consumerKey = ConfigurationManager.AppSettings["qb.consumerKey"];
        private readonly string _consumerSecret = ConfigurationManager.AppSettings["qb.consumerSecret"];
        private const IntuitServicesType IntuitServicesType = Intuit.Ipp.Core.IntuitServicesType.QBO;

        private ServiceContext GetContext(string realmId)
        {
            var permission = _oAuthService.Get(realmId);
            var oauthValidator = new OAuthRequestValidator(permission.AccessToken,
                permission.AccessTokenSecret, _consumerKey, _consumerSecret);
            var context = new ServiceContext(_appToken, permission.RealmId, IntuitServicesType,
                oauthValidator);
            return context;
        }

        private QueryService<T> GetQueryService<T>(string realmId)
        {
            return new QueryService<T>(GetContext(realmId));
        }

        private DataService GetDataService(string realmId)
        {
            return new DataService(GetContext(realmId));
        }

        public T[] List<T>(string realmId)
        {
            var queryService = GetQueryService<T>(realmId);
            return queryService.Select(x => x).ToArray();
        }

        public void Update(string realmId, SalesTransaction entity)
        {
            var dataService = GetDataService(realmId);
            dataService.Update(entity);
        }

        public T GetById<T>(string realmId, string id) where T : SalesTransaction
        {
            var service = GetQueryService<T>(realmId);
            var entityFromQuickBooks = service.Where(x => x.Id == id).FirstOrDefault();
            return entityFromQuickBooks;
        }

        public string GetTaxRateId(string realmId, decimal taxRate)
        {
            var queryService = GetQueryService<TaxRate>(realmId);
            return queryService
                .Where(x => x.RateValue == taxRate && x.Active)
                .Select(x => x.Id)
                .FirstOrDefault();
        }

        public TaxCode GetTaxCode(string realmId, string taxRateId)
        {
            var taxCodeQueryService = GetQueryService<TaxCode>(realmId);
            var stateTaxCodes = taxCodeQueryService.ExecuteIdsQuery("Select * From TaxCode");
            var taxCode = stateTaxCodes.Where(code => code != null)
                .Where(code => code.SalesTaxRateList != null)
                .Where(code => code.SalesTaxRateList.TaxRateDetail != null)
                .Where(code => code.SalesTaxRateList.TaxRateDetail[0] != null)
                .Where(code => code.SalesTaxRateList.TaxRateDetail[0].TaxRateRef != null)
                .Where(code => code.SalesTaxRateList.TaxRateDetail[0].TaxRateRef.Value != null)
                .FirstOrDefault(code => code.SalesTaxRateList.TaxRateDetail[0].TaxRateRef.Value == taxRateId);
            return taxCode;
        }

        public string GetNewTaxCodeId(string realmId, decimal percent)
        {
            var queryService = GetQueryService<TaxAgency>(realmId);
            var taxAgency = queryService.Where(x => x != null).FirstOrDefault();
            if (taxAgency == null)
            {
                var dataService = GetDataService(realmId);
                taxAgency = dataService.Add(new TaxAgency {DisplayName = "New Tax Agency"});
            }
            var name = $"{percent} percent tax";
            var taxRateDetailses = new[]
            {
                new TaxRateDetails
                {
                    RateValue = percent,
                    RateValueSpecified = true,
                    TaxAgencyId = taxAgency.Id,
                    TaxApplicableOn = TaxRateApplicableOnEnum.Sales,
                    TaxRateName = name
                }
            };
            var taxService = new TaxService {TaxCode = name, TaxRateDetails = taxRateDetailses};
            var globalTaxService = new GlobalTaxService(GetContext(realmId));
            taxService = globalTaxService.AddTaxCode(taxService);
            return taxService.TaxRateDetails[0].TaxRateId;
        }

        public ReportBasisEnum GetAccountingMethod(string realmId)
        {
            var dataService = GetDataService(realmId);
            var preferences = dataService.FindAll(new Preferences()).ToList();
            var accountingMethod = preferences[0].ReportPrefs.ReportBasis;
            return accountingMethod;
        }

        public IList<Deposit> FindAllDeposit(string realmId)
        {
            var dataService = GetDataService(realmId);
            return dataService.FindAll(new Deposit());
        }

        public void DeleteDeposit(string realmId, Deposit deposit)
        {
            GetDataService(realmId).Delete(deposit);
        }
    }
}