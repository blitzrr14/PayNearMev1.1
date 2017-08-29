using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayNearMe.Models;
using PayNearMe.Controllers.api;

namespace PayNearMe.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        private WebServiceController ws;
        public HomeController() 
        {
             ws = new WebServiceController();
        }
        
        public ActionResult Index()
        {
            if (Session["HomeModel"] != null)
            {

                var rFrex = ws.GetBranchRates("711", "3", "USD");
                if (rFrex.rescode == "1")
                {

                    Session.Add("ExchangeRate", Convert.ToDouble(rFrex.buying));

                }
                else
                {
                    Session["ErrorMessage"] = "Something Went Wrong, Please Try Again!";
                    return RedirectToAction("Login", "Account");
                }


                HomeModel model = new HomeModel();
                model = (HomeModel)Session["HomeModel"];
                model.DailyLimit = ws.getDailyLimit(Session["CustomerID"].ToString()).ToString();
                model.MonthlyLimit = ws.getMonthlyLimit(Session["CustomerID"].ToString()).ToString();
                model.tl = ws.getAllTransactionByCategoryM(Session["CustomerID"].ToString(), "", "", "Open").tl;
                model.ExchangeRate = Session["ExchangeRate"].ToString();
                return View(model);
            }
            else 
            {
                return RedirectToAction("Login","Account");
            }
        }   
	}
}