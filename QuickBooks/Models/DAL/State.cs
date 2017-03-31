using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;

namespace QuickBooks.Models.DAL
{
    public class State
    {
        public List<string> realmIds;
        public string selectedItem { get; set; }
    }
}