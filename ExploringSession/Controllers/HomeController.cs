using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExploringSession.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            var a = Session["test"];
            Session["test"] = "Hello";
            return View();
        }

        public ActionResult Test()
        {
            var test = Session["test"];
            return View("Index");
        }
    }
}