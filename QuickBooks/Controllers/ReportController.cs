using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Common.Logging;
using QuickBooks.Models.Business;
using QuickBooks.Models.Data;

namespace QuickBooks.Controllers
{
    public class ReportController : Controller
    {
        public ReportController(ICalculatingServicesRegistry calculatingServicesRegistry, IPersistingServicesRegistry persistingServicesRegistry)
        {
            _persistingServices = persistingServicesRegistry.GetServices();
            _calculatingServices = calculatingServicesRegistry.GetServices();
        }

        private static readonly ILog Log = LogManager.GetLogger<ReportController>();
        private readonly IList<ICalculatingService> _calculatingServices;
        private readonly IList<IPersistingService> _persistingServices;

        public ActionResult Save(State state)
        {
            try
            {
                var realmId = state.selectedItem;
                if (realmId != null)
                    SaveData(realmId);
                return RedirectToActionPermanent("Index", "Home");
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when you tried to pull entity", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        public ActionResult Recalculate(State state)
        {
            try
            {
                var realmId = state.selectedItem;
                if (realmId != null)
                    CalculateDocuments(realmId);
                return RedirectToActionPermanent("Index", "Home");
            }
            catch (Exception e)
            {
                Log.Error("Exception occured when you tried to recalculate sales tax", e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        private void SaveData(string realmId)
        {
            foreach (var service in _persistingServices)
            {
                service.Save(realmId);
            }
        }

        private void CalculateDocuments(string realmId)
        {
            var tasks = new Task[_calculatingServices.Count];
            var counter = 0;
            foreach (var service in _calculatingServices)
            {
                tasks[counter] = Task.Factory.StartNew(() => service.Calculate(realmId));
                counter++;
            }
            Task.WaitAll(tasks);
        }
    }
}