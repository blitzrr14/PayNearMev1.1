using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayNearMe.Models;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Configuration;
using System.Collections;
using PayNearMe.Content.Helper;
using MySql.Data.MySqlClient;
using log4net;
using log4net.Config;
using System.Data;
using System.Net.Mail;
using PayNearMe.Controllers.api;
using PayNearMe.Models.Response;
using System.Text;
//----------------------------
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml;
//----------------------------

namespace PayNearMe.Controllers
{
    [Authorize]
    public class RegisterController : Controller
    {
        //
        // GET: /Register/
        IDictionary tble;
        string server = string.Empty;
        private const string authentication_token = "TUxIVUlMTElFUkl0U2loTnNZekI0OTVRUTZKNU1YcjAvTzFyST0=";
        private const string authentication_code = "MLHUILLIER";
        private String connection = string.Empty;
        private String dbconofac = string.Empty;
        private static readonly ILog kplog = LogManager.GetLogger(typeof(RegisterController));
        WebServiceController service;
       

        public RegisterController() 
        {
             tble = (IDictionary)(ConfigurationManager.GetSection("PayNearMeAPISection"));
             server = tble["server"].ToString();
             connection = tble["globalcon"].ToString();
             dbconofac = tble["ofaccon"].ToString();
             service = new WebServiceController();
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            try
            {
                RegisterModel model = new RegisterModel();

                if (Session["ErrorMessage"] != null)
                {
                    model.errorMessage = Session["ErrorMessage"].ToString();
                }
                else 
                {
                    model.errorMessage = "";
                }

                // model.questions = getSecurityQuestions();
                // model.destinationCountry = "Philippines";
                // model.licensedStates = getLicensedStates();
                // Session.Add("State", model.licensedStates);
                // Session.Add("Question", model.questions);
                model.IDTypeDropDownList = service.getIDTypes();
                return View(model);
            }
            catch (Exception ex)
            {
                ErrorModel error = new ErrorModel();
                error.ErrorMessage = ex.ToString();
                error.ErrorCode = "500";
                return View("Error",error);
            }
        }

