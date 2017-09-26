using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using PayNearMe.Models;
using PayNearMe.Models.Response;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Collections;
using System.Configuration;
using System.Web.SessionState;
using PayNearMe.Controllers.api;
using Newtonsoft.Json;


namespace PayNearMe.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        IDictionary tble;
        String server = string.Empty;
        private String connection = string.Empty;
        private const string authentication_token = "TUxIVUlMTElFUkl0U2loTnNZekI0OTVRUTZKNU1YcjAvTzFyST0=";
        WebServiceController service;
        public AccountController()
        {
            service = new WebServiceController();
            tble = (IDictionary)(ConfigurationManager.GetSection("PayNearMeAPISection"));
            connection = tble["globalcon"].ToString();
            server = tble["server"].ToString();
        }

        //
        // GET: /Account/Login
        
        [AllowAnonymous]
        
        public ActionResult Login()

        {

            try
            {
                if (Session["UserID"] != null && Session["ErrorMessage"] == null)
                {
                 //  Session.Clear();

                   return RedirectToAction("Index", "Home");

                }
                else 
                {
                    if (Session["ErrorMessage"] != null)
                    {
                        Session.Add("fromLogin", true);
                    }
                    else 
                    {
                        Session.Clear();
                    }
                    
                    return View();
                }
                
            }
            catch (Exception ex)
            {
                Session["ErrorMessage"] = ex.ToString();
                return View();
            }



        }

        [AllowAnonymous]
        public ActionResult sessionExpire() 
        {
            Session.Clear();
            return RedirectToAction("LogIn", "Account");
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {

            try
            {
                
                    Session.Clear();
                    return RedirectToAction("Login");
                

            }
            catch (Exception)
            {

                return View("Error");
            }



        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult Submit(LoginViewModel model)
        {
            try
            {
            
                LoginResponse resp = new LoginResponse();
                resp = service.LoginPayNearMe(model);
                String jsonData = JsonConvert.SerializeObject(resp);
                
                if (resp.respcode == 1)
                {
                    Session.Add("ErrorMessage", null);
                    Session.Add("CustomerID", resp.customer.CustomerID);
                    Session.Add("UserID", model.EmailAddress);
                    
                    

                    HomeModel data = new HomeModel();
                    data.fullName = resp.fullName;
                    data.memberSince = Convert.ToDateTime(resp.signupDate).ToString("MMMM dd, yyyy");
                    data.lastLogin = Convert.ToDateTime(resp.lastLogin).ToString("MMMM dd, yyyy");
        


                    Session.Add("HomeModel", data);

                    return Json(jsonData);
                }
                else if (resp.respcode == 2) 
                {

                    Authenticate data = new Authenticate();

                    data.userId = model.EmailAddress;
                    Session.Add("Model", data);
                    return Json(jsonData);
                }
                else
                {
                    return Json(jsonData);
                }
            }
            catch (Exception ex)
            {
                return Json(JsonConvert.SerializeObject(new LoginResponse { respcode=0,message=ex.ToString()}));
            }

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


                throw new Exception(a.ToString());
            }
        }
       
        [AllowAnonymous]
        public ActionResult Register()
        {
            return RedirectToAction("Index", "Register");
        }

       



      

        
    }
}