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


namespace PayNearMe.Controllers.api
{
    public class WebServiceController : ApiController
    {

        IDictionary config;
        string server = string.Empty;
        private static readonly ILog kplog = LogManager.GetLogger(typeof(RegisterController));
        private MySqlCommand command;
        private String connection = string.Empty;
        private String dbconofac = string.Empty;
        private String PNMServer = string.Empty;
        private DateTime dt;
        private String loginuser = "boswebserviceusr";
        private String loginpass = "boyursa805";
        private MySqlCommand custcommand;
        private MySqlTransaction custtrans = null;
        private static String siteIdentifier = "S4989355513";
        private static String secretKey = "272141e0ddc56671";
        private String ftp = string.Empty;

        public WebServiceController()
        {
            log4net.Config.XmlConfigurator.Configure();
            config = (IDictionary)(ConfigurationManager.GetSection("PayNearMeAPISection"));
            server = config["server"].ToString();
            connection = config["globalcon"].ToString();
            dbconofac = config["ofaccon"].ToString();
            ftp = config["ftp"].ToString();

        }

        [HttpGet]
        public String generateKPTNGlobal(String branchcode, Int32 zonecode)
        {
            try
            {
                String guid = Guid.NewGuid().GetHashCode().ToString();
                Random rand = new Random();
                dt = getServerDateGlobal(false);
                jp.takel.PseudoRandom.MersenneTwister randGen = new jp.takel.PseudoRandom.MersenneTwister((uint)HiResDateTime.UtcNow.Ticks);
                kplog.Info("SUCCESS:: KPTN: " + ("MLG" + branchcode + dt.ToString("dd") + zonecode.ToString() + randGen.Next(1, int.MaxValue).ToString().PadLeft(10, '0') + dt.ToString("MM")));
                return "MLG" + branchcode + dt.ToString("dd") + zonecode.ToString() + randGen.Next(1, int.MaxValue).ToString().PadLeft(10, '0') + dt.ToString("MM"); ;
            }
            catch (Exception a)
            {
                kplog.Fatal("FAILED:: message: " + a.Message + " ErrorDetail: " + a.ToString());
                throw new Exception(a.ToString());
            }
        }

