using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PayNearMe.Models;
using PayNearMe.Models.Response;
using System.Web.Script.Serialization;
using System.IO;
using System.Net.Security;
using System.Configuration;
using System.Collections;
using PayNearMe.Content.Helper;
using MySql.Data.MySqlClient;
using log4net;
using log4net.Config;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using ZXing;
using System.Drawing;
using ZXing.Common;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Specialized;
using System.Threading;
using System.Web.Http.Controllers;
using System.Threading.Tasks;
using System.Globalization;
using CrystalDecisions.Shared;



namespace PayNearMe.Controllers.api
{
    public class WebServiceController : ApiController
    {

        IDictionary config;
        string server = string.Empty;
        private MySqlCommand command;
        private String connection = string.Empty;
        private String dbconofac = string.Empty;
        private String PNMServer = string.Empty;
        private DateTime dt;
        private MySqlCommand custcommand;
        private MySqlTransaction custtrans = null;
        private  String siteIdentifier = "";
        private  String secretKey = "";
        private static double pnmCharge = 3.99;
        private String ftp = string.Empty;
        private String http = string.Empty;
        private ILog kplog;
        private double dailyLimit = 0.0;
        private double monthlyLimit = 0.0;
        private static readonly HttpClient client = new HttpClient();
        private String smtpServer = string.Empty;
        private String smtpUser = String.Empty;
        private String smtpPass = String.Empty;
        private String smtpSender = String.Empty;
        private String mlforex = String.Empty;
        private String ftpUser = String.Empty;
        private String ftpPass = String.Empty;
        private String iDologyServer = String.Empty;
        private String iDologyUser = String.Empty;
        private String iDologyPass = String.Empty;
        private Boolean smtpSsl = false;
        private Boolean iDology = false;
      
        public WebServiceController()
        {
            
            config = (IDictionary)(ConfigurationManager.GetSection("PayNearMeAPISection"));
            server = config["server"].ToString();
            connection = config["globalcon"].ToString();
            mlforex = config["mlforexrate"].ToString();
            dbconofac = config["ofaccon"].ToString();
            dailyLimit = Convert.ToDouble(config["dailyLimit"]);
            monthlyLimit = Convert.ToDouble(config["monthlyLimit"]);
            ftp = config["ftp"].ToString();
            http = config["http"].ToString();
            kplog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            smtpServer = config["smtpServer"].ToString();
            smtpUser = config["smtpUser"].ToString();
            smtpPass = config["smtpPass"].ToString();
            smtpSender = config["smtpSender"].ToString();
            smtpSsl = Convert.ToBoolean(config["smtpSsl"]);
            secretKey = config["secretKey"].ToString();
            siteIdentifier = config["siteIdentifier"].ToString();
            ftpPass = config["ftpPass"].ToString();
            ftpUser = config["ftpUser"].ToString();
            iDologyServer = config["iDologyServer"].ToString();
            iDologyUser = config["iDologyUser"].ToString();
            iDologyPass = config["iDologyPass"].ToString();
            iDology = Convert.ToBoolean(config["iDology"]);

        }

        //done loggings
       
        public CustomerResultResponse insertbeneficiary(BeneficiaryModel bene)
        {
            kplog.Info("START--- > PARAMS: " + JsonConvert.SerializeObject(bene));
            String sendercustid = bene.SenderCustID;
            String rcvrfirstname = cleanString(bene.firstName);
            String rcvrlastname = cleanString(bene.lastname);
            String rcvrmiddlename = cleanString(bene.midlleName);
            String rcvrcountry = bene.country;
            String rcvrstreet = cleanString(bene.street);
            String rcvrcitystate = cleanString(bene.city);
            String rcvrzipcode = bene.zipcode;
            String rcvrbirthdate = bene.dateOfBirth;
            String rcvrgender = bene.gender;
            String rcvrrelation = cleanString(bene.relation);
            String rcvrcontactno = cleanString(bene.phoneNo);
            String rcvrcustid = bene.receiverCustID;
            String uploadpath = string.Empty;
            String filepath = string.Empty;

    

            try
            {
                dt = getServerDateGlobal1(false);
            }
            catch (Exception ex)
            {


                kplog.Fatal(ex);
                return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();

                        MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                        Int32 sr = 0;
                        Int32 srkyc = 0;
                        Int32 bkyc = 0;
                        string updatebeneficiary = String.Empty;
                        string updatecustomerseries = String.Empty;
                        string benecustid = String.Empty;
                        string benecustidkyc = String.Empty;
                        string benecustidkycsame = String.Empty;
                        string rcvid = String.Empty;
                        using (command = con.CreateCommand())
                        {

                            try
                            {
                                string checking = String.Empty;

                                if (string.IsNullOrEmpty(rcvrmiddlename))
                                {
                                   // checking = "select sendercustid from kpcustomersglobal.BeneficiaryHistory where sendercustid=@sendercustid and firstname=@firstname and lastname=@lastname;";
                                    checking = "select b.sendercustid,p.isActivate,p.ReceiverCustID from kpcustomersglobal.BeneficiaryHistory b inner join kpcustomersglobal.BeneficiaryPayNearMe p ON p.ReceiverCustID = b.CustIDB where b.sendercustid=@sendercustid and b.firstname=@firstname and b.lastname=@lastname;";
                                    rcvrmiddlename = "";
                                }
                                else
                                {
                                    checking = "select b.sendercustid,p.isActivate,p.ReceiverCustID  from kpcustomersglobal.BeneficiaryHistory b inner join kpcustomersglobal.BeneficiaryPayNearMe p ON p.ReceiverCustID = b.CustIDB where b.sendercustid=@sendercustid and b.firstname=@firstname and b.lastname=@lastname and b.middlename=@middlename;";
                                }
                                command.Transaction = trans;
                                command.CommandText = checking;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("sendercustid", sendercustid);
                                command.Parameters.AddWithValue("firstname", rcvrfirstname);
                                command.Parameters.AddWithValue("lastname", rcvrlastname);
                                command.Parameters.AddWithValue("middlename", rcvrmiddlename);
                                MySqlDataReader reader = command.ExecuteReader();
                                if (reader.Read())
                                {
                                    Int32 isactivate = Convert.ToInt32(reader["isActivate"]);
                                    String receiverCustID = reader["ReceiverCustID"].ToString();
                                    reader.Close();

                                    if (isactivate == 1)
                                    {
                                        con.Close();
                                        kplog.Info("Beneficiary Already Exist");
                                        return new CustomerResultResponse { respcode = 0, message = "Beneficiary Already Exist" };
                                    }
                                    else 
                                    {
                                        command.Parameters.Clear();
                                        command.CommandText = "UPDATE kpcustomersglobal.BeneficiaryPayNearMe SET isActivate = 1 where ReceiverCustID = @receiverCustID;";
                                        command.Parameters.AddWithValue("receiverCustID", receiverCustID);
                                        command.ExecuteNonQuery();

                                        string uptblbeneficiary = "update kpcustomersglobal.BeneficiaryHistory set MiddleName=@nrmname, FullName=@nrfullname, CityState=@nrcity, ZipCode=@nrzipcode, BirthDate=@nrbdate, Gender=@nrgender, Relation=@nrrelation, ContactNo=@nrcontact, lasttransdate = now(), Street = @nrstreet, Country = @nrcountry where sendercustid=@sendercustid1  and CustIDB=@rcvrcustid";
                                        command.CommandText = uptblbeneficiary;
                                        command.Parameters.Clear();
                                        command.Parameters.AddWithValue("sendercustid1", sendercustid);
                                        command.Parameters.AddWithValue("nrmname", rcvrmiddlename);
                                        command.Parameters.AddWithValue("nrfullname", rcvrlastname + ", " + rcvrfirstname + " " + rcvrmiddlename);
                                        command.Parameters.AddWithValue("nrcity", rcvrcitystate);
                                        command.Parameters.AddWithValue("nrzipcode", rcvrzipcode);
                                        command.Parameters.AddWithValue("nrbdate", rcvrbirthdate == String.Empty ? null : Convert.ToDateTime(rcvrbirthdate).ToString("yyyy-MM-dd"));
                                        command.Parameters.AddWithValue("nrgender", rcvrgender);
                                        command.Parameters.AddWithValue("nrrelation", rcvrrelation);
                                        command.Parameters.AddWithValue("nrcontact", rcvrcontactno);
                                        command.Parameters.AddWithValue("rcvrcustid", receiverCustID);
                                        command.Parameters.AddWithValue("nrstreet", rcvrstreet);
                                        command.Parameters.AddWithValue("nrcountry", rcvrcountry);
                                        command.ExecuteNonQuery();


                                        trans.Commit();
                                        con.Close();
                                        return new CustomerResultResponse { respcode = 1, message = "Beneficiary's information is successfully added!", receiverCustID = benecustid };
                                    }

                                    
                                }
                                else
                                {

                                    reader.Close();

                                    //KYC series
                                    String querykycseries = "select series from kpformsglobal.customerseries";
                                    command.CommandText = querykycseries;
                                    command.Parameters.Clear();
                                    MySqlDataReader rdrkyc = command.ExecuteReader();
                                    if (rdrkyc.HasRows)
                                    {
                                        rdrkyc.Read();
                                        if (!(rdrkyc["series"] == DBNull.Value))
                                        {
                                            srkyc = Convert.ToInt32(rdrkyc["series"].ToString());
                                        }
                                    }
                                    rdrkyc.Close();

                                    Int32 sr1kyc = srkyc + 1;
                                    if (srkyc == 0)
                                    {
                                        updatecustomerseries = "INSERT INTO kpformsglobal.customerseries(series,year) values('" + sr1kyc + "','" + dt.ToString("yyyy") + "')";
                                    }
                                    else
                                    {
                                        updatecustomerseries = "update kpformsglobal.customerseries set series = '" + sr1kyc + "', year = '" + dt.ToString("yyyy") + "'";
                                    }
                                    command.CommandText = updatecustomerseries;
                                    command.ExecuteNonQuery();
                                    benecustidkyc = generateCustIDGlobal(command);


                                    //Beneficiary series
                                    string slctmaxseries = "select series from kpformsglobal.beneficiaryseries";
                                    command.CommandText = slctmaxseries;
                                    command.Parameters.Clear();
                                    MySqlDataReader rdrseries = command.ExecuteReader();
                                    if (rdrseries.HasRows)
                                    {
                                        rdrseries.Read();
                                        if (!(rdrseries["series"] == DBNull.Value))
                                        {
                                            sr = Convert.ToInt32(rdrseries["series"].ToString());
                                        }
                                    }
                                    rdrseries.Close();

                                    Int32 sr1 = sr + 1;
                                    if (sr == 0)
                                    {
                                        updatebeneficiary = "INSERT INTO kpformsglobal.beneficiaryseries(series,year) values('" + sr1 + "','" + dt.ToString("yyyy") + "')";
                                    }
                                    else
                                    {
                                        updatebeneficiary = "update kpformsglobal.beneficiaryseries set series = '" + sr1 + "', year = '" + dt.ToString("yyyy") + "'";
                                    }
                                    command.CommandText = updatebeneficiary;
                                    command.ExecuteNonQuery();
                                    benecustid = generateBeneficiaryCustIDGlobal(command);
                                }
                                reader.Close();

                                command.CommandText = "INSERT INTO kpcustomersglobal.BeneficiaryHistory(custidb, custids, firstname, lastname, middlename, fullname, street, citystate, country, zipcode, birthdate, gender, relation, contactno, sendercustid, lasttransdate) values(@custidb, @custids, @firstname, @lastname, @middlename, @fullname, @street, @citystate, @country, @zipcode, @birthdate, @gender, @relation, @contactno, @sendercustid, now())";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("custidb", benecustid);
                                command.Parameters.AddWithValue("custids", bkyc == 1 ? benecustidkycsame : benecustidkyc);
                                command.Parameters.AddWithValue("firstname", rcvrfirstname);
                                command.Parameters.AddWithValue("lastname", rcvrlastname);
                                command.Parameters.AddWithValue("middlename", rcvrmiddlename);
                                command.Parameters.AddWithValue("fullname", rcvrlastname + ", " + rcvrfirstname + " " + rcvrmiddlename);
                                command.Parameters.AddWithValue("street", rcvrstreet);
                                command.Parameters.AddWithValue("citystate", rcvrcitystate);
                                command.Parameters.AddWithValue("country", rcvrcountry);
                                command.Parameters.AddWithValue("zipcode", rcvrzipcode);
                                command.Parameters.AddWithValue("birthdate", rcvrbirthdate == String.Empty ? null : Convert.ToDateTime(rcvrbirthdate).ToString("yyyy-MM-dd"));
                                command.Parameters.AddWithValue("gender", rcvrgender);
                                command.Parameters.AddWithValue("relation", rcvrrelation);
                                command.Parameters.AddWithValue("contactno", rcvrcontactno);
                                command.Parameters.AddWithValue("sendercustid", sendercustid);
                                int y = command.ExecuteNonQuery();
                                command.Parameters.Clear();

                                kplog.Info("Success kpcustomersglobal.BeneficiaryHistory");

                                if (!string.IsNullOrEmpty(bene.strBase64Image))
                                {
                                    String filename = getTimeStamp().ToString() + ".png";
                                    uploadpath = ftp + "/PayNearMe/Images/" + filename;
                                    uploadFileImage(bene.strBase64Image, uploadpath);
                                    filepath = http + "/PayNearMe/Images/" + filename;

                                }
                                    command.CommandText = "INSERT INTO kpcustomersglobal.BeneficiaryPayNearMe(ReceiverCustID,isActivate,ImagePath) VALUES (@benecustid,'1',@imagePath)";
                                    command.Parameters.AddWithValue("benecustid", benecustid);
                                    command.Parameters.AddWithValue("imagePath", filepath);
                                    int x = command.ExecuteNonQuery();

                                    kplog.Info("Success kpcustomersglobal.BeneficiaryPayNearMe");

                                
                                


                                if (y > 0 && x > 0)
                                {
                                    // PayNearMe API
                                    Int32 timestamp = getTimeStamp();
                                    string yearofbirth = Convert.ToDateTime(rcvrbirthdate).ToString("yyyy");

                                    string query = "city=" + rcvrcitystate + "&country=" + rcvrcountry + "&first_name=" + rcvrfirstname + "&last_name=" + rcvrlastname + "&middle_name=" + rcvrmiddlename + "&postal_code=" + rcvrzipcode + "&site_identifier=" + siteIdentifier + "&site_user_identifier=" + benecustid + "&street=" + rcvrstreet + "&timestamp=" + timestamp.ToString() +
                                                    "&user_type=receiver&version=2.0&year_of_birth=" + yearofbirth;



                                    string signature = generateSignature(query);

                                    query = query + "&signature=" + signature;

                                    Uri uri = new Uri(server + "/json-api/create_user?" + query);

                                    string res = SendRequest(uri);
                                    kplog.Info("Response: PayNearMe API: create_user: "+res);

                                    dynamic data = JObject.Parse(res);

                                    if (data.status == "ok")
                                    {
                                        trans.Commit();
                                        con.Close();
                                        kplog.Info("Beneficiary Successfully Added");
                                        return new CustomerResultResponse { respcode = 1, message = "Beneficiary's information is successfully added!", receiverCustID = benecustid };
                                    }
                                    else
                                    {
                                        trans.Rollback();
                                        con.Close();

                                        string error = "";
                                        for (int xx = 0; xx < data.errors.Count; xx++)
                                        {
                                            error = error + " " + data.errors[xx].description;
                                        }
                                        kplog.Error(error);
                                        return new CustomerResultResponse { respcode = 0, message = error };
                                    }


                                }

                                trans.Rollback();
                                con.Close();
                                kplog.Info("Error in Adding Beneficiary");
                                return new CustomerResultResponse { respcode = 0, message = "Error in Adding Beneficiary" };

                            }
                            catch (MySqlException myx)
                            {
                                trans.Rollback();
                                con.Close();
                                kplog.Fatal(myx.ToString());
                                return new CustomerResultResponse { respcode = 0, message = myx.Message, ErrorDetail = myx.ToString() };
                            }
                        }
                    }
                    catch (MySqlException mex)
                    {

                        con.Close();
                        kplog.Fatal(mex.ToString());
                        return new CustomerResultResponse { respcode = 0, message = mex.ToString(), ErrorDetail = mex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal(ex.ToString());
                return new CustomerResultResponse { respcode = 0, message = ex.ToString(), ErrorDetail = ex.ToString() };
            }
        }

        //done loggings RR
       
        public CustomerResultResponse updateBeneficiary(BeneficiaryModel model)
        {
            kplog.Info("START -->  PARAMS: " + JsonConvert.SerializeObject(model));

            String rcvrfirstname = cleanString(model.firstName);
            String rcvrlastname = cleanString(model.lastname);
            String rcvrmiddlename = cleanString(model.midlleName);
            String rcvrstreet = cleanString(model.street);
            String rcvrcitystate = cleanString(model.city);
            String rcvrcountry = model.country;
            String rcvrzipcode = model.zipcode;
            String rcvrbirthdate = model.dateOfBirth;
            String rcvrgender = model.gender;
            String rcvrrelation = cleanString(model.relation);
            String rcvrcontactno = (model.phoneNo).Trim();
            String rcvrcustid = model.receiverCustID;

            try
            {
                dt = getServerDateGlobal1(false);
            }
            catch (Exception ex)
            {
                kplog.Fatal(ex);
                return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
            }
            try
            {
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        string updatebeneficiary = String.Empty;
                        string updatecustomerseries = String.Empty;
                        string benecustid = String.Empty;
                        string benecustidkyc = String.Empty;
                        string benecustidkycsame = String.Empty;
                        string rcvid = String.Empty;

                        using (command = con.CreateCommand())
                        {

                            try
                            {

                                string checking = "select sendercustid from kpcustomersglobal.BeneficiaryHistory a inner join kpcustomersglobal.BeneficiaryPayNearMe b on a.CustIDB = b.ReceiverCustID where a.CustIDB=@rcvrcustid and sendercustid=@sendercustid1";
                                MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);
                                command.Transaction = trans;
                                command.CommandText = checking;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("rcvrcustid", rcvrcustid);
                                command.Parameters.AddWithValue("sendercustid1", model.SenderCustID);
                                MySqlDataReader reader = command.ExecuteReader();
                                if (reader.Read())
                                {
                                    reader.Close();

                                    string uptblbeneficiary = "update kpcustomersglobal.BeneficiaryHistory set FirstName=@nrfname, LastName=@nrlname, MiddleName=@nrmname, FullName=@nrfullname, CityState=@nrcity, ZipCode=@nrzipcode, BirthDate=@nrbdate, Gender=@nrgender, Relation=@nrrelation, ContactNo=@nrcontact, lasttransdate = now(), Street = @nrstreet, Country = @nrcountry where sendercustid=@sendercustid1  and CustIDB=@rcvrcustid";
                                    command.CommandText = uptblbeneficiary;
                                    command.Parameters.Clear();
                                    command.Parameters.AddWithValue("sendercustid1", model.SenderCustID);
                                    command.Parameters.AddWithValue("nrfname", rcvrfirstname);
                                    command.Parameters.AddWithValue("nrlname", rcvrlastname);
                                    command.Parameters.AddWithValue("nrmname", rcvrmiddlename);
                                    command.Parameters.AddWithValue("nrfullname", rcvrlastname + ", " + rcvrfirstname + " " + rcvrmiddlename);
                                    command.Parameters.AddWithValue("nrcity", rcvrcitystate);
                                    command.Parameters.AddWithValue("nrzipcode", rcvrzipcode);
                                    command.Parameters.AddWithValue("nrbdate", rcvrbirthdate == String.Empty ? null : Convert.ToDateTime(rcvrbirthdate).ToString("yyyy-MM-dd"));
                                    command.Parameters.AddWithValue("nrgender", rcvrgender);
                                    command.Parameters.AddWithValue("nrrelation", rcvrrelation);
                                    command.Parameters.AddWithValue("nrcontact", rcvrcontactno);
                                    command.Parameters.AddWithValue("rcvrcustid", rcvrcustid);
                                    command.Parameters.AddWithValue("nrstreet", rcvrstreet);
                                    command.Parameters.AddWithValue("nrcountry", rcvrcountry);
                                    int x = command.ExecuteNonQuery();

                                    String filepath = string.Empty;
                                    String uploadpath = string.Empty;
                                    if (!string.IsNullOrEmpty(model.strBase64Image))
                                    {
                                        String filename = getTimeStamp().ToString() + ".png";
                                        uploadpath = ftp + "/PayNearMe/Images/" + filename;
                                        uploadFileImage(model.strBase64Image, uploadpath);
                                        filepath = http + "/PayNearMe/Images/" + filename;


                                        command.Parameters.Clear();
                                        command.CommandText = "UPDATE kpcustomersglobal.BeneficiaryPayNearMe SET ImagePath = @image where ReceiverCustID=@rCustId";
                                        command.Parameters.AddWithValue("image", filepath);
                                        command.Parameters.AddWithValue("rCustId", rcvrcustid);
                                        command.ExecuteNonQuery();

                                        kplog.Info("Success Update kpcustomersglobal.BeneficiaryPayNearMe -- ImagePath ");
                                    }

                                    if (x > 0)
                                    {
                                        
                                        trans.Commit();
                                        con.Close();
                                        kplog.Info("Beneficiary Successfully Updated");
                                        return new CustomerResultResponse { respcode = 1, message = "Beneficiary's information is successfully updated!" };
                                        
                                    }
                                    else
                                    {
                                        trans.Rollback();
                                        con.Close();
                                        kplog.Info("Error in Updating Beneficiary");
                                        return new CustomerResultResponse { respcode = 0, message = "Error in Updating Beneficiary" };
                                    }

                                }

                                else
                                {
                                    reader.Close();
                                    con.Close();
                                    kplog.Info("Beneficiary Not Found");
                                    return new CustomerResultResponse { respcode = 0, message = "Beneficiary Not Found" };
                                }
                            }
                            catch (Exception ex)
                            {
                                kplog.Fatal(ex.ToString());
                                return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        kplog.Fatal(ex.ToString());
                        return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal(ex.ToString());
                return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
            }
        }


        //done loggings RR
       
        public CustomerResultResponse getbeneficiarylist(String sendercustid)
        {
            kplog.Info("START -->  PARAMS: sendercustid: " + sendercustid);
            try
            {

                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<BeneficiaryModel> list = new List<BeneficiaryModel>();
                            string query = "select a.firstname, a.lastname, a.middlename, a.fullname, a.street, a.citystate, a.country, if(a.zipcode is null,'', a.zipcode) as zipcode, date_format(a.birthdate,'%Y-%m-%d') as birthdate, a.gender, a.contactno, a.Relation,a.CustIDB,b.ImagePath from kpcustomersglobal.BeneficiaryHistory a inner join kpcustomersglobal.BeneficiaryPayNearMe b  ON a.CustIDB = b.ReceiverCustID where a.sendercustid=@sendercustid and b.isActivate = 1 order by LastTransDate DESC";
                            MySqlDataAdapter adapter = new MySqlDataAdapter();
                            cmd.CommandText = query;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("sendercustid", sendercustid);
                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                            {
                                while (rdr.Read())
                                {
                                    DateTime? dtcheck;
                                    try { dtcheck = Convert.ToDateTime(rdr["BirthDate"]); }
                                    catch (Exception) { dtcheck = null; }

                                    list.Add(new BeneficiaryModel
                                    {
                                
                                        address = rdr["street"].ToString() + " " + rdr["citystate"].ToString() + " " + rdr["zipcode"].ToString() + " " + rdr["country"].ToString(),
                                        receiverCustID = rdr["CustIDB"].ToString(),
                                        firstName = rdr["firstname"].ToString(),
                                        lastname = rdr["lastname"].ToString(),
                                        midlleName = rdr["middlename"].ToString(),
                                        city = rdr["CityState"].ToString(),
                                        country = rdr["Country"].ToString(),
                                        zipcode = rdr["ZipCode"].ToString(),
                                        dateOfBirth = dtcheck.HasValue ? Convert.ToDateTime(dtcheck.Value).ToString("MM/dd/yyyy") : "",
                                        gender = rdr["Gender"].ToString(),
                                        relation = rdr["Relation"].ToString(),
                                        phoneNo = rdr["ContactNo"].ToString(),
                                        street = rdr["street"].ToString(),
                                        SenderCustID = sendercustid,
                                        ImagePath = rdr["ImagePath"].ToString()
                                        
                                       
                                    });


                                 
                                }
                            }
                            rdr.Close();


                            if (list.Count > 0)
                            {
                                
                               
                                var response = new CustomerResultResponse { respcode = 1, message = "Found", benelist = list };
                                kplog.Info(JsonConvert.SerializeObject(response));
                                return response;
                            }
                            else
                            {
                                
                                kplog.Info("No Data Found");
                                return new CustomerResultResponse { respcode = 0, message = "No Beneficiary Found", benelist = null };
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        con.Close();
                        //custcon.CloseConnection();
                        kplog.Fatal(ex.ToString());
                        return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                //custcon.CloseConnection();
                kplog.Fatal(ex.ToString());
                return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
            }
        }


        //done loggings RR
      
        public BeneficiaryResponse getBeneficiaryInfo(String receiverCustID)
        {
            kplog.Info("PARAMS --- > receiverCustID: " + receiverCustID);

            try
            {
                DateTime? dtcheck;
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<BeneficiaryModel> list = new List<BeneficiaryModel>();
                            string query = "select SenderCustID,firstname, lastname, middlename, fullname, street, citystate, country, if(zipcode is null,'',zipcode) as zipcode, date_format(birthdate,'%Y-%m-%d') as birthdate, gender, contactno, Relation, CustIDB,b.ImagePath from kpcustomersglobal.BeneficiaryHistory a INNER JOIN kpcustomersglobal.BeneficiaryPayNearMe b ON a.CustIDB = b.ReceiverCustID where CustIDB=@rcvrCustID;";
                            MySqlDataAdapter adapter = new MySqlDataAdapter();
                            cmd.CommandText = query;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("rcvrCustID", receiverCustID);
                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                            {
                                rdr.Read();
                                try
                                {
                                    dtcheck = Convert.ToDateTime(rdr["BirthDate"]);
                                }
                                catch (Exception)
                                {

                                    dtcheck = null;
                                }


                                BeneficiaryModel model = new BeneficiaryModel()
                                {
                                    receiverCustID = rdr["CustIDB"].ToString(),
                                    firstName = rdr["firstname"].ToString(),
                                    lastname = rdr["lastname"].ToString(),
                                    midlleName = rdr["middlename"].ToString(),
                                    city = rdr["CityState"].ToString(),
                                    country = rdr["Country"].ToString(),
                                    zipcode = rdr["ZipCode"].ToString(),
                                    dateOfBirth = dtcheck.HasValue ? Convert.ToDateTime(dtcheck.Value).ToString("yyyy-MM-dd") : "",
                                    gender = rdr["Gender"].ToString(),
                                    relation = rdr["Relation"].ToString(),
                                    phoneNo = rdr["ContactNo"].ToString().Substring(1),
                                    street = rdr["street"].ToString(),
                                    SenderCustID = rdr["SenderCustID"].ToString(),
                                    ImagePath = rdr["ImagePath"].ToString()

                                };
                                
                                rdr.Close();
                                kplog.Info("FOUND: " + JsonConvert.SerializeObject(model));
                                kplog.Info("Success : Data Found");
                                return new BeneficiaryResponse { respcode = 1 , message = "Success", data = model};
                            }
                            else
                            {
                                rdr.Close();
                                kplog.Info("Success : No Data Found");
                                return new BeneficiaryResponse { respcode = 0, message = "No Data found", data = null };
                            }

                        }
                    }
                    catch (SqlException ex)
                    {
                        con.Close();
                        //custcon.CloseConnection();
                        kplog.Fatal(ex.ToString());
                        return new BeneficiaryResponse { respcode = 0, message = ex.ToString(), data = null };
                    }
                }
            }
            catch (Exception ex)
            {
                //custcon.CloseConnection();
                kplog.Fatal(ex.ToString());
                return new BeneficiaryResponse { respcode = 0, message = ex.ToString(), data = null };
            }




        }


