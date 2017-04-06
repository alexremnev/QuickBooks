namespace QuickBooks.Models.Business
{
    public class PersistingServicesRegistry : Registry<IPersistingService>, IPersistingServicesRegistry
    {
        public PersistingServicesRegistry(params IPersistingService[] services) : base(services)
        {
        }
    }
}