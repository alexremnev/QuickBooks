namespace QuickBooks.Models.Business
{
    public class UpdatingServicesRegistry : Registry<IUpdatingService>, IUpdatingServicesRegistry
    {
        public UpdatingServicesRegistry(params IUpdatingService[] services) : base(services)
        {
        }
    }
}