        //done loggings RR
        [HttpPost]
        public CustomerResultResponse deActivateBeneficiary(String receiverCustID)
        {
            kplog.Info("PARAMS --- > receiverCustID: " + receiverCustID);
            try
            {

                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<BeneficiaryModel> list = new List<BeneficiaryModel>();
                            string query = "select * from kpcustomersglobal.BeneficiaryHistory a inner join kpcustomersglobal.BeneficiaryPayNearMe b ON a.CustIDB = b.ReceiverCustID where a.CustIDB=@rcvrCustID;";
                            MySqlDataAdapter adapter = new MySqlDataAdapter();
                            cmd.CommandText = query;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("rcvrCustID", receiverCustID);
                            MySqlDataReader rdr = cmd.ExecuteReader();
                            if (rdr.HasRows)
                            {
                                rdr.Close();
                                cmd.Parameters.Clear();
                                cmd.CommandText = "Update kpcustomersglobal.BeneficiaryPayNearMe SET isActivate = 0 where ReceiverCustID = @CustID";
                                cmd.Parameters.AddWithValue("CustID", receiverCustID);
                                int x = cmd.ExecuteNonQuery();

                                if (x > 0)
                                {
                                    kplog.Info("SUCCESS : Successfully deactivated Beneficiary!");
                                    return new CustomerResultResponse { respcode = 1, message = "Successfully deactivated Beneficiary!" };
                                }
                                else
                                {
                                    kplog.Error("Error : Mysql Error");
                                    return new CustomerResultResponse { respcode = 0, message = "Mysql Error" };
                                }

                            }
                            else
                            {
                                rdr.Close();
                                kplog.Info("SUCCESS : Beneficiary does not exist!");
                                return new CustomerResultResponse { respcode = 0, message = "Beneficiary does not exist!" };
                            }

                        }
                    }
                    catch (SqlException ex)
                    {
                        con.Close();
                        //custcon.CloseConnection();
                        kplog.Fatal(ex.ToString());
                        return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                //custcon.CloseConnection();
                kplog.Fatal(ex.ToString());
                return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
            }

        }

        //done loggings RR
       
        public AddKYCResponse addKYCGlobal(CustomerModel req)
        {

            kplog.Info("PARAMS --> "+ JsonConvert.SerializeObject(req));
            String SenderFName = cleanString(req.firstName);
            String SenderLName = cleanString(req.lastName);
            String SenderMName = cleanString(req.middleName);
            String SenderStreet = cleanString(req.Street);
            String SenderCity = req.City;
            String SenderCountry = req.Country;
            String SenderGender = req.Gender;
            String SenderBirthdate = ConvertDateTime(req.BirthDate);
            String SenderBranchID = req.BranchID;
            String MobileNo = req.PhoneNo.Trim();
            String ZipCode = req.ZipCode;
            String activationCode = generateActivationCode();
            String mobileToken = generateMobileToken();
            String Email = req.UserID.Trim();
            String Password = req.Password;
            String Name = string.Empty;
            String State = req.State;
            String Gender = req.Gender;
            String CreatedBy = req.CreatedBy;
            String PhoneNo = req.PhoneNo;
            String IDNo = req.IDNo.Trim();
            String IDType = req.IDType;
            String ExpiryDate = ConvertDateTime(req.ExpiryDate);
            string strBase64 = req.strBase64Image;
            string strBase641 = req.strBase64Image1F;
            string strBase642 = req.strBase64Image1B;
            string strBase643 = req.strBase64Image2F;
            string strBase644 = req.strBase64Image2B;

            String filePath = string.Empty;
            String filePath1= string.Empty;
            String filePath2= string.Empty;
            String filePath3= string.Empty;
            String filePath4 = string.Empty;
            String browsepath = string.Empty;
            String browsepath1 = string.Empty;
            String browsepath2 = string.Empty;
            String browsepath3 = string.Empty;
            String browsepath4 = string.Empty;


            if (string.IsNullOrEmpty(SenderFName) || string.IsNullOrEmpty(SenderLName) || string.IsNullOrEmpty(Email))
            {
                return new AddKYCResponse { respcode = 0, message = "Please input required Fields!" };
            }

            if (SenderMName == null)
            {
                SenderMName = "";
            }


            if (SenderMName == "" || SenderMName == String.Empty)
            {
                Name = SenderFName + " " + SenderLName;
            }
            else
            {
                Name = SenderFName + " " + SenderMName + " " + SenderLName;
            }


            kplog.Info("SenderFName: " + SenderFName + ", SenderLName: " + SenderLName + " SenderMName: " + SenderMName + ", SenderStreet: " + SenderStreet + ", SenderProvinceCity: " + State + "SenderCountry: " + SenderCountry + ", ZipCode: " + ZipCode + ", SenderGender: " + SenderGender + ", SenderBirthdate: " + SenderBirthdate + ", SenderBranchID: " + SenderBranchID);





            try
            {

                if(iDology == true)
                {
                    var apiIDologyResp = ExpectID_IQ_Check(req).Result;

                    if (apiIDologyResp == "FAIL")
                    {
                        kplog.Error("apiIDOLOGY FAIL: Name=" + Name);
                        return new AddKYCResponse { respcode = 0, message = "Please check and make sure you provided a valid information, if persist please contact support!" };
                    }
                    else if (apiIDologyResp == "ERROR")
                    {
                        kplog.Error("apiIDOLOGY ERROR: Name=" + Name);
                        return new AddKYCResponse { respcode = 0, message = "Something went wrong, Please try Again!" };
                    }
                }
                


                //if (OfacMatch(Name))
                //{
                //    kplog.Error("OFAC FAIL: Name= " + Name);
                //    return new AddKYCResponse { respcode = 0, message = "Unable to register. Please contact Support!" };

                //}


                dt = getServerDateGlobal();
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                return new AddKYCResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString(), MLCardNo = null };
            }


            using (MySqlConnection custconn = new MySqlConnection(connection))
            {
                try
                {
                    custconn.Open();
                    Int32 sr = 0;
                    String custid = string.Empty;
                    string updatecustomerseries = String.Empty;
                    string senderid = String.Empty;
                    custtrans = custconn.BeginTransaction(IsolationLevel.ReadCommitted);

                    using (custcommand = custconn.CreateCommand())
                    {

                        custcommand.CommandText = "Select CustID FROM kpcustomersglobal.customers where FirstName = @fname and LastName = @lname and MiddleName = @mname and BirthDate = @bdate LIMIT 1";
                        custcommand.Parameters.AddWithValue("fname", SenderFName);
                        custcommand.Parameters.AddWithValue("lname", SenderLName);
                        custcommand.Parameters.AddWithValue("mname", SenderMName);
                        custcommand.Parameters.AddWithValue("bdate", SenderBirthdate);


                        using (MySqlDataReader Reader1 = custcommand.ExecuteReader())
                        {

                            if (Reader1.HasRows)
                            {
                                Reader1.Read();
                                custid = Reader1["CustID"].ToString();
                                Reader1.Close();
                                custcommand.Parameters.Clear();
                                custcommand.CommandText = "Select * from kpcustomersglobal.PayNearMe where CustomerID = @custID OR UserID=@userID";
                                custcommand.Parameters.AddWithValue("custID", custid);
                                custcommand.Parameters.AddWithValue("userID", Email);
                                MySqlDataReader rdrUni = custcommand.ExecuteReader();
                                if (rdrUni.HasRows)
                                {
                                    rdrUni.Close();
                                    kplog.Info("Customer Already Registered");
                                    return new AddKYCResponse { respcode = 0, message = "Customer already registered." };

                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(strBase64)) 
                                    { 

                                        String filename = getTimeStamp().ToString() + ".png"; 
                                        filePath = ftp + "/PayNearMe/Images/" + filename; 
                                        browsepath = http + "/PayNearMe/Images/" + filename; 
                                        uploadFileImage(strBase64, filePath);

                                        kplog.Info("UPLOAD: " + filePath);
                                    }
                                    if (!string.IsNullOrEmpty(strBase641))
                                    {
                                        String filename = getTimeStamp().ToString() +"1F"+ ".png";
                                        filePath1 = ftp + "/PayNearMe/Images/" + filename;
                                        browsepath1 = http + "/PayNearMe/Images/" + filename;
                                        uploadFileImage(strBase641, filePath1);
                                        kplog.Info("UPLOAD: " + filePath1);
                                    }
                                    if (!string.IsNullOrEmpty(strBase642))
                                    {
                                        String filename = getTimeStamp().ToString() + "1B" + ".png";
                                        filePath2 = ftp + "/PayNearMe/Images/" + filename;
                                        browsepath2 = http + "/PayNearMe/Images/" + filename;
                                        uploadFileImage(strBase642, filePath2);
                                        kplog.Info("UPLOAD: " + filePath2);
                                    }
                                    if (!string.IsNullOrEmpty(strBase643))
                                    {
                                        String filename = getTimeStamp().ToString() + "2F" + ".png";
                                        filePath3 = ftp + "/PayNearMe/Images/" + filename;
                                        browsepath3 = http + "/PayNearMe/Images/" + filename;
                                        uploadFileImage(strBase643, filePath3);
                                        kplog.Info("UPLOAD: " + filePath3);
                                    }
                                    if (!string.IsNullOrEmpty(strBase644))
                                    {
                                        String filename = getTimeStamp().ToString() + "2B" + ".png";
                                        filePath4 = ftp + "/PayNearMe/Images/" + filename;
                                        browsepath4 = http + "/PayNearMe/Images/" + filename;
                                        uploadFileImage(strBase644, filePath4);
                                        kplog.Info("UPLOAD: " + filePath4);
                                    }

                                    rdrUni.Close();

                                    custcommand.Parameters.Clear();
                                    custcommand.CommandText = "INSERT INTO kpcustomersglobal.PayNearMe"
                                                            + "(CustomerID, SignupDate, Password, UserID, FullName, PrivacyPolicyAgreement, ActivationCode,ImagePath,validID1Front,validID1Back,validID2Front,validID2Back,mobileToken) "
                                                            + "VALUES "
                                                            + "(@custID, NOW(), @Password, @UserID, @FullName, "
                                                            + "@PrivacyPolicyAgreement, @activationCode,@ImagePath,@ImagePath1,@ImagePath2,@ImagePath3,@ImagePath4,@mobileToken)";
                                    custcommand.Parameters.AddWithValue("custID", custid);
                                    custcommand.Parameters.AddWithValue("Password", Password);
                                    custcommand.Parameters.AddWithValue("UserID", Email);
                                    custcommand.Parameters.AddWithValue("FullName", Name);
                                    custcommand.Parameters.AddWithValue("PrivacyPolicyAgreement", true);
                                    custcommand.Parameters.AddWithValue("activationCode", activationCode);
                                    custcommand.Parameters.AddWithValue("mobileToken", mobileToken);
                                    custcommand.Parameters.AddWithValue("ImagePath", browsepath);
                                    custcommand.Parameters.AddWithValue("ImagePath1", browsepath1);
                                    custcommand.Parameters.AddWithValue("ImagePath2", browsepath2);
                                    custcommand.Parameters.AddWithValue("ImagePath3", browsepath3);
                                    custcommand.Parameters.AddWithValue("ImagePath4", browsepath4);
                                    custcommand.ExecuteNonQuery();

                                    kplog.Info("INSERT kpcustomersglobal.PayNearMe");
                                }

                                Int32 timestamp = getTimeStamp();
                                string yearofbirth = Convert.ToDateTime(SenderBirthdate).ToString("yyyy");

                                string queryAPI = "city=" + SenderCity + "&country=" + SenderCountry + "&first_name=" + SenderFName + "&last_name=" + SenderLName + "&middle_name=" + SenderMName + "&postal_code=" + ZipCode + "&site_identifier=" + siteIdentifier + "&site_user_identifier=" + custid + "&street=" + SenderStreet + "&timestamp=" + timestamp.ToString() +
                                                "&user_type=sender&version=2.0&year_of_birth=" + yearofbirth;



                                string signature = generateSignature(queryAPI);

                                queryAPI = queryAPI + "&signature=" + signature;

                                Uri uri = new Uri(server + "/json-api/create_user?" + queryAPI);

                                string res = SendRequest(uri);
                                kplog.Info("Response: PayNearMe API create_user: "+res);
                                dynamic data = JObject.Parse(res);

                                if (data.status == "ok")
                                {
                                    custtrans.Commit();
                                    custconn.Close();

                                    sendEmailActivation(Email, SenderFName, activationCode,mobileToken);

                                    kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " MLCardNo " + senderid);
                                    return new AddKYCResponse { respcode = 1, message = getRespMessage(1), MLCardNo = senderid };
                                }
                                else
                                {
                                    custtrans.Rollback();
                                    custconn.Close();

                                    string error = "";
                                    for (int xx = 0; xx < data.errors.Count; xx++)
                                    {
                                        error = error + " " + data.errors[xx].description;
                                    }
                                    kplog.Info(error);
                                    return new AddKYCResponse { respcode = 0, message = error };
                                }


                            }
                        }
                    }