        [AllowAnonymous]
        public ActionResult Authenticate() 
        {
            if (Session["Model"] != null)
            {
                Authenticate model = (Authenticate)Session["Model"];
                return View(model);
            }
            else 
            {
                return View();
            }
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AuthenticateCode(Authenticate model)
        {
            try
            {
             
                AuthenticateResponse resp = new AuthenticateResponse();
                var req = new AuthenticateRequest();
                req.UserID = model.userId;
                req.ActivationCode = model.authenticationCode;
                resp = service.authenticateSignup(req);

                if (resp.respcode == 1)
                {
                    LoginViewModel loginModel = new LoginViewModel();
                    loginModel.EmailAddress = resp.userID;
                    loginModel.Password = resp.password;

                    var respx = service.LoginPayNearMe(loginModel);

                    if (respx.respcode == 1)
                    {

                        Session.Add("UserID", resp.userID);
                        Session.Add("CustomerID", respx.customer.CustomerID);
                        Session.Add("ErrorMessage", null);
                        HomeModel data = new HomeModel();
                        data.fullName = respx.fullName;
                        data.memberSince = Convert.ToDateTime(respx.signupDate).ToString("MMMM dd, yyyy");
                        data.lastLogin = Convert.ToDateTime(respx.lastLogin).ToString("MMMM dd, yyyy");
                        Session.Add("HomeModel", data);

                        return Json(new { respcode = respx.respcode, message = respx.message }, JsonRequestBehavior.AllowGet);
                    }
                    else if (respx.respcode == 2)
                    {
                        Authenticate data = new Authenticate();

                        data.userId = model.userId;
                      
                        Session.Add("Model", data);
                        return Json(new { respcode = respx.respcode, message = respx.message }, JsonRequestBehavior.AllowGet);
                    }
                    else 
                    {
                        return Json(new { respcode = respx.respcode, message = respx.message }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { respcode = resp.respcode, message = resp.message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                return Json(new { respcode = 0, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ResendAuth(Authenticate model) 
        {
            try
            {
               

                var resp = service.resendActivationCode(model.userId);
                return Content(resp.message);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Register(RegisterModel model, HttpPostedFileBase slfprt)
        {

          

            try
           {

               //var apiIDologyResp = ExpectID_IQ_Check(model).Result;


               // if (apiIDologyResp == "FAIL")
               // {
               //     Session["ErrorMessage"] = "Unable to confirm user";
               //     return RedirectToAction("Index", "Register");
               // }
               // else if (apiIDologyResp == "ERROR")
               // {
               //     Session["ErrorMessage"] = "An Error has occured. [IDology]";
               //     return RedirectToAction("Index", "Register");
               // }


                string strBase64 = string.Empty;
                if (slfprt != null) 
                {
                    byte[] fileData = null;
                    using (var binaryReader = new BinaryReader(slfprt.InputStream))
                    {
                        fileData = binaryReader.ReadBytes(Convert.ToInt32(slfprt.InputStream.Length));
                    }

                    strBase64   = Convert.ToBase64String(fileData);
                }

                if (ModelState.IsValid)
                {
                  //  String test = model.userIPAddress;
                    Session.Add("Model", model);
                    model.data.Country = "USA";

                    String rValid = validateField(model);
                    if (rValid != "Success")
                    {
                        Session["ErrorMessage"] =  rValid;
                        return RedirectToAction("Index", "Register");
                    }

                    String fullName = string.Empty;
                    if (model.data.middleName != null && model.data.middleName != "" && model.data.middleName != string.Empty)
                    {
                        fullName  = model.data.firstName + " " + model.data.middleName + " " + model.data.lastName;
                    }
                    else 
                    {
                        fullName = model.data.firstName + " " + model.data.lastName;
                    }
                   
                    //if (OfacMatch(fullName))
                    //{
                    //    Session.Add("ErrorMessage", "Unable to register. Please proceed to the nearest branch");
                    //    Session["State"] = null;
                    //    Session["Question"] = null;
                    //    return RedirectToAction("Index", "Register");
                    //}

                    AddKYCResponse respGlobal = new AddKYCResponse();
                    String ActivationCode =  generateActivationCode();
                  
                    CustomerModel req = new CustomerModel();
                 
                    req = model.data;
                    req.activationCode = ActivationCode;
                    req.BranchID = "PayNearMe";
                    req.CreatedBy = "PayNearMe";
                    req.Password = model.data.Password;
                    req.strBase64Image = strBase64;
                    
                    respGlobal = service.addKYCGlobal(req);

                    if (respGlobal.respcode == 1)
                    {
                        Authenticate modelAuth = new Authenticate();
                        modelAuth.userId = model.data.UserID;
                        return View("Authenticate", modelAuth);
                    }
                    else if (respGlobal.respcode == 6)
                    {
                        Session["ErrorMessage"] = "Something went wrong, Please try again. Thank You!";
                        return RedirectToAction("Index", "Register");
                    }
                    else 
                    {
                        Session["ErrorMessage"] =  respGlobal.message;
                        return RedirectToAction("Index", "Register");
                    }
                }
                else 
                {

                    String errorALL = string.Empty;
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {
                            errorALL = errorALL + Environment.NewLine + error.ErrorMessage;
                        }
                    }

                    Session["ErrorMessage"] = errorALL;
                    return RedirectToAction("Index", "Register");
                }
            }
            catch (Exception ex)
            {
                Session["ErrorMessage"] = ex.ToString();
                return RedirectToAction("Index", "Register");
            }
        }

        [AllowAnonymous]
        public ActionResult Captcha() 
        {
            try
            {

            
            var r = Request.Params["g-recaptcha-response"];
            using (var wc = new WebClient())
            {
                var validateString = string.Format( "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}",
                                                    "6Ld_nRYUAAAAADsXrLPCxNu1kLMJC5ygSN_occa7",    // secret recaptcha key
                                                    r); // recaptcha value
                // Get result of recaptcha
                var recaptcha_result = wc.DownloadString(validateString);
                // Just check if request make by user or bot
                if (recaptcha_result.ToLower().Contains("false"))
                {
                    return Content("false"); ;
                }
            }
            return Content("true");

            }
            catch (Exception ex)
            {

                ErrorModel error = new ErrorModel();
                error.ErrorMessage = ex.ToString();
                error.ErrorCode = "500";
                return View("Error", error);
            }
        }

        [AllowAnonymous]
        public ActionResult getServer() 
        {
            server = server + "/jsps/";
            return Content(server);
        }
        private String SendRequest(String info, Uri uri)
        {
            try
            {
                String res = null;
                HttpWebRequest web = (HttpWebRequest)WebRequest.Create(uri);
                web.ContentType = "application/json";
                web.Method = "POST";
                web.Headers.Add("authentication_code", authentication_code);
                web.Headers.Add("authentication_token", authentication_token);
                web.ContentLength = info.Length;
                StreamWriter requestWriter = new StreamWriter(web.GetRequestStream(), System.Text.Encoding.ASCII);
                requestWriter.Write(info);
                requestWriter.Close();
                WebResponse webresponse = web.GetResponse();
                Stream response = webresponse.GetResponseStream();
                res = new StreamReader(response).ReadToEnd();

                return res;
            }
            catch (Exception ex)
            {
                //Kplog.Fatal(ex.ToString());
                return "Unable to process request. The system encountered some technical problem. Sorry for the inconvenience. " + ex.ToString() + "";
            }
        }

        public String getRespMessage(Int32 code)
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
                case 16:
                    return x = "Branch does not exist in Branch Charges.";
                case 17:
                    return x = "International Transaction is not allowed to payout.";

                default:
                    return x;
            }
        }

        public String validateField(RegisterModel model) 
        {
            String emailAddress = model.data.UserID;
            String password = model.data.Password;
            String firstName = model.data.firstName;
            String lastName = model.data.lastName;
            String dateOfBirth = model.data.BirthDate;
            String mobileNo = model.data.PhoneNo;
            String gender = model.data.Gender;
            String street = model.data.Street;
            String zipCode = model.data.ZipCode;
            String city = model.data.City;
            String country = model.data.Country;
            String state = model.data.State;



            if (emailAddress == String.Empty || emailAddress == null ||  emailAddress == "") 
            {
                return "Email Address is Required";
            }
            else if (password == String.Empty || password == null || password == "") 
            {
                return "Password is Required";
            }
            else if (firstName == String.Empty || firstName == null || firstName == "")
            {
                return "First Name is Required";
            }
            else if (lastName == String.Empty || lastName == null || lastName == "")
            {
                return "Last Name is Required";
            }
            else if (dateOfBirth == String.Empty || dateOfBirth == null || dateOfBirth == "")
            {
                return "Date of Birth is Required";
            }
            else if (mobileNo == String.Empty || mobileNo == null || mobileNo == "")
            {
                return "Mobile Number is Required";
            }
            else if (gender == String.Empty || gender == null || gender == "")
            {
                return "Gender is Required";
            }
            else if (street == String.Empty || street == null || street == "")
            {
                return "Street is Required";
            }
            else if (zipCode == String.Empty || zipCode == null || zipCode == "")
            {
                return "Zip Code is Required";
            }
            else if (city == String.Empty || city == null || city == "")
            {
                return "City is Required";
            }
            else if (country == String.Empty || country == null || country == "")
            {
                return "Country is Required";
            }
   
            else if (state == String.Empty || state == null || state == "")
            {
                return "State is Required";
            }
            return "Success";
        
        }

        private String generateInteractionId(String conn)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(conn)) 
                {
                    con.Open();
                    using (MySqlCommand command = con.CreateCommand())
                    {
                        command.CommandText = "SELECT NOW()+0";
                        string query = "SELECT NOW()+0 as datekaron;";
                        command.CommandText = query;

                        MySqlDataReader rdr = command.ExecuteReader();
                        if (rdr.Read())
                        {
                            var str = rdr["datekaron"].ToString();
                            var fileControlNo = str.Substring(2, 12);
                            rdr.Close();
                            return fileControlNo;
                        }
                        return "error";
                    }
                }
            }
            catch (Exception a)
            {
                kplog.Fatal("FAILED:: respcode: 0 message : " + getRespMessage(0) + " , ErrorDetail: " + a.ToString());
                throw new Exception(a.ToString());
            }
        }

        private String ConvertDateTime(String date) 
        {

            string month = "";
            string day = "";
            string year = "";

            year = date.Substring(6,4);
            day = date.Substring(3,2);
            month = date.Substring(0,2);

            date = year + "-" + month + "-" + day;

            date = Convert.ToDateTime(date).ToString("yyyy-MM-dd 00:00:00");
            return date;
            


        }

        

        private String generateActivationCode()
        {
            using (MySqlConnection custconn = new MySqlConnection(connection))
            {
                custconn.Open(); 
                using (MySqlCommand custcommand = custconn.CreateCommand())
                {
                    custcommand.CommandText = "select now()+0 as serverdt";
                    MySqlDataReader rdrserverdt = custcommand.ExecuteReader();
                    rdrserverdt.Read();
                    string x =  rdrserverdt["serverdt"].ToString().Substring(7, 7);
                    rdrserverdt.Close();
                    custconn.Close();
                    return x;
                }
            }
        }

      
  

       
    }
}