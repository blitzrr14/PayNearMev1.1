﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayNearMe.Models;
using System.Collections;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Net.Mail;
using System.Net;
using log4net;
using AESEncrypt;
using System.Threading;

namespace PayNearMe.Controllers
{
    public class ForgotPasswordController : Controller
    {
        IDictionary config;
        private String connection = string.Empty;
        private ILog kplog;
        private AESEncryption encdata = new AESEncryption();
        private String encStringKey = "B905BD7BFBD902DCB115B327F9018CEA";

        public ForgotPasswordController() {
            config = (IDictionary)(ConfigurationManager.GetSection("PayNearMeAPISection"));
            connection = config["globalcon"].ToString();
            kplog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CheckEmail(String email)
        {
            ForgotPasswordModelResponse response = new ForgotPasswordModelResponse();
            response.code = 0;
            if (!string.IsNullOrEmpty(email))
            {
                try
                {   using (MySqlConnection con = new MySqlConnection(connection))
                    {   con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {   cmd.CommandText = "Select UserID, CustomerID, FullName, securityCode from kpcustomersglobal.PayNearMe where UserID = @email";
                            cmd.Parameters.AddWithValue("email", email);

                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                            {   response.code = 1;
                                response.message = "Verified!..";

                                rdr.Read();
                                    response.encCID = encdata.AESEncrypt(rdr["CustomerID"].ToString(), encStringKey);
                                    response.encFN = encdata.AESEncrypt(rdr["FullName"].ToString(), encStringKey);
                                    response.isForgot = rdr["securityCode"].ToString() != string.Empty ? true : false; ;
                                rdr.Close();
                            }
                            else
                                response.message = "The email address specified <br /> has not been registered...";
                        }
                    }
                }
                catch (Exception ex)
                {   response.code = -1;
                    response.message = "A System Error has occured Please try again...";
                    kplog.Error("[CheckEmail] System Error: " + ex.Message);
                }
            }
            else
                response.message = "Please kindly provide an <br /> email address...";

            return new JsonResult { Data = response };
        }

        [HttpPost]
        public JsonResult RequestCode(String email, String encCID, String encFN)
        {   
            ForgotPasswordModelResponse response = new ForgotPasswordModelResponse();

            if (!string.IsNullOrEmpty(email) &&
                !string.IsNullOrEmpty(encFN) &&
                !string.IsNullOrEmpty(encCID) )
            {
                String custID = encdata.AESDecrypt(encCID.Replace(' ', '+'), encStringKey);
                String fullName = encdata.AESDecrypt(encFN.Replace(' ', '+'), encStringKey);

                Random random = new Random();
                const string chars = "9AB8CD7EF6GH5IJ4KL3MN2OP1QR0ST9UV8WX7YZ6012345";
                String securityCode = new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());

                try
                {   using (MySqlConnection con = new MySqlConnection(connection))
                    {   con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {   cmd.CommandText = "UPDATE `kpcustomersglobal`.`PayNearMe` SET securityCode = @securityCode WHERE CustomerID = @custID AND FullName = @fullName";
                            cmd.Parameters.AddWithValue("securityCode", securityCode);
                            cmd.Parameters.AddWithValue("custID", custID);
                            cmd.Parameters.AddWithValue("fullName", fullName);
                            if (cmd.ExecuteNonQuery() > 0)
                            {   if (sendSecurityCode(email, securityCode, custID, fullName))
                                {   response.code = 1;
                                    response.message = "Code sent...<br /> Please check your email";
                                }
                                else
                                {   response.code = 0;
                                    response.message = "Service error : Unable to mail code... <br /> send request timeout";
                                }
                            }
                            else
                                throw new Exception("Server Error : failed to update security code...");
                        }
                    }
                }
                catch (Exception ex)
                {   kplog.Error("[RequestCode] where email = '" + email + "', encCID = '" + encCID + "', encFN = '" + encFN + "'; ErrorMessage = " + ex.ToString());
                    response.code = -1;
                    response.message = "Server error upon generating security code";
                }
            }
            else
            {   response.code = 0;
                response.message = "Invalid request code data";
            }
            return new JsonResult { Data = response };
        }