                    using (custcommand = custconn.CreateCommand())
                    {

                        custcommand.Parameters.Clear();
                        custcommand.CommandText = "Select * from kpcustomersglobal.PayNearMe where  UserID=@userID";
                        custcommand.Parameters.AddWithValue("userID", Email);
                        MySqlDataReader rdrUni = custcommand.ExecuteReader();
                        if (rdrUni.HasRows)
                        {
                            rdrUni.Close();
                            kplog.Info("Customer Already Registered");
                            return new AddKYCResponse { respcode = 0, message = "Customer already registered." };

                        }

                        rdrUni.Close();
                        //string senderid = generateCustIDGlobal(custcommand);
                        String query = "select series from kpformsglobal.customerseries";
                        custcommand.Parameters.Clear();
                        custcommand.CommandText = query;
                        MySqlDataReader Reader = custcommand.ExecuteReader();
                        if (Reader.HasRows)
                        {
                            Reader.Read();
                            if (!(Reader["series"] == DBNull.Value))
                            {
                                sr = Convert.ToInt32(Reader["series"].ToString());
                            }
                        }
                        Reader.Close();

                        Int32 sr1 = sr + 1;
                        custcommand.Transaction = custtrans;

                        if (sr == 0)
                        {
                            updatecustomerseries = "INSERT INTO kpformsglobal.customerseries(series,year) values('" + sr1 + "','" + dt.ToString("yyyy") + "')";
                            kplog.Info("SUCCESS:: INSERT INTO kpformsglobal.customerseries: series: " + sr1 + " year: " + dt.ToString("yyyy"));
                        }
                        else
                        {
                            updatecustomerseries = "update kpformsglobal.customerseries set series = '" + sr1 + "', year = '" + dt.ToString("yyyy") + "'";
                            kplog.Info("SUCCESS:: UPDATE kpformsglobal.customerseries: SET series: " + sr1 + " year: " + dt.ToString("yyyy"));
                        }


                        custcommand.CommandText = updatecustomerseries;
                        custcommand.ExecuteNonQuery();

                        senderid = generateCustIDGlobal(custcommand);

                        String insertCustomer = "INSERT INTO kpcustomersglobal.customers (CustID, FirstName, LastName, MiddleName, Street, ProvinceCity, Country, ZipCode, Gender, Birthdate, DTCreated, CreatedBy, PhoneNo, Mobile, Email, CreatedByBranch,IDNo,IDType,ExpiryDate) VALUES (@SCustID, @SFirstName, @SLastName, @SMiddleName, @SStreet, @SProvinceCity, @SCountry, @SZipcode, @SGender, @SBirthdate, @DTCreated,@CreatedBy, @PhoneNo, @MobileNo, @Email,@CreatedByBranch,@IDNo,@IDType,@ExpiryDate);";
                        custcommand.CommandText = insertCustomer;

                        custcommand.Parameters.Clear();
                        custcommand.Parameters.AddWithValue("SCustID", senderid);
                        custcommand.Parameters.AddWithValue("SFirstName", SenderFName);
                        custcommand.Parameters.AddWithValue("SLastName", SenderLName);
                        custcommand.Parameters.AddWithValue("SMiddleName", SenderMName);
                        custcommand.Parameters.AddWithValue("SStreet", SenderStreet);
                        custcommand.Parameters.AddWithValue("SProvinceCity", State);
                        custcommand.Parameters.AddWithValue("SZipcode", ZipCode);
                        custcommand.Parameters.AddWithValue("SCountry", SenderCountry);
                        custcommand.Parameters.AddWithValue("SGender", SenderGender);
                        custcommand.Parameters.AddWithValue("SBirthdate", SenderBirthdate);
                        custcommand.Parameters.AddWithValue("DTCreated", dt.ToString("yyyy-MM-dd HH:mm:ss"));
                        custcommand.Parameters.AddWithValue("PhoneNo", PhoneNo);
                        custcommand.Parameters.AddWithValue("MobileNo", MobileNo);
                        custcommand.Parameters.AddWithValue("Email", Email);
                        custcommand.Parameters.AddWithValue("CreatedBy", "PayNearMe");
                        custcommand.Parameters.AddWithValue("CreatedByBranch", "PayNearMe");

                        custcommand.Parameters.AddWithValue("IDType", IDType);
                        custcommand.Parameters.AddWithValue("IDNo", IDNo);
                        custcommand.Parameters.AddWithValue("ExpiryDate", ExpiryDate);

                        custcommand.ExecuteNonQuery();

                        kplog.Info("INSERT kpcustomersglobal.customers");

                        String insertCustomerDetails = "INSERT INTO kpcustomersglobal.customersdetails(CustID,HomeCity) values(@dcustid,@dhomecity)";
                        custcommand.CommandText = insertCustomerDetails;
                        custcommand.Parameters.Clear();
                        custcommand.Parameters.AddWithValue("dcustid", senderid);
                        custcommand.Parameters.AddWithValue("dhomecity", SenderCity);
                        custcommand.ExecuteNonQuery();

                        kplog.Info("INSERT kpcustomersglobal.customersdetails");
                        //PAYNEARME
                        if (!string.IsNullOrEmpty(strBase64))
                        {

                            String filename = getTimeStamp().ToString() + ".png";
                            filePath = ftp + "/PayNearMe/Images/" + filename;
                            browsepath = http + "/PayNearMe/Images/" + filename;
                            uploadFileImage(strBase64, filePath);

                            kplog.Info("UPLOAD: filepath : " + browsepath);
                        }
                        if (!string.IsNullOrEmpty(strBase641))
                        {
                            String filename = getTimeStamp().ToString() + "1F" + ".png";
                            filePath1 = ftp + "/PayNearMe/Images/" + filename;
                            browsepath1 = http + "/PayNearMe/Images/" + filename;
                            uploadFileImage(strBase641, filePath1);
                            kplog.Info("UPLOAD: filepath : " + browsepath1);
                        }
                        if (!string.IsNullOrEmpty(strBase642))
                        {
                            String filename = getTimeStamp().ToString() + "1B" + ".png";
                            filePath2 = ftp + "/PayNearMe/Images/" + filename;
                            browsepath2 = http + "/PayNearMe/Images/" + filename;
                            uploadFileImage(strBase642, filePath2);
                            kplog.Info("UPLOAD: filepath : " + browsepath2);
                        }
                        if (!string.IsNullOrEmpty(strBase643))
                        {
                            String filename = getTimeStamp().ToString() + "2F" + ".png";
                            filePath3 = ftp + "/PayNearMe/Images/" + filename;
                            browsepath3 = http + "/PayNearMe/Images/" + filename;
                            uploadFileImage(strBase643, filePath3);
                            kplog.Info("UPLOAD: filepath : " + browsepath3);
                        }
                        if (!string.IsNullOrEmpty(strBase644))
                        {
                            String filename = getTimeStamp().ToString() + "2B" + ".png";
                            filePath4 = ftp + "/PayNearMe/Images/" + filename;
                            browsepath4 = http + "/PayNearMe/Images/" + filename;
                            uploadFileImage(strBase644, filePath4);
                            kplog.Info("UPLOAD: filepath : " + browsepath4);
                        }

                        custcommand.Parameters.Clear();
                        custcommand.CommandText = "INSERT INTO kpcustomersglobal.PayNearMe"
                                                + "(CustomerID, SignupDate, Password, UserID, FullName, PrivacyPolicyAgreement, ActivationCode,ImagePath,validID1Front,validID1Back,validID2Front,validID2Back,mobileToken) "
                                                + "VALUES "
                                                + "(@custID, NOW(), @Password, @UserID, @FullName, "
                                                + "@PrivacyPolicyAgreement, @activationCode,@ImagePath,@ImagePath1,@ImagePath2,@ImagePath3,@ImagePath4,@mobileToken)";
                        custcommand.Parameters.AddWithValue("custID", senderid);
                        custcommand.Parameters.AddWithValue("Password", Password);
                        custcommand.Parameters.AddWithValue("UserID", Email);
                        custcommand.Parameters.AddWithValue("FullName", Name);
                        custcommand.Parameters.AddWithValue("PrivacyPolicyAgreement", true);
                        custcommand.Parameters.AddWithValue("activationCode", activationCode);
                        custcommand.Parameters.AddWithValue("ImagePath", browsepath);
                        custcommand.Parameters.AddWithValue("ImagePath1", browsepath1);
                        custcommand.Parameters.AddWithValue("ImagePath2", browsepath2);
                        custcommand.Parameters.AddWithValue("ImagePath3", browsepath3);
                        custcommand.Parameters.AddWithValue("ImagePath4", browsepath4);
                        custcommand.Parameters.AddWithValue("mobileToken", mobileToken);
                        custcommand.ExecuteNonQuery();
                        //

                        kplog.Info("INSERT kpcustomersglobal.PayNearMe");

                        String insertCustomerLogs = "INSERT INTO kpadminlogsglobal.customerlogs(ScustID,Details,Syscreated,Syscreator,Type) values(@scustid,@details,now(),@creator,@type)";
                        custcommand.CommandText = insertCustomerLogs;
                        custcommand.Parameters.Clear();
                        custcommand.Parameters.AddWithValue("scustid", senderid);
                        custcommand.Parameters.AddWithValue("details", "{Name:" + " " + SenderFName + " " + SenderMName + " " + SenderLName + ", " + "Street:" + " " + SenderStreet + ", " + "ProvinceCity:" + " " + State + ", " + "ZipCode:" + " " + ZipCode + ", " + "Country:" + " " + SenderCountry + ", " + "Gender:" + " " + SenderGender + ", " + "BirthDate:" + " " + SenderBirthdate + ", " + "PhoneNo:" + " " + PhoneNo + ", " + "MobileNo:" + " " + MobileNo + ", " + "Email:" + " " + Email + ", " + "CreatedByBranch" + " PayNearMe}");
                        custcommand.Parameters.AddWithValue("creator", CreatedBy);
                        custcommand.Parameters.AddWithValue("type", "N");
                        custcommand.ExecuteNonQuery();



                        kplog.Info("SUCCESS:: INSERT INTO kpcustomersglobal.customers: SCustID: " + senderid + " " +
                        "SFirstName: " + SenderFName + " " +
                        "SLastName: " + SenderLName + " " +
                        "SMiddleName: " + SenderMName + " " +
                        "SStreet: " + SenderStreet + " " +
                        "SProvinceCity: " + State + " " +
                        "SZipcode: " + ZipCode + " " +
                        "SCountry: " + SenderCountry + " " +
                        "SGender: " + SenderGender + " " +
                        "SBirthdate: " + SenderBirthdate + " " +
                        "DTCreated: " + dt.ToString("yyyy-MM-dd HH:mm:ss") + " " +
                        "PhoneNo: " + PhoneNo + " " +
                        "MobileNo: " + MobileNo + " " +
                        "Email: " + Email + " " +
                        "CreatedBy: " + CreatedBy);


                        kplog.Info("SUCCESS:: INSERT INTO kpadminlogsglobal.customerlogs: scustid: " + senderid + " " +
                        "details: " + "{Name:" + " " + SenderFName + " " + SenderMName + " " + SenderLName + " " + "Street:" + " " + SenderStreet + " " + "ProvinceCity:" + " " + State + " " + "ZipCode:" + " " + ZipCode + " " + "Country:" + " " + SenderCountry + " " + "Gender:" + " " + SenderGender + "  + " + "BirthDate:" + " " + SenderBirthdate + " " + "PhoneNo:" + " " + PhoneNo + " " + "MobileNo:" + " " + MobileNo + " " + "Email:" + " " + Email + " " + "CreatedByBranch" + " PayNearMe}" + " " +
                        "creator: " + CreatedBy + " " +
                        "type: N");

                        Int32 timestamp = getTimeStamp();
                        string yearofbirth = Convert.ToDateTime(SenderBirthdate).ToString("yyyy");

                        string queryAPI = "city=" + SenderCity + "&country=" + SenderCountry + "&first_name=" + SenderFName + "&last_name=" + SenderLName + "&middle_name=" + SenderMName + "&postal_code=" + ZipCode + "&site_identifier=" + siteIdentifier + "&site_user_identifier=" + senderid + "&street=" + SenderStreet + "&timestamp=" + timestamp.ToString() +
                                        "&user_type=sender&version=2.0&year_of_birth=" + yearofbirth;



                        string signature = generateSignature(queryAPI);

                        queryAPI = queryAPI + "&signature=" + signature;

                        Uri uri = new Uri(server + "/json-api/create_user?" + queryAPI);

                        string res = SendRequest(uri);
                        kplog.Info("RESPONSE: PayNearMe API- create_user: "+res);
                        dynamic data = JObject.Parse(res);

                        if (data.status == "ok")
                        {
                            custtrans.Commit();
                            custconn.Close();
                            sendEmailActivation(Email, SenderFName, activationCode,mobileToken);
                            kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " MLCardNo " + senderid);
                            return new AddKYCResponse { respcode = 1, message = getRespMessage(1), MLCardNo = senderid };
                        }
                        else
                        {
                            custtrans.Rollback();
                            custconn.Close();

                            string error = "";
                            for (int xx = 0; xx < data.errors.Count; xx++)
                            {
                                error = error + " " + data.errors[xx].description;
                            }
                            kplog.Info(error);
                            return new AddKYCResponse { respcode = 0, message = error };
                        }




                    }
                }
                catch (Exception mex)
                {
                    custtrans.Rollback();
                    custconn.Close();
                    int respcode = 0;
                    if (mex.Message.StartsWith("Duplicate"))
                    {
                        Int32 sr = 0;
                        String updatecustomerseries = string.Empty;
                        respcode = 6;
                        kplog.Fatal("FAILED:: message: " + getRespMessage(respcode) + " ErrorDetail: " + mex.ToString());
                        using (MySqlConnection conTrap = new MySqlConnection(connection))
                        {
                            conTrap.Open();
                            using (MySqlCommand custcommand2 = conTrap.CreateCommand())
                            {
                                //string senderid = generateCustIDGlobal(custcommand);
                                String query = "select series from kpformsglobal.customerseries";
                                custcommand2.CommandText = query;
                                MySqlDataReader Reader = custcommand2.ExecuteReader();
                                if (Reader.HasRows)
                                {
                                    Reader.Read();
                                    if (!(Reader["series"] == DBNull.Value))
                                    {
                                        sr = Convert.ToInt32(Reader["series"]);//.ToString());
                                    }
                                }
                                Reader.Close();

                                Int32 sr1 = sr + 1;


                                if (sr == 0)
                                {
                                    updatecustomerseries = "INSERT INTO kpformsglobal.customerseries(series,year) values('" + sr1 + "','" + dt.ToString("yyyy") + "')";
                                    kplog.Info("SUCCESS:: INSERT INTO kpformsglobal.customerseries: series: " + sr1 + " year: " + dt.ToString("yyyy"));
                                }
                                else
                                {
                                    updatecustomerseries = "update kpformsglobal.customerseries set series = '" + sr1 + "', year = '" + dt.ToString("yyyy") + "'";
                                    kplog.Info("SUCCESS:: UPDATE kpformsglobal.customerseries: SET series: " + sr1 + " year: " + dt.ToString("yyyy"));
                                }
                                custcommand2.CommandText = updatecustomerseries;
                                custcommand2.ExecuteNonQuery();
                            }
                        }

                    }
                    custconn.Close();
                    kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(respcode) + " ErrorDetail: " + mex.ToString());
                    return new AddKYCResponse { respcode = respcode, message = getRespMessage(respcode), ErrorDetail = mex.ToString() };
                }
            }
        }

        [HttpGet]
        public AddKYCResponse getState(String zipCode)
        {
            try
            {

                zipCodeResp resp = new zipCodeResp();

                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select State , City from kpformsglobal.zipcodesG where ZipCode1 = @zipcode and State = 'California'";
                        cmd.Parameters.AddWithValue("zipcode", zipCode);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                        {

                            rdr.Read();
                            resp.State = rdr["State"].ToString();
                            resp.City = rdr["City"].ToString();
                            kplog.Info("SUCCESS : '" + resp + "'");
                            return new AddKYCResponse { respcode = 1, message = "Success", zCodeResp = resp };
                        }
                        else
                        {

                            rdr.Close();
                            kplog.Info("SUCCESS : INVALID ZIPCODE");
                            return new AddKYCResponse { respcode = 0, message = "Invalid Zip Code" };
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                kplog.Error("ERROR : '" + ex.ToString() + "'");
                return new AddKYCResponse { respcode = 0, message = "Error occured", ErrorDetail = ex.ToString() };
            }
        }


       
        public LoginResponse LoginPayNearMe(LoginViewModel model)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select * from kpcustomersglobal.PayNearMe where UserID = @email";
                        cmd.Parameters.AddWithValue("email", model.EmailAddress);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                        {


                            rdr.Close();
                            cmd.Parameters.Clear();
                            cmd.CommandText = "Select * from kpcustomersglobal.PayNearMe where UserID = @email and Password = @password";
                            cmd.Parameters.AddWithValue("email", model.EmailAddress);
                            cmd.Parameters.AddWithValue("password", model.Password);
                            MySqlDataReader rdrPass = cmd.ExecuteReader();
                            if (rdrPass.HasRows)
                            {
                                rdrPass.Read();

                                String filepath = rdrPass["ImagePath"].ToString();
                                Int32 isActive = Convert.ToInt32(rdrPass["isEmailActivated"]);

                                if (isActive == 0)
                                {
                                    rdrPass.Close();

                                    con.Close();
                                    kplog.Info("Account not yet activated");
                                    return new LoginResponse { respcode = 2, message = "Account not yet activated" };


                                }

                                String fullname = rdrPass["FullName"].ToString();
                                String signupDate = rdrPass["SignupDate"].ToString();
                                String lastLogin = rdrPass["LastLogin"].ToString();
                                String customerID = rdrPass["CustomerID"].ToString();
                                rdrPass.Close();

                                cmd.Parameters.Clear();
                                cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe Set LastLogin = NOW() where UserID = @email and Password = @password;";
                                cmd.Parameters.AddWithValue("email", model.EmailAddress);
                                cmd.Parameters.AddWithValue("password", model.Password);
                                cmd.ExecuteNonQuery();

                                if (string.IsNullOrEmpty(signupDate))
                                {
                                    signupDate = getServerDateGlobal().ToString();
                                }
                                if (string.IsNullOrEmpty(lastLogin))
                                {
                                    lastLogin = getServerDateGlobal().ToString();
                                }

                                cmd.Parameters.Clear();
                                cmd.CommandText = "Select a.FirstName,a.LastName,a.MiddleName,a.Street,a.ProvinceCity,a.Country,a.ZipCode,a.Gender,a.Birthdate,a.PhoneNo,a.Email,b.HomeCity from kpcustomersglobal.customers a INNER JOIN kpcustomersglobal.customersdetails b ON a.CustID=b.CustID  where a.custID = @CustID";
                                cmd.Parameters.AddWithValue("CustID", customerID);
                                MySqlDataReader rdrModel = cmd.ExecuteReader();

                                rdrModel.Read();



                                CustomerModel customer = new CustomerModel
                                {
                                    CustomerID = customerID,
                                    ImagePath = filepath,
                                    firstName = rdrModel["FirstName"].ToString(),
                                    lastName = rdrModel["LastName"].ToString(),
                                    middleName = rdrModel["MiddleName"].ToString(),
                                    Street = rdrModel["Street"].ToString(),
                                    State = rdrModel["ProvinceCity"].ToString(),
                                    Country = rdrModel["Country"].ToString(),
                                    ZipCode = rdrModel["ZipCode"].ToString(),
                                    Gender = rdrModel["Gender"].ToString(),
                                    BirthDate = rdrModel["Birthdate"].ToString(),
                                    PhoneNo = rdrModel["PhoneNo"].ToString(),
                                    UserID = rdrModel["Email"].ToString(),
                                    City = rdrModel["HomeCity"].ToString()
                                    
                                };


                                con.Close();
                                kplog.Info("Success : fullName = '" + fullname + "', signupDate = '" + signupDate + "', lastLogin = '" + lastLogin + "', customer = '" + customer + "'");
                                return new LoginResponse { respcode = 1, message = "Success", fullName = fullname, signupDate = signupDate, lastLogin = lastLogin, customer = customer };

                            }
                            else
                            {
                                rdrPass.Close();
                                con.Close();
                                kplog.Info("Invalid Password");
                                return new LoginResponse { respcode = 0, message = "Invalid Password" };
                            }

                        }
                        else
                        {
                            rdr.Close();
                            con.Close();
                            kplog.Info("Email not registed");
                            return new LoginResponse { respcode = 0, message = "Email not yet registered" };
                        }

                    }
                }
            }
            catch (Exception ex)
            {

                kplog.Error(ex.ToString());
                return new LoginResponse { respcode = 0, message = ex.ToString() };
            }
        }


        [HttpGet]
        public TransactionResponseMobile getKPTNbyAccount(string kptn, string CustomerID)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(connection))
                {

                    con.Open();
                    MySqlCommand cmd = con.CreateCommand();


                    List<TransactionDetailsM> listData = new List<TransactionDetailsM>();
                    int Count = 0;

                    if (kptn.Length != 21)
                    {
                        return new TransactionResponseMobile { respcode = 0, message = "Invalid Reference Number." };

                    }
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML, orderAmountTotal,orderChargePNM, orderTrackingUrl,status FROM paynearme.order" + kptn.Substring(19, 2) + " where siteOrderIdentifier = @kptn and senderIdentifier = '" + CustomerID + "' ";
                    cmd.Parameters.AddWithValue("@kptn", kptn);
                    MySqlDataReader rdr2 = cmd.ExecuteReader();
                    while (rdr2.Read())
                    {
                        listData.Add(new TransactionDetailsM
                        {
                            kptn = rdr2["siteOrderIdentifier"].ToString(),
                            TransDate = rdr2["orderCreated"].ToString(),
                            principal = rdr2["orderPrincipal"].ToString(),
                            charge = (Convert.ToDouble(rdr2["orderChargeML"]) + Convert.ToDouble(rdr2["orderChargePNM"])).ToString(),
                            totalamount = rdr2["orderAmountTotal"].ToString(),
                            trackingURL = rdr2["orderTrackingUrl"].ToString(),
                            Status = rdr2["status"].ToString() == "confirm" ? "Paid" : rdr2["status"].ToString(),
                            poamount = rdr2["orderPOAmountPHP"].ToString(),
                            RFullName = rdr2["beneficiaryname"].ToString()

                        });
                        Count = listData.Count;
                    }
                    rdr2.Close();
                    kplog.Info("Succes : Data Found, respcode = 1, listdata = '" + listData + "'");
                    return new TransactionResponseMobile { respcode = 1, tl = listData, count = Count };

                }
            }
            catch (Exception ex)
            {
                kplog.Error("Error : '" + ex.ToString() + "'");
                return new TransactionResponseMobile { respcode = 0, message = ex.ToString() };
            }
            
        }

        [HttpGet]
        public TransactionResponse getKPTNbyDetails(string CustomerID, string kptn)
        {
            //kptn = model.kptn;

            String fname = string.Empty;
            String lname = string.Empty;
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                //PendingTransaction model = new PendingTransaction();
          
                con.Open();
                MySqlCommand cmd = con.CreateCommand();
               
                String receiverID = string.Empty;
                string table = kptn.Substring(19, 2);
                TransactionDetailsModel model = new TransactionDetailsModel();
                   // string cstArr = ((i.ToString().Length == 1) ? "0" + i.ToString() : i.ToString());
                    cmd.Connection = con;
                    cmd.CommandText = "SELECT * FROM paynearme.order" + table + " where siteOrderIdentifier = '" + kptn + "' and senderIdentifier= '" + CustomerID + "'";
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        //  String kptn = reader["siteOrderIdentifier"].ToString();
                        receiverID = reader["receiverIdentifier"].ToString();
                        model.principal = reader["orderPrincipal"].ToString();
                        model.charge = (Convert.ToDouble(reader["orderChargeML"]) + Convert.ToDouble(reader["orderChargePNM"])).ToString();
                        model.poamount = reader["orderPOAmountPHP"].ToString();
                        model.totalamount = reader["orderAmountTotal"].ToString();
                        model.Status = reader["status"].ToString() == "confirm" ? "Paid" : reader["status"].ToString();
                        model.TransDate = reader["orderCreated"].ToString();
                        model.trackingURL = reader["orderTrackingUrl"].ToString();
                    }
                    else 
                    {
                        kplog.Info("Success : not  Found, respcode = 1");
                        return new TransactionResponse { respcode = 0,message ="not found", detail = null };
                    }

                    reader.Close();
                

                cmd.Connection = con;
                cmd.CommandText = "select * from kpcustomersglobal.customers where custID = '" + CustomerID + "'";
                MySqlDataReader rd1 = cmd.ExecuteReader();

                rd1.Read();

                model.SFirstname = rd1["FirstName"].ToString();
                model.SLastname = rd1["LastName"].ToString();
                model.SMiddlename = rd1["MiddleName"].ToString();

                rd1.Close();

                cmd.CommandText = "select FirstName, LastName, MiddleName from kpcustomersglobal.BeneficiaryHistory where CustIDB = '" + receiverID + "'";

                MySqlDataReader rd3 = cmd.ExecuteReader();

                rd3.Read();

                model.RFirstname = rd3["FirstName"].ToString();
                model.RLastname = rd3["LastName"].ToString();
                model.RMiddlename = rd3["MiddleName"].ToString();
                model.kptn = kptn;
                rd3.Close();
                kplog.Info("SUCCESS : Data Found , Respcode = 1, detail = '" + model + "'");
                return new TransactionResponse { respcode = 1,message="found", detail = model};
            }

        }

        [HttpGet]
        public TransactionResponseMobile getAllTransactionByCategoryM(string CustomerID, String month, String year, String status)
        {
            try
            {

                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    string customerid = string.Empty;
                    con.Open();
                    MySqlCommand cmd = con.CreateCommand();
                    List<TransactionDetailsM> listData = new List<TransactionDetailsM>();
                    int Count = 0;

                    
                    DateTime dt = getServerDateGlobal();
                    
                    String thisMonth = dt.ToString("MM");
                    String thisYear = dt.ToString("yyyy");
                    String lastMonth = dt.AddMonths(-1).ToString("MM");
                    String table = string.Empty;

                    cmd.Connection = con;
                    String sql = string.Empty;

                    if (string.IsNullOrEmpty(month))
                    {
                        table = thisMonth;
                    }
                    else 
                    {
                        table = month;
                    }
                    if (string.IsNullOrEmpty(year)) 
                    {
                        year = thisYear;
                    }

                    if (year == "now")
                    {

                        if (status == "Void")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal,orderTrackingUrl,'Void' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and NOW() > orderDuration and status != 'confirm' AND date(orderCreated) = curdate() ORDER BY orderCreated desc;";
                        }
                        else if (status == "Confirm")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Paid' as status FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` = 'confirm' AND date(orderCreated) = curdate() ORDER BY orderCreated desc";
                        }
                        else if (status == "Open")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Open' as status FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` != 'confirm' AND orderDuration > now()  AND date(orderCreated) = curdate()  ORDER BY orderCreated desc;";
                        }
                        else
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,IF(orderDuration > Now() and `status`!='confirm','Open',IF(orderDuration < NOW() AND `status` != 'confirm','Void','Paid')) as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' AND date(orderCreated) = curdate() ORDER BY orderCreated desc;";
                        }
                    }
                    else 
                    {
                        if (status == "Void")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal,orderTrackingUrl,'Void' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and NOW() > orderDuration and status != 'confirm' AND YEAR(orderCreated) = '" + year + "' ORDER BY orderCreated desc;";
                        }
                        else if (status == "Confirm")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Paid' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` = 'confirm' AND YEAR(orderCreated) = '" + year + "' ORDER BY orderCreated desc";
                        }
                        else if (status == "Open")
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,'Open' as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' and `status` != 'confirm' AND orderDuration > now()  AND YEAR(orderCreated) = '" + year + "'  ORDER BY orderCreated desc;";
                        }
                        else
                        {
                            sql = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal, orderTrackingUrl,IF(orderDuration > Now() and `status`!='confirm','Open',IF(orderDuration < NOW() AND `status` != 'confirm','Void','Paid')) as `status` FROM paynearme.order" + table + " WHERE senderIdentifier = '" + CustomerID + "' AND YEAR(orderCreated) = '" + year + "' ORDER BY orderCreated desc;";
                        }
                    
                    }
                
                       
                      

                    

                 
                    cmd.CommandText = sql;
                    MySqlDataReader rdr1 = cmd.ExecuteReader();

                    while (rdr1.Read())
                    {
                        listData.Add(new TransactionDetailsM
                        {
                            kptn = rdr1["siteOrderIdentifier"].ToString(),
                            TransDate = rdr1["orderCreated"].ToString(),
                            principal = rdr1["orderPrincipal"].ToString(), 
                            charge = (Convert.ToDouble(rdr1["orderChargeML"]) + Convert.ToDouble(rdr1["orderChargePNM"])).ToString(),
                            totalamount = rdr1["orderAmountTotal"].ToString(),
                            trackingURL = rdr1["orderTrackingUrl"].ToString(),
                            poamount = rdr1["orderPOAmountPHP"].ToString(),
                            Status = rdr1["status"].ToString(),
                            RFullName = rdr1["beneficiaryname"].ToString(),
                            exchangeRate = rdr1["orderExchangeRate"].ToString()

                        });
                        
                    }
                    rdr1.Close();
                    Count = listData.Count;



                    kplog.Info("Success : Data Found, respcode = 1, listdata = '" + listData + "'");
                    return new TransactionResponseMobile { respcode = 1, tl = listData ,count = Count};
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                return new TransactionResponseMobile { respcode = 0, message = ex.ToString() };
            }
        }

        public TransactionResponseMobile getRecentTransactions(string CustomerID) 
        {

            try
            {
                List<TransactionDetailsM> listData = new List<TransactionDetailsM>();
                Int32 Count = 0;

                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    DateTime dt = getServerDateGlobal(false);
                    String Month = dt.ToString("MM");
                    con.Open();



                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "SELECT orderExchangeRate, (select FullName as beneficiaryname from kpcustomersglobal.BeneficiaryHistory where CustIDB=receiverIdentifier) as beneficiaryname,orderPOAmountPHP,siteOrderIdentifier, orderCreated, orderPrincipal, orderChargeML,orderChargePNM, orderAmountTotal,orderTrackingUrl,IF(`status` != 'confirm',IF(NOW() > OrderDuration,'void','open'),'confirm') as `status` FROM paynearme.order" + Month + " WHERE senderIdentifier = @CustomerID  ORDER BY orderCreated desc LIMIT 10;";
                        cmd.Parameters.AddWithValue("CustomerID", CustomerID);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                        {
                            listData.Add(new TransactionDetailsM
                            {
                                kptn = rdr["siteOrderIdentifier"].ToString(),
                                TransDate = rdr["orderCreated"].ToString(),
                                principal = rdr["orderPrincipal"].ToString(),
                                charge = (Convert.ToDouble(rdr["orderChargeML"]) + Convert.ToDouble(rdr["orderChargePNM"])).ToString(),
                                totalamount = rdr["orderAmountTotal"].ToString(),
                                trackingURL = rdr["orderTrackingUrl"].ToString(),
                                poamount = rdr["orderPOAmountPHP"].ToString(),
                                Status = rdr["status"].ToString(),
                                RFullName = rdr["beneficiaryname"].ToString(),
                                exchangeRate = rdr["orderExchangeRate"].ToString()

                            });
                            
                        }
                        rdr.Close();
                        Count = listData.Count;

                        kplog.Info("Success : Data Found, respcode = 1, listdata = '" + listData + "'");
                        return new TransactionResponseMobile { respcode = 1, tl = listData, count = Count };

                    }
                }

            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                return new TransactionResponseMobile { respcode = 0, message = ex.ToString() };
            }
        
            
            
        
        
        }

        //Done Loggings RR
        [HttpPost]
        public ProfileResponse editProfile(CustomerModel model)
        {
            kplog.Info("START--- > PARAMS: "+JsonConvert.SerializeObject(model));
            try
            {
                String custid = String.Empty;
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = "Select CustomerID from kpcustomersglobal.PayNearMe where UserID = @email";
                        cmd.Parameters.AddWithValue("email", model.UserID);
                        MySqlDataReader rdr = cmd.ExecuteReader();      
                        if (rdr.HasRows)
                        {
                            rdr.Read();
                            custid = rdr["CustomerID"].ToString();
                            rdr.Close();

                            String bdate = ConvertDateTime(model.BirthDate);
                            String expiryDate = ConvertDateTime(model.ExpiryDate);
                            
                            cmd.Parameters.Clear();
                            cmd.CommandText = "UPDATE kpcustomersglobal.customers set  Street = @Street, ProvinceCity = @ProvinceCity, Country = @Country, ZipCode= @ZipCode, Birthdate = @BirthDate, Gender = @Gender,Mobile =@Mobile, IDType=@IDType,IDNo=@IDNo,ExpiryDate=@ExpiryDate where CustID = @CustID;";
                            cmd.Parameters.AddWithValue("Street", model.Street);
                            cmd.Parameters.AddWithValue("ProvinceCity", model.State);
                            cmd.Parameters.AddWithValue("Country", model.Country);
                            cmd.Parameters.AddWithValue("ZipCode", model.ZipCode);
                            cmd.Parameters.AddWithValue("BirthDate", bdate);
                            cmd.Parameters.AddWithValue("Gender", model.Gender);
                            cmd.Parameters.AddWithValue("Mobile", model.PhoneNo);
                            cmd.Parameters.AddWithValue("CustID", custid);
                            cmd.Parameters.AddWithValue("IDNo", model.IDNo);
                            cmd.Parameters.AddWithValue("IDType", model.IDType);
                            cmd.Parameters.AddWithValue("ExpiryDate", expiryDate);
                            cmd.ExecuteNonQuery();

                            kplog.Info("Success Update kpcustomersglobal.customers");

                            cmd.Parameters.Clear();
                            cmd.CommandText = "Update kpcustomersglobal.customersdetails set HomeCity = @City where custID = @CustID;";
                            cmd.Parameters.AddWithValue("City", model.City);
                            cmd.Parameters.AddWithValue("CustID", custid);
                            cmd.ExecuteNonQuery();

                            kplog.Info("Success Update kpcustomersglobal.customersdetails");


                            if (!string.IsNullOrEmpty(model.strBase64Image)) {
                                String filename = getTimeStamp().ToString() + ".png";
                                String browsepath = http + "/PayNearMe/Images/" + filename;
                                String filepath = ftp + "/PayNearMe/Images/" + filename;
                                uploadFileImage(model.strBase64Image, filepath);

                            cmd.Parameters.Clear();
                            cmd.CommandText = "Update kpcustomersglobal.PayNearMe set ImagePath = @ImagePath where CustomerID = @CustID;";
                            cmd.Parameters.AddWithValue("ImagePath", browsepath);
                            cmd.Parameters.AddWithValue("CustID", custid);
                            cmd.ExecuteNonQuery();

                            kplog.Info("Success Update kpcustomersglobal.PayNearMe - ImagePath");

                            }

                            if (!string.IsNullOrEmpty(model.strBase64Image1F))
                            {
                                String filename =getTimeStamp().ToString() + "1F.png";
                                String browsepath = http + "/PayNearMe/Images/" + filename;
                                String filepath = ftp + "/PayNearMe/Images/" + filename;
                                uploadFileImage(model.strBase64Image1F, filepath);

                                cmd.Parameters.Clear();
                                cmd.CommandText = "Update kpcustomersglobal.PayNearMe set validID1Front = @ImagePath where CustomerID = @CustID;";
                                cmd.Parameters.AddWithValue("ImagePath", browsepath);
                                cmd.Parameters.AddWithValue("CustID", custid);
                                cmd.ExecuteNonQuery();

                                kplog.Info("Success Update kpcustomersglobal.PayNearMe - validID1Front");

                            }

                            if (!string.IsNullOrEmpty(model.strBase64Image1B))
                            {
                                String filename = getTimeStamp().ToString() + "1B.png";
                                String browsepath = http + "/PayNearMe/Images/" + filename;
                                String filepath = ftp + "/PayNearMe/Images/" + filename;

                                uploadFileImage(model.strBase64Image1B, filepath);

                                cmd.Parameters.Clear();
                                cmd.CommandText = "Update kpcustomersglobal.PayNearMe set validID1Back = @ImagePath where CustomerID = @CustID;";
                                cmd.Parameters.AddWithValue("ImagePath", browsepath);
                                cmd.Parameters.AddWithValue("CustID", custid);
                                cmd.ExecuteNonQuery();


                                kplog.Info("Success Update kpcustomersglobal.PayNearMe - validID1Back");
                            }

                            if (!string.IsNullOrEmpty(model.strBase64Image2F))
                            {

                                String filename = getTimeStamp().ToString() + "2F.png";
                                String browsepath = http + "/PayNearMe/Images/" + filename;
                                String filepath = ftp + "/PayNearMe/Images/" + filename;


                                uploadFileImage(model.strBase64Image2F, filepath);

                                cmd.Parameters.Clear();
                                cmd.CommandText = "Update kpcustomersglobal.PayNearMe set validID2Front = @ImagePath where CustomerID = @CustID;";
                                cmd.Parameters.AddWithValue("ImagePath", browsepath);
                                cmd.Parameters.AddWithValue("CustID", custid);
                                cmd.ExecuteNonQuery();

                                kplog.Info("Success Update kpcustomersglobal.PayNearMe - validID2Front");
                            }

                            if (!string.IsNullOrEmpty(model.strBase64Image2B))
                            {
                                String filename = getTimeStamp().ToString() + "2B.png";
                                String browsepath = http + "/PayNearMe/Images/" + filename;
                                String filepath = ftp + "/PayNearMe/Images/" + filename;

                                uploadFileImage(model.strBase64Image2B, filepath);

                                cmd.Parameters.Clear();
                                cmd.CommandText = "Update kpcustomersglobal.PayNearMe set validID2Back = @ImagePath where CustomerID = @CustID;";
                                cmd.Parameters.AddWithValue("ImagePath", browsepath);
                                cmd.Parameters.AddWithValue("CustID", custid);
                                cmd.ExecuteNonQuery();

                                kplog.Info("Success Update kpcustomersglobal.PayNearMe - validID2Back");

                            }


                            con.Close();
                            kplog.Info("SUCCESSS : respcode 1, message: Sucessfully Updated -- END");
                            return new ProfileResponse { respcode = 1, message = "Profile Successfully Updated" };

                        }
                        else
                        {
                            rdr.Close();
                            con.Close();
                            kplog.Info("SUCCESSS : No Profile Found! -- END");
                            return new ProfileResponse { respcode = 0, message = "No Profile Found! " };
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                return new ProfileResponse { respcode = 0, message = ex.ToString() };
            }


        }
        //Done Loggings RR
        [HttpPost]
        public ProfileResponse changePassword(ChangePasswordModel model)
        {
            try
            {
                kplog.Info("START--- > PARAMS: " + JsonConvert.SerializeObject(model));

                using (MySqlConnection con = new MySqlConnection(connection))
                {

                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {

                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select CustomerID from kpcustomersglobal.PayNearMe where UserID = @email and Password = @oldpassword";
                        cmd.Parameters.AddWithValue("email", model.UserID);
                        cmd.Parameters.AddWithValue("oldpassword", model.currentPassword);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                        {

                            if (model.newPassword != model.confirmPassword)
                            {
                                rdr.Close();
                                con.Close();
                                kplog.Info("SUCCESSS : Password did not match!");
                                return new ProfileResponse { respcode = 0, message = "Password did not match!" };
                            }

                            rdr.Read();
                            String custid = rdr["CustomerID"].ToString();
                            rdr.Close();
                            cmd.Parameters.Clear();
                            cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe Set Password = @Password where CustomerID = @custID";
                            cmd.Parameters.AddWithValue("custID", custid);
                            cmd.Parameters.AddWithValue("Password", model.newPassword);
                            cmd.ExecuteNonQuery();


                            con.Close();
                            kplog.Info("SUCCESSS : Respcode = 1, message = Succesfully Change Password!");
                            return new ProfileResponse { respcode = 1, message = "Password is successfully updated!" };

                        }
                        else
                        {
                            kplog.Info("SUCCESSS : Respcode = 0, message = Incorrect Password!");
                            return new ProfileResponse { respcode = 0, message = "Incorrect Password!" };
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                return new ProfileResponse { respcode = 0, message = ex.ToString() };
            }
        }

       
        public ProfileResponse getProfile(String CustomerID)
        {

            try
            {

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {

                        String expirydate;
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select FirstName,LastName,MiddleName,Street,ProvinceCity as State, Country,ZipCode,BirthDate,c.HomeCity as City,Gender, Mobile,b.UserID as UserID, b.ImagePath as filepath,b.validID1Front,b.validID1Back,b.validID2Front,b.validID2Back,b.CustomerID,a.IDNo,a.IDType,a.ExpiryDate from kpcustomersglobal.customers a inner join kpcustomersglobal.PayNearMe b on a.CustID = b.CustomerID inner join kpcustomersglobal.customersdetails c on a.CustID = c.CustID where a.custID = @custID";
                        cmd.Parameters.AddWithValue("custID", CustomerID);
                        MySqlDataReader rdrProf = cmd.ExecuteReader();
                        if (rdrProf.HasRows)
                        {
                           
                            
                            rdrProf.Read();
                            String filepath = rdrProf["filepath"].ToString();
                            String filepath1 = rdrProf["validID1Front"].ToString();
                            String filepath2 = rdrProf["validID1Back"].ToString();
                            String filepath3 = rdrProf["validID2Front"].ToString();
                            String filepath4 = rdrProf["validID2Back"].ToString();
                            String bdate = rdrProf["BirthDate"].ToString();
                            if (bdate.StartsWith("00") || bdate == String.Empty)
                            {
                                bdate = "";
                            }
                            else
                            {
                                bdate = Convert.ToDateTime(rdrProf["BirthDate"]).ToString("MM/dd/yyyy");
                            }
                            if (string.IsNullOrEmpty(rdrProf["ExpiryDate"].ToString()) || rdrProf["ExpiryDate"].ToString().StartsWith("00"))
                            {
                                expirydate = "";
                            }
                            else
                            {
                                expirydate = Convert.ToDateTime(rdrProf["ExpiryDate"]).ToString("MM/dd/yyyy");
                            }
                            return new ProfileResponse
                            {
                                respcode = 1,
                                message = "Success",
                                sender = new CustomerModel
                                {
                                    firstName = rdrProf["Firstname"].ToString(),
                                    middleName = rdrProf["MiddleName"].ToString(),
                                    lastName = rdrProf["LastName"].ToString(),
                                    Street = rdrProf["Street"].ToString(),
                                    State = rdrProf["State"].ToString(),
                                    Country = rdrProf["Country"].ToString(),
                                    ZipCode = rdrProf["ZipCode"].ToString(),
                                    BirthDate = bdate,
                                    City = rdrProf["City"].ToString(),
                                    Gender = rdrProf["Gender"].ToString(),
                                    PhoneNo = rdrProf["Mobile"].ToString(),
                                    UserID = rdrProf["UserID"].ToString(),
                                    ImagePath = filepath,
                                    ImagePath1 = filepath1,
                                    ImagePath2 = filepath2,
                                    ImagePath3 = filepath3,
                                    ImagePath4 = filepath4,
                                    CustomerID = rdrProf["CustomerID"].ToString(),
                                    IDNo = rdrProf["IDNo"].ToString(),
                                    IDType = rdrProf["IDType"].ToString(),
                                    ExpiryDate = expirydate
                                }
                                


                            };
                        }
                        else
                        {

                            rdrProf.Close();
                            kplog.Info("SUCCESS : respcode = 0, message = not yet registered!");
                            return new ProfileResponse { respcode = 0, message = "Not yet registered" };

                        }
                 

                }


            }

            }
            catch (Exception ex)
            {

                return new ProfileResponse { respcode = 0, message = ex.ToString() };
            }
        }


        [HttpGet]
        public CustomerResultResponse resendActivationCode(String UserID)
        {

            String activationCode = generateActivationCode();
            String mobileToken = generateMobileToken();


            using (MySqlConnection con = new MySqlConnection(connection))
            {
                con.Open();
                String FullName = string.Empty;
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "Select isEmailActivated,FullName from kpcustomersglobal.PayNearMe where UserID = @UserID";
                cmd.Parameters.AddWithValue("UserID", UserID);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    rdr.Read();
                    Int32 isEmailAct = Convert.ToInt32(rdr["isEmailActivated"]);
                    FullName = rdr["FullName"].ToString();
                    rdr.Close();

                    if (isEmailAct == 1)
                    {
                        con.Close();
                        return new CustomerResultResponse { respcode = 1, message = "Account already Activated" };

                    }

                    cmd.Parameters.Clear();
                    cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe SET activationCode = @activationCode where UserID = @UserID";
                    cmd.Parameters.AddWithValue("activationCode", activationCode);
                    cmd.Parameters.AddWithValue("UserID", UserID);
                    int x = cmd.ExecuteNonQuery();

                    if (x > 0)
                    {
                        con.Close();
                        sendEmailActivation(UserID, FullName, activationCode,"");
                        kplog.Info("SUCCESS : respcode = 0, message = Success");
                        return new CustomerResultResponse { respcode = 1, message = "Success" };
                    }
                    else
                    {
                        con.Close();
                        kplog.Info("SUCCESS : respcode = 0, message = Failed");
                        return new CustomerResultResponse { respcode = 0, message = "Failed" };
                    }


                }
                else
                {

                    con.Close();
                    kplog.Info("SUCCESS : respcode = 0, message = Email does not exist!");
                    return new CustomerResultResponse { respcode = 0, message = "Email does not exist!" };

                }



            }



        }

        
        [HttpPost]
        public AuthenticateResponse authenticateSignup(AuthenticateRequest req)
        {
            String userID = req.UserID;
            String activationCode = req.ActivationCode;
            try
            {
                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    con.Open();

                    MySqlCommand cmd = con.CreateCommand();

                    cmd.CommandText = "Select * from kpcustomersglobal.PayNearMe where UserID = @UserID;";
                    cmd.Parameters.AddWithValue("UserID", userID);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        rdr.Close();
                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select Password from kpcustomersglobal.PayNearMe where activationCode = @activationCode and UserID = @UserID;";
                        cmd.Parameters.AddWithValue("activationCode", activationCode);
                        cmd.Parameters.AddWithValue("UserID", userID);
                        MySqlDataReader rdrCode = cmd.ExecuteReader();
                        if (rdrCode.HasRows)
                        {
                            rdrCode.Read();
                            String password = rdrCode["Password"].ToString();
                            rdrCode.Close();
                            cmd.Parameters.Clear();
                            cmd.CommandText = "UPDATE kpcustomersglobal.PayNearMe SET isEmailActivated = 1 where UserID = @UserID and activationCode = @activationCode";
                            cmd.Parameters.AddWithValue("activationCode", activationCode);
                            cmd.Parameters.AddWithValue("UserID", userID);
                            int x = cmd.ExecuteNonQuery();

                            if (x > 0)
                            {
                                con.Close();
                                return new AuthenticateResponse { respcode = 1, message = "Success", password = password, userID = userID };
                            }
                            else
                            {
                                con.Close();
                                return new AuthenticateResponse { respcode = 0, message = "Failed" };
                            }


                        }
                        else
                        {
                            kplog.Info("SUCCESS : respcode = 0, message = Invalid Activation code!");
                            return new AuthenticateResponse { respcode = 0, message = "Invalid Activation code" };
                        }

                    }
                    else
                    {
                        kplog.Info("SUCCESS : respcode = 0, message = UserID does not exist!");
                        return new AuthenticateResponse { respcode = 0, message = "UserID does not exist!" };
                    }


                }
            }
            catch (Exception ex)
            {
                kplog.Error("FAILED:: respcode: 0 message: '" + getRespMessage(0) +"'  ErrorDetail: '" + ex.ToString() + "'");
                return new AuthenticateResponse { respcode = 0, message = ex.ToString() };
            }



        }


        public CreateOrderResponse createOrder(TransactionModel model)
        {

            try
            {
                var sender = getProfile(model.senderCustID).sender;
                var receiver = getBeneficiaryInfo(model.receiverCustId).data;
                DateTime dt = getServerDateGlobal();
                model.KPTN = generateKPTNPayNearMe("711", 3, model.TransactionType);
                model.TransDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                String Month = dt.ToString("MM");
                model.Principal = Math.Round(model.Principal, 2);
                string amountTotal = (model.Principal + model.Charge).ToString();

                //generate control
                string station = model.TransactionType == "Web" ? "1" : "2";
                var resp = generateControlGlobal("boswebserviceusr", "boyursa805", "711", 0, "PNME7117113", 3, station);
                string controlNo = string.Empty;
                if (resp.respcode == 1)
                {
                    controlNo = resp.controlno;
                }
                else
                {
                    List<Error> err = new List<Error>();
                    err.Add(new Error
                    {

                        description = "MYSQL ERROR: Pls Contact Support"
                    });

                    return new CreateOrderResponse { status = "error", errors = err };
                }
                //create-order-pnm
                if (dt.ToString("HH") == "23") 
                {
                    List<Error> err = new List<Error>();
                    err.Add(new Error
                    {

                        description = "Sorry, transactions are not allowed beyond 11PM. Please transact again after 12AM. Thank You! "
                    });
                 
                    return new CreateOrderResponse { status = "error", errors = err };
                }
                string order_expiration_date = dt.ToString("yyyy-MM-dd 23:59:59 PT");
                string site_customer_identifier = controlNo;
                string min_payment = amountTotal;
                string site_customer_name = sender.firstName + " " + sender.lastName;
                string site_customer_email = sender.UserID;
                string site_customer_phone = sender.PhoneNo;
                bool site_customer_sms_ok = sender.sendSMS;
                string site_creator_identifier = sender.CustomerID;
                string order_amount = amountTotal;
                string order_currency = "USD";
                string receiver_user_identifier = receiver.receiverCustID;
                string sender_user_identifier = sender.CustomerID;
                string site_order_identifier = model.KPTN;
                string timestamp = getTimeStamp().ToString();
                string version = "2.0";

                string queryAPI = "minimum_payment_amount=" + min_payment + "&minimum_payment_currency=USD&order_amount=" + order_amount + "&order_currency=" + order_currency + "&order_expiration_date=" + order_expiration_date + "&order_type=exact&receiver_user_identifier=" + receiver_user_identifier +
                    "&return_html_slip=true&return_pdf_slip=true&sender_user_identifier=" + sender_user_identifier+ 
                    "&site_customer_email=" + site_customer_email +
                    "&site_customer_identifier=" + site_customer_identifier +
                    "&site_customer_name="+site_customer_name+
                    "&site_customer_phone="+site_customer_phone+"&site_customer_sms_ok="+site_customer_sms_ok+
                    "&site_identifier=" + siteIdentifier + "&site_order_description=" + "Test" + "&site_order_identifier=" + site_order_identifier +
                    "&timestamp=" + timestamp + "&version=" + version + "";

                string signature = generateSignature(queryAPI);

                queryAPI = queryAPI + "&signature=" + signature;

                Uri uri = new Uri(server + "/json-api/create_order?" + queryAPI);

                string res = SendRequest(uri);
                kplog.Info(res);
                CreateOrderResponse response = new CreateOrderResponse();
                
                dynamic data = JsonConvert.DeserializeObject(res, typeof(CreateOrderResponse));
                response = data;


                //XmlSerializer serializer = new XmlSerializer(typeof(CreateOrderResponse));                
                //using (TextReader reader = new StringReader(res)) 
                //{
                //dynamic data = serializer.Deserialize(reader);
                //    response = data;
                //}
                

                if (response.status == "ok")
                {
                    
                   
                    using(MySqlConnection con = new MySqlConnection(connection))
                    {
                        con.Open();
                        using(MySqlCommand cmd = con.CreateCommand())
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "INSERT INTO paynearme.order" + Month + " (siteOrderIdentifier,pnmOrderIdentifier,siteCustomerIdentifier,receiverIdentifier,senderIdentifier,orderCreated,orderPrincipal,orderChargeML,orderChargePNM,orderExchangeRate,orderAmountTotal,orderType,orderDuration,status,orderTrackingUrl,orderPOAmountPHP,ControlNo) " +
                                " VALUES(@siteOrderIdentifier,@pnmOrderIdentifier,@siteCustomerIdentifier,@receiverIdentifier,@senderIdentifier,@orderCreated,@orderPrincipal,@orderChargeML,@orderChargePNM,@orderExchangeRate,@orderAmountTotal,@orderType,@orderDuration,@status,@orderTrackingUrl,@orderPOAmountPHP,@ControlNo);";

                            cmd.Parameters.AddWithValue("siteOrderIdentifier", response.order.site_order_identifier);
                            cmd.Parameters.AddWithValue("pnmOrderIdentifier",response.order.pnm_order_identifier);
                            cmd.Parameters.AddWithValue("siteCustomerIdentifier",response.order.customer.site_customer_identifier);
                            cmd.Parameters.AddWithValue("receiverIdentifier", response.order.users.user[1].user_site_identifier);
                            cmd.Parameters.AddWithValue("senderIdentifier",response.order.users.user[0].user_site_identifier);
                            cmd.Parameters.AddWithValue("orderCreated",response.order.order_created);
                            cmd.Parameters.AddWithValue("orderPrincipal",model.Principal);
                            cmd.Parameters.AddWithValue("orderChargeML", (model.Charge - pnmCharge));
                            cmd.Parameters.AddWithValue("orderChargePNM", pnmCharge);
                            cmd.Parameters.AddWithValue("orderExchangeRate",model.ExchangeRate);
                            cmd.Parameters.AddWithValue("orderAmountTotal", response.order.order_amount);
                            cmd.Parameters.AddWithValue("orderType",response.order.order_type);
                            cmd.Parameters.AddWithValue("orderDuration", order_expiration_date);
                            cmd.Parameters.AddWithValue("status",response.order.order_status);
                            cmd.Parameters.AddWithValue("orderTrackingUrl",response.order.order_tracking_url);
                            cmd.Parameters.AddWithValue("orderPOAmountPHP", model.POAmountPHP);
                            cmd.Parameters.AddWithValue("ControlNo", controlNo);
                            int xx = cmd.ExecuteNonQuery();
                            if (xx > 0)
                            {
                                kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1));
                                con.Close();
                                return response;
                            }
                            else 
                            {
                                List<Error> err = new List<Error>();
                                err.Add(new Error
                                {

                                    description = "There was a problem processing your request please try again."
                                });
                                con.Close();
                                return new CreateOrderResponse { status = "error", errors = err };
                            }
                        }
                               
                    }
                            
                }
                else
                {
                          
                    return response;
                }
            }
            catch (Exception ex)
            {
                List<Error> err = new List<Error>();
                err.Add(new Error
                {

                    description = ex.ToString()
                });
                return new CreateOrderResponse { status = "error", errors = err };
            }
        }

        //FOREX
        [HttpGet]
        public getbrachrateclassificationresponse Getbranchrateclassification(String bcode, String zone)
        {
            kplog.Info("SUCCES:: message:: bcode: " + bcode + " zone: " + zone);
            try
            {
                String sql;
                sql = "call mlforexrate.sp_getbranchclassification ('" + bcode + "', '" + zone + "') ";

                using (MySqlConnection con = new MySqlConnection(mlforex)) 
                {
                    con.Open();
                    command = con.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;
                    MySqlDataReader dr = command.ExecuteReader();

                    if (dr.HasRows == true)
                    {
                        dr.Read();
                        kplog.Info("SUCCESS:: message: " + getRespMessage(1));
                        return new getbrachrateclassificationresponse { rescode = "1", msg = getRespMessage(1), bcode = (string)dr["branchcode"], bname = (string)dr["branchname"], zone = (string)dr["zonecode"], classification = (string)dr["classification"], description = (string)dr["descriptions"], buying = (string)dr["buying"], selling = (string)dr["selling"] };
                    }
                    else
                    {
                        kplog.Error("FAILED:: message: " + getRespMessage(1));
                        return new getbrachrateclassificationresponse { rescode = "0", msg = getRespMessage(0) };
                    }
                
                
                }
                
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: message: " + ex.ToString());
                return new getbrachrateclassificationresponse { rescode = "0", msg = ex.Message };
                // createLog.makeLog("ERROR" + ex.Message);
            }
           
        }

        [HttpGet]
        public getbranchratesresponse GetBranchRates(String bcode, String zone, String currency)
        {
           
            try
            {

               String classification = Getbranchrateclassification(bcode, zone).classification;
               kplog.Info("SUCCES:: message:: bcode: " + bcode + " zone: " + zone + " classification: " + classification + " currency: " + currency);

                String sql = String.Empty;
                String sqlmanual = String.Empty;
                String sqlchk = String.Empty;
                String remarks = String.Empty;
                Int32 identifier = 0;
                DateTime? effectivedate = null;

                sql = "call mlforexrate.sp_getbranchrates ('" + bcode + "', '" + zone + "', '" + currency + "', '" + classification + "');";
                sqlmanual = "SELECT b.branchname,b.branchcode,bm.curr_sell as selling,bm.curr_buy as buying,@pcur as currency FROM mlforexrate.brachrateclassification b INNER JOIN mlforexrate.branchforexmanual bm ON bm.branchcode = b.branchcode and bm.zonecode = b.zonecode WHERE bm.branchcode = @pbcode and bm.zonecode = @pzcode";
                sqlchk = "SELECT remarks, identifier, IF(effectivedate IS NULL,NULL,DATE_FORMAT(effectivedate,'%Y-%m-%d %H:%i:%s')) AS effectivedate FROM mlforexrate.branchforextagrates WHERE branchcode = @bcode and zonecode = @zcode";

                using (MySqlConnection con = new MySqlConnection(mlforex)) 
                {
                    command = new MySqlCommand();

                    con.Open();

                    command.CommandText = sqlchk;
                    command.Connection = con;
                    command.CommandType = CommandType.Text;
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("bcode", bcode);
                    command.Parameters.AddWithValue("zcode", zone);
                    MySqlDataReader rdrchk = command.ExecuteReader();
                    if (rdrchk.Read())
                    {
                        remarks = rdrchk["remarks"].ToString();
                        identifier = Convert.ToInt32(rdrchk["identifier"].ToString());
                        if (rdrchk["effectivedate"] == DBNull.Value)
                        {
                            effectivedate = null;
                        }
                        else
                        {
                            effectivedate = Convert.ToDateTime(rdrchk["effectivedate"]);
                        }
                    }
                    rdrchk.Close();

                    if (remarks != String.Empty)
                    {
                        if (remarks == "Automate")
                        {
                            command.CommandText = sql;
                            command.Connection = con;
                            command.CommandType = CommandType.Text;
                            command.Parameters.Clear();
                            MySqlDataReader dr = command.ExecuteReader();

                            if (dr.HasRows == true)
                            {
                                dr.Read();
                                //con.Close();
                                kplog.Info("SUCCES:: message: " + getRespMessage(1));
                                return new getbranchratesresponse { rescode = "1", msg = getRespMessage(1), selling = (decimal)dr["selling"], buying = (decimal)dr["buying"], branchcode = (string)dr["branchcode"], branchname = (string)dr["branchname"], currency = (string)dr["currency"] };


                            }
                            else
                            {
                                kplog.Error(getRespMessage(0));
                                return new getbranchratesresponse { rescode = "0", msg = getRespMessage(0) };
                            }
                        }
                        else
                        {
                            DateTime servrdt = Convert.ToDateTime(getServerDateGlobal());
                            int result = DateTime.Compare((DateTime)servrdt, (DateTime)effectivedate);
                            if (result >= 0)
                            {
                                command.CommandText = sqlmanual;
                                command.Connection = con;
                                command.CommandType = CommandType.Text;
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("pbcode", bcode);
                                command.Parameters.AddWithValue("pzcode", zone);
                                command.Parameters.AddWithValue("pcur", currency);
                                MySqlDataReader rdrgetmanual = command.ExecuteReader();
                                if (rdrgetmanual.Read())
                                {
                                    kplog.Info("SUCCES:: message: " + getRespMessage(1) + ", selling: " + ((decimal)rdrgetmanual["selling"]).ToString() + ", buying: " + ((decimal)rdrgetmanual["buying"]).ToString() + ", branchcode: " + ((string)rdrgetmanual["branchcode"]).ToString() + ", branchname: " + ((string)rdrgetmanual["branchname"]).ToString() + ", currency: " + ((string)rdrgetmanual["currency"]).ToString());
                                    return new getbranchratesresponse { rescode = "1", msg = getRespMessage(1), selling = (decimal)rdrgetmanual["selling"], buying = (decimal)rdrgetmanual["buying"], branchcode = (string)rdrgetmanual["branchcode"], branchname = (string)rdrgetmanual["branchname"], currency = (string)rdrgetmanual["currency"] };
                                }
                                rdrgetmanual.Close();
                                con.Close();
                            }
                            else
                            {
                                command.CommandText = sql;
                                command.Connection = con;
                                command.CommandType = CommandType.Text;
                                command.Parameters.Clear();
                                MySqlDataReader dr = command.ExecuteReader();

                                if (dr.HasRows == true)
                                {
                                    dr.Read();
                                    kplog.Info("SUCCESS:: message: " + getRespMessage(1) + ", selling: " + ((decimal)dr["selling"]).ToString() + ", buying: " + ((decimal)dr["buying"]).ToString() + ", branchcode :" + ((string)dr["branchcode"]).ToString() + ", branchname: " + ((string)dr["branchname"]).ToString() + ", currency :" + ((string)dr["currency"]).ToString());
                                    return new getbranchratesresponse { rescode = "1", msg = getRespMessage(1), selling = (decimal)dr["selling"], buying = (decimal)dr["buying"], branchcode = (string)dr["branchcode"], branchname = (string)dr["branchname"], currency = (string)dr["currency"] };

                                }
                                else
                                {
                                    con.Close();
                                    kplog.Error("FAILED:: message: " + getRespMessage(0));
                                    return new getbranchratesresponse { rescode = "0", msg = getRespMessage(0) };
                                }
                            }
                        }
                    }
                    else
                    {
                        command.CommandText = sql;
                        command.Connection = con;
                        command.CommandType = CommandType.Text;
                        command.Parameters.Clear();
                        MySqlDataReader dr = command.ExecuteReader();

                        if (dr.HasRows == true)
                        {
                            dr.Read();
                            kplog.Info("SUCCES:: message: " + getRespMessage(1) + ", selling: " + ((decimal)dr["selling"]).ToString() + ", buying: " + ((decimal)dr["buying"]).ToString() + ", branchcode :" + ((string)dr["branchcode"]).ToString() + ", branchname: " + ((string)dr["branchname"]).ToString() + ", currency :" + ((string)dr["currency"]).ToString());
                            return new getbranchratesresponse { rescode = "1", msg = getRespMessage(1), selling = (decimal)dr["selling"], buying = (decimal)dr["buying"], branchcode = (string)dr["branchcode"], branchname = (string)dr["branchname"], currency = (string)dr["currency"] };

                        }
                        else
                        {
                            con.Close();
                            kplog.Error("FAILED:: message: " + getRespMessage(0));
                            return new getbranchratesresponse { rescode = "0", msg = getRespMessage(0) };
                        }
                    }

                    con.Close();
                    kplog.Error("FAILED:: message: " + getRespMessage(0));
                    return new getbranchratesresponse { rescode = "0", msg = getRespMessage(0) };

                }
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: message: " + ex.ToString());
                return new getbranchratesresponse { rescode = "0", msg = ex.Message };
               //
            }

        }


        public DateTime getServerDateGlobal(Boolean isOpenConnection)
        {

            try
            {
                //throw new Exception(isOpenConnection.ToString());
                if (!isOpenConnection)
                {
                    using (MySqlConnection conn = new MySqlConnection(connection))
                    {
                        conn.Open();
                        using (MySqlCommand command = conn.CreateCommand())
                        {

                            DateTime serverdate;

                            command.CommandText = "Select NOW() as serverdt;";
                            using (MySqlDataReader Reader = command.ExecuteReader())
                            {
                                Reader.Read();
                                serverdate = Convert.ToDateTime(Reader["serverdt"]);
                                Reader.Close();
                                conn.Close();
                               

                                kplog.Info("SUCCESS:: Server Date: " + serverdate);
                                return serverdate;
                            }

                        }
                    }
                }
                else
                {
                    DateTime serverdate;

                    command.CommandText = "Select NOW() as serverdt;";

                    using (MySqlDataReader Reader = command.ExecuteReader())
                    {
                        Reader.Read();
                        serverdate = Convert.ToDateTime(Reader["serverdt"]);
                        Reader.Close();

                        kplog.Info("SUCCESS:: Server Date: " + serverdate);
                        return serverdate;
                    }
                }

            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: message: " + ex.Message + " ErrorDetail: " + ex.ToString());
                throw new Exception(ex.Message);
            }
        }

       // CALLBACKS

        //[HttpGet]
        //public IHttpActionResult authorize()
        //{
        //    try
        //    {

        //        kplog.Info("authorize Request START ------------ > ");

        //        var queryString = this.Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
        //        string siggy = SignatureUtils.Signature(this.Request.GetQueryNameValuePairs(), secretKey);

        //        if (siggy != queryString["signature"])
        //        {
        //            kplog.Info("UnAuthorize Request!");
        //            throw new RequestException("UnAuthorize Request", 400);
        //        }

        //        bool isTest = queryString["test"] != null && Boolean.Parse(queryString["test"]);
        //        if (isTest)
        //        {
        //            kplog.Info("This authorize request is a TEST!");
        //        }

        //        String pnmOrderIdentifier = queryString["pnm_order_identifier"];
        //        String siteOrderIdentifier = queryString["site_order_identifier"];
        //        String site_customer_identifier = queryString["site_customer_identifier"];
        //        String pnm_payment_identifier = queryString["pnm_payment_identifier"];
        //        String net_payment_amount = queryString["net_payment_amount"];
        //        String pnm_withheld_amount = queryString["pnm_withheld_amount"];
        //        String due_to_site_amount = queryString["due_to_site_amount"];
        //        Double site_processing_fee = Convert.ToDouble(queryString["net_payment_amount"]) - Convert.ToDouble(queryString["pnm_withheld_amount"]);
        //        String retailer_name = queryString["retailer_name"];
        //        String retailer_location_identifier = queryString["retailer_location_identifier"];

        //        kplog.Info("KPTN: " + siteOrderIdentifier);
        //        /* This is where you verify the information sent with the
        //           request, validate it within your system, and then return a
        //           response. Here we just accept payments with order identifiers of
        //           "TEST-123" if the request is test mode.
        //         */


        //        AuthorizationResponseBuilder auth = new AuthorizationResponseBuilder("2.0");
        //        //auth.SitePaymentIdentifier = siteOrderIdentifier;
        //        auth.PnmOrderIdentifier = pnmOrderIdentifier;

        //        string Month = siteOrderIdentifier.Substring(19, 2);

        //        String receiver = string.Empty;
        //        String POAmountPHP = string.Empty;

        //        bool accept = false;
        //        if (siteOrderIdentifier != null)
        //        {
        //            using (MySqlConnection con = new MySqlConnection(connection))
        //            {
        //                con.Open();
        //                using (MySqlCommand cmd = con.CreateCommand())
        //                {

        //                    cmd.CommandText = "Select orderPOAmountPHP, (select CONCAT(firstname,' ',lastname) from kpcustomersglobal.BeneficiaryHistory where custIDB = receiverIdentifier) as receiver FROM paynearme.order" + Month + " where siteOrderIdentifier = @siteOrderIdentifier;";
        //                    cmd.Parameters.AddWithValue("siteOrderIdentifier", siteOrderIdentifier);
        //                    MySqlDataReader rdr = cmd.ExecuteReader();
        //                    if (!rdr.HasRows)
        //                    {
        //                        kplog.Info("Example authorization " + siteOrderIdentifier + " will be DECLINED, Does not Exist in ML Database");
        //                        rdr.Close();
        //                        con.Close();
        //                    }
        //                    else
        //                    {
        //                        rdr.Read();
        //                        receiver = rdr["receiver"].ToString();
        //                        POAmountPHP = rdr["orderPOAmountPHP"].ToString();
        //                        rdr.Close();
        //                        cmd.Parameters.Clear();
        //                        cmd.CommandText = "Select * from paynearme.payment" + Month + " where pnmPaymentIdentifier = @pnmPaymentIdentifier";
        //                        cmd.Parameters.AddWithValue("pnmPaymentIdentifier", queryString["pnm_payment_identifier"]);
        //                        MySqlDataReader rdrPnm = cmd.ExecuteReader();
        //                        if (!rdrPnm.HasRows)
        //                        {
        //                            rdrPnm.Close();
        //                            cmd.Parameters.Clear();
        //                            cmd.CommandText = "INSERT INTO paynearme.payment" + Month + " (pnmPaymentIdentifier,pnmOrderIdentifier,siteOrderIdentifier,siteCustomerIdentifier,paymentDate,netPaymentAmount,pnmWithheldAmount,dueToSiteAmount,siteProcessingFee,retailerName,retailerLocationIdentifier) " +
        //                                "VALUES(@pnmPaymentIdentifier,@pnmOrderIdentifier,@siteOrderIdentifier,@siteCustomerIdentifier,NOW(),@netPaymentAmount,@pnmWithheldAmount,@dueToSiteAmount,@siteProcessingFee,@retailerName,@retailerLocationIdentifier);";
        //                            cmd.Parameters.AddWithValue("pnmPaymentIdentifier", pnm_payment_identifier);
        //                            cmd.Parameters.AddWithValue("siteOrderIdentifier", siteOrderIdentifier);
        //                            cmd.Parameters.AddWithValue("pnmOrderIdentifier", pnmOrderIdentifier);
        //                            cmd.Parameters.AddWithValue("siteCustomerIdentifier", site_customer_identifier);
        //                            cmd.Parameters.AddWithValue("netPaymentAmount", net_payment_amount);
        //                            cmd.Parameters.AddWithValue("pnmWithheldAmount", pnm_withheld_amount);
        //                            cmd.Parameters.AddWithValue("dueToSiteAmount", due_to_site_amount);
        //                            cmd.Parameters.AddWithValue("siteProcessingFee", site_processing_fee);
        //                            cmd.Parameters.AddWithValue("retailerName", retailer_name);
        //                            cmd.Parameters.AddWithValue("retailerLocationIdentifier", retailer_location_identifier);
        //                            int x = cmd.ExecuteNonQuery();

        //                            if (x < 1)
        //                            {
        //                                kplog.Error(" insertion params: pnmPaymentIdentifier: " + pnm_payment_identifier + " , siteOrderIdentifier: " + siteOrderIdentifier + ", pnmOrderIdentifier: " + pnmOrderIdentifier +
        //                                    ", siteCustomerIDentifier :" + site_customer_identifier + ", netPaymentAmount: " + net_payment_amount + ", pnmWithheldAmount: " + pnm_withheld_amount + ", dueToSiteAmount: " + due_to_site_amount);
        //                                throw new RequestException("Authorization: Error in insertion in paynearme.payment" + Month + " ----------- " + siteOrderIdentifier, 400);
        //                            }


        //                            kplog.Info("Example authorization: Success insertion in paynearme.payment" + Month + " ----- " + siteOrderIdentifier);

        //                        }

        //                        con.Close();
        //                        kplog.Info("Example authorization " + siteOrderIdentifier + " will be ACCEPTED");
        //                        accept = true;

        //                    }

        //                }
        //            }

        //        }
        //        else
        //        {
        //            kplog.Info("Example authorization " + siteOrderIdentifier + " will be DECLINED");
        //        }





        //        if (accept)
        //        {

        //            auth.AcceptPayment = true;
        //            /* You can set custom receipt text here (if you want) - if you
        //               don't want custom text, you can omit this
        //            */
        //            auth.Receipt = "Thank you for your order!" + Environment.NewLine + receiver + " can claim your money " +
        //                "with KPTN " + siteOrderIdentifier + Environment.NewLine + "amounting to " + POAmountPHP + " PHP." + Environment.NewLine + "Please bring a valid ID to any" + Environment.NewLine + "M Lhuillier branches in the Philippines.";
        //            auth.Memo = DateTime.Now.ToString();

        //        }
        //        else
        //        {
        //            auth.AcceptPayment = false;
        //            auth.Receipt = "Declined";
        //            auth.Memo = "Invalid payment: " + siteOrderIdentifier;
        //        }

        //        kplog.Info("authorize Request END ------------ > ");
        //        kplog.Debug("End handleAuthorizationRequest");
        //        return Ok(auth.Build());


        //    }
        //    catch (RequestException ex)
        //    {
        //        kplog.Fatal(ex.ToString());
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception err)
        //    {
        //        kplog.Fatal(err.ToString());
        //        return BadRequest(err.Message);
        //    }

        //}

        //[HttpGet]
        //public IHttpActionResult confirm()
        //{

        //    try
        //    {

        //        kplog.Info("Handling /confirm with ExampleConfirmationHandler");

        //        // Do some extra functions that the 'echo server' does for debugging
        //        var queryString = this.Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
        //        string siggy = SignatureUtils.Signature(this.Request.GetQueryNameValuePairs(), secretKey);

        //        if (siggy != queryString["signature"])
        //        {
        //            kplog.Info("UnAuthorize Request!");
        //            throw new RequestException("UnAuthorize Request", 400);
        //        }
        //        // If the url contains the parameter test=true (part of the signed params too!) then we flag this.
        //        // Do not handle test=true requests as real requests.
        //        bool isTest = queryString["test"] != null && Convert.ToBoolean(queryString["test"]);
        //        if (isTest)
        //        {
        //            kplog.Info("This confirmation request is a TEST!");
        //        }

        //        String identifier = queryString["site_order_identifier"];
        //        String pnmOrderIdentifier = queryString["pnm_order_identifier"];
        //        String pnmPaymentIdentifier = queryString["pnm_payment_identifier"];

        //        /* You must lookup the pnm_payment_identifier in your business system and prevent double posting.
        //           In the event of a duplicate callback from PayNearMe ( this can sometimes happen in a race or
        //           retry condition) you must respond to all duplicates, but do not post the payment.
           
        //           No stub code is provided for this check, and is left to the responsibility of the implementor.
           
        //           Now that you have responded to a /confirm, you need to keep a record of this pnm_payment_identifier.
        //        */

        //        if (pnmOrderIdentifier == null || pnmOrderIdentifier.Equals(""))
        //        {
        //            kplog.Error("pnm_order_identifier is empty or null, do not respond!");
        //            throw new RequestException("pnm_order_identifier is missing", 400);
        //        }
        //        bool decline = false;
        //        if (queryString["status"] != null && queryString["status"].Equals("decline"))
        //        {
        //            decline = true;
        //            kplog.Info("Status: declined, do not credit (site_order_identifier: " + identifier + ")");
        //        }
        //        if (!decline)
        //        {

        //            #region sendout
        //            string Month = identifier.Substring(19, 2);

        //            using (MySqlConnection con = new MySqlConnection(connection))
        //            {
        //                con.Open();
        //                using (MySqlCommand cmd = con.CreateCommand())
        //                {
        //                    cmd.CommandText = "Select * FROM paynearme.payment" + Month + " where pnmPaymentIdentifier = @pnmPaymentIdentifier";
        //                    cmd.Parameters.AddWithValue("pnmPaymentIdentifier", pnmPaymentIdentifier);
        //                    MySqlDataReader rdr = cmd.ExecuteReader();
        //                    if (!rdr.HasRows)
        //                    {

        //                        throw new RequestException("pnmPaymentIdentifier is missing", 400);
        //                    }
        //                    rdr.Read();

        //                    bool ispaid = Convert.ToBoolean(rdr["isPaid"]);
        //                    rdr.Close();
        //                    cmd.Parameters.Clear();
        //                    cmd.CommandText = "Select * from paynearme.order" + Month + " where siteOrderIdentifier=@siteOrderIdentifier;";
        //                    cmd.Parameters.AddWithValue("siteOrderIdentifier", identifier);
        //                    MySqlDataReader rdrPnm = cmd.ExecuteReader();
        //                    if (!rdrPnm.HasRows)
        //                    {

        //                        throw new RequestException("siteOrderIdentifier does not exist in ML Database", 400);
        //                    }

        //                    rdrPnm.Read();
        //                    string receiverIdentifier = rdrPnm["receiverIdentifier"].ToString();
        //                    string senderIdentifier = rdrPnm["senderIdentifier"].ToString();
        //                    double principal = Convert.ToDouble(rdrPnm["orderPrincipal"]);
        //                    //double chargeML = Convert.ToDouble(rdrPnm["orderChargeML"]);
        //                    string TransDate = rdrPnm["orderCreated"].ToString();
        //                    double exchangeRate = Convert.ToDouble(rdrPnm["orderExchangeRate"]);
        //                    double charge = Convert.ToDouble(rdrPnm["orderChargeML"]) + Convert.ToDouble(rdrPnm["orderChargePNM"]);
        //                    double total = Convert.ToDouble(rdrPnm["orderAmountTotal"]);
        //                    double POAmountPHP = Convert.ToDouble(rdrPnm["orderPOAmountPHP"]);
        //                    string controlNo = rdrPnm["controlNo"].ToString();
        //                    rdrPnm.Close();
        //                    con.Close();
        //                    var trans = new TransactionModel
        //                    {
        //                        Charge = charge,
        //                        receiverCustId = receiverIdentifier,
        //                        senderCustID = senderIdentifier,
        //                        Principal = principal,
        //                        ExchangeRate = exchangeRate,
        //                        KPTN = identifier,
        //                        Total = total,
        //                        TransDate = TransDate,
        //                        POAmountPHP = POAmountPHP,
        //                        controlNo = controlNo,
        //                        PaymentIdentifier = pnmPaymentIdentifier

        //                    };

        //                    if (!ispaid && isTest)
        //                    {
        //                        var model = new SendoutModel(trans);
        //                        model.Currency = "USD";
        //                        model.Username = "boswebserviceusr";
        //                        model.Password = "boyursa805";
        //                        model.pocurrency = "PHP";
        //                        model.trxntype = "INTERNATIONAL";
        //                        model.paymenttype = "CASH";
        //                        model.syscreator = "PNME7117113";
        //                        model.zonecode = 3;
        //                        model.branchcode = "711";
        //                        model.type = 0;
        //                        model.series = 2323;
        //                        model.OperatorID = trans.senderCustID;
        //                        model.station = identifier.Substring(20, 1) == "W" ? "1" : "2";

        //                        var resp = sendoutGlobal(model);

        //                        if (resp.respcode != 1)
        //                        {
        //                            throw new RequestException(resp.message, 400);
        //                        }

        //                    }

        //                }
        //            }
        //            #endregion

        //        }

        //        kplog.Info("Response sent for pnm_order_identifier: " + pnmOrderIdentifier + ", site_order_identifier: " + identifier);

        //        /* 
        //         * Now that you have responded to a /confirm, you need to keep a record
        //           of this pnm_payment_identifier and DO NOT POST any other /confirm 
        //           requests for that pnm_payment_identifier, however you  should still
        //           respond to all confirm requests, even duplicates.
        //        */



        //        ConfirmationResponsebuilder builder = new ConfirmationResponsebuilder("2.0");
        //        builder.PnmOrderIdentifier = pnmOrderIdentifier;
        //        kplog.Debug("End handleConfirmationRequest");
        //        return Ok(builder.Build());

        //    }
        //    catch (RequestException ex)
        //    {
        //        kplog.Fatal(ex.Message);
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception err)
        //    {
        //        kplog.Fatal(err.Message);
        //        return BadRequest(err.Message);
        //    }


        //}

    //Mobile Token
       

       [HttpGet]
       public List<System.Web.Mvc.SelectListItem> getIDTypes()
       {
           using (MySqlConnection con = new MySqlConnection(connection))
           {
               con.Open();
               con.CreateCommand();
               List<System.Web.Mvc.SelectListItem> list = new List<System.Web.Mvc.SelectListItem>();
               using (MySqlCommand cmd = con.CreateCommand())
               {
                   cmd.CommandText = "SELECT * FROM kpformsglobal.`sysallowedidtype` WHERE zonecode = 3 AND idType != 'VOTER`S ID';";
                   MySqlDataReader rdr = cmd.ExecuteReader();
                   if (rdr.HasRows)
                   {
                       list.Add(new System.Web.Mvc.SelectListItem
                       {
                           Value = "",
                           Text = "--SELECT--"
                       });

                       while (rdr.Read())
                       {
                           list.Add(new System.Web.Mvc.SelectListItem
                           {
                               Value = rdr["idType"].ToString(),
                               Text = rdr["idType"].ToString()
                           });
                       }
                       rdr.Close();
                       con.Close();
                       return list;
                   }
                   return null;
               }
           }

       }

        [HttpGet]
       public Double getDailyLimit(String CustID) 
       {
           using (MySqlConnection con = new MySqlConnection(connection)) 
           {

               DateTime dt = getServerDateGlobal();
               String table = dt.ToString("MM");
               Double limitSO = dailyLimit;
               Double dailySum = 0.0;
               con.Open();
            using(MySqlCommand cmd = con.CreateCommand())
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "Select IF(SUM(orderPrincipal) is not null,SUM(orderPrincipal),0.00) as limitSO from paynearme.order" + table + " where DATE_FORMAT(OrderCreated,'%Y-%m-%d') = DATE_FORMAT(Now(),'%Y-%m-%d')  and senderIdentifier = @custID;";
                cmd.Parameters.AddWithValue("custID", CustID);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    rdr.Read();
                    dailySum = Convert.ToDouble(rdr["limitSO"]);
                    limitSO = limitSO - dailySum;
                    if (limitSO < 0)
                    {
                        return 0.00;
                    }
                    else
                    {
                        return limitSO;
                    }
                }
                else 
                {
                    return limitSO;
                }
            }
           }
         
       }

        [HttpGet]
       public Double getMonthlyLimit(String CustID) 
       {
           using (MySqlConnection con = new MySqlConnection(connection))
           {

               DateTime dt = getServerDateGlobal();
               String table = dt.ToString("MM");
               Double limitMonthly = monthlyLimit;
               Double sumDaily = 0.0;
               Double sumPartial =  0.0;
               con.Open();
               using (MySqlCommand cmd = con.CreateCommand())
               {
                   cmd.Parameters.Clear();
                   cmd.CommandText = "Select IF(SUM(orderPrincipal) is not null,SUM(orderPrincipal),0.00) as monthlySum  from paynearme.order" + table + " where `status` = 'confirm' and DATE_FORMAT(OrderCreated,'%Y-%m-%d') != DATE_FORMAT(NOW(),'%Y-%m-%d') and senderIdentifier = @custID ;";
                   cmd.Parameters.AddWithValue("custID", CustID);
                   MySqlDataReader rdr = cmd.ExecuteReader();
                   rdr.Read();
                   sumPartial = Convert.ToDouble(rdr["monthlySum"]);

                   if (sumPartial > 0)
                   {
                    
                       
                       rdr.Close();
                       cmd.Parameters.Clear();
                       cmd.CommandText = "Select IF(SUM(orderPrincipal) is not null,SUM(orderPrincipal),0.00) as sumDaily from paynearme.order" + table + " where DATE_FORMAT(OrderCreated,'%Y-%m-%d') = DATE_FORMAT(Now(),'%Y-%m-%d') and senderIdentifier = @custID;";
                       cmd.Parameters.AddWithValue("custID", CustID);
                       rdr = cmd.ExecuteReader();
                       rdr.Read();
                       sumDaily = Convert.ToDouble(rdr["sumDaily"]);
                       if (sumDaily > 0)
                       {

                           limitMonthly = limitMonthly - (sumDaily + sumPartial);

                           if (limitMonthly < 0) { limitMonthly = 0.00; }
                           return limitMonthly;



                       }
                       else 
                       {
                           limitMonthly = limitMonthly - sumPartial;
                           if (limitMonthly < 0) { limitMonthly = 0.00; }
                           return limitMonthly;
                       }

                   }
                   else
                   {
                       rdr.Close();
                       cmd.Parameters.Clear();
                       cmd.CommandText = "Select IF(SUM(orderPrincipal) is not null,SUM(orderPrincipal),0.00) as sumDaily from paynearme.order" + table + " where DATE_FORMAT(OrderCreated,'%Y-%m-%d') = DATE_FORMAT(Now(),'%Y-%m-%d') and senderIdentifier = @custID;";
                       cmd.Parameters.AddWithValue("custID", CustID);
                       rdr = cmd.ExecuteReader();
                       rdr.Read();
                       sumDaily = Convert.ToDouble(rdr["sumDaily"]);
                       if (sumDaily > 0)
                       {
                           

                           limitMonthly = limitMonthly - sumDaily;
                         
                           return limitMonthly;
                       }
                       else 
                       {
                           
                           return limitMonthly;
                       }
                   }
                  
               }
           }
       
       }


        #region private method
        

        [HttpGet]
        public void sendEmailActivation(String userID, String firstName, String activationCode, String mobileToken)
        {

            DateTime dt = getServerDateGlobal(false);
            SmtpClient client = new SmtpClient();
            client.EnableSsl = smtpSsl;
            client.UseDefaultCredentials = false;
            client.Host = smtpServer;
            client.Port = 587;
            client.Credentials = new NetworkCredential(smtpUser, smtpPass);
            MailMessage msg = new MailMessage();

            msg.To.Add(userID);

            msg.From = new MailAddress(smtpSender);
            msg.Subject = "ML Remit - Email Activation";


            //msg.Body = "["+dt.ToString("yyyy-MM-dd HH:mm:ss")+"] Start of message <br /><br />"
            //        + "<img src='https://mlremit.mlhuillier1.com/paynearme/Images/logo_en.png'/><br/>"
            //        + "Good day Ma'am/Sir <b>" + firstName + "</b>,<br />"
            //        + "<br/>With M. Lhuillier it is easy to send money to your friends and family around "
            //        + "different parts of the world in a fast, convenient and secure way.<br />"
            //        + "Please confirm your e-mail address to activate <br/> M. Lhuillier account. <br/>"
            //        + "Let's activate your account! <br />"
            //        + "Activation Code : <b>" + activationCode + "</b>"
            //        + "<br />"
            //        + "<br />This mail is auto generated. Please do not reply. <br /><br />"
            //        + "["+dt.ToString("yyyy-MM-dd HH:mm:ss")+"] End of message <br />";

            msg.Body = "Good day Ma'am/Sir " + firstName + ",<br /><br />"
                  + "With Mlhuillier as easy to send money to your friends and family around<br />"
                  + "different parts of the world in a fast, convenient and secure way.<br /><br />"
                  + "Please confirm your e-mail address to activate Mlhuillier account.<br /><br />"
                  + "Activation Code : " + activationCode + " <br /><br />"
                  + "This mail is auto generated. Please do not reply. <br /><br />";
                  
                    

            //msg.Body = "Good day Ma'am/Sir " + firstName + ",<br /><br />"
            //         + "With Mlhuillier as easy to send money to your friends and family around<br />"
            //         + "different parts of the world in a fast, convenient and secure way.<br /><br />"
            //         + "Please confirm your e-mail address to activate Mlhuillier account.<br /><br />"
            //         + "Activation Code : <b>" + activationCode + "</b> <br /><br />";
            
            msg.IsBodyHtml = true;
            try
            {
                client.Send(msg);
                kplog.Info("Sent: " + JsonConvert.SerializeObject(msg));

            }
            catch (Exception err)
            {
                kplog.Error(err.ToString());

            }
        }

        private void uploadFileImage(string strBase64, string filepath)
        {
            try
            {
                byte[] byteresponse = Convert.FromBase64String(strBase64);
                Stream stream2 = new MemoryStream(byteresponse);
                Uri path = new Uri(filepath);
                FtpWebRequest req = (FtpWebRequest)WebRequest.Create(path);
                req.UseBinary = true;
                req.Method = WebRequestMethods.Ftp.UploadFile;
                req.Credentials = new NetworkCredential(ftpUser, ftpPass);
                req.ContentLength = stream2.Length;
                Stream reqStream = req.GetRequestStream();
                stream2.Seek(0, SeekOrigin.Begin);
                stream2.CopyTo(reqStream);
                reqStream.Close();
            }
            catch (Exception ex)
            {

                kplog.Error("FILE UPLOAD ----------> " + ex.ToString());
            }



        }

        //private SendoutResponse sendoutGlobal(SendoutModel model)
        //{

        //    try
        //    {

        //        if (!authenticate(model.Username, model.Password))
        //        {
        //            kplog.Error("FAILED:: respcode: 7 message: " + getRespMessage(7));
        //            return new SendoutResponse { respcode = 7, message = getRespMessage(7) };
        //        }


        //        CustomerModel sender = getProfile(model.transaction.senderCustID).sender;
        //        BeneficiaryModel receiver = getBeneficiaryInfo(model.transaction.receiverCustId).data;

        //        int xsave = 0;

        //        DateTime dt = getServerDateGlobal();
        //        int sr = Convert.ToInt32(model.series);
        //        String month = dt.ToString("MM");
        //        String tblorig = "sendout" + month + dt.ToString("dd");


        //        String controlno = model.transaction.controlNo;
        //        String OperatorID = model.OperatorID;
        //        String station = model.station;
        //        String IsRemote = string.Empty;
        //        String RemoteBranch = string.Empty;
        //        String RemoteOperatorID = string.Empty;
        //        String RemoteReason = string.Empty;
        //        String RemoteBranchCode = string.Empty;
        //        Int32 remotezcode = 0;
        //        Int32 type = model.type;


        //        String ispassword = string.Empty;
        //        String transpassword = string.Empty;
        //        String purpose = string.Empty;
        //        String syscreatr = model.syscreator;
        //        String source = string.Empty;
        //        String currency = model.Currency;
        //        Double principal = model.transaction.Principal;
        //        Double charge = model.transaction.Charge;
        //        Double othercharge = 0.00;
        //        Double redeem = 0.00;
        //        Double total = model.transaction.Total;
        //        String promo = string.Empty;
        //        String relation = receiver.relation;
        //        String message = string.Empty;
        //        String idtype = string.Empty;
        //        String idno = string.Empty;
        //        String pocurrency = model.pocurrency;
        //        String paymenttype = model.paymenttype;
        //        String bankname = string.Empty;
        //        String cardcheckno = string.Empty;
        //        String cardcheckexpdate = string.Empty;
        //        Double exchangerate = model.transaction.ExchangeRate;
        //        Double poamount = model.transaction.POAmountPHP;
        //        String trxntype = model.trxntype;
        //        Int32 zonecode = model.zonecode;
        //        String bcode = model.branchcode;
        //        String orno = String.Empty;

        //        String SenderFName = sender.firstName;
        //        String SenderLName = sender.lastName;
        //        String SenderMName = sender.middleName;
        //        String SenderName = SenderLName + ", " + SenderFName + " " + SenderMName;
        //        String SenderStreet = sender.Street;
        //        String SenderProvinceCity = sender.City;
        //        String SenderCountry = sender.Country;
        //        String SenderGender = sender.Gender;
        //        String SenderContactNo = sender.PhoneNo;
        //        Int32 SenderIsSMS = 0;
        //        String SenderBirthDate = Convert.ToDateTime(sender.BirthDate).ToString("yyyy-MM-dd");
        //        String SenderBranchID = "";

        //        String ReceiverFName = receiver.firstName;
        //        String ReceiverLName = receiver.lastname;
        //        String ReceiverMName = receiver.midlleName;
        //        String ReceiverName = ReceiverLName + ", " + ReceiverFName + " " + ReceiverMName;
        //        String ReceiverStreet = receiver.street;
        //        String ReceiverProvinceCity = receiver.city;
        //        String ReceiverCountry = receiver.country;
        //        String ReceiverGender = receiver.gender;
        //        String ReceiverBirthDate = Convert.ToDateTime(receiver.dateOfBirth).ToString("yyyy-MM-dd");
        //        String ReceiverContactNo = receiver.phoneNo;



        //        using (MySqlConnection checkinglang = new MySqlConnection(connection))
        //        {
        //            checkinglang.Open();
        //            Int32 maxontrans = 0;
        //            try
        //            {
        //                using (command = checkinglang.CreateCommand())
        //                {
        //                    string checkifcontrolexist = "select controlno from " + generateTableNameGlobal(0, dt.ToString()) + " where controlno=@controlno";
        //                    command.CommandTimeout = 0;
        //                    command.CommandText = checkifcontrolexist;
        //                    command.Parameters.AddWithValue("controlno", controlno);
        //                    MySqlDataReader controlexistreader = command.ExecuteReader();
        //                    if (controlexistreader.HasRows)
        //                    {
        //                        controlexistreader.Close();

        //                        string getcontrolmax = "select max(substring(controlno,length(controlno)-5,length(controlno))) as max from " + generateTableNameGlobal(0, dt.ToString()) + " where if(isremote=1,remotebranch,branchcode) = @branchcode and stationid = @stationid and if(remotezonecode=0 or remotezonecode is null, zonecode,remotezonecode) =@zonecode";
        //                        command.CommandText = getcontrolmax;
        //                        command.Parameters.Clear();
        //                        command.Parameters.AddWithValue("branchcode", bcode);
        //                        command.Parameters.AddWithValue("stationid", station);
        //                        command.Parameters.AddWithValue("zonecode", zonecode);

        //                        MySqlDataReader controlmaxreader = command.ExecuteReader();
        //                        if (controlmaxreader.Read())
        //                        {
        //                            sr = Convert.ToInt32(controlmaxreader["max"].ToString()) + 1;
        //                        }
        //                        controlmaxreader.Close();

        //                        command.CommandText = "update kpformsglobal.control set nseries = @series where bcode = @bcode and station = @st and zcode = @zcode and type = @tp";
        //                        command.Parameters.Clear();
        //                        command.Parameters.AddWithValue("st", station);
        //                        command.Parameters.AddWithValue("bcode", bcode);
        //                        command.Parameters.AddWithValue("series", sr + 1 > 999999 ? 000001 : sr + 1);
        //                        command.Parameters.AddWithValue("zcode", zonecode);
        //                        command.Parameters.AddWithValue("tp", type);
        //                        int abc101 = command.ExecuteNonQuery();

        //                        String xst = String.Empty;
        //                        String xbcode = String.Empty;
        //                        Int32 xzcode = 0;

        //                        xst = station;
        //                        xbcode = bcode;
        //                        xzcode = zonecode;

        //                        kplog.Info("UPDATE kpformsglobal.control:: series: " + sr + " WHERE bcode: " + xbcode + " st: " + xst + " zcode: " + xzcode + " tp: " + type);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                kplog.Error("FAILED:: ErrorDetail: " + ex.ToString());
        //                throw new Exception("Error sa pg-update sa control!: " + ex.ToString() + "max:" + maxontrans + 1);
        //            }

        //        }


        //        StringBuilder query = new StringBuilder("Insert into " + generateTableNameGlobal(0, dt.ToString()) + "(");
        //        List<string> li = new List<string>();
        //        List<string> param = new List<string>();

        //        param.Add("ControlNo");
        //        param.Add("OperatorID");
        //        param.Add("StationID");
        //        param.Add("IsRemote");
        //        param.Add("RemoteBranch");
        //        param.Add("RemoteOperatorID");
        //        param.Add("Reason");
        //        param.Add("IsPassword");
        //        param.Add("TransPassword");
        //        param.Add("Purpose");
        //        param.Add("syscreator");
        //        param.Add("Source");
        //        param.Add("Currency");
        //        param.Add("Principal");
        //        param.Add("Charge");
        //        param.Add("OtherCharge");
        //        param.Add("Redeem");
        //        param.Add("Total");
        //        param.Add("Promo");
        //        param.Add("Relation");
        //        param.Add("Message");
        //        param.Add("IDType");
        //        param.Add("IDNo");
        //        param.Add("PreferredCurrency");
        //        param.Add("PaymentType");
        //        param.Add("BankName");
        //        param.Add("CardCheckNo");
        //        param.Add("CardCheckExpDate");
        //        param.Add("ExchangeRate");
        //        param.Add("AmountPO");
        //        param.Add("TransType");
        //        param.Add("isClaimed");
        //        param.Add("IsCancelled");
        //        param.Add("KPTNNo");
        //        param.Add("ORNo");
        //        param.Add("syscreated");
        //        param.Add("BranchCode");
        //        param.Add("ZoneCode");
        //        param.Add("TransDate");
        //        param.Add("ExpiryDate");
        //        param.Add("SenderMLCardNo");
        //        param.Add("SenderFName");
        //        param.Add("SenderLName");
        //        param.Add("SenderMName");
        //        param.Add("SenderName");
        //        param.Add("SenderStreet");
        //        param.Add("SenderProvinceCity");
        //        param.Add("SenderCountry");
        //        param.Add("SenderGender");
        //        param.Add("SenderContactNo");
        //        param.Add("SenderBirthDate");
        //        param.Add("SenderBranchID");
        //        param.Add("ReceiverFName");
        //        param.Add("ReceiverLName");
        //        param.Add("ReceiverMName");
        //        param.Add("ReceiverName");
        //        param.Add("ReceiverStreet");
        //        param.Add("ReceiverProvinceCity");
        //        param.Add("ReceiverCountry");
        //        param.Add("ReceiverGender");
        //        param.Add("ReceiverContactNo");
        //        param.Add("ReceiverBirthDate");
        //        param.Add("SenderIsSMS");
        //        param.Add("RemoteZoneCode");
        //        param.Add("vat");
        //        param.Add("SenderCustID");


        //        using (MySqlConnection conn = new MySqlConnection(connection))
        //        {
        //            conn.Open();

        //            MySqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        //            try
        //            {
        //                using (command = conn.CreateCommand())
        //                {
        //                    command.CommandText = "SET autocommit=0;";
        //                    command.ExecuteNonQuery();

        //                    command.Transaction = trans;

        //                    for (var f = 0; f < param.Count; f++)
        //                    {
        //                        query.Append("`").Append(param[f]).Append("`");
        //                        if ((f + 1) != param.Count)
        //                        {
        //                            query.Append(",");
        //                        }
        //                        li.Add(param[f]);

        //                    }
        //                    query.Append(") values ( ");

        //                    for (var f = 0; f < param.Count; f++)
        //                    {
        //                        query.Append("@").Append(param[f]);
        //                        if ((f + 1) != param.Count)
        //                        {
        //                            query.Append(", ");
        //                        }
        //                        li.Add(param[f]);

        //                    }
        //                    query.Append(")");
        //                }

        //                using (command = conn.CreateCommand())
        //                {
        //                    //13 14 17
        //                    if (Convert.ToDouble(principal) == 0 || Convert.ToDouble(charge) == 0 || Convert.ToDouble(total) == 0)
        //                    {
        //                        kplog.Error("Error in data:: respcode: 15 message: " + getRespMessage(15));
        //                        return new SendoutResponse { respcode = 15, message = getRespMessage(15) };
        //                    }


        //                    RemoteReason = null;

        //                    orno = generateResiboGlobal(bcode, zonecode, command);


        //                    command.CommandText = query.ToString();
        //                    command.Parameters.AddWithValue("ControlNo", controlno);
        //                    command.Parameters.AddWithValue("OperatorID", OperatorID);
        //                    command.Parameters.AddWithValue("StationID", string.Empty);
        //                    command.Parameters.AddWithValue("IsRemote", string.Empty);
        //                    command.Parameters.AddWithValue("RemoteBranch", string.Empty);
        //                    command.Parameters.AddWithValue("RemoteOperatorID", string.Empty);
        //                    command.Parameters.AddWithValue("Reason", string.Empty);
        //                    command.Parameters.AddWithValue("IsPassword", string.Empty);
        //                    command.Parameters.AddWithValue("TransPassword", string.Empty);
        //                    command.Parameters.AddWithValue("Purpose", string.Empty);
        //                    command.Parameters.AddWithValue("syscreator", syscreatr);
        //                    command.Parameters.AddWithValue("Source", string.Empty);
        //                    command.Parameters.AddWithValue("Currency", currency);
        //                    command.Parameters.AddWithValue("Principal", principal);
        //                    command.Parameters.AddWithValue("Charge", charge);
        //                    command.Parameters.AddWithValue("OtherCharge", othercharge);
        //                    command.Parameters.AddWithValue("Redeem", redeem);
        //                    command.Parameters.AddWithValue("Total", total);
        //                    command.Parameters.AddWithValue("Promo", promo);
        //                    command.Parameters.AddWithValue("Relation", relation);
        //                    command.Parameters.AddWithValue("Message", message);
        //                    command.Parameters.AddWithValue("IDType", idtype);
        //                    command.Parameters.AddWithValue("IDNo", idno);
        //                    command.Parameters.AddWithValue("PreferredCurrency", pocurrency);
        //                    command.Parameters.AddWithValue("PaymentType", paymenttype);
        //                    command.Parameters.AddWithValue("BankName", bankname);
        //                    command.Parameters.AddWithValue("CardCheckNo", cardcheckno);
        //                    command.Parameters.AddWithValue("CardCheckExpDate", cardcheckexpdate);
        //                    command.Parameters.AddWithValue("ExchangeRate", exchangerate);
        //                    command.Parameters.AddWithValue("AmountPO", poamount);
        //                    command.Parameters.AddWithValue("TransType", trxntype);
        //                    command.Parameters.AddWithValue("IsClaimed", 0);
        //                    command.Parameters.AddWithValue("IsCancelled", 0);
        //                    command.Parameters.AddWithValue("ORNo", orno);
        //                    command.Parameters.AddWithValue("KPTNNo", model.transaction.KPTN);
        //                    command.Parameters.AddWithValue("syscreated", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                    command.Parameters.AddWithValue("BranchCode", bcode);
        //                    command.Parameters.AddWithValue("ZoneCode", zonecode);
        //                    command.Parameters.AddWithValue("TransDate", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                    command.Parameters.AddWithValue("ExpiryDate", String.Empty);
        //                    command.Parameters.AddWithValue("SenderMLCardNO", String.Empty);
        //                    command.Parameters.AddWithValue("SenderFName", SenderFName);
        //                    command.Parameters.AddWithValue("SenderLName", SenderLName);
        //                    command.Parameters.AddWithValue("SenderMName", SenderMName);
        //                    command.Parameters.AddWithValue("SenderName", SenderLName + ", " + SenderFName + " " + SenderMName);
        //                    command.Parameters.AddWithValue("SenderStreet", SenderStreet);
        //                    command.Parameters.AddWithValue("SenderProvinceCity", SenderProvinceCity);
        //                    command.Parameters.AddWithValue("SenderCountry", SenderCountry);
        //                    command.Parameters.AddWithValue("SenderGender", SenderGender);
        //                    command.Parameters.AddWithValue("SenderContactNo", SenderContactNo);
        //                    command.Parameters.AddWithValue("SenderIsSMS", SenderIsSMS);
        //                    command.Parameters.AddWithValue("SenderBirthdate", SenderBirthDate);
        //                    command.Parameters.AddWithValue("SenderBranchID", SenderBranchID);
        //                    command.Parameters.AddWithValue("ReceiverFName", ReceiverFName);
        //                    command.Parameters.AddWithValue("ReceiverLName", ReceiverLName);
        //                    command.Parameters.AddWithValue("ReceiverMName", ReceiverMName);
        //                    command.Parameters.AddWithValue("ReceiverName", ReceiverLName + ", " + ReceiverFName + " " + ReceiverMName);
        //                    command.Parameters.AddWithValue("ReceiverStreet", ReceiverStreet);
        //                    command.Parameters.AddWithValue("ReceiverProvinceCity", ReceiverProvinceCity);
        //                    command.Parameters.AddWithValue("ReceiverCountry", ReceiverCountry);
        //                    command.Parameters.AddWithValue("ReceiverGender", ReceiverGender);
        //                    command.Parameters.AddWithValue("ReceiverContactNo", ReceiverContactNo);
        //                    command.Parameters.AddWithValue("ReceiverBirthdate", ReceiverBirthDate);
        //                    command.Parameters.AddWithValue("RemoteZoneCode", remotezcode);
        //                    command.Parameters.AddWithValue("vat", model.transaction.vat);
        //                    command.Parameters.AddWithValue("SenderCustID", sender.CustomerID);

        //                    try
        //                    {
        //                        xsave = command.ExecuteNonQuery();

        //                        command.Parameters.Clear();
        //                        command.CommandText = "UPDATE paynearme.payment" + month + " SET isPaid = 1 where pnmPaymentIdentifier = @pnmPaymentIdentifier";
        //                        command.Parameters.AddWithValue("pnmPaymentIdentifier", model.transaction.PaymentIdentifier);
        //                        int x1 = command.ExecuteNonQuery();

        //                        command.Parameters.Clear();
        //                        command.CommandText = "UPDATE paynearme.order" + month + " SET status = 'confirm' where siteOrderIdentifier = @siteOrderIdentifier";
        //                        command.Parameters.AddWithValue("siteOrderIdentifier", model.transaction.KPTN);
        //                        int x2 = command.ExecuteNonQuery();

        //                        if (xsave < 1 || x1 < 1 || x2 < 1)
        //                        {
        //                            trans.Rollback();
        //                            kplog.Error("Error in saving " + generateTableNameGlobal(0, null) + ":: respcode: 12 message: " + getRespMessage(12) + "ErrorDetail: Review parameters transStatus: Rollback");

        //                            return new SendoutResponse { respcode = 12, message = getRespMessage(12), ErrorDetail = "Review paramerters." };
        //                        }
        //                        else
        //                        {
        //                            using (command = conn.CreateCommand())
        //                            {

        //                                kplog.Info("INSERT INTO " + generateTableNameGlobal(0, dt.ToString()));


        //                                dt = getServerDateGlobal();

        //                                String insert = "Insert into kptransactionsglobal.sendout" + month + " (controlno,kptnno,orno,operatorid," +
        //                                "stationid,isremote,remotebranch,remoteoperatorid,reason,ispassword,transpassword,purpose,isclaimed,iscancelled," +
        //                                "syscreated,syscreator,source,currency,principal,charge,othercharge,redeem,total,promo,senderissms,relation,message," +
        //                                "idtype,idno,expirydate,branchcode,zonecode,transdate,sendermlcardno,senderfname,senderlname,sendermname,sendername," +
        //                                "senderstreet,senderprovincecity,sendercountry,sendergender,sendercontactno,senderbirthdate,senderbranchid," +
        //                                "receiverfname,receiverlname,receivermname,receivername,receiverstreet,receiverprovincecity,receivercountry," +
        //                                "receivergender,receivercontactno,receiverbirthdate,vat,remotezonecode,tableoriginated,`year`,pocurrency,poamount,ExchangeRate," +
        //                                "paymenttype,bankname,cardcheckno,cardcheckexpdate,TransType) values (@controlno,@kptnno,@orno,@operatorid," +
        //                                "@stationid,@isremote,@remotebranch,@remoteoperatorid,@reason,@ispassword,@transpassword,@purpose,@isclaimed,@iscancelled," +
        //                                "@syscreated,@syscreator,@source,@currency,@principal,@charge,@othercharge,@redeem,@total,@promo,@senderissms,@relation,@message," +
        //                                "@idtype,@idno,@expirydate,@branchcode,@zonecode,@transdate,@sendermlcardno,@senderfname,@senderlname,@sendermname,@sendername," +
        //                                "@senderstreet,@senderprovincecity,@sendercountry,@sendergender,@sendercontactno,@senderbirthdate,@senderbranchid," +
        //                                "@receiverfname,@receiverlname,@receivermname,@receivername,@receiverstreet,@receiverprovincecity,@receivercountry," +
        //                                "@receivergender,@receivercontactno,@receiverbirthdate,@vat,@remotezonecode,@tableoriginated,@yr,@pocurrency,@poamount,@exchangerate," +
        //                                "@paymenttype,@bankname,@cardcheckno,@cardcheckexpdate,@transtype)";
        //                                command.CommandText = insert;

        //                                command.Parameters.AddWithValue("controlno", controlno);
        //                                command.Parameters.AddWithValue("kptnno", model.transaction.KPTN);
        //                                command.Parameters.AddWithValue("orno", orno);
        //                                command.Parameters.AddWithValue("operatorid", OperatorID);
        //                                command.Parameters.AddWithValue("stationid", station);
        //                                command.Parameters.AddWithValue("isremote", IsRemote);
        //                                command.Parameters.AddWithValue("remotebranch", RemoteBranch);
        //                                command.Parameters.AddWithValue("remoteoperatorid", RemoteOperatorID);
        //                                command.Parameters.AddWithValue("reason", RemoteReason);
        //                                command.Parameters.AddWithValue("ispassword", ispassword);
        //                                command.Parameters.AddWithValue("transpassword", transpassword);
        //                                command.Parameters.AddWithValue("purpose", purpose);
        //                                command.Parameters.AddWithValue("isclaimed", 0);
        //                                command.Parameters.AddWithValue("iscancelled", 0);
        //                                command.Parameters.AddWithValue("syscreated", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                                command.Parameters.AddWithValue("syscreator", syscreatr);
        //                                command.Parameters.AddWithValue("source", source);
        //                                command.Parameters.AddWithValue("currency", currency);
        //                                command.Parameters.AddWithValue("principal", principal);
        //                                command.Parameters.AddWithValue("charge", charge);
        //                                command.Parameters.AddWithValue("othercharge", othercharge);
        //                                command.Parameters.AddWithValue("redeem", redeem);
        //                                command.Parameters.AddWithValue("total", total);
        //                                command.Parameters.AddWithValue("promo", promo);
        //                                command.Parameters.AddWithValue("senderissms", SenderIsSMS);
        //                                command.Parameters.AddWithValue("relation", relation);
        //                                command.Parameters.AddWithValue("message", message);
        //                                command.Parameters.AddWithValue("idtype", idtype);
        //                                command.Parameters.AddWithValue("idno", idno);
        //                                command.Parameters.AddWithValue("expirydate", "");
        //                                command.Parameters.AddWithValue("branchcode", bcode);
        //                                command.Parameters.AddWithValue("zonecode", zonecode);
        //                                command.Parameters.AddWithValue("transdate", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                                command.Parameters.AddWithValue("sendermlcardno", "");
        //                                command.Parameters.AddWithValue("senderfname", SenderFName);
        //                                command.Parameters.AddWithValue("senderlname", SenderLName);
        //                                command.Parameters.AddWithValue("sendermname", SenderMName);
        //                                command.Parameters.AddWithValue("sendername", SenderLName + ", " + SenderFName + " " + SenderMName);
        //                                command.Parameters.AddWithValue("senderstreet", SenderStreet);
        //                                command.Parameters.AddWithValue("senderprovincecity", SenderProvinceCity);
        //                                command.Parameters.AddWithValue("sendercountry", SenderCountry);
        //                                command.Parameters.AddWithValue("sendergender", SenderGender);
        //                                command.Parameters.AddWithValue("sendercontactno", SenderContactNo);
        //                                command.Parameters.AddWithValue("senderbirthdate", SenderBirthDate);
        //                                command.Parameters.AddWithValue("senderbranchid", SenderBranchID);
        //                                command.Parameters.AddWithValue("receiverfname", ReceiverFName);
        //                                command.Parameters.AddWithValue("receiverlname", ReceiverLName);
        //                                command.Parameters.AddWithValue("receivermname", ReceiverMName);
        //                                command.Parameters.AddWithValue("receivername", ReceiverLName + ", " + ReceiverFName + " " + ReceiverMName);
        //                                command.Parameters.AddWithValue("receiverstreet", ReceiverStreet);
        //                                command.Parameters.AddWithValue("receiverprovincecity", ReceiverProvinceCity);
        //                                command.Parameters.AddWithValue("receivercountry", ReceiverCountry);
        //                                command.Parameters.AddWithValue("receivergender", ReceiverGender);
        //                                command.Parameters.AddWithValue("receivercontactno", ReceiverContactNo);
        //                                command.Parameters.AddWithValue("receiverbirthdate", ReceiverBirthDate);
        //                                command.Parameters.AddWithValue("chargeto", " ");
        //                                command.Parameters.AddWithValue("vat", model.transaction.vat);
        //                                command.Parameters.AddWithValue("remotezonecode", remotezcode);
        //                                command.Parameters.AddWithValue("tableoriginated", tblorig);
        //                                command.Parameters.AddWithValue("yr", dt.ToString("yyyy"));
        //                                command.Parameters.AddWithValue("pocurrency", pocurrency);
        //                                command.Parameters.AddWithValue("poamount", poamount);
        //                                command.Parameters.AddWithValue("exchangerate", exchangerate);
        //                                command.Parameters.AddWithValue("paymenttype", paymenttype);
        //                                command.Parameters.AddWithValue("bankname", bankname);
        //                                command.Parameters.AddWithValue("cardcheckno", cardcheckno);
        //                                command.Parameters.AddWithValue("cardcheckexpdate", cardcheckexpdate);
        //                                command.Parameters.AddWithValue("transtype", trxntype);
        //                                command.ExecuteNonQuery();







        //                            }
        //                        }
        //                    }
        //                    catch (MySqlException myyyx)
        //                    {
        //                        //if (myyyx.Message.Contains("Duplicate"))
        //                        if (myyyx.Number == 1062)
        //                        {
        //                            command.Parameters.Clear();

        //                            command.CommandText = "update kpformsglobal.control set nseries = @series where bcode = @bcode and station = @st and zcode = @zcode and type = @tp";
        //                            command.Parameters.AddWithValue("st", station);
        //                            command.Parameters.AddWithValue("bcode", bcode);
        //                            command.Parameters.AddWithValue("series", sr + 1 > 999999 ? 000001 : sr + 1);
        //                            command.Parameters.AddWithValue("zcode", zonecode);
        //                            command.Parameters.AddWithValue("tp", type);
        //                            command.ExecuteNonQuery();
        //                            int x = command.ExecuteNonQuery();
        //                            if (x < 1)
        //                            {
        //                                kplog.Error("IsRemote: 0:: Error in updating control:: respcode: 12 message: " + getRespMessage(12) + " ErrorDetail: Review parameters.");
        //                                kplog.Error("FAILED:: MySql ErrorCode: " + myyyx.Number + " ErrorDetail: " + myyyx.ToString() + " transStatus: Rollback");

        //                                trans.Rollback();

        //                                return new SendoutResponse { respcode = 12, message = getRespMessage(12), ErrorDetail = "Review parameters." };
        //                            }


        //                            kplog.Error("Error in saving transaction:: respcode: 13 message: Problem saving transaction. Please close the sendout window and open again. Thank you. ErrorDetail: Review parameters.");
        //                            kplog.Error("FAILED:: MySql ErrorCode: " + myyyx.Number + " ErrorDetail: " + myyyx.ToString() + " transStatus: Commit");

        //                            trans.Commit();
        //                            conn.Close();
        //                            //return new SendoutResponse { respcode = 13, message = "Problem saving transaction. Please close the sendout window and open again. Thank you.", ErrorDetail = "Review parameters." };
        //                            throw new RequestException("Problem saving transaction, Retry", 400);
        //                        }
        //                        else
        //                        {
        //                            if (myyyx.Number == 1213)
        //                            {
        //                                kplog.Error("FAILED:: respcode: 11 message: " + getRespMessage(11) + " MySql ErrorCode: " + myyyx.Number + " ErrorDetail: " + myyyx.ToString() + " transStatus: Rollback");

        //                                trans.Rollback();

        //                                throw new RequestException("Problem saving transaction, Retry", 400);
        //                            }
        //                            else
        //                            {
        //                                kplog.Error("FAILED:: respcode: 0 message: " + getRespMessage(0) + " MySql ErrorCode: " + myyyx.Number + " ErrorDetail: " + myyyx.ToString() + " transStatus: Rollback");

        //                                trans.Rollback();

        //                                throw new RequestException("Problem saving transaction, Retry", 400);
        //                            }
        //                        }
        //                    }


        //                    //command.CommandText = "update kpformsglobal.control set nseries = @series where bcode = @bcode and station = @st and zcode = @zcode and type = @tp";
        //                    //command.Parameters.AddWithValue("st", station);
        //                    //command.Parameters.AddWithValue("bcode", bcode);
        //                    ////command.Parameters.AddWithValue("series", sr + 1);
        //                    //command.Parameters.AddWithValue("series", sr + 1 > 999999 ? 000001 : sr + 1);
        //                    //command.Parameters.AddWithValue("zcode", zonecode);
        //                    //command.Parameters.AddWithValue("tp", type);
        //                    //command.ExecuteNonQuery();
        //                    //int x1 = command.ExecuteNonQuery();
        //                    //if (x1 < 1)
        //                    //{
        //                    //    kplog.Error("IsRemote: 0:: Error in updating control:: respcode: 12 message: " + getRespMessage(12) + " ErrorDetail: Review parameters. transStatus: Rollback");
        //                    //    trans.Rollback();
        //                    //    conn.Close();
        //                    //    return new SendoutResponse { respcode = 12, message = getRespMessage(12), ErrorDetail = "Review parameters." };
        //                    //}


        //                    updateResiboGlobal(bcode, zonecode, orno, ref command);


        //                    if (xsave == 1)
        //                    {
        //                        String custS = getcustomertable(SenderLName);
        //                        command.Parameters.Clear();
        //                        command.CommandText = "kpadminlogsglobal.save_customers";
        //                        command.CommandType = CommandType.StoredProcedure;
        //                        command.Parameters.AddWithValue("tblcustomer", custS);
        //                        command.Parameters.AddWithValue("kptnno", model.transaction.KPTN);
        //                        command.Parameters.AddWithValue("controlno", controlno);
        //                        command.Parameters.AddWithValue("transdate", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                        command.Parameters.AddWithValue("fname", SenderFName);
        //                        command.Parameters.AddWithValue("lname", SenderLName);
        //                        command.Parameters.AddWithValue("mname", SenderMName);
        //                        command.Parameters.AddWithValue("sobranch", SenderBranchID);
        //                        command.Parameters.AddWithValue("pobranch", "");
        //                        command.Parameters.AddWithValue("isremote", IsRemote);
        //                        command.Parameters.AddWithValue("remotebranch", (RemoteBranch.Equals(DBNull.Value) ? null : RemoteBranch));
        //                        command.Parameters.AddWithValue("cancelledbranch", String.Empty);
        //                        command.Parameters.AddWithValue("status", 0);
        //                        command.Parameters.AddWithValue("syscreated", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                        command.Parameters.AddWithValue("syscreator", syscreatr);
        //                        command.Parameters.AddWithValue("customertype", "S");
        //                        command.Parameters.AddWithValue("amount", total);
        //                        command.ExecuteNonQuery();

        //                        String custR = getcustomertable(ReceiverLName);
        //                        command.Parameters.Clear();
        //                        command.CommandText = "kpadminlogsglobal.save_customers";
        //                        command.CommandType = CommandType.StoredProcedure;
        //                        command.Parameters.AddWithValue("tblcustomer", custR);
        //                        command.Parameters.AddWithValue("kptnno", model.transaction.KPTN);
        //                        command.Parameters.AddWithValue("controlno", controlno);
        //                        command.Parameters.AddWithValue("transdate", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                        command.Parameters.AddWithValue("fname", ReceiverFName);
        //                        command.Parameters.AddWithValue("lname", ReceiverLName);
        //                        command.Parameters.AddWithValue("mname", ReceiverMName);
        //                        command.Parameters.AddWithValue("sobranch", SenderBranchID);
        //                        command.Parameters.AddWithValue("pobranch", "");
        //                        command.Parameters.AddWithValue("isremote", IsRemote);
        //                        command.Parameters.AddWithValue("remotebranch", (RemoteBranch.Equals(DBNull.Value) ? null : RemoteBranch));
        //                        command.Parameters.AddWithValue("cancelledbranch", String.Empty);
        //                        command.Parameters.AddWithValue("status", 0);
        //                        command.Parameters.AddWithValue("syscreated", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                        command.Parameters.AddWithValue("syscreator", syscreatr);
        //                        command.Parameters.AddWithValue("customertype", "R");
        //                        command.Parameters.AddWithValue("amount", total);
        //                        command.ExecuteNonQuery();

        //                        command.Parameters.Clear();
        //                        command.CommandText = "kpadminlogsglobal.savelog53";
        //                        command.CommandType = CommandType.StoredProcedure;

        //                        command.Parameters.AddWithValue("kptnno", model.transaction.KPTN);
        //                        command.Parameters.AddWithValue("action", "SENDOUT");
        //                        command.Parameters.AddWithValue("isremote", IsRemote);
        //                        command.Parameters.AddWithValue("txndate", dt);
        //                        command.Parameters.AddWithValue("stationcode", string.Empty);
        //                        command.Parameters.AddWithValue("stationno", station);
        //                        command.Parameters.AddWithValue("zonecode", zonecode);
        //                        command.Parameters.AddWithValue("branchcode", bcode);
        //                        command.Parameters.AddWithValue("operatorid", OperatorID);
        //                        command.Parameters.AddWithValue("cancelledreason", DBNull.Value);
        //                        command.Parameters.AddWithValue("remotereason", RemoteReason);
        //                        command.Parameters.AddWithValue("remotebranch", (RemoteBranchCode.Equals(DBNull.Value)) ? null : RemoteBranchCode);
        //                        command.Parameters.AddWithValue("remoteoperator", (RemoteOperatorID.Equals(DBNull.Value)) ? null : RemoteOperatorID);
        //                        command.Parameters.AddWithValue("oldkptnno", DBNull.Value);
        //                        command.Parameters.AddWithValue("remotezonecode", remotezcode);
        //                        command.Parameters.AddWithValue("type", "N");
        //                        command.ExecuteNonQuery();

        //                        kplog.Info("INSERT SENDER INFO LOGS:: tblcustomer: " + custS + " kptn: " + model.transaction.KPTN + " controlno: " + controlno + " transdate: " + dt.ToString("yyyy-MM-dd HH:mm:ss") + " fname: " + SenderFName + " lname: " + SenderLName + " mname: " + SenderMName + " SObranch: " + SenderBranchID + " pobranch: isremote: " + IsRemote + " remotebranch: " + (RemoteBranch.Equals(DBNull.Value) ? null : RemoteBranch) + " cancelledbranch: status: 0 syscreated: " + dt.ToString("yyyy-MM-dd HH:mm:ss") + " syscreator: " + syscreatr + " customertype: S amount: " + total);
        //                        kplog.Info("INSERT RECEIVER INFO LOGS:: tblcustomer: " + custR + " kptn: " + model.transaction.KPTN + " controlno: " + controlno + " transdate: " + dt.ToString("yyyy-MM-dd HH:mm:ss") + " fname: " + ReceiverFName + " lname: " + ReceiverLName + " mname: " + ReceiverMName + " SObranch: " + SenderBranchID + " pobranch: isremote: " + IsRemote + " remotebranch: " + (RemoteBranch.Equals(DBNull.Value) ? null : RemoteBranch) + " cancelledbranch: status: 0 syscreated: " + dt.ToString("yyyy-MM-dd HH:mm:ss") + " syscreator: " + syscreatr + " customertype: R amount: " + total);
        //                        kplog.Info("INSERT TRANSACTION LOGS:: kptnno: " + model.transaction.KPTN + " action: SENDOUT isremote: " + IsRemote + " txndate: " + dt + " stationcode: " + "" + " stationno: " + station + " zonecode: " + zonecode + " branchcode: " + bcode + " operatorid: " + OperatorID + " cancelledreason: " + DBNull.Value + " remotereason: " + RemoteReason + " remotebranch: " + (RemoteBranchCode.Equals(DBNull.Value) ? null : RemoteBranchCode) + " remoteoperator: " + (RemoteOperatorID.Equals(DBNull.Value) ? null : RemoteOperatorID) + " oldkptnno: " + DBNull.Value + " remotezonecode: " + remotezcode + " type: N");
        //                    }

        //                    kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " kptn: " + model.transaction.KPTN + " orno: " + orno + " transdate: " + dt + " transStatus: Commit");

        //                    trans.Commit();
        //                    conn.Close();
        //                    return new SendoutResponse { respcode = 1, message = getRespMessage(1), kptn = model.transaction.KPTN, orno = orno, transdate = dt };

        //                }

        //            }
        //            catch (MySqlException myx)
        //            {
        //                if (myx.Number == 1213)
        //                {
        //                    kplog.Error("FAILED:: respcode: 11 message: " + getRespMessage(11) + " MySql ErrorCode: " + myx.Number + " ErrorDetail: " + myx.ToString() + " transStatus: Rollback");
        //                    trans.Rollback();
        //                    throw new RequestException("Problem saving transaction, Retry", 400);
        //                }
        //                else
        //                {
        //                    kplog.Error("FAILED:: respcode: 0 message: " + getRespMessage(0) + " MySql ErrorCode: " + myx.Number + " ErrorDetail: " + myx.ToString() + " transStatus: Rollback");
        //                    trans.Rollback();
        //                    throw new RequestException("Problem saving transaction, Retry", 400);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                kplog.Error("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString() + " transStatus: Rollback");
        //                trans.Rollback();
        //                throw new RequestException("Problem saving transaction, Retry", 400);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        kplog.Error("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString() + " transStatus: Rollback");
        //        throw new RequestException("Problem saving transaction, Retry", 400);
        //    }
        //}
        //end Charging
        //private String generateTableNameGlobal(Int32 type, String TransDate)
        //{
        //    //DateTime dt = getServerDate(false);

        //    if (TransDate == null)
        //    {
        //        if (type == 0)
        //        {
        //            //         kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.sendout" : "kpglobal.sendout" + dt.ToString("MM") + dt.ToString("dd")));
        //            return "kpglobal.sendout" + dt.ToString("MM") + dt.ToString("dd");
        //        }
        //        else if (type == 1)
        //        {
        //            //  kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.payout" : "kpglobal.payout" + dt.ToString("MM") + dt.ToString("dd")));
        //            return "kpglobal.payout" + dt.ToString("MM") + dt.ToString("dd");
        //        }
        //        else if (type == 2)
        //        {
        //            //  kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.tempkptn" : "kpglobal.tempkptn"));
        //            return "kpglobal.tempkptn";
        //        }
        //        else
        //        {
        //            kplog.Error("FAILED:: message: Invalid transaction type");
        //            throw new Exception("Invalid transaction type");
        //        }
        //    }
        //    else
        //    {
        //        DateTime TransDatetoDate = Convert.ToDateTime(TransDate);
        //        if (type == 0)
        //        {
        //            //  kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.sendout" : "kpglobal.sendout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd")));
        //            return "kpglobal.sendout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd");
        //        }
        //        else if (type == 1)
        //        {
        //            // kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.payout" : "kpglobal.payout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd")));
        //            return "kpglobal.payout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd");
        //        }
        //        else if (type == 2)
        //        {
        //            //  kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.tempkptn" : "kpglobal.tempkptn"));
        //            return "kpglobal.tempkptn";
        //        }
        //        else
        //        {
        //            kplog.Error("FAILED:: message: Invalid transaction type");
        //            throw new Exception("Invalid transaction type");
        //        }
        //    }
        //}

        //private String generateResiboGlobal(string branchcode, Int32 zonecode, MySqlCommand command)
        //{
        //    try
        //    {

        //        dt = getServerDateGlobal(true);
        //        string query = "select oryear,branchcode,zonecode,series from kpformsglobal.resibo where branchcode = @bcode1 and zonecode = @zcode1 FOR UPDATE";
        //        command.CommandText = query;
        //        command.Parameters.AddWithValue("bcode1", branchcode);
        //        command.Parameters.AddWithValue("zcode1", zonecode);

        //        using (MySqlDataReader dataReader = command.ExecuteReader())
        //        {
        //            if (dataReader.HasRows)
        //            {
        //                dataReader.Read();
        //                Int32 series = Convert.ToInt32(dataReader["series"]) + 1;
        //                String oryear = dataReader["oryear"].ToString().Substring(2);
        //                dataReader.Close();

        //                kplog.Info("Generate receipt:: receipt: " + dt.ToString("yy") + "-" + series.ToString().PadLeft(6, '0'));
        //                kplog.Info("SUCCESS generating receipt");
        //                return dt.ToString("yy") + "-" + series.ToString().PadLeft(6, '0');
        //            }
        //            else
        //            {
        //                dataReader.Close();
        //                command.Parameters.Clear();
        //                command.CommandText = "update kpformsglobal.resibo set `lock` = 1 where branchcode = @bcode2 and zonecode = @zcode2";
        //                command.Parameters.AddWithValue("bcode2", branchcode);
        //                command.Parameters.AddWithValue("zcode2", zonecode);
        //                command.ExecuteNonQuery();

        //                command.Parameters.Clear();
        //                command.CommandText = "insert into kpformsglobal.resibo (oryear, branchcode, zonecode, series) values (@year, @bcode2, @zcode2, @ser)";
        //                command.Parameters.AddWithValue("year", dt.ToString("yyyy"));
        //                command.Parameters.AddWithValue("bcode2", branchcode);
        //                command.Parameters.AddWithValue("zcode2", zonecode);
        //                command.Parameters.AddWithValue("ser", 1);
        //                command.ExecuteNonQuery();
        //                int ser = 1;

        //                kplog.Info("UPDATE kpformsglobal.resibo:: lock: 1 WHERE branchcode: " + branchcode + " zonecode: " + zonecode);
        //                kplog.Info("INSERT INTO kpformsglobal.resibo:: year: " + dt.ToString("yyyy") + " bcode2: " + branchcode + " zcode2: " + zonecode + " ser: 1");
        //                kplog.Info("Generate receipt:: receipt: " + dt.ToString("yy") + "-" + ser.ToString().PadLeft(6, '0'));
        //                kplog.Info("SUCCESS generating receipt");

        //                return dt.ToString("yy") + "-" + ser.ToString().PadLeft(6, '0');
        //            }
        //        }

        //    }
        //    catch (MySqlException myx)
        //    {
        //        kplog.Error("FAILED:: ErrorDetail: " + myx.ToString());
        //        throw new Exception(myx.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        kplog.Error("FAILED:: ErrorDetail: " + ex.ToString());
        //        throw new Exception(ex.ToString());
        //    }

        //}

        private ChargeResponse calculateChargeGlobalMobile(String bcode, String zcode)
        {


            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    using (command = conn.CreateCommand())
                    {

                        DateTime NullDate = DateTime.MinValue;

                        Decimal dec = 0;
                        conn.Open();
                        MySqlTransaction trans = conn.BeginTransaction();
                        List<ChargeList> listofCharge = new List<ChargeList>();
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
                                    String queryRates = "SELECT * FROM kpformsglobal.charges WHERE `type` = @type;";
                                    command.CommandText = queryRates;

                                    command.Parameters.AddWithValue("type", type);

                                    MySqlDataReader ReaderRates = command.ExecuteReader();
                                    if (ReaderRates.HasRows)
                                    {
                                        while (ReaderRates.Read())
                                        {
                                            listofCharge.Add(new ChargeList
                                            {
                                                minAmount = Convert.ToDouble(ReaderRates["MinAmount"]),
                                                maxAmount = Convert.ToDouble(ReaderRates["MaxAmount"]),
                                                chargeValue = Convert.ToDouble(ReaderRates["ChargeValue"]) + pnmCharge,
                                            });
                                        }
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
                                        String queryRates = "SELECT * FROM kpformsglobal.charges WHERE `type` = @type;";
                                        command.CommandText = queryRates;

                                        command.Parameters.AddWithValue("type", nextID);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.HasRows)
                                        {
                                            while (ReaderRates.Read())
                                            {
                                                listofCharge.Add(new ChargeList
                                                {
                                                    minAmount = Convert.ToDouble(ReaderRates["MinAmount"]),
                                                    maxAmount = Convert.ToDouble(ReaderRates["MaxAmount"]),
                                                    chargeValue = Convert.ToDouble(ReaderRates["ChargeValue"]) + pnmCharge,
                                                });
                                            }
                                            ReaderRates.Close();
                                        }
                                    }
                                    else
                                    {


                                        command.Parameters.Clear();
                                        String queryRates = "SELECT * WHERE ROUND(@amount,2) BETWEEN MinAmount AND MaxAmount AND `type` = @type;";
                                        command.CommandText = queryRates;

                                        command.Parameters.AddWithValue("type", type);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.HasRows)
                                        {
                                            while (ReaderRates.Read())
                                            {
                                                listofCharge.Add(new ChargeList
                                                {
                                                    minAmount = Convert.ToDouble(ReaderRates["MinAmount"]),
                                                    maxAmount = Convert.ToDouble(ReaderRates["MaxAmount"]),
                                                    chargeValue = Convert.ToDouble(ReaderRates["ChargeValue"]) + pnmCharge,
                                                });
                                            }
                                            ReaderRates.Close();
                                        }
                                    }
                                }


                            }
                            //trans.Commit();
                            conn.Close();
                            kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " charges: " + listofCharge);
                            return new ChargeResponse { respcode = 1, message = getRespMessage(1), listofcharges = listofCharge };


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


        private String ConvertDateTime(String date)
        {

            string month = "";
            string day = "";
            string year = "";

            year = date.Substring(6, 4);
            day = date.Substring(3, 2);
            month = date.Substring(0, 2);

            date = year + "-" + month + "-" + day;

            date = Convert.ToDateTime(date).ToString("yyyy-MM-dd 00:00:00");
            return date;



        }

        private DateTime getServerDateGlobal()
        {
            try
            {
                //throw new Exception(isOpenConnection.ToString());             
                using (MySqlConnection conn = new MySqlConnection(connection))
                {
                    conn.Open();
                    using (MySqlCommand command = conn.CreateCommand())
                    {
                        DateTime serverdate;

                        command.CommandText = "Select NOW() as serverdt;";
                        using (MySqlDataReader Reader = command.ExecuteReader())
                        {
                            Reader.Read();
                            serverdate = Convert.ToDateTime(Reader["serverdt"]);
                            Reader.Close();
                            conn.Close();


                            kplog.Info("SUCCESS:: Server Date: " + serverdate);
                            return serverdate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: message: " + ex.Message + " ErrorDetail: " + ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        //private Boolean OfacMatch(String name)
        //{
        //    Int32 Percentage = 100;

        //    using (MySqlConnection con = new MySqlConnection(dbconofac))
        //    {
        //        try
        //        {
        //            con.Open();
        //            using (MySqlCommand cmd = con.CreateCommand())
        //            {


        //                cmd.Parameters.Clear();
        //                cmd.CommandText = "SELECT * FROM( " +
        //                "Select o.fullNAme, o.uid, o.firstName, o.lastName, o.sdnType,  split_str(split_str(split_str(o.dateOfBirthList,'dateOfBirth',2),'\":\"',2),'\",',1) AS dateOfBirth, split_str(split_str(split_str(o.placeOfBirthList,'placeOfBirth',2),'\":\"',2),'\",',1) AS placeofbirth, a.fullName as alias, o.soundexvalue, " +
        //                "ROUND(JaroWinkler((o.fullName),@FullName)*100,0) as score1, " +
        //                "ROUND(JaroWinkler((o.rfullName),@FullName)*100,0) as score2, " +
        //                "ROUND(JaroWinkler((o.lastname),@FullName)*100,0) as score3, " +
        //                "ROUND(JaroWinkler((o.firstname),@FullName)*100,0) as score4 " +
        //                "FROM kpofacglobal.ofac o LEFT JOIN kpofacglobal.aliasofac a ON a.CustomerID = o.uid WHERE " +
        //                "ROUND(JaroWinkler((o.fullName),@FullName)*100,0)>=@Percent OR " +
        //                "ROUND(JaroWinkler((o.rfullName),@FullName)*100,0)>=@Percent OR " +
        //                "ROUND(JaroWinkler((o.firstName),@FullName)*100,0)>=@Percent OR " +
        //                "ROUND(JaroWinkler((o.lastName),@FullName)*100,0)>=@Percent " +
        //                " UNION DISTINCT " +
        //                "Select o.fullNAme, o.uid, o.firstName, o.lastName, o.sdnType, split_str(split_str(split_str(o.dateOfBirthList,'dateOfBirth',2),'\":\"',2),'\",',1) " +
        //                " AS dateOfBirth, split_str(split_str(split_str(o.placeOfBirthList,'placeOfBirth',2),'\":\"',2),'\",',1) " +
        //                " AS placeofbirth, a.fullName as alias, a.soundexvalue, " +
        //                "ROUND(JaroWinkler((a.fullName),@FullName)*100,0) as score1, " +
        //                "ROUND(JaroWinkler((a.rfullName),@FullName)*100,0) as score2, " +
        //                "ROUND(JaroWinkler((a.lastname),@FullName)*100,0) as score3, " +
        //                "ROUND(JaroWinkler((a.firstname),@FullName)*100,0) as score4 " +
        //                "FROM kpofacglobal.ofac o LEFT JOIN kpofacglobal.aliasofac a ON a.CustomerID = o.uid WHERE " +
        //                "ROUND(JaroWinkler((a.fullName),@FullName)*100,0)>=@Percent OR " +
        //                "ROUND(JaroWinkler((a.rfullName),@FullName)*100,0)>=@Percent or " +
        //                "ROUND(JaroWinkler((a.firstName),@FullName)*100,0)>=@Percent or " +
        //                "ROUND(JaroWinkler((a.lastName),@FullName)*100,0)>=@Percent )as xx";

        //                cmd.Parameters.AddWithValue("FullName", name);
        //                cmd.Parameters.AddWithValue("Percent", Percentage);
        //                MySqlDataReader rcvRdr = cmd.ExecuteReader();

        //                if (rcvRdr.HasRows)
        //                {
        //                    con.Close();
        //                    return true;
        //                }
        //                else
        //                {
        //                    con.Close();
        //                    return false;
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            kplog.Error("ERROR : '" + ex.ToString() + "'");
        //            throw new Exception(ex.ToString());
        //        }
        //    }
        //}

        private ChargeResponse calculateChargePerBranchGlobalMobile(String bcode, String zcode)
        {

            using (MySqlConnection conn = new MySqlConnection(connection))
            {
                try
                {
                    using (command = conn.CreateCommand())
                    {

                        DateTime NullDate = DateTime.MinValue;

                        conn.Open();
                        MySqlTransaction trans = conn.BeginTransaction();

                        try
                        {

                            List<ChargeList> listofCharge = new List<ChargeList>();
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
                                    String queryRates = "SELECT * FROM kpformsglobal.ratesperbranchcharges WHERE  `type` = @type;";
                                    command.CommandText = queryRates;
                                    command.Parameters.AddWithValue("type", type);
                                    MySqlDataReader rdr = command.ExecuteReader();

                                    MySqlDataReader ReaderRates = command.ExecuteReader();
                                    if (rdr.HasRows)
                                    {
                                        while (rdr.Read())
                                        {
                                            listofCharge.Add(new ChargeList
                                            {
                                                minAmount = Convert.ToDouble(rdr["MinAmount"]),
                                                maxAmount = Convert.ToDouble(rdr["MaxAmount"]),
                                                chargeValue = Convert.ToDouble(rdr["ChargeValue"]) + pnmCharge,
                                            });
                                        }
                                        rdr.Close();
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



                                        command.Parameters.Clear();
                                        String queryRates = "SELECT ChargeValue AS charge FROM kpformsglobal.ratesperbranchcharges WHERE ROUND(@amount,2) BETWEEN MinAmount AND MaxAmount AND `type` = @type;";
                                        command.CommandText = queryRates;

                                        command.Parameters.AddWithValue("type", nextID);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.HasRows)
                                        {
                                            while (ReaderRates.Read())
                                            {
                                                listofCharge.Add(new ChargeList
                                                {
                                                    minAmount = Convert.ToDouble(ReaderRates["MinAmount"]),
                                                    maxAmount = Convert.ToDouble(ReaderRates["MaxAmount"]),
                                                    chargeValue = Convert.ToDouble(ReaderRates["ChargeValue"]) + pnmCharge,
                                                });
                                            }
                                            ReaderRates.Close();
                                        }
                                    }
                                    else
                                    {
                                        //ReaderNextRates.Close();


                                        command.Parameters.Clear();
                                        String queryRates = "SELECT * FROM kpformsglobal.ratesperbranchcharges WHERE  `type` = @type;";
                                        command.CommandText = queryRates;

                                        command.Parameters.AddWithValue("type", type);

                                        MySqlDataReader ReaderRates = command.ExecuteReader();
                                        if (ReaderRates.HasRows)
                                        {
                                            while (ReaderRates.Read())
                                            {
                                                listofCharge.Add(new ChargeList
                                                {
                                                    minAmount = Convert.ToDouble(ReaderRates["MinAmount"]),
                                                    maxAmount = Convert.ToDouble(ReaderRates["MaxAmount"]),
                                                    chargeValue = Convert.ToDouble(ReaderRates["ChargeValue"]) + pnmCharge,
                                                });
                                            }
                                            ReaderRates.Close();
                                        }
                                    }
                                }


                            }
                            else
                            {
                                kplog.Error("FAILED:: respcode: 16 message: " + getRespMessage(16) + " charges: " + listofCharge);
                                Reader.Close();
                                conn.Close();
                                return new ChargeResponse { respcode = 16, message = getRespMessage(16), listofcharges = listofCharge };
                            }
                            //trans.Commit();
                            conn.Close();
                            kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " charges: " + listofCharge);
                            return new ChargeResponse { respcode = 1, message = getRespMessage(1), listofcharges = listofCharge };
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
        //private String getcustomertable(String lastname)
        //{
        //    String customers = "";
        //    lastname.ToUpper();
        //    if (lastname.StartsWith("A") || lastname.StartsWith("B") || lastname.StartsWith("C"))
        //    {
        //        customers = "AtoC";
        //    }
        //    else if (lastname.StartsWith("D") || lastname.StartsWith("E") || lastname.StartsWith("F"))
        //    {
        //        customers = "DtoF";
        //    }
        //    else if (lastname.StartsWith("G") || lastname.StartsWith("H") || lastname.StartsWith("I"))
        //    {
        //        customers = "GtoI";
        //    }
        //    else if (lastname.StartsWith("J") || lastname.StartsWith("K") || lastname.StartsWith("L"))
        //    {
        //        customers = "JtoL";
        //    }
        //    else if (lastname.StartsWith("M") || lastname.StartsWith("N") || lastname.StartsWith("O"))
        //    {
        //        customers = "MtoO";
        //    }
        //    else if (lastname.StartsWith("P") || lastname.StartsWith("Q") || lastname.StartsWith("R"))
        //    {
        //        customers = "PtoR";
        //    }
        //    else if (lastname.StartsWith("S") || lastname.StartsWith("T") || lastname.StartsWith("U"))
        //    {
        //        customers = "StoU";
        //    }
        //    else if (lastname.StartsWith("V") || lastname.StartsWith("W") || lastname.StartsWith("X"))
        //    {
        //        customers = "VtoX";
        //    }
        //    else if (lastname.StartsWith("Y") || lastname.StartsWith("Z"))
        //    {
        //        customers = "YtoZ";
        //    }

        //    kplog.Info("SUCCESS:: TableCustomer: " + customers);
        //    return customers;
        //}

        //private Boolean updateResiboGlobal(string branchcode, Int32 zonecode, String resibo, ref MySqlCommand command)
        //{
        //    try
        //    {
        //        MySqlCommand cmdReader;
        //        using (cmdReader = new MySqlConnection(connection).CreateCommand())
        //        {

        //            dt = getServerDateGlobal(true);

        //            Int32 series = Convert.ToInt32(resibo.Substring(3, resibo.Length - 3));

        //            command.Parameters.Clear();
        //            command.CommandText = "update kpformsglobal.resibo set series = @series where branchcode = @bcode2 and zonecode = @zcode2";
        //            command.Parameters.AddWithValue("bcode2", branchcode);
        //            command.Parameters.AddWithValue("zcode2", zonecode);
        //            command.Parameters.AddWithValue("series", series);
        //            command.ExecuteNonQuery();
        //            command.Parameters.Clear();

        //            kplog.Info("UPDATE receipt:: branchcode: " + branchcode + " zonecode: " + zonecode + " series: " + series);
        //            kplog.Info("SUCCESS updating receipt");
        //            return true;
        //        }
        //    }
        //    catch (MySqlException myx)
        //    {
        //        kplog.Error("FAILED:: ErrorDetail: " + myx.ToString());
        //        throw new Exception(myx.ToString());
        //    }
        //    catch (Exception ex)
        //    {
        //        kplog.Error("FAILED:: ErrorDetail: " + ex.ToString());
        //        throw new Exception(ex.ToString());
        //    }
        //}

        private async Task<String> ExpectID_IQ_Check(CustomerModel model)//ExpectID_IQ_Fields fields)
        {
            #region inputs

            DateTime bDate = Convert.ToDateTime(model.BirthDate);

            var values = new Dictionary<string, string> {
                                                            { "username", iDologyUser },//*
                                                            { "password", iDologyPass },//*
                                                            { "invoice", "" },
                                                            { "amount", "" },
                                                            { "shipping", "" },
                                                            { "tax", "" },
                                                            { "total", "" },
                                                            { "idType", model.IDType },
                                                            { "idIssuer", "" },
                                                            { "idNumber", model.IDNo },
                                                            { "paymentMethod", "" },
                                                            { "firstName", model.firstName },//*
                                                            { "lastName", model.lastName },//*
                                                            { "address", model.Street },//*
                                                            { "city", model.City },//*
                                                            { "state", model.StateAbbr },//*
                                                            { "zip", model.ZipCode },//*
                                                            { "ssnLast4", "" },
                                                            { "ssn", "" },
                                                            { "dobMonth", bDate.ToString("MM") },
                                                            { "dobDay",bDate.ToString("dd")},
                                                            { "dobYear",bDate.ToString("yyyy") },
                                                            { "ipAddress", "" },
                                                            { "email", model.UserID },
                                                            { "telephone", model.PhoneNo },
                                                            { "sku", "" },
                                                            { "uid", "" },
                                                            { "altAddress", "" },
                                                            { "altCity", "" },
                                                            { "altState", "" },
                                                            { "altZip", "" },
                                                            { "purchaseDate", "" },
                                                            { "captureQueryId", "" },
                                                            { "score", "" },
                                                            { "c_custome_field_1", "" },
                                                            { "c_custome_field_2", "" },
                                                            { "c_custome_field_3", "" },
                                                        };
            #endregion

            String returnee = "FAIL";
            HttpContent content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(iDologyServer, content).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(responseString);
            XmlNode xn;

            List<String> errList = new List<String> { };
            try
            {
                xn = xml.SelectSingleNode("/response/summary-result")["key"];
                if (xn.InnerText == "id.success")
                {
                    returnee = "PASS";
                }
            }
            catch (Exception)
            {
                XmlNodeList xnList = xml.SelectNodes("/response/error");
                foreach (XmlNode xmlNode in xnList)
                {
                    errList.Add(xmlNode.InnerText);
                }
                kplog.Error(errList);
                returnee = "ERROR";
            }
            return returnee;
        }

        private String cleanString(String str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                TextInfo myTI = new CultureInfo("en-US", false).TextInfo;

                return myTI.ToTitleCase(str.Trim());

            }
            else
            {
                return "";
            }
        }

        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        private void generateReportMobile(List<TransactionDetailsM> TransactionDetailsM, String CustomerID)
        {

            try
            {
                CustomerModel model = getProfile(CustomerID).sender;
                String name = model.firstName + " " + model.lastName;
                DateTime dt = getServerDateGlobal(false);
                String email = model.UserID;
                String fileName = dt.ToString("yyyy-MM-dd") + "_" + generateActivationCode();
                Models.Reports.MobileTransReport rpt = new Models.Reports.MobileTransReport();

                rpt.SetDataSource(TransactionDetailsM);
                rpt.SetParameterValue("accountid", name);
                rpt.SetParameterValue("Date", dt.ToString("MM/dd/yyyy"));


                using (var stream = rpt.ExportToStream(ExportFormatType.PortableDocFormat))
                {



                    SmtpClient client = new SmtpClient();
                    client.EnableSsl = smtpSsl;
                    client.UseDefaultCredentials = false;
                    client.Host = smtpServer;
                    client.Port = 587;
                    client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    MailMessage msg = new MailMessage();
                    msg.To.Add(email);

                    //msg.From = new MailAddress("donotreply@mlhuillier1.com");
                    //msg.Subject = "ML Remit - Transaction Report";
                    //msg.Body = "Good day Ma'am/Sir  ,<br /><br />"
                    //         + "With Mlhuillier as easy to send money to your friends and family around<br />"
                    //         + "different parts of the world in a fast, convenient and secure way.<br /><br />"
                    //         + "Attached is your Transaction Report.<br /><br />";

                    msg.Body = "<div style=\"font-size: 16px; font-family: Consolas; text-align: justify; margin: 0 auto; width: 500px; color: black; padding: 20px; border-left: 1px solid #130d01; border-right: 1px solid #130d01; border-radius: 20px;\">"
                   + "<img src='https://mlremit.mlhuillier1.com/paynearme/Images/logo_en.png' style='margin-left:15%'/>"
                   + "<p> Good day Ma'am/Sir <b>" + model.firstName + "</b>,</p>"
                   + "<p>"
                   + "With M. Lhuillier it is easy to send money to your friends and family around "
                   + "different parts of the world in a fast, convenient and secure way."
                   + "</p>"
                   + "Attached File is your Transaction Report, Thank You!.<br /><br />"
                   + "<br /><br />"
                   + "<div style=\"font-size: 14px; border-top: 1px solid lightgray; text-align: center; padding-top: 5px; background-color: gray;\">"
                   + "-- This mail is auto generated. Please do not reply. --"
                   + "</div></div>";

                    msg.Attachments.Add(new Attachment(stream, fileName + ".pdf"));
                    msg.IsBodyHtml = true;
                    client.Send(msg);

                }

            }
            catch (Exception ex)
            {

                kplog.Error(ex.ToString());
                throw;
            }


        }

        private String generateKPTNPayNearMe(String branchcode, Int32 zonecode, String TransactionType)
        {
            kplog.Info("START--- > PARAMS: bcode" + branchcode + " zcode" + zonecode + " TransactionType " + TransactionType);
            try
            {
                String guid = Guid.NewGuid().GetHashCode().ToString();
                Random rand = new Random();
                dt = getServerDateGlobal(false);
                jp.takel.PseudoRandom.MersenneTwister randGen = new jp.takel.PseudoRandom.MersenneTwister((uint)HiResDateTime.UtcNow.Ticks);

                string randNum = string.Empty;
                while (randNum.Length != 9)
                {
                    randNum = randGen.Next(1, int.MaxValue).ToString().PadLeft(9, '0');
                }


                if (TransactionType == "Web")
                {
                    kplog.Info("SUCCESS:: KPTN: " + ("MLG" + branchcode + dt.ToString("dd") + zonecode.ToString() + randGen.Next(1, int.MaxValue).ToString().PadLeft(9, '0') + dt.ToString("MM")));
                    return "MLG" + branchcode + dt.ToString("dd") + zonecode.ToString() + "W" + randNum + dt.ToString("MM");
                }
                else
                {
                    kplog.Info("SUCCESS:: KPTN: " + ("MLG" + branchcode + dt.ToString("dd") + zonecode.ToString() + randGen.Next(1, int.MaxValue).ToString().PadLeft(9, '0') + dt.ToString("MM")));
                    return "MLG" + branchcode + dt.ToString("dd") + zonecode.ToString() + "M" + randNum + dt.ToString("MM");
                }



            }
            catch (Exception a)
            {
                kplog.Fatal("FAILED:: message: " + a.Message + " ErrorDetail: " + a.ToString());
                throw new Exception(a.ToString());
            }
        }
        //done loggings
        private ControlResponse generateControlGlobal(String Username, String Password, String branchcode, Int32 type, String OperatorID, Int32 ZoneCode, String StationNumber)
        {
            kplog.Info("Username: " + Username + ", Password: " + Password + ", BranchCode: " + branchcode + "ZoneCode: " + ZoneCode + ", OperatorID: " + OperatorID);
            if (StationNumber.ToString().Equals("0"))
            {
                kplog.Error("FAILED:: respcode: 13 message: " + getRespMessage(13));
                return new ControlResponse { respcode = 13, message = getRespMessage(13) };
            }
           

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connection))
                {
                    using (command = conn.CreateCommand())
                    {
                        conn.Open();
                        MySqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                        command.Transaction = trans;
                        try
                        {
                            dt = getServerDateGlobal(true);
                            String control = "MLG";
                            Int32 nseries;
                            String nseries1 = string.Empty;
                            String getcontrolmax = string.Empty;



                            command.CommandText = "Select station, bcode, userid, nseries, zcode, type from kpformsglobal.control where station = @st and bcode = @bcode and zcode = @zcode and `type` = @tp FOR UPDATE";

                            command.Parameters.AddWithValue("st", StationNumber);
                            command.Parameters.AddWithValue("bcode", branchcode);
                            command.Parameters.AddWithValue("zcode", ZoneCode);
                            command.Parameters.AddWithValue("tp", type);
                            MySqlDataReader Reader = command.ExecuteReader();

                            if (Reader.HasRows)
                            {

                                Reader.Read();

                                if (type == 0)
                                {
                                    control += "S0" + ZoneCode.ToString() + "-" + StationNumber + "-" + branchcode;
                                }
                                else if (type == 1)
                                {
                                    control += "P0" + ZoneCode.ToString() + "-" + StationNumber + "-" + branchcode;
                                }
                                else if (type == 2)
                                {
                                    control += "S0" + ZoneCode.ToString() + "-" + StationNumber + "-R" + branchcode;
                                }
                                else if (type == 3)
                                {
                                    control += "P0" + ZoneCode.ToString() + "-" + StationNumber + "-R" + branchcode;
                                }
                                else
                                {
                                    kplog.Error("FAILED:: message: Invalid type value");
                                    throw new Exception("Invalid type value");
                                }
                                String s = Reader["Station"].ToString();
                                nseries = Convert.ToInt32(Reader["nseries"].ToString().PadLeft(6, '0'));
                                Reader.Close();


                                command.CommandText = "update kpformsglobal.control set nseries = @series where bcode = @bcode and station = @st and zcode = @zcode and type = @tp";
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("st", StationNumber);
                                command.Parameters.AddWithValue("bcode", branchcode);
                                command.Parameters.AddWithValue("series", nseries + 1 > 999999 ? 000001 : nseries + 1);
                                command.Parameters.AddWithValue("zcode", ZoneCode);
                                command.Parameters.AddWithValue("tp", type);
                                int abc101 = command.ExecuteNonQuery();


                                trans.Commit();
                                conn.Close();



                                kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " controlno: " + (control + "-" + dt.ToString("yy") + "-" + nseries) + " nseries: " + nseries);
                                return new ControlResponse { respcode = 1, message = getRespMessage(1), controlno = control + "-" + dt.ToString("yy") + "-" + nseries.ToString().PadLeft(6, '0'), nseries = nseries.ToString().PadLeft(6, '0') };

                            }
                            else
                            {
                                Reader.Close();
                                command.CommandText = "Insert into kpformsglobal.control (`station`,`bcode`,`userid`,`nseries`,`zcode`, `type`) values (@station,@branchcode,@uid,1,@zonecode,@type)";
                                if (type == 0)
                                {
                                    control += "S0" + ZoneCode.ToString() + "-" + StationNumber + "-" + branchcode;
                                }
                                else if (type == 1)
                                {
                                    control += "P0" + ZoneCode.ToString() + "-" + StationNumber + "-" + branchcode;
                                }
                                else if (type == 2)
                                {
                                    control += "S0" + ZoneCode.ToString() + "-" + StationNumber + "-R" + branchcode;
                                }
                                else if (type == 3)
                                {
                                    control += "P0" + ZoneCode.ToString() + "-" + StationNumber + "-R" + branchcode;
                                }
                                else
                                {
                                    kplog.Error("FAILED:: message: Invalid type value");
                                    throw new Exception("Invalid type value");
                                }
                                command.Parameters.AddWithValue("station", StationNumber);
                                command.Parameters.AddWithValue("branchcode", branchcode);
                                command.Parameters.AddWithValue("uid", OperatorID);
                                command.Parameters.AddWithValue("zonecode", ZoneCode);
                                command.Parameters.AddWithValue("type", type);
                                int x = command.ExecuteNonQuery();

                                trans.Commit();
                                conn.Close();

                                kplog.Info("SUCCESS:: INSERT INTO kpformsglobal.control: station: " + StationNumber + " " +
                                "branchcode: " + branchcode + " " +
                                "uid: " + OperatorID + " " +
                                "zonecode: " + ZoneCode + " " +
                                "type: " + type);

                                kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " control: " + (control + "-" + dt.ToString("yy") + "-" + "000001") + " nseries: 000001");
                                return new ControlResponse { respcode = 1, message = getRespMessage(1), controlno = control + "-" + dt.ToString("yy") + "-" + "000001", nseries = "000001" };
                            }
                        }
                        catch (MySqlException ex)
                        {
                            trans.Rollback();
                            conn.Close();
                            kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());
                            return new ControlResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());

                return new ControlResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
            }

            catch (Exception ex)
            {
                kplog.Fatal("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString());

                return new ControlResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
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

        private DateTime getServerDateGlobal(Boolean isOpenConnection, MySqlCommand mycommand)
        {

            try
            {
                //throw new Exception(isOpenConnection.ToString());
                if (!isOpenConnection)
                {
                    using (MySqlConnection conn = new MySqlConnection(connection))
                    {
                        conn.Open();
                        using (MySqlCommand command = conn.CreateCommand())
                        {

                            DateTime serverdate;

                            command.CommandText = "Select NOW() as serverdt;";
                            using (MySqlDataReader Reader = command.ExecuteReader())
                            {
                                Reader.Read();

                                serverdate = Convert.ToDateTime(Reader["serverdt"]);
                                Reader.Close();
                                conn.Close();


                                kplog.Info("SUCCESS: server = '" + serverdate + "'");
                                return serverdate;
                            }

                        }
                    }
                }
                else
                {

                    DateTime serverdate = DateTime.Now;

                    mycommand.CommandText = "Select NOW() as serverdt;";

                    using (MySqlDataReader Reader = mycommand.ExecuteReader())
                    {
                        Reader.Read();
                        serverdate = Convert.ToDateTime(Reader["serverdt"]);
                        Reader.Close();
                        kplog.Info("SUCCESS: server = '" + serverdate + "'");
                        return serverdate;
                    }


                }

            }
            catch (Exception ex)
            {
                kplog.Error("ERROR: message = '" + ex.ToString() + "'");
                throw new Exception(ex.Message);
            }
        }

        private DateTime getServerDateGlobal1(Boolean isOpenConnection)
        {

            try
            {
                //throw new Exception(isOpenConnection.ToString());
                if (!isOpenConnection)
                {
                    using (MySqlConnection conn = new MySqlConnection(connection))
                    {
                        conn.Open();
                        using (MySqlCommand command = conn.CreateCommand())
                        {

                            DateTime serverdate;

                            command.CommandText = "Select NOW() as serverdt;";
                            using (MySqlDataReader Reader = command.ExecuteReader())
                            {
                                Reader.Read();

                                serverdate = Convert.ToDateTime(Reader["serverdt"]);
                                Reader.Close();
                                conn.Close();
                                kplog.Info("SUCCESS: server = '" + serverdate + "'");
                                return serverdate;
                            }

                        }
                    }
                }
                else
                {

                    DateTime serverdate;

                    command.CommandText = "Select NOW() as serverdt;";

                    using (MySqlDataReader Reader = command.ExecuteReader())
                    {
                        Reader.Read();
                        serverdate = Convert.ToDateTime(Reader["serverdt"]);
                        Reader.Close();
                        kplog.Info("SUCCESS: server = '" + serverdate + "'");
                        return serverdate;
                    }


                }

            }
            catch (Exception ex)
            {
                kplog.Error("ERROR: message = '" + ex.ToString() + "'");
                throw new Exception(ex.Message);
            }
        }

        private String generateCustIDGlobal(MySqlCommand command)
        {
            try
            {
                dt = getServerDateGlobal(true, command);

                String query = "select series from kpformsglobal.customerseries";
                command.CommandText = query;
                MySqlDataReader Reader = command.ExecuteReader();

                Reader.Read();
                String series = Reader["series"].ToString();
                Reader.Close();

                return "N1" + dt.ToString("yy") + dt.ToString("MM") + series.PadLeft(8, '0');
            }
            catch (Exception ex)
            {
                //kplog.Fatal(ex.ToString());
                throw new Exception(ex.ToString());
            }

        }
        //done loggings
        private String generateBeneficiaryCustIDGlobal(MySqlCommand command)
        {
            try
            {

                dt = getServerDateGlobal(true, command);

                String query = "select series from kpformsglobal.beneficiaryseries";
                command.CommandText = query;
                MySqlDataReader Reader = command.ExecuteReader();

                Reader.Read();
                String series = Reader["series"].ToString();
                Reader.Close();
                kplog.Info("N1" + dt.ToString("yy") + dt.ToString("MM") + series.PadLeft(8, '0'));
                return "N1" + dt.ToString("yy") + dt.ToString("MM") + series.PadLeft(8, '0');

            }
            catch (Exception ex)
            {
                //kplog.Fatal(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }


        private String generateSignature(string query)
        {
            var x = query.Replace("=", string.Empty);
            var y = x.Replace("&", string.Empty);


            query = y + secretKey;

            using (MD5 md5Hash = MD5.Create())
            {
                string hash = GetMd5Hash(md5Hash, query);

                return hash;
            }


        }



        private String SendRequest(Uri uri)
        {
            try
            {
                String res = null;
                HttpWebRequest web = (HttpWebRequest)WebRequest.Create(uri);
                web.Method = "GET";
                WebResponse webresponse = web.GetResponse();
                Stream response = webresponse.GetResponseStream();
                res = new StreamReader(response).ReadToEnd();
                webresponse.Close();


                return res;
            }
            catch (Exception ex)
            {
                //Kplog.Fatal(ex.ToString());
                return "Unable to process request. The system encountered some technical problem. Sorry for the inconvenience. " + ex.ToString() + "";
            }
        }

        private Int32 getTimeStamp()
        {

            using (MySqlConnection con = new MySqlConnection(connection))
            {
                con.Open();
                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT UNIX_TIMESTAMP() as tstamp;";
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    rdr.Read();
                    return Convert.ToInt32(rdr["tstamp"]);
                }

            }
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
                    string x = rdrserverdt["serverdt"].ToString().Substring(8, 6);
                    rdrserverdt.Close();
                    custconn.Close();
                    return x;

                }
            }
        }

        private String generateMobileToken()
        {
            using (MySqlConnection custconn = new MySqlConnection(connection))
            {
                custconn.Open();
                using (MySqlCommand custcommand = custconn.CreateCommand())
                {
                    custcommand.CommandText = "select now()+0 as serverdt";
                    MySqlDataReader rdrserverdt = custcommand.ExecuteReader();
                    rdrserverdt.Read();
                    string x = rdrserverdt["serverdt"].ToString().Substring(10, 4);
                    rdrserverdt.Close();
                    custconn.Close();
                    return x;

                }
            }
        }
        #endregion

     
    
    }

    
}
