using System.Collections.Generic;
using QuickBooks.Models.Data;

namespace QuickBooks.Models.Repository
{
    public interface IOAuthRepository : IRepository<OAuth>
    {
        /// <summary>
        /// Rerutns list of Oauth from database.
        /// </summary>
        /// <returns>list of Oauth or null in case if entities not found.</returns>
        IList<OAuth> List();
    }
}