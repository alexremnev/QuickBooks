using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using Common.Logging;
using QuickBooks.Models.Business;
using QuickBooks.Models.DAL;

namespace QuickBooks.Controllers
{
    public class ReportController : Controller
    {
        public ReportController(params IPersisting[] services)
        {
            _services = services;
        }

        private static readonly ILog Log = LogManager.GetLogger<ReportController>();
        private readonly IList<IPersisting> _services;

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
            foreach (var service in _services)
            {
                service.Save(realmId);
            }
        }

        private void CalculateDocuments(string realmId)
        {
            var tasks = new Task[_services.Count];
            var counter = 0;
            foreach (var service in _services)
            {
                tasks[counter] = Task.Factory.StartNew(() => service.Calculate(realmId));
                counter++;
            }
//            for (var i = 0; i < _services.Count; i++)
//            {
//                var i1 = i;
//                tasks[i] = Task.Factory.StartNew(() => _services[i1].Calculate());
//            }
            Task.WaitAll(tasks);
        }
    }
}