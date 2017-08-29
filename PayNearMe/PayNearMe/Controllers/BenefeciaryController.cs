using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayNearMe.Models;
using PayNearMe.Controllers.api;

namespace PayNearMe.Controllers
{
    public class BenefeciaryController : Controller
    {
        
        // GET: /Benefeciary/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Benefeciaries() {

            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            WebServiceController ws = new WebServiceController();

            CustomerResultResponse resp = new CustomerResultResponse();

            resp = ws.getbeneficiarylist(Session["CustomerID"].ToString());

            List<Benefeciary> receiver = new List<Benefeciary>();

            BenefeciariesModel model = new BenefeciariesModel();

            if (resp.respcode == 1)
            {
                for (int i = 0; i < resp.benelist.Count; i++)
                {
                    String address = string.Empty;
                    String firstName = resp.benelist[i].firstName;
                    String lastName = resp.benelist[i].lastname;
                    String middleName = resp.benelist[i].midlleName;
                    String rCustID = resp.benelist[i].receiverCustID;
                    String phoneno = resp.benelist[i].phoneNo;
                    String name = string.Empty;

                    if (string.IsNullOrEmpty(middleName))
                    {
                        name = firstName + " " + lastName;
                    }
                    else
                    {
                        name = firstName + " " + middleName + " " + lastName;
                    }

                    address = resp.benelist[i].address;

                    receiver.Add(new Benefeciary()
                    {
                        HeaderName = firstName + " " + lastName,
                        Name = name,
                        Address = address,
                        Phone = phoneno,
                        Payer = "M LHuillier",
                        ReceiverCustID = rCustID
                    });

                }

                model.benefeciaries = receiver;
                return View(model);
            }
            else 
            {
                Session["ErrorMessage"] = resp.message;
                return View();
            }
        }

        [HttpPost]
        public ActionResult SelectBeneficiary(String receiverCustID)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            WebServiceController ws = new WebServiceController();
           
            var resp = ws.getBeneficiaryInfo(receiverCustID);
            resp.data.dateOfBirth = DateTime.Parse(resp.data.dateOfBirth.ToString()).ToString("MM/dd/yyyy");
            
            if (resp.respcode == 1)
            {
                return PartialView("EditBeneficiary",resp.data);
            }
            else 
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public ActionResult EditReceiver(BeneficiaryModel model)
        {
            WebServiceController ws = new WebServiceController();

            model.SenderCustID = Session["CustomerID"].ToString();
            model.phoneNo = "0" + model.phoneNo;
            CustomerResultResponse resp =  ws.updateBeneficiary(model);

           // Session.Add("ErrorMessage", resp.message);

            Session["ErrorMessage"] = resp.message;

            return RedirectToAction("Benefeciaries", "Benefeciary");
        }


        
        [HttpPost]
        public JsonResult Submit(BeneficiaryModel model)
        {
            try
            {
                WebServiceController ws = new WebServiceController();
                CustomerResultResponse resp = new CustomerResultResponse();
                model.SenderCustID = Session["CustomerID"].ToString();
                model.phoneNo = "0" + model.phoneNo;
                string timestamp = string.Empty;
                resp = ws.insertbeneficiary(model);

                return Json(resp.message);
            }
            catch (Exception ex)
            {
                ModelError err = new ModelError(ex, ex.ToString());
                return Json(ex.ToString());
            }
        }

        [HttpPost]
        public ActionResult AddBeneficiary() 
        {
            return PartialView("AddBeneficiary");
        }
	}
}