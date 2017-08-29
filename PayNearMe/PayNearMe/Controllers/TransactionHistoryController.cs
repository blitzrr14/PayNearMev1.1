using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using PayNearMe.Models;
using PayNearMe.Models.Response;
using PayNearMe.Controllers;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Configuration;
using System.Web.Script.Serialization;
using PayNearMe.Controllers.api;
using CrystalDecisions.Shared;
using System.IO;

namespace PayNearMeSendOUT.Controllers
{
    public class TransactionHistoryController : Controller
    {
        WebServiceController service;
        List<SelectListItem> ids;
        public TransactionHistoryController()
        {
            service = new WebServiceController();
            ids = new List<SelectListItem>(){
                 new SelectListItem { Value = "",Text= "Select",Selected = true},
                new SelectListItem { Value = "01",Text= "01"},
                new SelectListItem { Value = "02",Text= "02"},
                new SelectListItem { Value = "03",Text= "03"},
                new SelectListItem { Value = "04",Text= "04"},
                new SelectListItem { Value = "05",Text= "05"},
                new SelectListItem { Value = "06",Text= "06"},
                new SelectListItem { Value = "07",Text= "07"},
                new SelectListItem { Value = "08",Text= "08"},
                new SelectListItem { Value = "09",Text= "09"},
                new SelectListItem { Value = "10",Text= "10"},
                new SelectListItem { Value = "11",Text= "11"},
                new SelectListItem { Value = "12",Text= "12"},
            };
        }


        public ActionResult Transaction(TransactionHistory model)
        {
            //LoginViewModel view = new LoginViewModel();
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            model.month = ids;

            TransactionResponseMobile res = new TransactionResponseMobile();
            res = service.getAllTransactionByCategoryM(Session["CustomerID"].ToString(), "", "", "");
            if (Request.Form["search"] != null)
                return ViewTransaction(model);
            if (res.respcode == 1)
            {
                model.tl = res.tl;
                model.Count = res.count;
                return View("TransactionHistory", model);
            }
            else
            {
                return View(res.tl);
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "View")]
        public ActionResult ViewTransaction(TransactionHistory model)
        {

            model.month = ids;
            if (model.kptn != null)
            {
                TransactionResponseMobile res = new TransactionResponseMobile();

                res = service.getKPTNbyAccount(model.kptn, Session["CustomerID"].ToString());
                if (res.respcode == 1)
                {
                    model.tl = res.tl;
                    model.Count = res.count;
                    return View("TransactionHistory", model);
                }
                else
                {
                    TempData.Add("ErrorMessage", res.message);
                    return View("TransactionHistory", model);
                }

            }
            else
            {
                if (model.This == 0)
                {
                    //PendingTransaction modelx = new PendingTransaction();
                    //return View("TransactionHistory", modelx);
                    if (Session["UserID"] == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }
                    TransactionResponseMobile res = new TransactionResponseMobile();

                    res = service.getAllTransactionByCategoryM(Session["CustomerID"].ToString(), "", "", model.Status);
                    if (Request.Form["search"] != null)
                        return ViewTransaction(model);
                    if (res.respcode == 1)
                    {
                        model.tl = res.tl;
                        model.Count = res.count;
                        return View("TransactionHistory", model);
                    }
                    else
                    {
                        return View("TransactionHistory", model);
                    }
                }
                else
                {


                    TransactionResponseMobile res2 = new TransactionResponseMobile();
                    if (model.This == 3)
                    {
                        res2 = service.getAllTransactionByCategoryM(Session["CustomerID"].ToString(), "", "", model.Status);
                    }
                    else if (model.This == 2)
                    {
                        res2 = service.getAllTransactionByCategoryM(Session["CustomerID"].ToString(), "", "now", model.Status);
                    }
                    else if (model.This == 4)
                    {
                        DateTime dt = service.getServerDateGlobal(false);
                        String month = dt.AddMonths(-1).ToString("MM");
                        res2 = service.getAllTransactionByCategoryM(Session["CustomerID"].ToString(), month, "", model.Status);
                    }
                    else if (model.This == 5)
                    {
                        res2 = service.getAllTransactionByCategoryM(Session["CustomerID"].ToString(), model.monthValue, model.yearValue, model.Status);
                    }

                    model.Count = res2.count;
                    model.tl = res2.tl;

                    if (res2.respcode == 1)
                    {
                        model.tl = res2.tl;
                        model.Count = res2.count;
                        return View("TransactionHistory", model);
                    }
                    else
                    {
                        return View("TransactionHistory", model);
                    }
                }
                
            }
        }


        [HttpPost]
        [MultipleButton(Name="action",Argument="Download")]
        public ActionResult downloadPDF(TransactionHistory model) 
        {

            try
            {
                String Month = string.Empty;
                String Year = string.Empty;
                DateTime dt = service.getServerDateGlobal(false);

               
                TransactionResponseMobile resp = new TransactionResponseMobile(); ;
                if (!string.IsNullOrEmpty(model.kptn))
                {
                    resp = service.getKPTNbyAccount(model.kptn, Session["CustomerID"].ToString());
                }
                else 
                {
                    if (model.This != 0 || model.This != null)
                    {
                        switch (model.This)
                        {

                            case 2:
                                Year = "now";
                                Month = "";
                                break;
                            case 3:
                                Month = "";
                                Year = "";
                                break;
                            case 4:
                                Month = dt.AddMonths(-1).ToString("MM");
                                Year = "";
                                break;
                            case 5:
                                Month = model.monthValue;
                                Year = model.yearValue;
                                break;
                            default:
                                Month = "";
                                Year = "";
                                break;
                        }
                    }

                    resp = service.getAllTransactionByCategoryM(Session["CustomerID"].ToString(), Month, Year, model.Status);
                }
              

                if (resp.tl.Count > 0)
            {
                PayNearMe.Models.Reports.MobileTransReport rpt = new PayNearMe.Models.Reports.MobileTransReport();
                
                string reportFileName = ((HomeModel)Session["HomeModel"]).fullName + "_" + dt.ToString("yyyy-MM-dd") + ".pdf";
                rpt.SetDataSource(resp.tl);
                rpt.SetParameterValue("Date", dt.ToString("MM/dd/yyyy"));
                rpt.SetParameterValue("accountid", ((HomeModel)Session["HomeModel"]).fullName);

                Stream stream = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", reportFileName);
            }
            else
            {
                model.month = ids;
                Session["ErrorMessage"] = "No Items Found.";
                return View("TransactionHistory", model);
            }
            }
            catch (Exception ex)
            {
                model.month = ids;
                Session["ErrorMessage"] = ex.ToString();
                return View("TransactionHistory",model);
            }


           
        }
    }
}
