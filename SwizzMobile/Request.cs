using SwizzMobile.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace SwizzMobile
{
    public class SwizzRequest
    {
        public static SwizzKash_Mobile_DBEntities db;
        public SwizzKash_Mobile_DBEntities newdb;
        public string MSISDN;
        public string SESSIONID;
        public string SERVICECODE;
        public string CLIENT_IDN;
        public string CLIENT_NAMEN;
        public string USSDSTRING;
        public string SACCO_CODE;
        public string IMSI;
        public string Currentoption;
        public int? menucount = 0;
        public int? gobackcount = 0;
        public Customer customer;
        public int Entry;
        public bool? active = false;
        public bool? goback = false;
        public Ussd session;
        public int? buff;
        public TRANSACTION_TYPE transactionType;

        public static SwizzRequest SaveRequest(SwizzRequest r)
        {

            Ussd ussd = new Ussd();
            ussd.SESSION = r.SESSIONID;
            ussd.Phone = r.MSISDN;
            ussd.OPTION = r.Currentoption;
            ussd.Menu_Count = 0;
            ussd.Active = false;
            ussd.GoBack = false;
            ussd.Transaction_Time = DateTime.Now;
            ussd.USSD_STRING = r.USSDSTRING;
            ussd.Code = r.SERVICECODE;
            ussd.SACCO_CODE = r.SACCO_CODE;
            ussd.IMSI = r.IMSI;
            ussd.Trans_Type = (int)r.transactionType;
            db.Ussds.Add(ussd);

            return r;

        }

        public static void savetologs(SwizzRequest req, string response)
        {
            try
            {
                string newline = "&#x000a;";
                string newresponse = response.Replace(newline, " \n");
                using (var newdb = new SwizzKash_Mobile_DBEntities())
                {
                    USSD_LOGS newlogs = new USSD_LOGS();
                    newlogs.client_id = req.CLIENT_IDN;
                    newlogs.client_name = req.CLIENT_NAMEN;
                    newlogs.Telephone = req.MSISDN;
                    newlogs.Response_string = newresponse;
                    newlogs.Request_string = req.USSDSTRING;
                    newlogs.Date_Request = DateTime.Now;

                    newdb.USSD_LOGS.Add(newlogs);
                    newdb.SaveChanges();

                }
            }
            catch (Exception ex)
            {

            }
        }
        public static SwizzRequest Updaterequest(SwizzRequest r)
        {

            Ussd ussd = SwizzRequest.db.Ussds.FirstOrDefault(o => o.SESSION == r.SESSIONID && o.Phone == r.MSISDN && o.SACCO_CODE ==r.SACCO_CODE);
            ussd.SESSION = r.SESSIONID;
            ussd.Phone = r.MSISDN;
            ussd.Active = ussd.Active == true ? true : r.active;
            ussd.OPTION = ussd.OPTION;
            ussd.SACCO_CODE = ussd.SACCO_CODE;
            ussd.Menu_Count = r.session.Menu_Count;
            ussd.Transaction_Time = DateTime.Now;
            ussd.USSD_STRING = ussd.USSD_STRING + r.USSDSTRING.Last();
            ussd.Value = r.buff.ToString();
            ussd.Code = r.SERVICECODE;
            ussd.GoBack = false;
            ussd.Entry = (int)r.Entry;
            ussd.Trans_Type = (int)r.transactionType;
            return r;
        }
        public static SwizzRequest NextUpdate(SwizzRequest r)
        {

            Ussd ussd = SwizzRequest.db.Ussds.FirstOrDefault(o => o.SESSION == r.SESSIONID && o.Phone == r.MSISDN && o.SACCO_CODE == r.SACCO_CODE);
            ussd.SESSION = r.SESSIONID;
            ussd.Phone = r.MSISDN;
            ussd.SACCO_CODE = r.SACCO_CODE;
            ussd.OPTION = r.Currentoption;
            ussd.Active = ussd.Active == true ? true : r.active;
            ussd.GoBack = ussd.GoBack == false;
            ussd.Menu_Count = (int)r.menucount;
            ussd.Transaction_Time = DateTime.Now;
            ussd.USSD_STRING = ussd.USSD_STRING + r.USSDSTRING.Last();
            ussd.Code = r.SERVICECODE;
            ussd.Entry = (int)r.Entry;
            ussd.Value = r.buff.ToString();
            ussd.Trans_Type = (int)r.transactionType;
            return r;
        }
        public static SwizzRequest GoBackUpdate(SwizzRequest r)
        {

            Ussd ussd = SwizzRequest.db.Ussds.FirstOrDefault(o => o.SESSION == r.SESSIONID && o.Phone == r.MSISDN && o.SACCO_CODE == r.SACCO_CODE);
            ussd.SESSION = r.SESSIONID;
            ussd.Phone = r.MSISDN;
            ussd.SACCO_CODE = r.SACCO_CODE;
            ussd.OPTION = r.Currentoption;
            ussd.GoBack = (bool)(ussd.GoBack == true ? true : r.goback);
            //ussd.Active = ussd.Active == true ? true : r.active;
            ussd.Menu_Count = (int)r.menucount;
            ussd.Transaction_Time = DateTime.Now;
            ussd.USSD_STRING = ussd.USSD_STRING + r.USSDSTRING.Last();
            ussd.Code = r.SERVICECODE;
            ussd.Entry = (int)r.Entry;
            ussd.Value = r.buff.ToString();
            ussd.Trans_Type = (int)r.transactionType;
            return r;
        }
        public static void GetSession(ref SwizzRequest r)
        {
            SwizzRequest rr = r;
            r.session = SwizzRequest.db.Ussds.FirstOrDefault(o => o.SESSION == rr.SESSIONID && o.Phone == rr.MSISDN );
            if (r.session != null)
            {
                r.transactionType = (TRANSACTION_TYPE)r.session.Trans_Type;
                r.SACCO_CODE = r.session.SACCO_CODE;
            }
        }
        public static string logpath = @"C:\SwizzKash USSD LOGS\";
        public static void LogEntryOnFile(string transactionId, string clientRequest)
        {
            logpath = @"C:\SwizzKash USSD LOGS\";
            string logFileName = $"SwizzKASHLogs-{DateTime.Now:yyyy-MM-dd}-{transactionId} .txt";
            string logFilePath = Path.Combine(logpath, logFileName);

            if (!Directory.Exists(logpath))
            {
                Directory.CreateDirectory(logpath);
            }
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - [{Thread.CurrentThread.ManagedThreadId}]-[{transactionId}] {clientRequest}");
            }

        }
        public static void LogWithdrawOnFile(string transactionId, string clientRequest)
        {
            string logFileName = $"SwizzC2BLogs-{DateTime.Now:yyyy-MM-dd}-{transactionId} .txt";
            logpath = @"C:\SwizzKash C2B LOGS\";
            string logFilePath = Path.Combine(logpath, logFileName);

            if (!Directory.Exists(logpath))
            {
                Directory.CreateDirectory(logpath);
            }
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - [{Thread.CurrentThread.ManagedThreadId}]-[{transactionId}] {clientRequest}");
            }

        }
    }
    public class imamarisha
    {
        public string sessionId { get; set; }
        public string phoneNumber { get; set; }
        public string serviceCode { get; set; }
        public string text { get; set; }
    }
    public class CLanguage
    {
        public SwizzRequest request;
        public RESPONSE message;

        public static string selectclients(SwizzRequest request)
        {

            string newline = "&#x000a;";

            string reslt = string.Empty;

            int count = 1;
            string accounts = string.Empty;


            reslt = "CON Select Your Sacco below " +
                accounts + newline;


            return reslt;
        }

    }
    public enum RESPONSE
    {
        NotRegistered = 0,
        NotActive = 1,
        PIN = 2,
        SelectClient = 3,
        Menu = 4,
        InvalidPIN = 5,
        Agency = 6
    }

    public enum TRANSACTION_TYPE
    {
        PIN_CHANGE = 0,
        selectclient = 1,
        PIN = 2,
        verifyPin = 3,
        Menu = 4,

        CASHWITHDRAW = 5,
        BALANCE_ENQUIRY = 6,
        MINISTATEMENT = 7,
        ACCOUNT_INFO = 8,
        bosatransfer = 9,
        fosatransfer = 10,

        transfer = 11,
        fosatransferother = 12,
        LOANS = 13,

        LOANBALANCE_ENQUIRY = 14,
        LOAN_REPAYMENT = 15,
        balances = 16,
        SHARESBALANCES = 17,
        LOAN_MOBILE = 18,
        PAYBILL_STK = 19,
        paybillloanrepayment = 20,
        agency = 21,
        agencycashwithdrawal = 22,
        enter_pin = 23,
        STATEMENT = 24,

        FULLSTATEMENT = 26,
        LoanStatus = 27,
        MicroFinance = 28,
        MicroTransfer = 29,

        AIRTIME_PURCHASE = 30,

        LOAN_SALARYADVANCE = 31,
        LOAN_DIVIDENDADVANCE = 32,

        AccountbalanceAll = 33,
        APAImarisha = 33,
        RUBANIMenu = 34,
        //POLYTECHMenu = 34,
        UpdateMemberDetails = 35,
        AprroveLoans = 36,
        ApplyFosaLoans = 37,

        AGCashDeposit = 38,
        AGUtilityPayment = 39,
        AGLoanRepayment = 40,
        AGShareCapital = 41,
        AGDepositContribution = 42,
        AGSecurityFund = 43,
        AGRegistrationFee = 44,
        AGAgentFloat = 45,
        AGAgentSummary = 46,
        AGChangePin = 47,

        applyTopUP = 48,
        SalaryAdvanceTopUp = 49,
        Guarantorship = 50,

        LOAN_GUARANTEES = 51,
        LOAN_GUARANTORS = 52,

        FOSA_TO_BOSA = 53,
        FOSA_TO_FOSA = 54,
        FOSA_TO_LOAN = 55,
        FOSA_TO_OTHER_MEMBER = 56,

        MINISTATEMENT_FOSA = 57,
        MINISTATEMENT_SHARES = 58,
        MINISTATEMENT_DEPOSITS = 59,
        MINISTATEMENT_LOAN = 60,

        INSTANT_LOAN_REQUEST = 61,
        INSTANT_LOAN_LIMIT = 62,
        INSTANT_LOAN_BALANCE = 63,
        INSTANT_LOAN_REPAY = 64,
    }

}