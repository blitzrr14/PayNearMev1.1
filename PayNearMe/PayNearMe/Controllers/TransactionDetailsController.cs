using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayNearMe.Models;
using PayNearMe.Controllers;
using PayNearMe.Controllers.api;
using PayNearMe.Models.Response;

namespace PayNearMeSendOUT.Controllers
{
    public class TransactionDetailsController : Controller
    {
        //
        // GET: /TransactionDetails/
        WebServiceController service;
        public TransactionDetailsController()
       {
           service = new WebServiceController();
       }
        public ActionResult TransactionDetails(PendingTransaction model)
        {

           
            TransactionResponse res = new TransactionResponse();
             
            res = service.getKPTNbyDetails(Session["CustomerID"].ToString(),model.kptn);
            if (res.respcode == 1)
            {
                

                return View(res.detail);
            }

            return View(model);
        }
    }
}