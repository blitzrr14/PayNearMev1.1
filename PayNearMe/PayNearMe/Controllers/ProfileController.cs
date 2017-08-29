using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayNearMe.Models;
using PayNearMe.Controllers.api;
using PayNearMe.Models.Response;
using System.IO;
using MySql.Data.MySqlClient;

namespace PayNearMe.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        //
        // GET: /Profile/
        [AllowAnonymous]
        public ActionResult Index()
        {

            if (Session["UserID"] == null) 
            {
                return RedirectToAction("Login", "Account");
            }
            ProfileResponse resp = new ProfileResponse();
            WebServiceController ws = new WebServiceController();
            
            resp = ws.getProfile(Session["CustomerID"].ToString());
            resp.IDs = ws.getIDTypes();
            if (resp.respcode == 1)
            {

                return View(resp);
            }
            else 
            {
                Session["ErrorMessage"] = resp.message;
                return RedirectToAction("Login", "Account");
            }
            
        }

        [AllowAnonymous]
        public ActionResult EditProfile(ProfileResponse model, HttpPostedFileBase slfprt, HttpPostedFileBase slfprt1F, HttpPostedFileBase slfprt1B, HttpPostedFileBase slfprt2F, HttpPostedFileBase slfprt2B) 
        {

            if (Session["UserID"] == null) 
            {
                return RedirectToAction("Login", "Account");
            }

            #region IDImage
            if (slfprt != null)
            {
                byte[] fileData = null;
                using (var binaryReader = new BinaryReader(slfprt.InputStream))
                {
                    fileData = binaryReader.ReadBytes(Convert.ToInt32(slfprt.InputStream.Length));
                }

                string strBase64 = Convert.ToBase64String(fileData);
                model.sender.strBase64Image = strBase64;
            }
            else
            {
                model.sender.strBase64Image = "";
            }

            if (slfprt1F != null)
            {
                byte[] fileData = null;
                using (var binaryReader = new BinaryReader(slfprt1F.InputStream))
                {
                    fileData = binaryReader.ReadBytes(Convert.ToInt32(slfprt1F.InputStream.Length));
                }

                string strBase64 = Convert.ToBase64String(fileData);
                model.sender.strBase64Image1F = strBase64;
            }
            else
            {
                model.sender.strBase64Image1F = "";
            }

            if (slfprt1B != null)
            {
                byte[] fileData = null;
                using (var binaryReader = new BinaryReader(slfprt1B.InputStream))
                {
                    fileData = binaryReader.ReadBytes(Convert.ToInt32(slfprt1B.InputStream.Length));
                }

                string strBase64 = Convert.ToBase64String(fileData);
                model.sender.strBase64Image1B = strBase64;
            }
            else
            {
                model.sender.strBase64Image1B = "";
            }

            if (slfprt2F != null)
            {
                byte[] fileData = null;
                using (var binaryReader = new BinaryReader(slfprt2F.InputStream))
                {
                    fileData = binaryReader.ReadBytes(Convert.ToInt32(slfprt2F.InputStream.Length));
                }

                string strBase64 = Convert.ToBase64String(fileData);
                model.sender.strBase64Image2F = strBase64;
            }
            else
            {
                model.sender.strBase64Image2F = "";
            }

            if (slfprt2B != null)
            {
                byte[] fileData = null;
                using (var binaryReader = new BinaryReader(slfprt2B.InputStream))
                {
                    fileData = binaryReader.ReadBytes(Convert.ToInt32(slfprt2B.InputStream.Length));
                }

                string strBase64 = Convert.ToBase64String(fileData);
                model.sender.strBase64Image2B = strBase64;
            }
            else
            {
                model.sender.strBase64Image2B = "";
            }
            #endregion
            

            WebServiceController ws = new WebServiceController();
            ProfileResponse resp = new ProfileResponse();
            resp = ws.editProfile(model.sender);
            return Json(resp, JsonRequestBehavior.AllowGet);
            
        
        }

        [AllowAnonymous]
        public ActionResult ChangePassword() 
        {

            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Session["ErrorMessage"] = null;
            return View();
        }


        [AllowAnonymous]
        public ActionResult ChangePasswordPost(ChangePasswordModel model) 
        {



            WebServiceController service = new WebServiceController();
            ProfileResponse resp = new ProfileResponse();
            
            model.UserID = Session["UserID"].ToString();
            resp = service.changePassword(model);

            if (resp.respcode == 1) {

                Session["ErrorMessage"] = "Password is successfully updated!";
                return View("~/Views/Profile/ChangePassword.cshtml");
            }
            else
            {
                Session["ErrorMessage"] = resp.message;
                return View("~/Views/Profile/ChangePassword.cshtml");
            }
            
           
        }

       
	}
}