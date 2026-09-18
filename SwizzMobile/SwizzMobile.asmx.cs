using log4net;
using Newtonsoft.Json;
using SwizzMobile.Clients;
using SwizzMobile.Models;
using SwizzMobile.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Web;
using System.Web.Services;
using static SwizzMobile.Clients.BambuiWS;
//using static SwizzMobile.Clients.POLYTECH;

namespace SwizzMobile
{
    [WebService(Namespace = "http://swizzkash.co.ke/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    public class SwizzMobile : System.Web.Services.WebService
    {
        public static readonly ILog iLog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string res = "Thank you, SwizzKASH";
        public static string newline = "&#x000a;";
        public SwizzRequest request = new SwizzRequest();
        public CLanguage lang = new CLanguage();


        [WebMethod]
        public string USSD(string MSISDN, string SESSION_ID, string SERVICE_CODE, string USSD_STRING, string SACCO_CODE)
        {
            string IMSI = "";
            try
            {
                //  iLog.Info("test--------" + end);
                request.MSISDN = "+" + MSISDN;
                request.SESSIONID = SESSION_ID;
                request.SERVICECODE = SERVICE_CODE;
                request.USSDSTRING = USSD_STRING;
                request.SACCO_CODE = SACCO_CODE;
                request.IMSI = IMSI;
                lang.request = request;

                LogRequest(request);
                res = Menu(request); // ** the rest of s-mobile


            }
            catch (Exception ex)
            {
                iLog.Info(">>>>" + USSD_STRING + ">>>>" + MSISDN + ">>>>" + SERVICE_CODE + " error" + ex.Message);
            }
            SwizzRequest.savetologs(request, res);
            return res;
        }
        public string Menu(SwizzRequest request)
        {

            string res = "Thank you, SwizzKASH";

            using (SwizzRequest.db = new SwizzKash_Mobile_DBEntities())
            {
                try
                {
                    SwizzRequest.GetSession(ref request);
                    if (request.session == null)
                    {
                        request.transactionType = TRANSACTION_TYPE.Menu;
                        SwizzRequest.SaveRequest(request);
                    }
                    return route(ref request);
                }
                catch (Exception ex)
                {
                    iLog.Info("two" + ex);
                }
                finally
                {
                    string[] ussd_string = request.USSDSTRING.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                    if (request.transactionType == TRANSACTION_TYPE.Menu)
                    {
                        SwizzRequest.SaveRequest(request);
                    }
                    else if (ussd_string.Length >= 1)
                    {
                        if (request.menucount > 0)
                        {
                            SwizzRequest.NextUpdate(request);
                        }
                        else
                        {
                            if (request.goback == false)
                                SwizzRequest.Updaterequest(request);
                        }
                    }
                    SwizzRequest.db.SaveChanges();
                }
            }
            return res;
        }
        private string route(ref SwizzRequest request)
        {
            SwizzRequest req = request;

            switch (request.transactionType)
            {
                case TRANSACTION_TYPE.Menu:
                    if (request.session == null)
                    {
                        string[] ussd_stringClient = request.USSDSTRING.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);

                        var custClient = new List<Customer>();
                        Customer ctomclient = new Customer();
                        ctomclient.Client = ussd_stringClient[0];
                        ctomclient.Telephone = request.MSISDN;
                        ctomclient.Active = true;
                        custClient.Add(ctomclient);

                        Ussd ussd = new Ussd();
                        ussd.Phone = request.MSISDN;
                        ussd.SACCO_CODE = request.SACCO_CODE;
                        ussd.Active = false;
                        ussd.Client = ussd_stringClient[0];
                        ussd.OPTION = request.USSDSTRING;
                        ussd.USSD_STRING = request.USSDSTRING;
                        ussd.Menu_Count = 0;

                        request.customer = custClient.ElementAt(0);
                        request.transactionType = TRANSACTION_TYPE.Menu;
                        request.session = ussd;
                        request.USSDSTRING = ussd_stringClient[0];
                        SwizzRequest.SaveRequest(request);
                        SwizzRequest.db.SaveChanges();
                    }

                    res = BambuiWS.ProcessRequest(request);

                    break;
                default:
                    {
                        string[] ussd_stringClient = request.USSDSTRING.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);

                        var custClient = new List<Customer>();
                        Customer ctomclient = new Customer();
                        ctomclient.Client = ussd_stringClient[0];
                        ctomclient.Telephone = request.MSISDN;
                        ctomclient.SaccoCode = request.SACCO_CODE;
                        ctomclient.Active = true;
                        custClient.Add(ctomclient);

                        //Ussd ussd = new Ussd();
                        //ussd.Phone = request.MSISDN;
                        //ussd.Active = false;
                        //ussd.Client = ussd_stringClient[0];
                        //ussd.OPTION = request.USSDSTRING;
                        //ussd.USSD_STRING = request.USSDSTRING;
                        //ussd.Menu_Count = 0;

                        request.customer = custClient.ElementAt(0);
                        //request.session = ussd;
                        //request.USSDSTRING = ussd_stringClient[0];
                        SwizzRequest.SaveRequest(request);
                        SwizzRequest.db.SaveChanges();

                        request.Entry = request.session.Entry;

                        res = BambuiWS.ProcessRequest(request);

                        break;
                    }
            }
            return res;
        }
        private static void LogRequest(SwizzRequest request)
        {
            SwizzRequest.LogEntryOnFile(request.SESSIONID, $"Received request: {JsonConvert.SerializeObject(request)}");
        }

        [WebMethod]
        public string PaybillTransaction(string traceid, string documentno, string accountno, string accountname, string phonenumber, decimal amountpaidin, decimal saccopaybillbalance, DateTime transactiondate, string transactiontype, string narration)
        {
            var resp = Config.service.PaybillTransaction(traceid, documentno, accountno, accountname, phonenumber, amountpaidin, saccopaybillbalance, transactiondate, transactiontype, narration);
            return resp;
        }

        //[WebMethod]
        public string WithdrawRequest(string account, decimal amount, string traceid, string phonenumber, DateTime transactionDate)
        {
            var response = string.Empty;
            try
            {
                response = Config.service.WithdrawRequest(account, amount, traceid, phonenumber, transactionDate);
            }
            catch (Exception ex)
            {
                response = "";
            }
            return response;
        }

        [WebMethod]
        public string WithdrawCommit(string documentNo, string sessionId, string phonenumber, string account, decimal amount, decimal corporateCharges, decimal charges, DateTime transactionDate, string appType, string description, string mpesanames)
        {
            var response = string.Empty;
            SwizzRequest.LogWithdrawOnFile(documentNo, $"Recieved WithdrawCommit: Document number - {documentNo}; SessionId - {sessionId}; PhoneNumber - {phonenumber}; Account - {account}; Amount - {amount}; corporateCharges - {corporateCharges}; charges - {charges}; transactionDate - {transactionDate}; appType- {appType}; description - {description}; mpesanames - {mpesanames}");

            try
            {
                using (var db = new SwizzKash_Mobile_DBEntities())
                {
                    var trx = db.MPESA_Withdrawals.FirstOrDefault(w => w.SESSION_ID == sessionId);

                    if (trx != null)
                    {
                        trx.Sent_to_Journal = "Yes";
                        trx.Date_Sent_to_Journal = DateTime.Now;
                        trx.Time_Sent_to_Journal = DateTime.Now;
                        trx.Request_Confirmed = "Yes";
                        trx.Status = "COMPLETED";
                        trx.Comments = "Committed successfully";

                        db.SaveChanges();
                    }

                }
                response = Config.service.WithdrawCommit(documentNo,
                    sessionId,
                    phonenumber.Replace("254", "0"),
                    account,
                    amount,
                    corporateCharges,
                    charges,
                    transactionDate,
                    appType,
                    description,
                    mpesanames);
            }
            catch (Exception ex)
            {
                response = ex.Message;
            }
            SwizzRequest.LogWithdrawOnFile(documentNo, $"Returned response: {response}");
            return response;
        }

        [WebMethod]
        public string WithdrawDecline(string sessionId, DateTime transactionDate)
        {
            SwizzRequest.LogWithdrawOnFile(sessionId, $"Recieved WithdrawDecline: SessionId - {sessionId}; transactionDate - {transactionDate};");
            var response = string.Empty;
            try
            {
                using (var db = new SwizzKash_Mobile_DBEntities())
                {
                    var trx = db.MPESA_Withdrawals.FirstOrDefault(w => w.SESSION_ID == sessionId);

                    if (trx != null)
                    {
                        trx.Sent_to_Journal = "Yes";
                        trx.Date_Sent_to_Journal = DateTime.Now;
                        trx.Time_Sent_to_Journal = DateTime.Now;
                        trx.Request_Confirmed = "Yes";
                        trx.Status = "Declined";
                        trx.Comments = "Declined";

                        db.SaveChanges();
                    }

                }
                response = Config.service.WithdrawDecline(sessionId, transactionDate);
            }
            catch (Exception ex)
            {
                response += ex.Message;
            }
            SwizzRequest.LogWithdrawOnFile(sessionId, $"Returned response: {response}");
            return response;
        }

        [WebMethod]
        public string GetAccountDetailsMember(string phoneNumber)
        {
            var response = string.Empty;
            try
            {
                response = Config.service.GetAccountDetailsMember(phoneNumber);
            }
            catch (Exception ex)
            {

            }
            return response;
        }
    }
}