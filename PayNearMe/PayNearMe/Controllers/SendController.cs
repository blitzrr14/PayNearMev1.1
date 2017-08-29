using log4net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PayNearMe.Controllers.api;
using PayNearMe.Models;
using PayNearMe.Models.Response;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ZXing;
using ZXing.Common;

namespace PayNearMe.Controllers
{
   
    public class SendController : Controller
    {
        //
        // GET: /Send/
        WebServiceController service;
        private String connection = string.Empty;
        IDictionary config;
        private ILog kplog;
        private static double pnmCharge = 3.99;
        private String siteIdentifier = string.Empty;
       
        public SendController() 
        {
            config = (IDictionary)(ConfigurationManager.GetSection("PayNearMeAPISection"));
            connection = config["globalcon"].ToString();
            siteIdentifier = config["siteIdentifier"].ToString();
            
            service = new WebServiceController();
          
            kplog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }


        public ActionResult Send()
        {

            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            SendModel model = new SendModel();

            if (TempData["ErrorMessage"] != null) 
            {
                model.Error = TempData["ErrorMessage"].ToString();
            }

          
         
            
            CustomerResultResponse res = new CustomerResultResponse();
            res = service.getbeneficiarylist(Session["CustomerID"].ToString());

            List<SelectListItem> listselect = new List<SelectListItem>();

            listselect.Add(new SelectListItem 
            {
                Value = "",
                Text = "--Select--"
            });

            if (res.benelist != null)
            {

                for (int x = 0; x < res.benelist.Count; x++)
                {
                    listselect.Add(new SelectListItem
                    {
                        Value = res.benelist[x].receiverCustID,
                        Text = res.benelist[x].firstName + " " + res.benelist[x].lastname

                    });
                }

            }

            var rFrex = service.GetBranchRates("711", "3", "USD");
            if (rFrex.rescode == "1")
            {

                model.trans.ExchangeRate = Convert.ToDouble(rFrex.buying);

            }
            else
            {
                Session["ErrorMessage"] = "Something Went Wrong, Please Try Again!";
                return RedirectToAction("Login", "Account");
            }


            model.trans.ExchangeRate = Convert.ToDouble(Session["ExchangeRate"]);
            model.beneficiarylist = listselect;
            TempData["ErrorMessage"] = null;
            model.siteIdentifier = siteIdentifier;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(SendModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    String errorALL = string.Empty;
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {
                             errorALL = errorALL + Environment.NewLine + error.ErrorMessage;
                        }
                    }

                    return Json(new { respcode = "failed", message = errorALL }, JsonRequestBehavior.AllowGet);
                }


            
                
              
                var trans = new TransactionModel()
                {
                    TransactionType = "Web",
                   // KPTN = ws.generateKPTNGlobal("711",3),
                    Principal = model.trans.Principal,
                    Charge = model.trans.Charge,
                    ExchangeRate = model.trans.ExchangeRate,
                    POAmountPHP = model.trans.POAmountPHP,
                    Total = model.trans.Total,
                    receiverCustId = model.receiver,
                    senderCustID = Session["CustomerID"].ToString()
                    

                };

