using System.Collections.Generic;

namespace QuickBooks.Models.Business
{
    public abstract class Registry<T> 
    {
        protected Registry(params T [] services)
        {
            _services = services;
        }

        private readonly IList<T> _services;

        public IList<T> GetServices()
        {
            return _services;
        }
    }
}