        [HttpPost]
        public JsonResult ResendCode(String email, String encCID, String encFN)
        {
            ForgotPasswordModelResponse response = new ForgotPasswordModelResponse();
            response.code = 0;

            if (!string.IsNullOrEmpty(email) &&
                !string.IsNullOrEmpty(encFN) &&
                !string.IsNullOrEmpty(encCID))
            {
                String custID = encdata.AESDecrypt(encCID.Replace(' ', '+'), encStringKey);
                String fullName = encdata.AESDecrypt(encFN.Replace(' ', '+'), encStringKey);

                try
                {   using (MySqlConnection con = new MySqlConnection(connection))
                    {   con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            cmd.CommandText = "SELECT securityCode FROM `kpcustomersglobal`.`PayNearMe` WHERE UserID = @email AND CustomerID = @custID AND FullName = @fullName";
                            cmd.Parameters.AddWithValue("email", email);
                            cmd.Parameters.AddWithValue("custID", custID);
                            cmd.Parameters.AddWithValue("fullName", fullName);

                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                            {   rdr.Read();
                                String securityCode = rdr["securityCode"].ToString();
                                rdr.Close();

                                if (securityCode != string.Empty)
                                {
                                    if (sendSecurityCode(email, securityCode, custID, fullName))
                                    {   response.code = 1;
                                        response.message = "Code sent...<br /> Please check your email";
                                    }
                                    else
                                        response.message = "Service error : Unable to mail code... <br /> send request timeout";
                                }
                                else
                                    response.message = "Data provided an error <br /> Please refresh the page..."; 
                            }
                            else
                                response.message = "Unable to find user info <br /> Please refresh the page...."; 
                        }
                    }
                }
                catch (Exception ex)
                {
                    kplog.Error("[ResendCode] where email = '" + email + "', encCID = '" + encCID + "', encFN = '" + encFN + "'; ErrorMessage = " + ex.ToString());
                    response.code = -1;
                    response.message = "Server error upon resending security code";
                }
            }
            return new JsonResult { Data = response };
        }

        private string generateAutoForgotPasswordLink(String email, String securityCode, String custID, String fullName)
        {
            string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/";
            string controllerLink = "ForgotPassword/fpConfReq?";
            string encData = "e=" + encdata.AESEncrypt(email.ToString(), encStringKey).Replace(' ', '+') + '&'
                           + "sc=" + encdata.AESEncrypt(securityCode.ToString(), encStringKey).Replace(' ', '+') + '&'
                           + "cid=" + encdata.AESEncrypt(custID.ToString(), encStringKey).Replace(' ', '+') + '&'
                           + "fn=" + encdata.AESEncrypt(fullName.ToString(), encStringKey).Replace(' ', '+');
                           
            return baseUrl + controllerLink + encData;
        }

        private Boolean sendSecurityCode(String email, String SecurityCode, String CustID, String FullName)
        {
            String autolink = generateAutoForgotPasswordLink(email, SecurityCode, CustID, FullName);
            SmtpClient client = new SmtpClient();
            client.EnableSsl = false;
            client.UseDefaultCredentials = true;
            client.Host = "smtp.gmail.com";                                                     
            client.Port = 587;
            client.Credentials = new NetworkCredential("fake01esoj@gmail.com", "M@K#d0nj0s31"); 
            MailMessage msg = new MailMessage();
            msg.To.Add(email);
            msg.From = new MailAddress("ML PayNearMe<fake01esoj@gmail.com>");
            msg.Subject = "PayNearMe - Request Forgot Password";
            msg.Body = "<div style=\"font-size: 16px; font-family: Consolas; text-align: justify; margin: 0 auto; width: 500px; color: black; padding: 20px; border-left: 1px solid #FFF0CA; border-right: 1px solid #FFF0CA; border-radius: 20px;\">"
                     + "<p> Good day Ma'am/Sir <b>" + FullName + "</b>,</p>"
                     + "<p>"
                     + "With M. Lhuillier it is easy to send money to your friends and family around "
                     + "different parts of the world in a fast, convenient and secure way."
                     + "</p>"
                     + "<p> You have requested for a forgot password security code. </p>"
                     + "Let's retrieve your account! <br />"
                     + "Security Code : <b>" + SecurityCode + "</b> <br />"
                     + "Copy and enter the code  provided or just "
                     + "<a href=\"" + autolink + "\" target=\"_blank\" style=\"font-size: 17px;\"> Click Here </a>"
                     + "to proceed on retrieving your password"
                     + "<br /><br />"
                     + "If for instance the above button link doesn't work please copy and paste the link below to your browser to proceed."
                     + "<br /><br /><code>" + autolink
                     + "</code><br /><br />"
                     + "<div style=\"font-size: 14px; background-color: lightgray; padding: 0 15px; \">"
                     + "If by in any chance you did not forget or you have remembered your password or "
                     + "you did not request this security code then please ignore this message. Thank You!"
                     + "</div><br /><br />"
                     + "<div style=\"font-size: 14px; border-top: 1px solid lightgray; text-align: center; padding-top: 5px; background-color: gray;\">"
                     + "-- This mail is auto generated. Please do not reply. --"
                     + "</div></div>";
            msg.IsBodyHtml = true;

            Boolean isSent = false;
            for (int retries = 0; retries < 3; retries++)
            {   try
                {   client.Send(msg);
                    isSent = true;
                    retries = 3;
                }
                catch (Exception err)
                {
                    if (retries == 2)
                        kplog.Error(err.ToString());
                    if (retries < 2)
                        Thread.Sleep(1300); //Delay for 1.3seconds
                }
            }
            return isSent;
        }

