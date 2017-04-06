namespace QuickBooks.Models.Business
{
    public class CalculatingServicesRegistry : Registry<ICalculatingService>, ICalculatingServicesRegistry
    {
        public CalculatingServicesRegistry(params ICalculatingService[] services) : base(services)
        {
        }
    }
}