        public ControlResponse generateControlGlobal(String Username, String Password, String branchcode, Int32 type, String OperatorID, Int32 ZoneCode, String StationNumber)
        {
            kplog.Info("Username: " + Username + ", Password: " + Password + ", BranchCode: " + branchcode + "ZoneCode: " + ZoneCode + ", OperatorID: " + OperatorID);
            if (StationNumber.ToString().Equals("0"))
            {
                kplog.Error("FAILED:: respcode: 13 message: " + getRespMessage(13));
                return new ControlResponse { respcode = 13, message = getRespMessage(13) };
            }
            if (!authenticate(Username, Password))
            {
                kplog.Error("FAILED:: respcode: 7 message: " + getRespMessage(7));
                return new ControlResponse { respcode = 7, message = getRespMessage(7) };
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
                            String nseries = string.Empty;
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
                                nseries = Reader["nseries"].ToString().PadLeft(6, '0');
                                Reader.Close();

                                
                                trans.Commit();
                                conn.Close();

                              

                                kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " controlno: " + (control + "-" + dt.ToString("yy") + "-" + nseries) + " nseries: " + nseries);
                                return new ControlResponse { respcode = 1, message = getRespMessage(1), controlno = control + "-" + dt.ToString("yy") + "-" + nseries, nseries = nseries };

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

        private Boolean authenticate(String username, String password)
        {

            if (loginuser.Equals(username) && loginpass.Equals(password))
            {
                return true;
            }
            else
            {
                return false;
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
                        return serverdate;
                    }


                }

            }
            catch (Exception ex)
            {
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
                        return serverdate;
                    }


                }

            }
            catch (Exception ex)
            {
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

                return "N1" + dt.ToString("yy") + dt.ToString("MM") + series.PadLeft(8, '0');

            }
            catch (Exception ex)
            {
                //kplog.Fatal(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        [HttpPost]
        public CustomerResultResponse insertbeneficiary(BeneficiaryModel bene)
        {
            log4net.Config.XmlConfigurator.Configure();
            String Username = "boswebserviceusr";
            String Password = "boyursa805";
            String sendercustid = bene.SenderCustID;
            String rcvrfirstname = bene.firstName;
            String rcvrlastname = bene.lastname;
            String rcvrmiddlename = bene.midlleName;
            String rcvrcountry = bene.country;
            String rcvrstreet = bene.street;
            String rcvrcitystate = bene.city;
            String rcvrzipcode = bene.zipcode;
            String rcvrbirthdate = bene.dateOfBirth;
            String rcvrgender = bene.gender;
            String rcvrrelation = bene.relation;
            String rcvrcontactno = bene.phoneNo;
            String rcvrcustid = bene.receiverCustID;


            String rcvrprovince = bene.province;

            if (!authenticate(Username, Password))
            {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
                kplog.Info(getRespMessage(7));
                return new CustomerResultResponse { respcode = 7, message = getRespMessage(7) };
            }

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

                                if (rcvrmiddlename == null)
                                {
                                    checking = "select sendercustid from kpcustomersglobal.BeneficiaryHistory where sendercustid=@sendercustid and firstname=@firstname and lastname=@lastname;";
                                    rcvrmiddlename = "";
                                }
                                else
                                {
                                    checking = "select sendercustid from kpcustomersglobal.BeneficiaryHistory where sendercustid=@sendercustid and firstname=@firstname and lastname=@lastname and middlename=@middlename;";
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
                                    reader.Close();

                                    con.Close();
                                    kplog.Info("Beneficiary Already Exist");
                                    return new CustomerResultResponse { respcode = 0, message = "Beneficiary Already Exist" };
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
                                command.CommandText = "INSERT INTO kpcustomersglobal.BeneficiaryPayNearMe(ReceiverCustID,isActivate) VALUES (@benecustid,'1')";
                                command.Parameters.AddWithValue("benecustid", benecustid);
                                int x = command.ExecuteNonQuery();





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

                                    dynamic data = JObject.Parse(res);

                                    if (data.status == "ok")
                                    {
                                        trans.Commit();
                                        con.Close();
                                        kplog.Info("Beneficiary Successfully Added");
                                        return new CustomerResultResponse { respcode = 1, message = "Beneficiary Successfully Added" };
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
                                        kplog.Info(error);
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
                                kplog.Fatal(myx);
                                return new CustomerResultResponse { respcode = 0, message = myx.Message, ErrorDetail = myx.ToString() };
                            }
                        }
                    }
                    catch (MySqlException mex)
                    {

                        con.Close();
                        kplog.Fatal(mex);
                        return new CustomerResultResponse { respcode = 0, message = mex.ToString(), ErrorDetail = mex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal(ex);
                return new CustomerResultResponse { respcode = 0, message = ex.ToString(), ErrorDetail = ex.ToString() };
            }
        }

        [HttpPost]
        public CustomerResultResponse updateBeneficiary(BeneficiaryModel model)
        {


            String rcvrfirstname = model.firstName;
            String rcvrlastname = model.lastname;
            String rcvrmiddlename = model.midlleName;
            String rcvrstreet = model.street;
            String rcvrcitystate = model.city;
            String rcvrcountry = model.country;
            String rcvrzipcode = model.zipcode;
            String rcvrbirthdate = model.dateOfBirth;
            String rcvrgender = model.gender;
            String rcvrrelation = model.relation;
            String rcvrcontactno = model.phoneNo;
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

                                    if (x > 0)
                                    {
                                        //Int32 timestamp = getTimeStamp();
                                        //string yearofbirth = Convert.ToDateTime(rcvrbirthdate).ToString("yyyy");

                                        //string query = "city=" + rcvrcitystate + "&country=" + rcvrcountry + "&first_name=" + rcvrfirstname + "&last_name=" + rcvrlastname + "&middle_name=" + rcvrmiddlename + "&postal_code=" + rcvrzipcode + "&site_identifier=" + siteIdentifier + "&site_user_identifier=" + rcvrcustid + "&street=" + rcvrstreet + "&timestamp=" + timestamp.ToString() +
                                        //                "&version=2.0&year_of_birth=" + yearofbirth;



                                        //string signature = generateSignature(query);

                                        //query = query + "&signature=" + signature;

                                        //Uri uri = new Uri(server + "/json-api/change_user?" + query);

                                        //string res = SendRequest(uri);

                                        //dynamic data = JObject.Parse(res);

                                        //if (data.status == "ok")
                                        //{
                                        //    trans.Commit();
                                        //    con.Close();
                                        //    kplog.Info("Beneficiary Successfully Updated");
                                        //    return new CustomerResultResponse { respcode = 1, message = "Beneficiary Successfully Updated" };
                                        //}
                                        //else
                                        //{
                                        //    trans.Rollback();
                                        //    con.Close();

                                        //    string error = "";
                                        //    for (int xx = 0; xx < data.errors.Count; xx++)
                                        //    {
                                        //        error = error + " " + data.errors[xx].description;
                                        //    }
                                        //    kplog.Info(error);
                                        //    return new CustomerResultResponse { respcode = 0, message = error };
                                        //}

                                        trans.Commit();
                                        con.Close();
                                        kplog.Info("Beneficiary Successfully Updated");
                                        return new CustomerResultResponse { respcode = 1, message = "Beneficiary Successfully Updated" };
                                        
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
                                kplog.Fatal(ex);
                                return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        kplog.Fatal(ex);
                        return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                kplog.Fatal(ex);
                return new CustomerResultResponse { respcode = 0, message = ex.Message, ErrorDetail = ex.ToString() };
            }
        }

        [HttpGet]
        public CustomerResultResponse getbeneficiarylist(String sendercustid)
        {

            try
            {

                using (MySqlConnection con = new MySqlConnection(connection))
                {
                    try
                    {
                        con.Open();
                        using (MySqlCommand cmd = con.CreateCommand())
                        {
                            List<Receiver> list = new List<Receiver>();
                            string query = "select a.firstname, a.lastname, a.middlename, a.fullname, a.street, a.citystate, a.country, if(a.zipcode is null,'', a.zipcode) as zipcode, date_format(a.birthdate,'%Y-%m-%d') as birthdate, a.gender, a.contactno, a.Relation,a.CustIDB from kpcustomersglobal.BeneficiaryHistory a inner join kpcustomersglobal.BeneficiaryPayNearMe b  ON a.CustIDB = b.ReceiverCustID where a.sendercustid=@sendercustid and b.isActivate = 1 order by LastTransDate DESC";
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
                                    list.Add(new Receiver
                                    {
                                        receiverCustID = rdr["CustIDB"].ToString(),
                                        firstName = rdr["firstname"].ToString(),
                                        lastname = rdr["lastname"].ToString(),
                                        midlleName = rdr["middlename"].ToString(),
                                        address = rdr["street"].ToString() + " " + rdr["citystate"].ToString() + " " + rdr["zipcode"].ToString() + " " + rdr["country"].ToString(),
                                        dateOfBirth = dtcheck.HasValue ? Convert.ToDateTime(dtcheck.Value).ToString("MM/d/yyyy") : "",
                                        gender = rdr["Gender"].ToString(),
                                        relation = rdr["Relation"].ToString(),
                                        phoneNo = rdr["ContactNo"].ToString(),
                                        street = rdr["street"].ToString(),
                                        city = rdr["citystate"].ToString(),
                                        zipcode = rdr["zipcode"].ToString(),
                                        country = rdr["country"].ToString()
                                       
                                    });
                                }
                            }
                            rdr.Close();


                            if (list.Count > 0)
                            {
                                
                                kplog.Info("Found");
                                return new CustomerResultResponse { respcode = 1, message = "Found", benelist = list };
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
                        kplog.Fatal(ex);
                        return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                //custcon.CloseConnection();
                kplog.Fatal(ex);
                return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
            }
        }

        [HttpGet]
        public BeneficiaryResponse getBeneficiaryInfo(String receiverCustID)
        {

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
                            string query = "select firstname, lastname, middlename, fullname, street, citystate, country, if(zipcode is null,'',zipcode) as zipcode, date_format(birthdate,'%Y-%m-%d') as birthdate, gender, contactno, Relation, CustIDB from kpcustomersglobal.BeneficiaryHistory  where CustIDB=@rcvrCustID;";
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
                                    dateOfBirth = dtcheck.HasValue ? Convert.ToDateTime(dtcheck.Value).ToString("MM/dd/yyyy") : "",
                                    gender = rdr["Gender"].ToString(),
                                    relation = rdr["Relation"].ToString(),
                                    phoneNo = rdr["ContactNo"].ToString(),
                                    street = rdr["street"].ToString(),



                                };
                                rdr.Close();
                                return new BeneficiaryResponse { respcode = 1 , message = "Success", data = model};
                            }
                            else
                            {
                                rdr.Close();
                                return new BeneficiaryResponse { respcode = 0, message = "No Data found", data = null };
                            }

                        }
                    }
                    catch (SqlException ex)
                    {
                        con.Close();
                        //custcon.CloseConnection();
                        kplog.Fatal(ex);
                        return new BeneficiaryResponse { respcode = 0, message = ex.ToString(), data = null };
                    }
                }
            }
            catch (Exception ex)
            {
                //custcon.CloseConnection();
                kplog.Fatal(ex);
                return new BeneficiaryResponse { respcode = 0, message = ex.ToString(), data = null };
            }




        }

        [HttpPost]
        public CustomerResultResponse deActivateBeneficiary(String receiverCustID)
        {

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
                                    return new CustomerResultResponse { respcode = 1, message = "Successfully deactivated Beneficiary!" };
                                }
                                else
                                {
                                    return new CustomerResultResponse { respcode = 0, message = "Mysql Error" };
                                }

                            }
                            else
                            {
                                rdr.Close();
                                return new CustomerResultResponse { respcode = 0, message = "Beneficiary does not exist!" };
                            }

                        }
                    }
                    catch (SqlException ex)
                    {
                        con.Close();
                        //custcon.CloseConnection();
                        kplog.Fatal(ex);
                        return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                //custcon.CloseConnection();
                kplog.Fatal(ex);
                return new CustomerResultResponse { respcode = 0, message = ex.ToString() };
            }

        }

        //Register
        [HttpPost]
        public AddKYCResponse addKYCGlobal(CustomerModel req)
        {

            log4net.Config.XmlConfigurator.Configure();
            String SenderFName = req.firstName;
            String SenderLName = req.lastName;
            String SenderMName = req.middleName;
            String SenderStreet = req.Street;
            String SenderCity = req.City;
            String SenderCountry = req.Country;
            String SenderGender = req.Gender;
            String SenderBirthdate = ConvertDateTime(req.BirthDate);
            String SenderBranchID = req.BranchID;
            String MobileNo = req.PhoneNo;
            String ZipCode = req.ZipCode;
            String activationCode = generateActivationCode();
            String Email = req.UserID;
            String Password = req.Password;
            String Name = string.Empty;
            String State = req.State;
            String Gender = req.Gender;
            String CreatedBy = req.CreatedBy;
            String PhoneNo = req.PhoneNo;
            String IDNo = req.IDNo;
            String IDType = req.IDType;
            String ExpiryDate = req.ExpiryDate;
            string strBase64 = req.strBase64Image;

            if (string.IsNullOrEmpty(strBase64) || string.IsNullOrEmpty(SenderFName) || string.IsNullOrEmpty(SenderLName) || string.IsNullOrEmpty(Email))
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
                                    return new AddKYCResponse { respcode = 0, message = "Customer already registered." };

                                }
                                else
                                {
                                    String filePath = ftp+"/PayNearMe/Images/" + getTimeStamp().ToString();
                                    uploadFileImage(strBase64, filePath);

                                    rdrUni.Close();

                                    custcommand.Parameters.Clear();
                                    custcommand.CommandText = "INSERT INTO kpcustomersglobal.PayNearMe"
                                                            + "(CustomerID, SignupDate, Password, UserID, FullName, PrivacyPolicyAgreement, ActivationCode,ImagePath) "
                                                            + "VALUES "
                                                            + "(@custID, NOW(), @Password, @UserID, @FullName, "
                                                            + ", @PrivacyPolicyAgreement, @activationCode,@ImagePath)";
                                    custcommand.Parameters.AddWithValue("custID", custid);
                                    custcommand.Parameters.AddWithValue("Password", Password);
                                    custcommand.Parameters.AddWithValue("UserID", Email);
                                    custcommand.Parameters.AddWithValue("FullName", Name);
                                    custcommand.Parameters.AddWithValue("PrivacyPolicyAgreement", true);
                                    custcommand.Parameters.AddWithValue("activationCode", activationCode);
                                    custcommand.Parameters.AddWithValue("ImagePath", filePath);
                                    custcommand.ExecuteNonQuery();
                                }

                                Int32 timestamp = getTimeStamp();
                                string yearofbirth = Convert.ToDateTime(SenderBirthdate).ToString("yyyy");

                                string queryAPI = "city=" + SenderCity + "&country=" + SenderCountry + "&first_name=" + SenderFName + "&last_name=" + SenderLName + "&middle_name=" + SenderMName + "&postal_code=" + ZipCode + "&site_identifier=" + siteIdentifier + "&site_user_identifier=" + custid + "&street=" + SenderStreet + "&timestamp=" + timestamp.ToString() +
                                                "&user_type=sender&version=2.0&year_of_birth=" + yearofbirth;



                                string signature = generateSignature(queryAPI);

                                queryAPI = queryAPI + "&signature=" + signature;

                                Uri uri = new Uri(server + "/json-api/create_user?" + queryAPI);

                                string res = SendRequest(uri);

                                dynamic data = JObject.Parse(res);

                                if (data.status == "ok")
                                {
                                    custtrans.Commit();
                                    custconn.Close();

                                    sendEmailActivation(Email, SenderFName, activationCode);

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

                        //string senderid = generateCustIDGlobal(custcommand);
                        String query = "select series from kpformsglobal.customerseries";
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

                        String filePath = ftp+"/PayNearMe/Images/" + getTimeStamp().ToString();
                        uploadFileImage(strBase64, filePath);

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

                        String insertCustomerDetails = "INSERT INTO kpcustomersglobal.customersdetails(CustID,HomeCity) values(@dcustid,@dhomecity)";
                        custcommand.CommandText = insertCustomerDetails;
                        custcommand.Parameters.Clear();
                        custcommand.Parameters.AddWithValue("dcustid", senderid);
                        custcommand.Parameters.AddWithValue("dhomecity", SenderCity);
                        custcommand.ExecuteNonQuery();

                        custcommand.Parameters.Clear();
                        custcommand.CommandText = "INSERT INTO kpcustomersglobal.PayNearMe"
                                                + "(CustomerID, SignupDate, Password, UserID, FullName, PrivacyPolicyAgreement, ActivationCode,ImagePath) "
                                                + "VALUES "
                                                + "(@custID, NOW(), @Password, @UserID, @FullName, "
                                                + "@PrivacyPolicyAgreement, @activationCode,@ImagePath)";
                        custcommand.Parameters.AddWithValue("custID", senderid);
                        custcommand.Parameters.AddWithValue("Password", Password);
                        custcommand.Parameters.AddWithValue("UserID", Email);
                        custcommand.Parameters.AddWithValue("FullName", Name);
                        custcommand.Parameters.AddWithValue("PrivacyPolicyAgreement", true);
                        custcommand.Parameters.AddWithValue("activationCode", activationCode);
                        custcommand.Parameters.AddWithValue("ImagePath", filePath);
                        custcommand.ExecuteNonQuery();



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

                        dynamic data = JObject.Parse(res);

                        if (data.status == "ok")
                        {
                            custtrans.Commit();
                            custconn.Close();
                            sendEmailActivation(Email, SenderFName, activationCode);
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
                        cmd.CommandText = "Select State , City from kpformsglobal.zipcodesG where ZipCode1 = @zipcode";
                        cmd.Parameters.AddWithValue("zipcode", zipCode);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        if (rdr.HasRows)
                        {

                            rdr.Read();
                            resp.State = rdr["State"].ToString();
                            resp.City = rdr["City"].ToString();
                            return new AddKYCResponse { respcode = 1, message = "Success", zCodeResp = resp };
                        }
                        else
                        {

                            rdr.Close();
                            return new AddKYCResponse { respcode = 0, message = "Invalid Zipcode" };
                        }

                    }

                }
            }
            catch (Exception ex)
            {

                return new AddKYCResponse { respcode = 0, message = "Error occured", ErrorDetail = ex.ToString() };
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

        private Boolean OfacMatch(String name)
        {
            Int32 Percentage = 100;

            using (MySqlConnection con = new MySqlConnection(dbconofac))
            {
                try
                {
                    con.Open();
                    using (MySqlCommand cmd = con.CreateCommand())
                    {


                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT * FROM( " +
                        "Select o.fullNAme, o.uid, o.firstName, o.lastName, o.sdnType,  split_str(split_str(split_str(o.dateOfBirthList,'dateOfBirth',2),'\":\"',2),'\",',1) AS dateOfBirth, split_str(split_str(split_str(o.placeOfBirthList,'placeOfBirth',2),'\":\"',2),'\",',1) AS placeofbirth, a.fullName as alias, o.soundexvalue, " +
                        "ROUND(JaroWinkler((o.fullName),@FullName)*100,0) as score1, " +
                        "ROUND(JaroWinkler((o.rfullName),@FullName)*100,0) as score2, " +
                        "ROUND(JaroWinkler((o.lastname),@FullName)*100,0) as score3, " +
                        "ROUND(JaroWinkler((o.firstname),@FullName)*100,0) as score4 " +
                        "FROM kpofacglobal.ofac o LEFT JOIN kpofacglobal.aliasofac a ON a.CustomerID = o.uid WHERE " +
                        "ROUND(JaroWinkler((o.fullName),@FullName)*100,0)>=@Percent OR " +
                        "ROUND(JaroWinkler((o.rfullName),@FullName)*100,0)>=@Percent OR " +
                        "ROUND(JaroWinkler((o.firstName),@FullName)*100,0)>=@Percent OR " +
                        "ROUND(JaroWinkler((o.lastName),@FullName)*100,0)>=@Percent " +
                        " UNION DISTINCT " +
                        "Select o.fullNAme, o.uid, o.firstName, o.lastName, o.sdnType, split_str(split_str(split_str(o.dateOfBirthList,'dateOfBirth',2),'\":\"',2),'\",',1) " +
                        " AS dateOfBirth, split_str(split_str(split_str(o.placeOfBirthList,'placeOfBirth',2),'\":\"',2),'\",',1) " +
                        " AS placeofbirth, a.fullName as alias, a.soundexvalue, " +
                        "ROUND(JaroWinkler((a.fullName),@FullName)*100,0) as score1, " +
                        "ROUND(JaroWinkler((a.rfullName),@FullName)*100,0) as score2, " +
                        "ROUND(JaroWinkler((a.lastname),@FullName)*100,0) as score3, " +
                        "ROUND(JaroWinkler((a.firstname),@FullName)*100,0) as score4 " +
                        "FROM kpofacglobal.ofac o LEFT JOIN kpofacglobal.aliasofac a ON a.CustomerID = o.uid WHERE " +
                        "ROUND(JaroWinkler((a.fullName),@FullName)*100,0)>=@Percent OR " +
                        "ROUND(JaroWinkler((a.rfullName),@FullName)*100,0)>=@Percent or " +
                        "ROUND(JaroWinkler((a.firstName),@FullName)*100,0)>=@Percent or " +
                        "ROUND(JaroWinkler((a.lastName),@FullName)*100,0)>=@Percent )as xx";

                        cmd.Parameters.AddWithValue("FullName", name);
                        cmd.Parameters.AddWithValue("Percent", Percentage);
                        MySqlDataReader rcvRdr = cmd.ExecuteReader();

                        if (rcvRdr.HasRows)
                        {
                            con.Close();
                            return true;
                        }
                        else
                        {
                            con.Close();
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
        }

        //LOGIN

        [HttpPost]
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
                                return new LoginResponse { respcode = 1, message = "Success", fullName = fullname, signupDate = signupDate, lastLogin = lastLogin, customer = customer };

                            }
                            else
                            {
                                rdrPass.Close();
                                con.Close();
                                return new LoginResponse { respcode = 0, message = "Invalid Password" };
                            }

                        }
                        else
                        {
                            rdr.Close();
                            con.Close();
                            return new LoginResponse { respcode = 0, message = "Email not yet registered" };
                        }

                    }
                }
            }
            catch (Exception ex)
            {


                return new LoginResponse { respcode = 0, message = ex.ToString() };
            }
        }

        //PENDING
        [HttpGet]
        public TransactionResponse getKPTNbyAccount(PendingTransaction model, string UserID)
        {
            //kptn = model.kptn;
            String fname = string.Empty;
            String lname = string.Empty;
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                //PendingTransaction model = new PendingTransaction();
                string customerid = string.Empty;
                con.Open();
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "Select CustomerID from kpcustomersglobal.PayNearMe where UserID = @userid";


                cmd.Parameters.AddWithValue("@userid", UserID);
                MySqlDataReader rdrPass = cmd.ExecuteReader();
                if (rdrPass.HasRows)
                {
                    rdrPass.Read();
                    customerid = rdrPass["CustomerID"].ToString();

                }

                rdrPass.Close();


                cmd.CommandText = "Select FirstName,LastName from kpcustomersglobal.customers where CustID = @custid";
                cmd.Parameters.AddWithValue("@custid", customerid);
                MySqlDataReader rdrPass1 = cmd.ExecuteReader();
                if (rdrPass1.HasRows)
                {
                    rdrPass1.Read();
                    fname = rdrPass1["FirstName"].ToString();
                    lname = rdrPass1["LastName"].ToString();
                }
                rdrPass1.Close();



                DataTable dt = new DataTable();
                cmd.Connection = con;
                cmd.CommandText = "SELECT KPTN, ControlNo, TransDate, FirstName, LastName, Amount, STATUS FROM kpadminlogsglobal.customerYtoZ where FirstName = '" + fname + "' AND LastName = '" + lname + "'";
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    reader.Close();

                    cmd.Parameters.Clear();
                    cmd.CommandText = "select KPTN, ControlNo, TransDate, FirstName, LastName, Amount, Status from kpadminlogsglobal.customerYtoZ where KPTN = @kptn";
                    cmd.Parameters.AddWithValue("@kptn", model.kptn);
                    MySqlDataReader rdr1 = cmd.ExecuteReader();
                    if (rdr1.HasRows)
                    {
                        rdr1.Close();
                        MySqlDataAdapter adap = new MySqlDataAdapter();
                        adap.SelectCommand = cmd;
                        adap.Fill(dt);
                        return new TransactionResponse { respcode = 1, table = dt };
                    }
                    else
                    {
                        return new TransactionResponse { respcode = 1, table = null };
                    }

                    //MySqlDataAdapter adap = new MySqlDataAdapter();
                    //adap.SelectCommand = cmd;
                    //adap.Fill(dt);
                    //return new LoginResponse { respcode = 1, table = dt };
                }
                else
                {
                    return new TransactionResponse { respcode = 0, table = null };
                }
            }





        }

        [HttpGet]
        public TransactionResponse getAllTransactionByAccount(string UserID)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                String fname = string.Empty;
                String lname = string.Empty;

                string customerid = string.Empty;
                con.Open();
                MySqlCommand cmd = con.CreateCommand();
                cmd.CommandText = "Select CustomerID from kpcustomersglobal.PayNearMe where UserID = @userid";

                cmd.Parameters.AddWithValue("@userid", UserID);
                MySqlDataReader rdrPass = cmd.ExecuteReader();
                if (rdrPass.HasRows)
                {
                    rdrPass.Read();
                    customerid = rdrPass["CustomerID"].ToString();

                }

                rdrPass.Close();


                cmd.CommandText = "Select FirstName,LastName from kpcustomersglobal.customers where CustID = @custid";
                cmd.Parameters.AddWithValue("@custid", customerid);
                MySqlDataReader rdrPass1 = cmd.ExecuteReader();
                if (rdrPass1.HasRows)
                {
                    rdrPass1.Read();
                    fname = rdrPass1["FirstName"].ToString();
                    lname = rdrPass1["LastName"].ToString();
                }
                rdrPass1.Close();



                DataTable dt = new DataTable();
                cmd.Connection = con;
                cmd.CommandText = "SELECT KPTN, ControlNo, TransDate, FirstName, LastName, Amount, STATUS FROM kpadminlogsglobal.customerYtoZ where FirstName = '" + fname + "' AND LastName = '" + lname + "'";
                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    int count = 0;
                    reader.Close();
                    MySqlDataAdapter adap = new MySqlDataAdapter();
                    adap.SelectCommand = cmd;
                    adap.Fill(dt);
                    count = dt.Rows.Count;
                    return new TransactionResponse { respcode = 1, table = dt, count = count };
                }
                else
                {
                    return new TransactionResponse { respcode = 0, table = null };
                }
            }
        }

        //profile
        [HttpPost]
        public ProfileResponse editProfile(CustomerModel model)
        {
            //select custid using email
            //use custid to change details in kpcustomersglobal


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
                            String bdate = Convert.ToDateTime(model.BirthDate).ToString("yyyy-MM-dd");
                            String expiryDate = Convert.ToDateTime(model.ExpiryDate).ToString("yyyy-MM-dd");
                            String dateformat = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
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

                            cmd.Parameters.Clear();
                            cmd.CommandText = "Update kpcustomersglobal.customersdetails set HomeCity = @City where custID = @CustID;";
                            cmd.Parameters.AddWithValue("City", model.City);
                            cmd.Parameters.AddWithValue("CustID", custid);
                            cmd.ExecuteNonQuery();


                            if (model.strBase64Image != "") { 
                            String filePath = ftp+"/PayNearMe/Images/" + getTimeStamp().ToString();
                            uploadFileImage(model.strBase64Image, filePath);

                            cmd.Parameters.Clear();
                            cmd.CommandText = "Update kpcustomersglobal.PayNearMe set ImagePath = @ImagePath where CustomerID = @CustID;";
                            cmd.Parameters.AddWithValue("ImagePath", filePath);
                            cmd.Parameters.AddWithValue("CustID", custid);
                            cmd.ExecuteNonQuery();

                            }


                            con.Close();
                            return new ProfileResponse { respcode = 1, message = "Profile Successfully Updated" };

                        }
                        else
                        {
                            rdr.Close();
                            con.Close();
                            return new ProfileResponse { respcode = 0, message = "No Profile Found!" };
                        }


                    }
                }
            }
            catch (Exception ex)
            {


                return new ProfileResponse { respcode = 0, message = ex.ToString() };
            }


        }

        [HttpPost]
        public ProfileResponse changePassword(ChangePasswordModel model)
        {
            try
            {
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
                            return new ProfileResponse { respcode = 1, message = "Successfully Changed Password!" };

                        }
                        else
                        {
                            return new ProfileResponse { respcode = 0, message = "Incorrect Password!" };
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
        public ProfileResponse getProfile(String UserID)
        {
            using (MySqlConnection con = new MySqlConnection(connection))
            {
                con.Open();

                using (MySqlCommand cmd = con.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "Select * from kpcustomersglobal.PayNearMe where UserID = @userID";
                    cmd.Parameters.AddWithValue("userID", UserID);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.HasRows)
                    {
                        rdr.Read();
                        String custID = rdr["CustomerID"].ToString();

                        rdr.Close();

                        cmd.Parameters.Clear();
                        cmd.CommandText = "Select FirstName,LastName,MiddleName,Street,ProvinceCity as State, Country,ZipCode,BirthDate,c.HomeCity as City,Gender, Mobile,b.UserID as UserID, b.ImagePath as filepath,b.CustomerID,a.IDNo,a.IDType,a.ExpiryDate from kpcustomersglobal.customers a inner join kpcustomersglobal.PayNearMe b on a.CustID = b.CustomerID inner join kpcustomersglobal.customersdetails c on a.CustID = c.CustID where a.custID = @custID";
                        cmd.Parameters.AddWithValue("custID", custID);
                        MySqlDataReader rdrProf = cmd.ExecuteReader();
                        if (rdrProf.HasRows)
                        {


                            rdrProf.Read();
                            String filepath = rdrProf["filepath"].ToString();
                            String bdate = rdrProf["BirthDate"].ToString();
                            if (bdate.StartsWith("00") || bdate == String.Empty)
                            {
                                bdate = "";
                            }
                            else
                            {
                                bdate = Convert.ToDateTime(rdrProf["BirthDate"]).ToString("MM/dd/yyyy");
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
                                    CustomerID = rdrProf["CustomerID"].ToString(),
                                    IDNo = rdrProf["IDNo"].ToString(),
                                    IDType = rdrProf["IDType"].ToString(),
                                    ExpiryDate = rdrProf["ExpiryDate"].ToString()
                                }
                                


                            };
                        }
                        else
                        {

                            rdrProf.Close();
                            return new ProfileResponse { respcode = 0, message = "Not yet registered" };

                        }
                    }
                    else
                    {
                        rdr.Close();
                        return new ProfileResponse { respcode = 0, message = "Not yet registered" };
                    }

                }


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

        private Object SendRequestPOST(Uri uri)
        {
            try
            {
           //     String res = null;
                HttpWebRequest web = (HttpWebRequest)WebRequest.Create(uri);
                web.Method = "POST";
                WebResponse webresponse = web.GetResponse();
                Stream response = webresponse.GetResponseStream();
                StreamReader reader = new StreamReader(response);
                XmlSerializer serializer = new XmlSerializer(typeof(Response));
                Response resp = (Response)serializer.Deserialize(reader);
                reader.Close();
                webresponse.Close();


                return resp;
            }
            catch (Exception ex)
            {
                //Kplog.Fatal(ex.ToString());
               throw new Exception("Unable to process request. The system encountered some technical problem. Sorry for the inconvenience. " + ex.ToString() + "");
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
                    return Convert.ToInt32(rdr["tstamp"]) + 54000;
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
                    string x = rdrserverdt["serverdt"].ToString().Substring(7, 7);
                    rdrserverdt.Close();
                    custconn.Close();
                    return x;

                }
            }
        }

        [HttpGet]
        public CustomerResultResponse resendActivationCode(String UserID)
        {

            String activationCode = generateActivationCode();


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
                        sendEmailActivation(UserID, FullName, activationCode);
                        return new CustomerResultResponse { respcode = 1, message = "Success" };
                    }
                    else
                    {
                        con.Close();
                        return new CustomerResultResponse { respcode = 0, message = "Failed" };
                    }


                }
                else
                {

                    con.Close();
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
                            return new AuthenticateResponse { respcode = 0, message = "Invalid Activation code" };
                        }

                    }
                    else
                    {
                        return new AuthenticateResponse { respcode = 0, message = "UserID does not exist!" };
                    }


                }
            }
            catch (Exception ex)
            {

                return new AuthenticateResponse { respcode = 0, message = ex.ToString() };
            }



        }

        private void sendEmailActivation(String userID, String firstName, String activationCode)
        {
            SmtpClient client = new SmtpClient();
            client.EnableSsl = true;
            client.UseDefaultCredentials = true;
            //client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Host = "email-smtp.us-east-1.amazonaws.com";
            client.Port = 587;
            client.Credentials = new NetworkCredential("AKIAI6QNPK53NGX6XJNQ", "An3KLx4RDIp/AOWGjNZoNIoZ1AwWvd0k+GtI7oekTg9E");
            MailMessage msg = new MailMessage();
            msg.To.Add(userID);

            msg.From = new MailAddress("ML Wallet Philippines<donotreply.mlwallet@mlhuillier.com>");
            msg.Subject = "PayNearMe - Email Activation";
            msg.Body = "Good day Ma'am/Sir " + firstName + ",<br /><br />"
                     + "With Mlhuillier as easy to send money to your freinds and family around<br />"
                     + "different parts of the world in a fast, convenient and secure way.<br /><br />"
                     + "Please confirm your e-mail address to activate Mlhuillier account.<br /><br />"
                     + "Activation Code : <b>" + activationCode + "</b>";
            msg.IsBodyHtml = true;
            try
            {
                client.Send(msg);

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
                req.Credentials = new NetworkCredential("", "");
                req.ContentLength = stream2.Length;
                Stream reqStream = req.GetRequestStream();
                stream2.Seek(0, SeekOrigin.Begin);
                stream2.CopyTo(reqStream);
                reqStream.Close();
            }
            catch (Exception ex)
            {

                kplog.Error(ex.ToString());
            }



        }

        private String downloadFileImage(string filepath)
        {

            WebClient ftpClient = new WebClient();
            ftpClient.Credentials = new NetworkCredential("", "");
            byte[] imageByte = ftpClient.DownloadData(filepath);
            String base64String = Convert.ToBase64String(imageByte);

            return base64String;
        }

        //[HttpPost]
        //public SendoutResponse sendoutGlobal(SendoutReq model)
        //{

        //    try
        //    {



        //        if (!authenticate(model.Username, model.Password))
        //        {
        //            kplog.Error("FAILED:: respcode: 7 message: " + getRespMessage(7));
        //            return new SendoutResponse { respcode = 7, message = getRespMessage(7) };
        //        }

        //        int xsave = 0;

        //        DateTime dt = getServerDateGlobal();
        //        int sr = Convert.ToInt32(model.transaction.series);
        //        String month = dt.ToString("MM");
        //        String tblorig = "sendout" + month + dt.ToString("dd");


        //        String controlno = model.transaction.controlno;
        //        String OperatorID = model.transaction.OperatorID;
        //        String station = string.Empty;
        //        String IsRemote = string.Empty;
        //        String RemoteBranch = string.Empty;
        //        String RemoteOperatorID = string.Empty;
        //        String RemoteReason = string.Empty;
        //        String RemoteBranchCode = string.Empty;
        //        Int32 remotezcode = 0;
        //        Int32 type = model.transaction.type;


        //        String ispassword = string.Empty;
        //        String transpassword = string.Empty;
        //        String purpose = string.Empty;
        //        String syscreatr = model.transaction.syscreator;
        //        String source = string.Empty;
        //        String currency = model.transaction.currency;
        //        Double principal = model.transaction.principal;
        //        Double charge = model.transaction.charge;
        //        Double othercharge = 0.00;
        //        Double redeem = 0.00;
        //        Double total = model.transaction.total;
        //        String promo = string.Empty;
        //        String relation = model.receiver.relation;
        //        String message = string.Empty;
        //        String idtype = string.Empty;
        //        String idno = string.Empty;
        //        String pocurrency = model.transaction.pocurrency;
        //        String paymenttype = model.transaction.paymenttype;
        //        String bankname = string.Empty;
        //        String cardcheckno = string.Empty;
        //        String cardcheckexpdate = string.Empty;
        //        Double exchangerate = model.transaction.exchangerate;
        //        Double poamount = model.transaction.poamount;
        //        String trxntype = model.transaction.trxntype;
        //        Int32 zonecode = model.transaction.zonecode;
        //        String bcode = model.transaction.branchcode;
        //        String orno = String.Empty;

        //        String SenderFName = model.sender.firstName;
        //        String SenderLName = model.sender.lastName;
        //        String SenderMName = model.sender.middleName;
        //        String SenderName = SenderLName + ", " + SenderFName + " " + SenderMName;
        //        String SenderStreet = model.sender.Street;
        //        String SenderProvinceCity = model.sender.City;
        //        String SenderCountry = model.sender.Country;
        //        String SenderGender = model.sender.Gender;
        //        String SenderContactNo = model.sender.PhoneNo;
        //        Int32 SenderIsSMS = 0;
        //        String SenderBirthDate = model.sender.BirthDate;
        //        String SenderBranchID = "";

        //        String ReceiverFName = model.receiver.firstName;
        //        String ReceiverLName = model.receiver.lastname;
        //        String ReceiverMName = model.receiver.midlleName;
        //        String ReceiverName = ReceiverLName + ", " + ReceiverFName + " " + ReceiverMName;
        //        String ReceiverStreet = model.receiver.street;
        //        String ReceiverProvinceCity = model.receiver.city;
        //        String ReceiverCountry = model.receiver.country;
        //        String ReceiverGender = model.receiver.gender;
        //        String ReceiverBirthDate = model.receiver.dateOfBirth;
        //        String ReceiverContactNo = model.receiver.phoneNo;



        //        using (MySqlConnection checkinglang = new MySqlConnection(connection))
        //        {
        //            checkinglang.Open();
        //            Int32 maxontrans = 0;
        //            try
        //            {
        //                using (command = checkinglang.CreateCommand())
        //                {
        //                    string checkifcontrolexist = "select controlno from " + generateTableNameGlobal(0, null) + " where controlno=@controlno";
        //                    command.CommandTimeout = 0;
        //                    command.CommandText = checkifcontrolexist;
        //                    command.Parameters.AddWithValue("controlno", controlno);
        //                    MySqlDataReader controlexistreader = command.ExecuteReader();
        //                    if (controlexistreader.HasRows)
        //                    {
        //                        controlexistreader.Close();

        //                        string getcontrolmax = "select max(substring(controlno,length(controlno)-5,length(controlno))) as max from " + generateTableNameGlobal(0, null) + " where if(isremote=1,remotebranch,branchcode) = @branchcode and stationid = @stationid and if(remotezonecode=0 or remotezonecode is null, zonecode,remotezonecode) =@zonecode";
        //                        command.CommandText = getcontrolmax;
        //                        command.Parameters.Clear();
        //                        command.Parameters.AddWithValue("branchcode", IsRemote == "1" ? RemoteBranchCode : bcode);
        //                        command.Parameters.AddWithValue("stationid", station);
        //                        command.Parameters.AddWithValue("zonecode", remotezcode == 0 ? zonecode : remotezcode);

        //                        MySqlDataReader controlmaxreader = command.ExecuteReader();
        //                        if (controlmaxreader.Read())
        //                        {
        //                            sr = Convert.ToInt32(controlmaxreader["max"].ToString()) + 1;
        //                        }
        //                        controlmaxreader.Close();

        //                        command.CommandText = "update kpformsglobal.control set nseries = @series where bcode = @bcode and station = @st and zcode = @zcode and type = @tp";
        //                        command.Parameters.Clear();
        //                        command.Parameters.AddWithValue("st", IsRemote == "1" ? "00" : station);
        //                        command.Parameters.AddWithValue("bcode", IsRemote == "1" ? RemoteBranchCode : bcode);
        //                        command.Parameters.AddWithValue("series", sr + 1 > 999999 ? 000001 : sr + 1);
        //                        command.Parameters.AddWithValue("zcode", remotezcode == 0 ? zonecode : remotezcode);
        //                        command.Parameters.AddWithValue("tp", type);
        //                        int abc101 = command.ExecuteNonQuery();

        //                        String xst = String.Empty;
        //                        String xbcode = String.Empty;
        //                        Int32 xzcode = 0;
        //                        if (IsRemote == "1")
        //                        {
        //                            xst = "00";
        //                            xbcode = RemoteBranch;
        //                        }
        //                        else
        //                        {
        //                            xst = station;
        //                            xbcode = bcode;
        //                        }

        //                        if (remotezcode == 0)
        //                            xzcode = zonecode;
        //                        else
        //                            xzcode = remotezcode;

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


        //        StringBuilder query = new StringBuilder("Insert into " + generateTableNameGlobal(0, null) + "(");
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

        //                try
        //                {
        //                    xsave = command.ExecuteNonQuery();
        //                    if (xsave < 1)
        //                    {
        //                        trans.Rollback();
                               
        //                        kplog.Error("Error in saving " + generateTableNameGlobal(0, null) + ":: respcode: 12 message: " + getRespMessage(12) + "ErrorDetail: Review parameters transStatus: Rollback");
                            
        //                        return new SendoutResponse { respcode = 12, message = getRespMessage(12), ErrorDetail = "Review paramerters." };
        //                    }
        //                    else
        //                    {
        //                        using (command = conn.CreateCommand())
        //                        {

        //                            kplog.Info("INSERT INTO " + generateTableNameGlobal(0, null) + ":: ");
                                  

        //                            dt = getServerDateGlobal();

        //                            String insert = "Insert into kptransactionsglobal.sendout" + month + " (controlno,kptnno,orno,operatorid," +
        //                            "stationid,isremote,remotebranch,remoteoperatorid,reason,ispassword,transpassword,purpose,isclaimed,iscancelled," +
        //                            "syscreated,syscreator,source,currency,principal,charge,othercharge,redeem,total,promo,senderissms,relation,message," +
        //                            "idtype,idno,expirydate,branchcode,zonecode,transdate,sendermlcardno,senderfname,senderlname,sendermname,sendername," +
        //                            "senderstreet,senderprovincecity,sendercountry,sendergender,sendercontactno,senderbirthdate,senderbranchid," +
        //                            "receiverfname,receiverlname,receivermname,receivername,receiverstreet,receiverprovincecity,receivercountry," +
        //                            "receivergender,receivercontactno,receiverbirthdate,vat,remotezonecode,tableoriginated,`year`,pocurrency,poamount,ExchangeRate," +
        //                            "paymenttype,bankname,cardcheckno,cardcheckexpdate,TransType) values (@controlno,@kptnno,@orno,@operatorid," +
        //                            "@stationid,@isremote,@remotebranch,@remoteoperatorid,@reason,@ispassword,@transpassword,@purpose,@isclaimed,@iscancelled," +
        //                            "@syscreated,@syscreator,@source,@currency,@principal,@charge,@othercharge,@redeem,@total,@promo,@senderissms,@relation,@message," +
        //                            "@idtype,@idno,@expirydate,@branchcode,@zonecode,@transdate,@sendermlcardno,@senderfname,@senderlname,@sendermname,@sendername," +
        //                            "@senderstreet,@senderprovincecity,@sendercountry,@sendergender,@sendercontactno,@senderbirthdate,@senderbranchid," +
        //                            "@receiverfname,@receiverlname,@receivermname,@receivername,@receiverstreet,@receiverprovincecity,@receivercountry," +
        //                            "@receivergender,@receivercontactno,@receiverbirthdate,@vat,@remotezonecode,@tableoriginated,@yr,@pocurrency,@poamount,@exchangerate," +
        //                            "@paymenttype,@bankname,@cardcheckno,@cardcheckexpdate,@transtype)";
        //                            command.CommandText = insert;

        //                            command.Parameters.AddWithValue("controlno", controlno);
        //                            command.Parameters.AddWithValue("kptnno", model.transaction.KPTN);
        //                            command.Parameters.AddWithValue("orno", orno);
        //                            command.Parameters.AddWithValue("operatorid", OperatorID);
        //                            command.Parameters.AddWithValue("stationid", station);
        //                            command.Parameters.AddWithValue("isremote", IsRemote);
        //                            command.Parameters.AddWithValue("remotebranch", RemoteBranch);
        //                            command.Parameters.AddWithValue("remoteoperatorid", RemoteOperatorID);
        //                            command.Parameters.AddWithValue("reason", RemoteReason);
        //                            command.Parameters.AddWithValue("ispassword", ispassword);
        //                            command.Parameters.AddWithValue("transpassword", transpassword);
        //                            command.Parameters.AddWithValue("purpose", purpose);
        //                            command.Parameters.AddWithValue("isclaimed", 0);
        //                            command.Parameters.AddWithValue("iscancelled", 0);
        //                            command.Parameters.AddWithValue("syscreated", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                            command.Parameters.AddWithValue("syscreator", syscreatr);
        //                            command.Parameters.AddWithValue("source", source);
        //                            command.Parameters.AddWithValue("currency", currency);
        //                            command.Parameters.AddWithValue("principal", principal);
        //                            command.Parameters.AddWithValue("charge", charge);
        //                            command.Parameters.AddWithValue("othercharge", othercharge);
        //                            command.Parameters.AddWithValue("redeem", redeem);
        //                            command.Parameters.AddWithValue("total", total);
        //                            command.Parameters.AddWithValue("promo", promo);
        //                            command.Parameters.AddWithValue("senderissms", SenderIsSMS);
        //                            command.Parameters.AddWithValue("relation", relation);
        //                            command.Parameters.AddWithValue("message", message);
        //                            command.Parameters.AddWithValue("idtype", idtype);
        //                            command.Parameters.AddWithValue("idno", idno);
        //                            command.Parameters.AddWithValue("expirydate", "");
        //                            command.Parameters.AddWithValue("branchcode", bcode);
        //                            command.Parameters.AddWithValue("zonecode", zonecode);
        //                            command.Parameters.AddWithValue("transdate", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                            command.Parameters.AddWithValue("sendermlcardno", "");
        //                            command.Parameters.AddWithValue("senderfname", SenderFName);
        //                            command.Parameters.AddWithValue("senderlname", SenderLName);
        //                            command.Parameters.AddWithValue("sendermname", SenderMName);
        //                            command.Parameters.AddWithValue("sendername", SenderLName + ", " + SenderFName + " " + SenderMName);
        //                            command.Parameters.AddWithValue("senderstreet", SenderStreet);
        //                            command.Parameters.AddWithValue("senderprovincecity", SenderProvinceCity);
        //                            command.Parameters.AddWithValue("sendercountry", SenderCountry);
        //                            command.Parameters.AddWithValue("sendergender", SenderGender);
        //                            command.Parameters.AddWithValue("sendercontactno", SenderContactNo);
        //                            command.Parameters.AddWithValue("senderbirthdate", SenderBirthDate);
        //                            command.Parameters.AddWithValue("senderbranchid", SenderBranchID);
        //                            command.Parameters.AddWithValue("receiverfname", ReceiverFName);
        //                            command.Parameters.AddWithValue("receiverlname", ReceiverLName);
        //                            command.Parameters.AddWithValue("receivermname", ReceiverMName);
        //                            command.Parameters.AddWithValue("receivername", ReceiverLName + ", " + ReceiverFName + " " + ReceiverMName);
        //                            command.Parameters.AddWithValue("receiverstreet", ReceiverStreet);
        //                            command.Parameters.AddWithValue("receiverprovincecity", ReceiverProvinceCity);
        //                            command.Parameters.AddWithValue("receivercountry", ReceiverCountry);
        //                            command.Parameters.AddWithValue("receivergender", ReceiverGender);
        //                            command.Parameters.AddWithValue("receivercontactno", ReceiverContactNo);
        //                            command.Parameters.AddWithValue("receiverbirthdate", ReceiverBirthDate);
        //                            command.Parameters.AddWithValue("chargeto", " ");
        //                            command.Parameters.AddWithValue("vat", model.transaction.vat);
        //                            command.Parameters.AddWithValue("remotezonecode", remotezcode);
        //                            command.Parameters.AddWithValue("tableoriginated", tblorig);
        //                            command.Parameters.AddWithValue("yr", dt.ToString("yyyy"));
        //                            command.Parameters.AddWithValue("pocurrency", pocurrency);
        //                            command.Parameters.AddWithValue("poamount", poamount);
        //                            command.Parameters.AddWithValue("exchangerate", exchangerate);
        //                            command.Parameters.AddWithValue("paymenttype", paymenttype);
        //                            command.Parameters.AddWithValue("bankname", bankname);
        //                            command.Parameters.AddWithValue("cardcheckno", cardcheckno);
        //                            command.Parameters.AddWithValue("cardcheckexpdate", cardcheckexpdate);
        //                            command.Parameters.AddWithValue("transtype", trxntype);
        //                            command.ExecuteNonQuery();

        //                            String xxscustid = String.Empty;
        //                            command.Parameters.Clear();
        //                            command.CommandText = "SELECT CustID FROM kpcustomersglobal.customers WHERE FirstName=@xfname AND LastName=@xlname AND MiddleName=@xmname AND DATE_FORMAT(Birthdate,'%Y-%m-%d')=@xbdate";
        //                            command.Parameters.AddWithValue("xfname", SenderFName);
        //                            command.Parameters.AddWithValue("xlname", SenderLName);
        //                            command.Parameters.AddWithValue("xmname", SenderMName);
        //                            command.Parameters.AddWithValue("xbdate", SenderBirthDate);
        //                            MySqlDataReader rdrcust = command.ExecuteReader();
        //                            if (rdrcust.Read())
        //                            {
        //                                xxscustid = rdrcust["CustID"].ToString();
        //                                rdrcust.Close();

        //                                command.Parameters.Clear();
        //                                command.CommandText = "UPDATE " + generateTableNameGlobal(0, null) + " SET SenderCustID=@xcustid WHERE KPTNNo=@xkptn";
        //                                command.Parameters.AddWithValue("xkptn", model.transaction.KPTN);
        //                                command.Parameters.AddWithValue("xcustid", xxscustid);
        //                                command.ExecuteNonQuery();
        //                            }
        //                            rdrcust.Close();

        //                        }
        //                    }
        //                }
        //                catch (MySqlException myyyx)
        //                {
        //                    //if (myyyx.Message.Contains("Duplicate"))
        //                    if (myyyx.Number == 1062)
        //                    {
        //                        command.Parameters.Clear();
        //                        if (IsRemote.Equals("1"))
        //                        {
        //                            command.CommandText = "update kpformsglobal.control set nseries = @series where bcode = @bcode and station = @st and zcode = @zcode and type = @tp";
        //                            command.Parameters.AddWithValue("st", "00");
        //                            command.Parameters.AddWithValue("bcode", RemoteBranch);
                                    
        //                            command.Parameters.AddWithValue("series", sr + 1 > 999999 ? 000001 : sr + 1);
        //                            command.Parameters.AddWithValue("zcode", remotezcode);
        //                            command.Parameters.AddWithValue("tp", type);
        //                            int x = command.ExecuteNonQuery();
        //                            if (x < 1)
        //                            {
        //                                kplog.Error("IsRemote: 1:: Error in updating control:: respcode: 12 message: " + getRespMessage(12) + " ErrorDetail: Review parameters.");
        //                                kplog.Error("FAILED:: MySql ErrorCode: " + myyyx.Number + " ErrorDetail: " + myyyx.ToString() + " transStatus: Rollback");
                                        
        //                                trans.Rollback();
                                        
        //                                return new SendoutResponse { respcode = 12, message = getRespMessage(12), ErrorDetail = "Review paramerters." };
        //                            }
        //                        }
        //                        else
        //                        {
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
        //                        }

        //                        kplog.Error("Error in saving transaction:: respcode: 13 message: Problem saving transaction. Please close the sendout window and open again. Thank you. ErrorDetail: Review parameters.");
        //                        kplog.Error("FAILED:: MySql ErrorCode: " + myyyx.Number + " ErrorDetail: " + myyyx.ToString() + " transStatus: Commit");
                                  
        //                        trans.Commit();
        //                        conn.Close();
        //                        return new SendoutResponse { respcode = 13, message = "Problem saving transaction. Please close the sendout window and open again. Thank you.", ErrorDetail = "Review parameters." };
        //                    }
        //                    else
        //                    {
        //                        if (myyyx.Number == 1213)
        //                        {
        //                            kplog.Error("FAILED:: respcode: 11 message: " + getRespMessage(11) + " MySql ErrorCode: " + myyyx.Number + " ErrorDetail: " + myyyx.ToString() + " transStatus: Rollback");
                                      
        //                            trans.Rollback();
                                      
        //                            return new SendoutResponse { respcode = 11, message = getRespMessage(11), ErrorDetail = "Problem occured during saving. Please resave the transaction." };
        //                        }
        //                        else
        //                        {
        //                            kplog.Error("FAILED:: respcode: 0 message: " + getRespMessage(0) + " MySql ErrorCode: " + myyyx.Number + " ErrorDetail: " + myyyx.ToString() + " transStatus: Rollback");
                                       
        //                            trans.Rollback();
                                     
        //                            return new SendoutResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = myyyx.ToString() };
        //                        }
        //                    }
        //                }

                  
        //                    command.CommandText = "update kpformsglobal.control set nseries = @series where bcode = @bcode and station = @st and zcode = @zcode and type = @tp";
        //                    command.Parameters.AddWithValue("st", station);
        //                    command.Parameters.AddWithValue("bcode", bcode);
        //                    //command.Parameters.AddWithValue("series", sr + 1);
        //                    command.Parameters.AddWithValue("series", sr + 1 > 999999 ? 000001 : sr + 1);
        //                    command.Parameters.AddWithValue("zcode", zonecode);
        //                    command.Parameters.AddWithValue("tp", type);
        //                    command.ExecuteNonQuery();
        //                    int x1 = command.ExecuteNonQuery();
        //                    if (x1 < 1)
        //                    {
        //                        kplog.Error("IsRemote: 0:: Error in updating control:: respcode: 12 message: " + getRespMessage(12) + " ErrorDetail: Review parameters. transStatus: Rollback");
        //                        trans.Rollback();
        //                        conn.Close();
        //                        return new SendoutResponse { respcode = 12, message = getRespMessage(12), ErrorDetail = "Review parameters." };
        //                    }
                        

        //                    updateResiboGlobal(bcode, zonecode, orno, ref command);
                        

        //                if (xsave == 1)
        //                {
        //                    String custS = getcustomertable(SenderLName);
        //                    command.Parameters.Clear();
        //                    command.CommandText = "kpadminlogsglobal.save_customers";
        //                    command.CommandType = CommandType.StoredProcedure;
        //                    command.Parameters.AddWithValue("tblcustomer", custS);
        //                    command.Parameters.AddWithValue("kptnno", model.transaction.KPTN);
        //                    command.Parameters.AddWithValue("controlno", controlno);
        //                    command.Parameters.AddWithValue("transdate", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                    command.Parameters.AddWithValue("fname", SenderFName);
        //                    command.Parameters.AddWithValue("lname", SenderLName);
        //                    command.Parameters.AddWithValue("mname", SenderMName);
        //                    command.Parameters.AddWithValue("sobranch", SenderBranchID);
        //                    command.Parameters.AddWithValue("pobranch", "");
        //                    command.Parameters.AddWithValue("isremote", IsRemote);
        //                    command.Parameters.AddWithValue("remotebranch", (RemoteBranch.Equals(DBNull.Value) ? null : RemoteBranch));
        //                    command.Parameters.AddWithValue("cancelledbranch", String.Empty);
        //                    command.Parameters.AddWithValue("status", 0);
        //                    command.Parameters.AddWithValue("syscreated", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                    command.Parameters.AddWithValue("syscreator", syscreatr);
        //                    command.Parameters.AddWithValue("customertype", "S");
        //                    command.Parameters.AddWithValue("amount", total);
        //                    command.ExecuteNonQuery();

        //                    String custR = getcustomertable(ReceiverLName);
        //                    command.Parameters.Clear();
        //                    command.CommandText = "kpadminlogsglobal.save_customers";
        //                    command.CommandType = CommandType.StoredProcedure;
        //                    command.Parameters.AddWithValue("tblcustomer", custR);
        //                    command.Parameters.AddWithValue("kptnno", model.transaction.KPTN);
        //                    command.Parameters.AddWithValue("controlno", controlno);
        //                    command.Parameters.AddWithValue("transdate", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                    command.Parameters.AddWithValue("fname", ReceiverFName);
        //                    command.Parameters.AddWithValue("lname", ReceiverLName);
        //                    command.Parameters.AddWithValue("mname", ReceiverMName);
        //                    command.Parameters.AddWithValue("sobranch", SenderBranchID);
        //                    command.Parameters.AddWithValue("pobranch", "");
        //                    command.Parameters.AddWithValue("isremote", IsRemote);
        //                    command.Parameters.AddWithValue("remotebranch", (RemoteBranch.Equals(DBNull.Value) ? null : RemoteBranch));
        //                    command.Parameters.AddWithValue("cancelledbranch", String.Empty);
        //                    command.Parameters.AddWithValue("status", 0);
        //                    command.Parameters.AddWithValue("syscreated", dt.ToString("yyyy-MM-dd HH:mm:ss"));
        //                    command.Parameters.AddWithValue("syscreator", syscreatr);
        //                    command.Parameters.AddWithValue("customertype", "R");
        //                    command.Parameters.AddWithValue("amount", total);
        //                    command.ExecuteNonQuery();

        //                    command.Parameters.Clear();
        //                    command.CommandText = "kpadminlogsglobal.savelog53";
        //                    command.CommandType = CommandType.StoredProcedure;

        //                    command.Parameters.AddWithValue("kptnno", model.transaction.KPTN);
        //                    command.Parameters.AddWithValue("action", "SENDOUT");
        //                    command.Parameters.AddWithValue("isremote", IsRemote);
        //                    command.Parameters.AddWithValue("txndate", dt);
        //                    command.Parameters.AddWithValue("stationcode", string.Empty);
        //                    command.Parameters.AddWithValue("stationno", station);
        //                    command.Parameters.AddWithValue("zonecode", zonecode);
        //                    command.Parameters.AddWithValue("branchcode", bcode);
        //                    command.Parameters.AddWithValue("operatorid", OperatorID);
        //                    command.Parameters.AddWithValue("cancelledreason", DBNull.Value);
        //                    command.Parameters.AddWithValue("remotereason", RemoteReason);
        //                    command.Parameters.AddWithValue("remotebranch", (RemoteBranchCode.Equals(DBNull.Value)) ? null : RemoteBranchCode);
        //                    command.Parameters.AddWithValue("remoteoperator", (RemoteOperatorID.Equals(DBNull.Value)) ? null : RemoteOperatorID);
        //                    command.Parameters.AddWithValue("oldkptnno", DBNull.Value);
        //                    command.Parameters.AddWithValue("remotezonecode", remotezcode);
        //                    command.Parameters.AddWithValue("type", "N");
        //                    command.ExecuteNonQuery();

        //                    kplog.Info("INSERT SENDER INFO LOGS:: tblcustomer: " + custS + " kptn: " + model.transaction.KPTN + " controlno: " + controlno + " transdate: " + dt.ToString("yyyy-MM-dd HH:mm:ss") + " fname: " + SenderFName + " lname: " + SenderLName + " mname: " + SenderMName + " SObranch: " + SenderBranchID + " pobranch: isremote: " + IsRemote + " remotebranch: " + (RemoteBranch.Equals(DBNull.Value) ? null : RemoteBranch) + " cancelledbranch: status: 0 syscreated: " + dt.ToString("yyyy-MM-dd HH:mm:ss") + " syscreator: " + syscreatr + " customertype: S amount: " + total);
        //                    kplog.Info("INSERT RECEIVER INFO LOGS:: tblcustomer: " + custR + " kptn: " + model.transaction.KPTN + " controlno: " + controlno + " transdate: " + dt.ToString("yyyy-MM-dd HH:mm:ss") + " fname: " + ReceiverFName + " lname: " + ReceiverLName + " mname: " + ReceiverMName + " SObranch: " + SenderBranchID + " pobranch: isremote: " + IsRemote + " remotebranch: " + (RemoteBranch.Equals(DBNull.Value) ? null : RemoteBranch) + " cancelledbranch: status: 0 syscreated: " + dt.ToString("yyyy-MM-dd HH:mm:ss") + " syscreator: " + syscreatr + " customertype: R amount: " + total);
        //                    kplog.Info("INSERT TRANSACTION LOGS:: kptnno: " + model.transaction.KPTN + " action: SENDOUT isremote: " + IsRemote + " txndate: " + dt + " stationcode: " + "" + " stationno: " + station + " zonecode: " + zonecode + " branchcode: " + bcode + " operatorid: " + OperatorID + " cancelledreason: " + DBNull.Value + " remotereason: " + RemoteReason + " remotebranch: " + (RemoteBranchCode.Equals(DBNull.Value) ? null : RemoteBranchCode) + " remoteoperator: " + (RemoteOperatorID.Equals(DBNull.Value) ? null : RemoteOperatorID) + " oldkptnno: " + DBNull.Value + " remotezonecode: " + remotezcode + " type: N");
        //                }

        //                kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1) + " kptn: " + model.transaction.KPTN + " orno: " + orno + " transdate: " + dt + " transStatus: Commit");
                       
        //                trans.Commit();
        //                conn.Close();
        //                return new SendoutResponse { respcode = 1, message = getRespMessage(1), kptn = model.transaction.KPTN, orno = orno, transdate = dt };

        //                }

        //            }
        //            catch (MySqlException myx)
        //            {
        //                if (myx.Number == 1213)
        //                {
        //                    kplog.Error("FAILED:: respcode: 11 message: " + getRespMessage(11) + " MySql ErrorCode: " + myx.Number + " ErrorDetail: " + myx.ToString() + " transStatus: Rollback");
        //                    trans.Rollback();
        //                    return new SendoutResponse { respcode = 11, message = getRespMessage(11), ErrorDetail = "Problem occured during saving. Please resave the transaction." };
        //                }
        //                else
        //                {
        //                    kplog.Error("FAILED:: respcode: 0 message: " + getRespMessage(0) + " MySql ErrorCode: " + myx.Number + " ErrorDetail: " + myx.ToString() + " transStatus: Rollback");
        //                    trans.Rollback();
        //                    return new SendoutResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = myx.ToString() };
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                kplog.Error("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString() + " transStatus: Rollback");
        //                trans.Rollback();
        //                return new SendoutResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        kplog.Error("FAILED:: respcode: 0 message: " + getRespMessage(0) + " ErrorDetail: " + ex.ToString() + " transStatus: Rollback");
        //        return new SendoutResponse { respcode = 0, message = getRespMessage(0), ErrorDetail = ex.ToString() };
        //    }
        //}

        [HttpPost]
        public CreateOrderResponse sendoutTemp(SendoutRequest model)
        {
            try
            {
                DateTime dt = getServerDateGlobal();
                
                model.transaction.Status = "PENDING";
                model.transaction.TransDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                String Month = dt.ToString("MM");
                model.transaction.Principal = Math.Round(model.transaction.Principal, 2);
                
                using (MySqlConnection con = new MySqlConnection(connection)) 
                {
                   

                    con.Open();

                    MySqlTransaction trans = con.BeginTransaction(IsolationLevel.ReadCommitted);


                    using (MySqlCommand cmd = con.CreateCommand()) 
                    {
                        cmd.Transaction = trans;
                        cmd.Parameters.Clear();
                        cmd.CommandText = "INSERT INTO kptransactionsglobal.paynearme" + Month + " (KPTN,TransDate,SenderCustID,SenderFName,SenderMName,SenderLName,ReceiverCustID,ReceiverFName,ReceiverMName,ReceiverLName,Principal,Charge,OtherCharge,ExchangeRate,Vat,PayoutAmount,PayoutAmountPHP,Status,Total) " +
                                            "VALUES(@KPTN,@TransDate,@sCustID,@sFName,@sMName,@sLName,@rCustID,@rFName,@rMName,@rLName,@Principal,@Charge,@OtherCharge,@ExchangeRate,@Vat,@POAmount,@POAmountPHP,@Status,@Total);";
                        cmd.Parameters.AddWithValue("KPTN", model.transaction.KPTN);
                        cmd.Parameters.AddWithValue("TransDate", model.transaction.TransDate);
                        cmd.Parameters.AddWithValue("sFName", model.sender.firstName);
                        cmd.Parameters.AddWithValue("sMName", model.sender.middleName);
                        cmd.Parameters.AddWithValue("sLName", model.sender.lastName);
                        cmd.Parameters.AddWithValue("sCustID", model.sender.CustomerID);
                        cmd.Parameters.AddWithValue("rFName", model.receiver.firstName);
                        cmd.Parameters.AddWithValue("rMName", model.receiver.midlleName);
                        cmd.Parameters.AddWithValue("rLName", model.receiver.lastname);
                        cmd.Parameters.AddWithValue("rCustID", model.receiver.receiverCustID);
                        cmd.Parameters.AddWithValue("Principal", model.transaction.Principal);
                        cmd.Parameters.AddWithValue("Charge", model.transaction.Charge);
                        cmd.Parameters.AddWithValue("OtherCharge", model.transaction.OtherCharge);
                        cmd.Parameters.AddWithValue("ExchangeRate", model.transaction.ExchangeRate);
                        cmd.Parameters.AddWithValue("POAmount", model.transaction.POAmount);
                        cmd.Parameters.AddWithValue("POAmountPHP", model.transaction.POAmountPHP);
                        cmd.Parameters.AddWithValue("Total", model.transaction.Total);
                        cmd.Parameters.AddWithValue("Vat", model.transaction.vat);
                        cmd.Parameters.AddWithValue("Status", model.transaction.Status);

                        int x = cmd.ExecuteNonQuery();
                        if (x > 0)
                        {

                            string queryAPI = "order_amount=" + model.transaction.Total + "&order_currency=USD&order_type=exact&receiver_user_identifier=" + model.receiver.receiverCustID + "&return_html_slip=true&sender_user_identifier=" + model.sender.CustomerID + "&site_customer_email=" + model.sender.UserID + "&site_customer_identifier=" + model.sender.CustomerID + "&site_customer_phone=" + model.sender.PhoneNo + "&site_identifier=" + siteIdentifier + "&site_order_description=" + "test only" + "&site_order_identifier=" + model.transaction.KPTN + "&timestamp=" + getTimeStamp().ToString() +
                                                "&version=2.0";

                            //string queryAPI = "order_amount=" + model.transaction.Principal + "&order_currency=USD&order_type=exact&return_html_slip=true&site_customer_email=" + model.sender.UserID + "&site_customer_identifier=" + model.transaction.KPTN + "&site_customer_phone=" + model.sender.PhoneNo + "&site_identifier=" + siteIdentifier + "&site_order_description=" + "test only" + "&site_order_identifier=" + model.transaction.KPTN + "&timestamp=" + getTimeStamp().ToString() +
                            //                    "&version=2.0";

               
                            string signature = generateSignature(queryAPI);

                            queryAPI = queryAPI + "&signature=" + signature;

                            Uri uri = new Uri(server + "/json-api/create_order?" + queryAPI);

                            string res = SendRequest(uri);

                            CreateOrderResponse response = new CreateOrderResponse();

                            dynamic data = JsonConvert.DeserializeObject(res, typeof(CreateOrderResponse));

                            response = data;
                           if (response.status == "ok")
                            {
                                trans.Commit();
                                kplog.Info("SUCCESS:: respcode: 1 message: " + getRespMessage(1));
                                return response;
                            }
                            else
                            {
                                trans.Rollback();
                                con.Close();
                                return response;
                            }


                            
                        }
                        else 
                        {
                            List<Error> err = new List<Error>();
                            err.Add(new Error { 
                            
                                description = "Error inserting global db"
                            });
                            return new CreateOrderResponse { status = "error", errors = err };
                        }

                    }
                
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

                using (MySqlConnection con = new MySqlConnection(connection)) 
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
        public getbranchratesresponse GetBranchRates(String bcode, String zone, String classification, String currency)
        {
            kplog.Info("SUCCES:: message:: bcode: " + bcode + " zone: " + zone + " classification: " + classification + " currency: " + currency);
            try
            {
                String sql = String.Empty;
                String sqlmanual = String.Empty;
                String sqlchk = String.Empty;
                String remarks = String.Empty;
                Int32 identifier = 0;
                DateTime? effectivedate = null;

                sql = "call mlforexrate.sp_getbranchrates ('" + bcode + "', '" + zone + "', '" + currency + "', '" + classification + "');";
                sqlmanual = "SELECT b.branchname,b.branchcode,bm.curr_sell as selling,bm.curr_buy as buying,@pcur as currency FROM mlforexrate.brachrateclassification b INNER JOIN mlforexrate.branchforexmanual bm ON bm.branchcode = b.branchcode and bm.zonecode = b.zonecode WHERE bm.branchcode = @pbcode and bm.zonecode = @pzcode";
                sqlchk = "SELECT remarks, identifier, IF(effectivedate IS NULL,NULL,DATE_FORMAT(effectivedate,'%Y-%m-%d %H:%i:%s')) AS effectivedate FROM mlforexrate.branchforextagrates WHERE branchcode = @bcode and zonecode = @zcode";

                using (MySqlConnection con = new MySqlConnection(connection)) 
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
        //endForex


        //CHARGING
       
        private ChargeResponse calculateChargeGlobal(Double amount, String bcode, String zcode)
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
                    using (command = conn.CreateCommand())
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

        [HttpGet]
        public ChargeResponse getCharge(Double amount, String bcode, String zcode) 
        {
            var chargeGResp = calculateChargePerBranchGlobal(amount, bcode, zcode);
            if (chargeGResp.respcode == 1) 
            {

                return chargeGResp;

            }
            else if (chargeGResp.respcode == 16)
            {
                var response = calculateChargeGlobal(amount, bcode, zcode);
                if (response.charge == 0 || response.charge == null)
                {
                    return new ChargeResponse { respcode = 1, message = "Success", charge = 0 };
                 

                }
                else
                {
                    return response;
                }

            }
            else 
            {

                return chargeGResp;
            }
        }

        //end Charging
        public String generateTableNameGlobal(Int32 type, String TransDate)
        {
            //DateTime dt = getServerDate(false);

            if (TransDate == null)
            {
                if (type == 0)
                {
           //         kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.sendout" : "kpglobal.sendout" + dt.ToString("MM") + dt.ToString("dd")));
                    return "paynearme.sendout" + dt.ToString("MM") + dt.ToString("dd");
                }
                else if (type == 1)
                {
                  //  kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.payout" : "kpglobal.payout" + dt.ToString("MM") + dt.ToString("dd")));
                    return "paynearme.payout" + dt.ToString("MM") + dt.ToString("dd");
                }
                else if (type == 2)
                {
                  //  kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.tempkptn" : "kpglobal.tempkptn"));
                    return "paynearme.tempkptn";
                }
                else
                {
                    kplog.Error("FAILED:: message: Invalid transaction type");
                    throw new Exception("Invalid transaction type");
                }
            }
            else
            {
                DateTime TransDatetoDate = Convert.ToDateTime(TransDate);
                if (type == 0)
                {
                  //  kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.sendout" : "kpglobal.sendout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd")));
                    return "paynearme.sendout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd");
                }
                else if (type == 1)
                {
                   // kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.payout" : "kpglobal.payout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd")));
                    return "paynearme.payout" + TransDatetoDate.ToString("MM") + TransDatetoDate.ToString("dd");
                }
                else if (type == 2)
                {
                  //  kplog.Info("SUCCESS:: TableGlobal: " + ((isUse365Global == 0) ? "kpglobal.tempkptn" : "kpglobal.tempkptn"));
                    return "paynearme.tempkptn";
                }
                else
                {
                    kplog.Error("FAILED:: message: Invalid transaction type");
                    throw new Exception("Invalid transaction type");
                }
            }
        }

        private String generateResiboGlobal(string branchcode, Int32 zonecode, MySqlCommand command)
        {
            try
            {

                dt = getServerDateGlobal(true);
                string query = "select oryear,branchcode,zonecode,series from kpformsglobal.resibo where branchcode = @bcode1 and zonecode = @zcode1 FOR UPDATE";
                command.CommandText = query;
                command.Parameters.AddWithValue("bcode1", branchcode);
                command.Parameters.AddWithValue("zcode1", zonecode);

                using (MySqlDataReader dataReader = command.ExecuteReader())
                {
                    if (dataReader.HasRows)
                    {
                        dataReader.Read();
                        Int32 series = Convert.ToInt32(dataReader["series"]) + 1;
                        String oryear = dataReader["oryear"].ToString().Substring(2);
                        dataReader.Close();

                        kplog.Info("Generate receipt:: receipt: " + dt.ToString("yy") + "-" + series.ToString().PadLeft(6, '0'));
                        kplog.Info("SUCCESS generating receipt");
                        return dt.ToString("yy") + "-" + series.ToString().PadLeft(6, '0');
                    }
                    else
                    {
                        dataReader.Close();
                        command.Parameters.Clear();
                        command.CommandText = "update kpformsglobal.resibo set `lock` = 1 where branchcode = @bcode2 and zonecode = @zcode2";
                        command.Parameters.AddWithValue("bcode2", branchcode);
                        command.Parameters.AddWithValue("zcode2", zonecode);
                        command.ExecuteNonQuery();

                        command.Parameters.Clear();
                        command.CommandText = "insert into kpformsglobal.resibo (oryear, branchcode, zonecode, series) values (@year, @bcode2, @zcode2, @ser)";
                        command.Parameters.AddWithValue("year", dt.ToString("yyyy"));
                        command.Parameters.AddWithValue("bcode2", branchcode);
                        command.Parameters.AddWithValue("zcode2", zonecode);
                        command.Parameters.AddWithValue("ser", 1);
                        command.ExecuteNonQuery();
                        int ser = 1;

                        kplog.Info("UPDATE kpformsglobal.resibo:: lock: 1 WHERE branchcode: " + branchcode + " zonecode: " + zonecode);
                        kplog.Info("INSERT INTO kpformsglobal.resibo:: year: " + dt.ToString("yyyy") + " bcode2: " + branchcode + " zcode2: " + zonecode + " ser: 1");
                        kplog.Info("Generate receipt:: receipt: " + dt.ToString("yy") + "-" + ser.ToString().PadLeft(6, '0'));
                        kplog.Info("SUCCESS generating receipt");

                        return dt.ToString("yy") + "-" + ser.ToString().PadLeft(6, '0');
                    }
                }

            }
            catch (MySqlException myx)
            {
                kplog.Error("FAILED:: ErrorDetail: " + myx.ToString());
                throw new Exception(myx.ToString());
            }
            catch (Exception ex)
            {
                kplog.Error("FAILED:: ErrorDetail: " + ex.ToString());
                throw new Exception(ex.ToString());
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

        public String getcustomertable(String lastname)
        {
            String customers = "";
            lastname.ToUpper();
            if (lastname.StartsWith("A") || lastname.StartsWith("B") || lastname.StartsWith("C"))
            {
                customers = "AtoC";
            }
            else if (lastname.StartsWith("D") || lastname.StartsWith("E") || lastname.StartsWith("F"))
            {
                customers = "DtoF";
            }
            else if (lastname.StartsWith("G") || lastname.StartsWith("H") || lastname.StartsWith("I"))
            {
                customers = "GtoI";
            }
            else if (lastname.StartsWith("J") || lastname.StartsWith("K") || lastname.StartsWith("L"))
            {
                customers = "JtoL";
            }
            else if (lastname.StartsWith("M") || lastname.StartsWith("N") || lastname.StartsWith("O"))
            {
                customers = "MtoO";
            }
            else if (lastname.StartsWith("P") || lastname.StartsWith("Q") || lastname.StartsWith("R"))
            {
                customers = "PtoR";
            }
            else if (lastname.StartsWith("S") || lastname.StartsWith("T") || lastname.StartsWith("U"))
            {
                customers = "StoU";
            }
            else if (lastname.StartsWith("V") || lastname.StartsWith("W") || lastname.StartsWith("X"))
            {
                customers = "VtoX";
            }
            else if (lastname.StartsWith("Y") || lastname.StartsWith("Z"))
            {
                customers = "YtoZ";
            }

            kplog.Info("SUCCESS:: TableCustomer: " + customers);
            return customers;
        }

        private Boolean updateResiboGlobal(string branchcode, Int32 zonecode, String resibo, ref MySqlCommand command)
        {
            try
            {
                MySqlCommand cmdReader;
                using (cmdReader = new MySqlConnection(connection).CreateCommand())
                {

                    dt = getServerDateGlobal(true);

                    Int32 series = Convert.ToInt32(resibo.Substring(3, resibo.Length - 3));

                    command.Parameters.Clear();
                    command.CommandText = "update kpformsglobal.resibo set series = @series where branchcode = @bcode2 and zonecode = @zcode2";
                    command.Parameters.AddWithValue("bcode2", branchcode);
                    command.Parameters.AddWithValue("zcode2", zonecode);
                    command.Parameters.AddWithValue("series", series);
                    command.ExecuteNonQuery();
                    command.Parameters.Clear();

                    kplog.Info("UPDATE receipt:: branchcode: " + branchcode + " zonecode: " + zonecode + " series: " + series);
                    kplog.Info("SUCCESS updating receipt");
                    return true;
                }
            }
            catch (MySqlException myx)
            {
                kplog.Error("FAILED:: ErrorDetail: " + myx.ToString());
                throw new Exception(myx.ToString());
            }
            catch (Exception ex)
            {
                kplog.Error("FAILED:: ErrorDetail: " + ex.ToString());
                throw new Exception(ex.ToString());
            }
        }
    
    
        //CALLBACKS

        [HttpPost]
        public payment_authorization_response authorize(string due_to_site_amount, string due_to_site_currency, 
            string net_payment_amount, string net_payment_currency, string payment_amount, string payment_currency,
            string payment_date, string payment_latitude, string payment_longitude, string pnm_order_identifier, string pnm_payment_identifier, string pnm_withheld_amount,
            string pnm_withheld_currency, string retailer_location_address, string retailer_location_identifier, string retailer_name, string signature, string site_customer_identifier, string site_identifier,
            string site_order_identifier,string test, string timestamp, string version) 

        {
            return new payment_authorization_response
            {

                authorization = new payment_authorization_responseAuthorization
                {
                    accept_payment = payment_authorization_responseAuthorizationAccept_payment.yes,
                    site_payment_identifier = "test-1234567890",
                    pnm_order_identifier = pnm_order_identifier,
                    pnm_payment_identifier = pnm_payment_identifier,
                    decline_reason="",
                },
                version = "2.0"
            };

            //return "asdasdsa";
        }

       [HttpPost]
        public payment_confirmation_response confirm(string due_to_site_amount, string due_to_site_currency, string net_payment_amount, string net_payment_currency,
           string order_payee_identifier, string payment_amount, string payment_latitude, string payment_longitude,
           string payment_timestamp, string pnm_order_identifier, string pnm_payment_identifier, string pnm_withheld_amount, string pnm_withheld_currency, string retailer_location_address,
           string retailer_location_identifier, string retailer_name, string signature,
           string site_customer_identifier, string site_identifier, string site_order_identifier, string standin,
           string status, string test, string timestamp,
           string version)
        {

            return new payment_confirmation_response
            {
                confirmation = new payment_confirmation_responseConfirmation 
                {
                    pnm_order_identifier = pnm_order_identifier
                },
                version = "2.0"
            };
        }

       [HttpGet]
       public Response test() 
       {

           string query = "?username=che.pada&password=Temp1234$&firstName=test&lastName=TEst&address=test&zip=90001";
            Uri uri = new Uri("https://web.idologylive.com/api/idiq.svc"+query);
            Object response = SendRequestPOST(uri);

            return (Response)response;
       }

    
    }

    public class Response 
    {
        public String error { get; set; }
    }
}