        [HttpPost]
        public JsonResult CheckCode(String email, String securityCode, String encCID, String encFN)
        {
            ForgotPasswordModelResponse response = new ForgotPasswordModelResponse();
            response.code = 0;
            try
            {
                if (securityCode.Length == 4 &&
                    !string.IsNullOrEmpty(email) &&
                    !string.IsNullOrEmpty(encFN) &&
                    !string.IsNullOrEmpty(encCID) &&                     
                    !string.IsNullOrEmpty(securityCode))
                {
                    String custID = encdata.AESDecrypt(encCID.Replace(' ', '+'), encStringKey);
                    String fullName = encdata.AESDecrypt(encFN.Replace(' ', '+'), encStringKey);
                    if (verifyCode(email, securityCode, custID, fullName))
                    {
                        response.code = 1;
                        response.message = "Verified...";
                    }
                    else
                        response.message = "Invalid security code...";
                }
                else
                    response.message = "Invalid request code data <i>(empty)</i>";
            }
            catch (Exception ex)
            {
                kplog.Error("[CheckCode] where email = '" + email + "', code = '" + securityCode + "', encCID = '" + encCID + "', encFN = '" + encFN + "'; ErrorMessage = " + ex.ToString());
                response.code = -1;
                response.message = "Server error upon verifying security code. Please try again... ";
            }

            return new JsonResult { Data = response };
        }

        public ActionResult fpConfReq(String e, String sc, String cid, String fn)
        {
            String eDecoded = string.Empty, cidDecoded = string.Empty, scDecoded = string.Empty, fnDecoded = string.Empty;
            ForgotPasswordModel response = new ForgotPasswordModel();
            
            try
            {
                eDecoded = encdata.AESDecrypt(e.Replace(' ', '+'), encStringKey);
                cidDecoded = encdata.AESDecrypt(cid.Replace(' ', '+'), encStringKey);
                scDecoded = encdata.AESDecrypt(sc.Replace(' ', '+'), encStringKey);
                fnDecoded = encdata.AESDecrypt(fn.Replace(' ', '+'), encStringKey);

                response.email = eDecoded;
                response.securityCode = scDecoded;
                
                if (verifyCode(eDecoded, scDecoded, cidDecoded, fnDecoded))
                    return View(response);
                else
                    return View("Error", new ErrorModel { ErrorCode = "fp", ErrorMessage = "○ Unable to determine request which could be :"
                                                                                         + "<br />─ Server may be down at the moment, please try again later"
                                                                                         + "<br />─ Perhaps the link your browsing might be expired"
                                                                                         + "<br />─ If the problem persist please reprocess your forgot password request to generate another link thru email." });
            }
            catch (Exception ex)
            {
                kplog.Error("[fpConfReq] Unable to read forgot password autogenerated link..." + ex.Message);
                return View("Error", new ErrorModel { ErrorCode = "fp", ErrorMessage = "Service error, Please generate another by reprocessing your forgot password request." });
            }
        }

        public Boolean verifyCode(String email, String securityCode, String custID, String fullName)
        {
            Boolean isVerified = false;
            try
            { using (MySqlConnection con = new MySqlConnection(connection))
                {   con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {   cmd.CommandText = "SELECT securityCode FROM `kpcustomersglobal`.`PayNearMe` "
                                        + "WHERE UserID = @email AND CustomerID = @custID AND securityCode = @code AND FullName = @fullName";
                        cmd.Parameters.AddWithValue("email", email);
                        cmd.Parameters.AddWithValue("code", securityCode);
                        cmd.Parameters.AddWithValue("custID", custID);
                        cmd.Parameters.AddWithValue("fullName", fullName);

                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                            isVerified = true;
                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Error("[verifyCode] System Error..." + ex.Message);
            }

            return isVerified;
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public JsonResult fpChangePassword (ForgotPasswordModel fp)
        {
            ForgotPasswordModelResponse response = new ForgotPasswordModelResponse();
            response.code = 0;

            try
            {   using (MySqlConnection con = new MySqlConnection(connection))
                {   con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {       
                        cmd.CommandText = "UPDATE `kpcustomersglobal`.`PayNearMe` "
                                        + "SET securityCode = null, Password = @npasswrd "
                                        + "WHERE UserID = @email AND securityCode = @code";
                        cmd.Parameters.AddWithValue("email", fp.email);
                        cmd.Parameters.AddWithValue("code", fp.securityCode);
                        cmd.Parameters.AddWithValue("npasswrd", fp.fpNewPassword);
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            response.code = 1;
                            response.message = "Successfully Updated Password! <br /> Redirecting to login page...";
                        }
                        else
                            response.message = "Data mismatch [unable to update]...<br /> Please refresh the page and try again";
                    }
                }
            }
            catch (Exception ex)
            {
                response.code = -1;
                response.message = "Server error in confirming new password...";
                kplog.Error("[fpChangePassword] System Error..." + ex.Message);
            }
            return new JsonResult { Data = response };
        }
    }
}