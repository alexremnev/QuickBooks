using System.Collections.Generic;
using Intuit.Ipp.Data;

namespace QuickBooks.Models.Business
{
    public interface IQBApi
    {
        T[] List<T>(string realmId);
        void Update(string realmId, SalesTransaction entity);
        T GetById<T>(string realmId, string id) where T : SalesTransaction;
        string GetTaxRateId(string realmId, decimal taxRate);
        TaxCode GetTaxCode(string realmId, string taxRateId);
        string GetNewTaxCodeId(string realmId, decimal percent);
        ReportBasisEnum GetAccountingMethod(string realmId);
        IList<Deposit> FindAllDeposit(string realmId);
        void DeleteDeposit(string realmId, Deposit deposit);
    }
}