                var response = service.createOrder(trans);
                if (response.status == "ok")
                {
                    return Json(new { respcode = response.status, trackingUrl = response.order.order_tracking_url, amount = response.order.order_amount }, JsonRequestBehavior.AllowGet);
                }
                else 
                {
                    return Json(new { respcode = "failed", message = response.errors[0].description }, JsonRequestBehavior.AllowGet);
                }
    
              
           
            }
            catch (Exception ex)
            {
                return Json(new { respcode = "failed", message = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public ActionResult getCharge(Double amount, String bcode, String zcode)
        {

            Double dailyLimit = service.getDailyLimit(Session["CustomerID"].ToString());
            Double monthlyLimit = service.getMonthlyLimit(Session["CustomerID"].ToString());

            if (amount > dailyLimit)
            {
                return Json(new ChargeResponse { respcode = 0, message = "Daily Limit Exceeded", charge = 0 }, JsonRequestBehavior.AllowGet);
            }
            else if (amount > monthlyLimit)
            {
                return Json(new ChargeResponse { respcode = 0, message = "Monthly Limit Exceeded", charge = 0 }, JsonRequestBehavior.AllowGet);
            }

            var chargeGResp = calculateChargePerBranchGlobal(amount, bcode, zcode);
            if (chargeGResp.respcode == 1)
            {
                chargeGResp.charge = chargeGResp.charge + Convert.ToDecimal(pnmCharge);
                return Json(chargeGResp, JsonRequestBehavior.AllowGet);

            }
            else if (chargeGResp.respcode == 16)
            {
                var response = calculateChargeGlobal(amount, bcode, zcode);
                if (response.charge == 0)
                {
                    return Json(new ChargeResponse { respcode = 1, message = "Success", charge = 0 }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    response.charge = response.charge + Convert.ToDecimal(pnmCharge);
                    return Json(response,JsonRequestBehavior.AllowGet);
                }

            }
            else
            {

                return Json(chargeGResp, JsonRequestBehavior.AllowGet);
            }
        }

        private ChargeResponse calculateChargeGlobal(Double amount, String bcode, String zcode)
        {



            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    using (MySqlCommand command = conn.CreateCommand())
                    {

                        DateTime NullDate = DateTime.MinValue;

                        Decimal dec = 0;
                        conn.Open();
                        MySqlTransaction trans = conn.BeginTransaction();

                        try
                        {
                            String query = "SELECT nextID,currID,nDateEffectivity,cDateEffectivity,cEffective,nextID, NOW() as currentDate FROM kpformsglobal.headercharges WHERE cEffective = 1;";

                            command.CommandText = query;
                            MySqlDataReader Reader = command.ExecuteReader();

                            if (Reader.Read())
                            {
                                Int32 nextID = Convert.ToInt32(Reader["nextID"]);
                                Int32 type = Convert.ToInt32(Reader["currID"]);

                                DateTime nDateEffectivity = (Reader["nDateEffectivity"].ToString().StartsWith("0")) ? NullDate : Convert.ToDateTime(Reader["nDateEffectivity"]);
                                DateTime currentDate = Convert.ToDateTime(Reader["currentDate"]);

                                if (nextID == 0)
                                {

                                    Reader.Close();
                                    String queryRates = "SELECT ChargeValue AS charge FROM kpformsglobal.charges WHERE ROUND(@amount,2) BETWEEN MinAmount AND MaxAmount AND `type` = @type;";
                                    command.CommandText = queryRates;
                                    command.Parameters.AddWithValue("amount", amount);
                                    command.Parameters.AddWithValue("type", type);

                                    MySqlDataReader ReaderRates = command.ExecuteReader();
                                    if (ReaderRates.Read())
                                    {
                                        dec = (Decimal)ReaderRates["charge"];
                                        ReaderRates.Close();
                                    }
                                }
                                else
                                {
                                    Reader.Close();

                                    int result = DateTime.Compare(nDateEffectivity, currentDate);

                                    if (result < 0)
                                    {



                                        command.Transaction = trans;
                                        command.Parameters.Clear();
                                        String updateRates = "update kpformsglobal.headercharges SET  cEffective = 2 where cEffective = 1";
                                        command.CommandText = updateRates;
                                        command.ExecuteNonQuery();

                                        command.Parameters.Clear();
                                        String updateRates1 = "update kpformsglobal.headercharges SET cEffective = 1 where currID = @curr";
                                        command.CommandText = updateRates1;
                                        command.Parameters.AddWithValue("curr", nextID);
                                        command.ExecuteNonQuery();

                                        command.Parameters.Clear();
                                        String insertLog = "insert into kpadminlogsglobal.kpratesupdatelogs (ModifiedRatesID, NewRatesID, DateModified, Modifier) values (@ModifiedRatesID, @NewRatesID, NOW(), @Modifier);";
                                        command.CommandText = insertLog;
                                        command.Parameters.AddWithValue("ModifiedRatesID", nextID - 1);
                                        command.Parameters.AddWithValue("NewRatesID", nextID);
                                        command.Parameters.AddWithValue("Modifier", "boskpws");
                                        command.ExecuteNonQuery();

                                        trans.Commit();

                                        kplog.Info("SUCCESS:: UPDATE kpformsglobal.headercharges: SET cEffective: 2 WHERE cEffective: 1");
                                        kplog.Info("SUCCESS:: UPDATE kpformsglobal.headercharges: SET cEffective: 1 WHERE currID: " + nextID);
                                        kplog.Info("SUCCESS:: INSERT INTO kpadminlogsglobal.kpratesupdatelogs: ModifiedRatesID: " + (nextID - 1) + " " +
                                            "NewRatesID: " + nextID + " " +
                                            "Modifier: boskpws");


                                        command.Parameters.Clear();
                                        String queryRates = "SELECT ChargeValue AS charge FROM kpformsglobal.charges WHERE ROUND(@amount,2) BETWEEN MinAmount AND MaxAmount AND `type` = @type;";
                                        command.CommandText = queryRates;
                                        command.Parameters.AddWithValue("amount", amount);
                                        command.Parameters.AddWithValue("type", nextID);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.Read())
                                        {

                                            dec = (Decimal)ReaderRates["charge"];
                                            ReaderRates.Close();
                                        }
                                    }
                                    else
                                    {


                                        command.Parameters.Clear();
                                        String queryRates = "SELECT ChargeValue AS charge FROM kpformsglobal.charges WHERE ROUND(@amount,2) BETWEEN MinAmount AND MaxAmount AND `type` = @type;";
                                        command.CommandText = queryRates;
                                        command.Parameters.AddWithValue("amount", amount);
                                        command.Parameters.AddWithValue("type", type);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.Read())
                                        {
                                            //ReaderRates.Read();
                                            dec = (Decimal)ReaderRates["charge"];
                                            ReaderRates.Close();
                                        }
                                    }
                                }


                            }
                            //trans.Commit();
                            conn.Close();
                            kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " charge: " + dec);
                            return new ChargeResponse { respcode = 1, message = getRespMessage(1), charge = dec };


                        }
                        catch (MySqlException mex)
                        {
                            kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + mex.ToString());
                            trans.Rollback();
                            conn.Close();
                            return new ChargeResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = mex.ToString() };
                        }
                    }

                }
                catch (Exception ex)
                {
                    kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                    conn.Close();
                    return new ChargeResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
                }
            }
        }

        private ChargeResponse calculateChargePerBranchGlobal(Double amount, String bcode, String zcode)
        {

            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    using (MySqlCommand command = conn.CreateCommand())
                    {

                        DateTime NullDate = DateTime.MinValue;

                        Decimal dec = 0;
                        conn.Open();
                        MySqlTransaction trans = conn.BeginTransaction();

                        try
                        {
                            String query = "SELECT nextID,currID,nDateEffectivity,cDateEffectivity,cEffective,nextID, NOW() as currentDate FROM kpformsglobal.ratesperbranchheader WHERE cEffective = 1 and branchcode = @bcode and zonecode = @zcode;";

                            command.CommandText = query;
                            command.Parameters.AddWithValue("bcode", bcode);
                            command.Parameters.AddWithValue("zcode", zcode);
                            MySqlDataReader Reader = command.ExecuteReader();

                            if (Reader.Read())
                            {
                                Int32 nextID = Convert.ToInt32(Reader["nextID"]);
                                Int32 type = Convert.ToInt32(Reader["currID"]);
                                //String ndate = (Reader["nDateEffectivity"].ToString().StartsWith("0")) ? null : Convert.ToDateTime(Reader["nDateEffectivity"]).ToString();
                                DateTime nDateEffectivity = (Reader["nDateEffectivity"].ToString().StartsWith("0")) ? NullDate : Convert.ToDateTime(Reader["nDateEffectivity"]);
                                DateTime currentDate = Convert.ToDateTime(Reader["currentDate"]);
                                //throw new Exception(nDateEffectivity.ToString());
                                if (nextID == 0)
                                {
                                    Reader.Close();
                                    String queryRates = "SELECT ChargeValue AS charge FROM kpformsglobal.ratesperbranchcharges WHERE ROUND(@amount,2) BETWEEN MinAmount AND MaxAmount AND `type` = @type;";
                                    command.CommandText = queryRates;
                                    command.Parameters.AddWithValue("amount", amount);
                                    command.Parameters.AddWithValue("type", type);

                                    MySqlDataReader ReaderRates = command.ExecuteReader();
                                    if (ReaderRates.Read())
                                    {
                                        dec = (Decimal)ReaderRates["charge"];
                                        ReaderRates.Close();
                                    }
                                }
                                else
                                {
                                    Reader.Close();

                                    int result = DateTime.Compare(nDateEffectivity, currentDate);

                                    if (result < 0)
                                    {

                                        //ReaderNextRates.Close();
                                        //UPDATE ANG TABLE EFFECTIVE
                                        // 0 = pending, 1 = current chage, 2 = unused

                                        //try
                                        //{
                                        command.Transaction = trans;
                                        command.Parameters.Clear();
                                        String updateRates = "update kpformsglobal.ratesperbranchheader SET  cEffective = 2 where cEffective = 1 and branchcode = @bcode and zonecode = @zcode";
                                        command.CommandText = updateRates;
                                        command.Parameters.AddWithValue("bcode", bcode);
                                        command.Parameters.AddWithValue("zcode", zcode);
                                        command.ExecuteNonQuery();

                                        command.Parameters.Clear();
                                        String updateRates1 = "update kpformsglobal.ratesperbranchheader SET cEffective = 1 where currID = @curr and branchcode = @bcode and zonecode = @zcode";
                                        command.CommandText = updateRates1;
                                        command.Parameters.AddWithValue("curr", nextID);
                                        command.Parameters.AddWithValue("bcode", bcode);
                                        command.Parameters.AddWithValue("zcode", zcode);
                                        command.ExecuteNonQuery();

                                        command.Parameters.Clear();
                                        String insertLog = "insert into kpadminlogsglobal.kpratesupdatelogs (ModifiedRatesID, NewRatesID, DateModified, Modifier) values (@ModifiedRatesID, @NewRatesID, NOW(), @Modifier);";
                                        command.CommandText = insertLog;
                                        command.Parameters.AddWithValue("ModifiedRatesID", nextID - 1);
                                        command.Parameters.AddWithValue("NewRatesID", nextID);
                                        command.Parameters.AddWithValue("Modifier", "boskpws");
                                        command.ExecuteNonQuery();

                                        trans.Commit();

                                        kplog.Info("SUCCESS:: UPDATE kpformsglobal.ratesperbranchheader: SET cEffective: 2 WHERE cEffective: 1 AND branchcode: " + bcode + " AND zonecode: " + zcode);
                                        kplog.Info("SUCCESS:: UPDATE kpformsglobal.ratesperbranchheader: SET cEffective: 1 WHERE currID: " + nextID + " AND branchcode: " + bcode + " AND zonecode: " + zcode);
                                        kplog.Info("SUCCESS:: INSERT INTO kpadminlogsglobal.kpratesupdatelogs: ModifiedRatesID: " + (nextID - 1) + " " +
                                            "NewRatesID: " + nextID + " " +
                                            "Modifier: boskpws");

                                        //}catch(MySqlException ex){
                                        //    //trans.Rollback();
                                        //    Reader.Close();

                                        //    throw new Exception(ex.ToString());
                                        //}

                                        command.Parameters.Clear();
                                        String queryRates = "SELECT ChargeValue AS charge FROM kpformsglobal.ratesperbranchcharges WHERE ROUND(@amount,2) BETWEEN MinAmount AND MaxAmount AND `type` = @type;";
                                        command.CommandText = queryRates;
                                        command.Parameters.AddWithValue("amount", amount);
                                        command.Parameters.AddWithValue("type", nextID);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.Read())
                                        {
                                            //ReaderRates.Read();
                                            dec = (Decimal)ReaderRates["charge"];
                                            ReaderRates.Close();
                                        }
                                    }
                                    else
                                    {
                                        //ReaderNextRates.Close();


                                        command.Parameters.Clear();
                                        String queryRates = "SELECT ChargeValue AS charge FROM kpformsglobal.ratesperbranchcharges WHERE ROUND(@amount,2) BETWEEN MinAmount AND MaxAmount AND `type` = @type;";
                                        command.CommandText = queryRates;
                                        command.Parameters.AddWithValue("amount", amount);
                                        command.Parameters.AddWithValue("type", type);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.Read())
                                        {
                                            //ReaderRates.Read();
                                            dec = (Decimal)ReaderRates["charge"];
                                            ReaderRates.Close();
                                        }
                                    }
                                }


                            }
                            else
                            {
                                kplog.Error("FAILED:: respcode: 16 message: " + getRespMessage(16) + " charge: " + dec);
                                Reader.Close();
                                conn.Close();
                                return new ChargeResponse { respcode = 16, message = getRespMessage(16), charge = dec };
                            }
                            //trans.Commit();
                            conn.Close();
                            kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " charge: " + dec);
                            return new ChargeResponse { respcode = 1, message = getRespMessage(1), charge = dec };
                        }
                        catch (MySqlException mex)
                        {
                            kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + mex.ToString());
                            trans.Rollback();
                            conn.Close();
                            return new ChargeResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = mex.ToString() };
                        }
                    }

                }
                catch (Exception ex)
                {
                    kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                    conn.Close();
                    return new ChargeResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
                }
            }
        }


        private String getRespMessage(Int32 code)
        {
            String x = "SYSTEM_ERROR";
            switch (code)
            {
                case 1:
                    return x = "Success";
                case 2:
                    return x = "Duplicate kptn";
                case 3:
                    return x = "KPTN already claimed";
                case 4:
                    return x = "KPTN not found";
                case 5:
                    return x = "Customer not found";
                case 6:
                    return x = "Customer already exist";
                case 7:
                    return x = "Invalid credentials";
                case 8:
                    return x = "KPTN already cancelled";
                case 9:
                    return x = "Transaction is not yet claimed";
                case 10:
                    return x = "Version does not match";
                case 11:
                    return x = "Problem occured during saving. Please resave the transaction.";
                case 12:
                    return x = "Problem saving transaction. Please close the sendout form and open it again. Thank you.";
                case 13:
                    return x = "Invalid station number.";
                case 14:
                    return x = "Error generating receipt number.";
                case 15:
                    return x = "Unable to save transaction. Invalid amount provided.";
                default:
                    return x;
            }


        }
    }
}
