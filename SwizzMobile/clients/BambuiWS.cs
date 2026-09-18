using log4net;
using Newtonsoft.Json;
using SwizzMobile.Models;
using SwizzMobile.PolytechWebRef;
using SwizzMobile.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;

namespace SwizzMobile.Clients
{
    public class BambuiWS
    {
        private static string newline = "&#x000a;";
        public static string res = "";
        static List<string> fosaAccountsList = new List<string>();
        static List<string> fosaAccountsNumbersList = new List<string>();
        static List<string> outLoansList = new List<string>();
        public static SwizzKash_Mobile_DBEntities transdb;
        public static SwizzKash_Mobile_DBEntities db;
        public static string[] accarray;
        public static string[] lnarray;
        public static decimal qualification = 0;
        private static readonly ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string API_BASE_URL = "https://unmarriageable-overhomely-mayson.ngrok-free.dev/api/ussd";

        public static SwizzKash_Mobile_DBEntities Db
        {
            get { return db; }
            set { db = value; }
        }

        public BambuiWS() { }

        public static void EnableTrustedHosts()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);
        }

        //Always get the last thing the user typed
        private static string GetLastInput(string[] ussd_string)
        {
            return ussd_string.Length > 0 ? ussd_string[ussd_string.Length - 1] : string.Empty;
        }

        // -- Main menu string
        private static string MainMenu()
        {
            return "CON 1. Member Info. " + newline +
                   "2. Balance Enquiry. " + newline +
                   "3. Cash Withdrawal to MPESA. " + newline +
                   "4. Cash Deposit via MPESA. " + newline +
                   "5. Statement. " + newline +
                   "6. Funds Transfer. " + newline +
                   "7. Loans. " + newline;
        }

        public static string ProcessRequest(SwizzRequest request)
        {
            try
            {
                string[] ussd_string;
                string tempstring = string.Empty;

                if (request.session.Active == true)
                {
                    EnableTrustedHosts();
                    tempstring = request.USSDSTRING.Trim().Remove(0, request.session.OPTION.Trim().Length);
                    ussd_string = tempstring.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    EnableTrustedHosts();
                    ussd_string = request.USSDSTRING.Trim().Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                    if (ussd_string[0].Length >= 4) { }
                    else { ussd_string = ussd_string.Skip(1).ToArray(); }
                }

                // remove 98 from string since it is a new page
                bool a = Array.Exists(ussd_string, element => element == "98");
                if (a)
                {
                    string numToRemove = "98";
                    int numIdx = Array.IndexOf(ussd_string, numToRemove);
                    List<string> tmp = new List<string>(ussd_string);
                    tmp.RemoveAt(numIdx);
                    ussd_string = tmp.ToArray();
                }

                //get last input user typed
                string lastInput = GetLastInput(ussd_string);

                //first request, show main menu
                if (ussd_string.Length <= 1)
                {
                    res = MainMenu();
                    request.transactionType = TRANSACTION_TYPE.RUBANIMenu;
                    SwizzRequest.LogEntryOnFile(request.SESSIONID, $"Returned response: {res}");
                    return res;
                }

                //route based on current transaction type stored in session
                switch (request.transactionType)
                {
                    case TRANSACTION_TYPE.RUBANIMenu:
                        res = HandleMainMenu(request, lastInput);
                        break;
                    case TRANSACTION_TYPE.balances:
                        res = HandleBalances(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.LOANS:
                        res = HandleLoans(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.CASHWITHDRAW:
                        res = HandleCashWithdraw(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.PAYBILL_STK:
                        res = HandlePaybillSTK(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.STATEMENT:
                        res = HandleStatement(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.MINISTATEMENT:
                        res = HandleMiniStatement(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.transfer:
                        res = HandleTransfer(request, lastInput);
                        break;
                    case TRANSACTION_TYPE.fosatransfer:
                        res = HandleFosaTransfer(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.bosatransfer:
                        res = HandleBosaTransfer(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.FOSA_TO_OTHER_MEMBER:
                        res = HandleFosaToOtherMember(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.LOAN_REPAYMENT:
                        res = HandleLoanRepayment(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.LOAN_MOBILE:
                        res = HandleLoanMobile(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.LOAN_SALARYADVANCE:
                        res = HandleLoanSalaryAdvance(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.LOAN_DIVIDENDADVANCE:
                        res = HandleLoanDividendAdvance(request, lastInput, ussd_string);
                        break;
                    case TRANSACTION_TYPE.PIN_CHANGE:
                        res = HandlePinChange(request, lastInput, ussd_string);
                        break;
                    default:
                        res = MainMenu();
                        request.transactionType = TRANSACTION_TYPE.RUBANIMenu;
                        break;
                }
            }

            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
            SwizzRequest.LogEntryOnFile(request.SESSIONID, $"Returned response: {res}");
            return res;
        }

        // MAIN MENU HANDLER
        private static string HandleMainMenu(SwizzRequest request, string lastInput)
        {
            switch (lastInput)
            {
                case "1":
                    {
                        NextMenu(request);
                        return ChoiceMemberInfo(request, "1");
                    }

                case "2":
                    {
                        request.transactionType = TRANSACTION_TYPE.balances;
                        return "CON Select Option " + newline +
                               "1. M-Wallet Balance " + newline +
                               "2. BOSA Balance " + newline +
                               "0. Back";
                    }

                case "3":
                    {
                        request.transactionType = TRANSACTION_TYPE.CASHWITHDRAW;
                        return "CON Withdraw Cash" + newline +
                            "1. From M-Wallet" + newline +
                            "0. Back";

                        //List<Account> accountsList = GetWSSAccounts(request);
                        //if (accountsList is null)
                        //    return "END Accounts not found, Kindly contact Rubani Sacco";

                        //string straccounts = string.Empty;
                        //int menupos = 0;
                        //foreach (var account in accountsList)
                        //{
                        //    menupos += 1;
                        //    straccounts += $"{menupos}. {account.AccountName}." + newline;
                        //}
                        //return straccounts == string.Empty
                        //    ? "END Accounts not found, Kindly contact Rubani Sacco"
                        //    : "CON Select wallet account below " + newline + straccounts + newline + "0. Back";
                    }

                case "4":
                    {
                        request.transactionType = TRANSACTION_TYPE.PAYBILL_STK;
                        return "CON Select Option. " + newline +
                               "1. Deposit to M-Wallet. " + newline +
                               "2. Deposit to BOSA. " + newline +
                               "3. Loan Repayment" + newline +
                               "0. BACK";
                    }

                case "5":
                    {
                        request.transactionType = TRANSACTION_TYPE.STATEMENT;
                        return "CON Select Option. " + newline +
                               "1. M-Wallet Statement" + newline +
                               "2. Deposit Contribution. " + newline +
                               "3. Share Capital" + newline +
                               "4. Loans" + newline +
                               "0. BACK";
                    }
                case "6":
                    {
                        request.transactionType = TRANSACTION_TYPE.transfer;
                        return "CON Select Option " + newline +
                               "1. M-Wallet Transfer " + newline +
                               "2. BOSA Transfer " + newline +
                               "0. Back";
                    }

                case "7":
                    {
                        request.transactionType = TRANSACTION_TYPE.LOANS;
                        return "CON Select Option " + newline +
                               "1. Loan Balances " + newline +
                               "2. Loan Repayment " + newline +
                               "3. M-Loan " + newline +
                               "0. Back";
                    }

                case "0":
                    return MainMenu();

                default:
                    {
                        // invalid - keep transactionType as RubaniMenu, show main menu again
                        return "CON Invalid option, please try again " + newline +
                               "1. Member Info. " + newline +
                               "2. Balance Enquiry. " + newline +
                               "3. Cash Withdrawal to MPESA. " + newline +
                               "4. Cash Deposit via MPESA. " + newline +
                               "5. Statement. " + newline +
                               "6. Funds Transfer. " + newline +
                               "7. Loans. " + newline;
                    }
            }
        }

        // BALANCES HANDLER
        // BALANCES HANDLER
        private static string HandleBalances(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            switch (lastInput)
            {
                case "1":
                    {
                        request.transactionType = TRANSACTION_TYPE.BALANCE_ENQUIRY;
                        try
                        {
                            using (var transdb = new SwizzKash_Mobile_DBEntities())
                            {
                                MTransaction mTransaction = new MTransaction
                                {
                                    TransactionDate = DateTime.Now,
                                    ClientCode = request.customer.Client,
                                    SessionID = request.SESSIONID,
                                    AccountNo = request.MSISDN,
                                    AccountName = "",
                                    DocumentNo = "BE" + request.SESSIONID,
                                    TransactionType = (int)TRANSACTION_TYPE.BALANCE_ENQUIRY,
                                    Telephone = request.MSISDN,
                                    SaccoCode = request.SACCO_CODE,
                                    Posted = false,
                                    DatePosted = null,
                                    DestinationAccount = "",
                                    LoanNo = "",
                                    Status = 0,
                                    Comments = "",
                                    Amount = 0,
                                    Charge = 5,
                                    Description = TRANSACTION_TYPE.BALANCE_ENQUIRY.ToString(),
                                    ApplicationType = "USSD",
                                    SentToCBS = false
                                };

                                transdb.MTransactions.Add(mTransaction);
                                transdb.SaveChanges();
                                int entryNo = mTransaction.EntryNo;

                                using (var db = new SwizzKash_Mobile_DBEntities())
                                {
                                    var thistrans = db.MTransactions.FirstOrDefault(b =>
                                        b.EntryNo == entryNo &&
                                        b.TransactionType == (int)TRANSACTION_TYPE.BALANCE_ENQUIRY);

                                    if (thistrans != null)
                                    {
                                        EnquireBalanceResponse enquireBalanceResponse = null;
                                        var client = new System.Net.Http.HttpClient();
                                        var payload = JsonConvert.SerializeObject(new
                                        {
                                            PhoneNumber = request.MSISDN.Replace("254", "0"),
                                            SaccoCode = request.SACCO_CODE,
                                            AccountId = thistrans.AccountNo,
                                        });

                                        var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                                        var httpResponse = client.PostAsync($"{API_BASE_URL}/EnquireWalletBalance", content).Result;
                                        var balanceWallet = httpResponse.Content.ReadAsStringAsync().Result;

                                        if (!string.IsNullOrEmpty(balanceWallet))
                                            enquireBalanceResponse = JsonConvert.DeserializeObject<EnquireBalanceResponse>(balanceWallet);

                                        if (enquireBalanceResponse == null || enquireBalanceResponse.StatusCode != "000")
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.FAILED;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireBalanceResponse?.StatusDescription ?? "Balance request failed.";
                                            db.SaveChanges();
                                            return "END " + (enquireBalanceResponse?.StatusDescription ?? "Could not process request.");
                                        }
                                        else
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.SUCCESS;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireBalanceResponse.StatusDescription;
                                            db.SaveChanges();

                                            // TODO: send SMS with enquireBalanceResponse.AccountBal once SMS method is confirmed

                                            return "CON " + enquireBalanceResponse.StatusDescription + newline +
                                                   "Balance: KES " + enquireBalanceResponse.AccountBalance.ToString("#,##0.00") + "\n0. BACK";
                                        }
                                    }
                                    return "END Could not process your request. Kindly try again later.";
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                            return "END An error occurred. Please try again later.";
                        }
                    }

                case "2":
                    {
                        request.transactionType = TRANSACTION_TYPE.SHARESBALANCES;
                        try
                        {
                            using (var transdb = new SwizzKash_Mobile_DBEntities())
                            {
                                MTransaction mTransaction = new MTransaction
                                {
                                    TransactionDate = DateTime.Now,
                                    ClientCode = request.customer.Client,
                                    SessionID = request.SESSIONID,
                                    AccountNo = request.MSISDN,
                                    SaccoCode = request.SACCO_CODE,
                                    AccountName = "",
                                    DocumentNo = "SB" + request.SESSIONID,
                                    TransactionType = (int)TRANSACTION_TYPE.SHARESBALANCES,
                                    Telephone = request.MSISDN,
                                    Posted = false,
                                    DatePosted = null,
                                    DestinationAccount = "",
                                    LoanNo = "",
                                    Status = 0,
                                    Comments = "",
                                    Amount = 0,
                                    Charge = 10,
                                    Description = TRANSACTION_TYPE.SHARESBALANCES.ToString(),
                                    ApplicationType = "USSD",
                                    SentToCBS = false
                                };

                                transdb.MTransactions.Add(mTransaction);
                                transdb.SaveChanges();
                                int entryNo = mTransaction.EntryNo;
                                NextMenu(request);

                                using (var db = new SwizzKash_Mobile_DBEntities())
                                {
                                    var thistrans = db.MTransactions.FirstOrDefault(b =>
                                        b.EntryNo == entryNo &&
                                        b.TransactionType == (int)TRANSACTION_TYPE.SHARESBALANCES);

                                    if (thistrans != null)
                                    {
                                        EnquireBalanceResponse enquireBalanceResponse = null;
                                        var client = new System.Net.Http.HttpClient();
                                        var payload = JsonConvert.SerializeObject(new
                                        {
                                            PhoneNumber = request.MSISDN.Replace("254", "0"),
                                            SaccoCode = request.SACCO_CODE,
                                            AccountId = thistrans.AccountNo
                                        });
                                        var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                                        var httpResponse = client.PostAsync($"{API_BASE_URL}/EnquireBOSABalance ", content).Result;
                                        var balanceBOSA = httpResponse.Content.ReadAsStringAsync().Result;

                                        if (!string.IsNullOrEmpty(balanceBOSA))
                                            enquireBalanceResponse = JsonConvert.DeserializeObject<EnquireBalanceResponse>(balanceBOSA);

                                        if (enquireBalanceResponse == null || enquireBalanceResponse.StatusCode != "000")
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.FAILED;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireBalanceResponse?.StatusDescription ?? "Balance request failed.";
                                            db.SaveChanges();
                                            return "END " + (enquireBalanceResponse?.StatusDescription ?? "Your Shares balance Request could not be processed. Please try again later.");
                                        }
                                        else
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.SUCCESS;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireBalanceResponse.StatusDescription;
                                            db.SaveChanges();
                                            //return "CON " + enquireBalanceResponse.StatusDescription + "\n0. BACK";
                                            return "CON " + enquireBalanceResponse.StatusDescription + newline +
                                   "Balance: KES " + enquireBalanceResponse.AccountBalance.ToString("#,##0.00") + "\n0. BACK";
                                        }
                                    }
                                    return "END Could not process your request. Kindly try again later or contact Rubani Sacco.";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                            return "END An error occurred while processing your request. Please try again later.";
                        }
                    }
                case "0":
                    {
                        request.transactionType = TRANSACTION_TYPE.RUBANIMenu;
                        return MainMenu();
                    }
                default:
                    {
                        // invalid - keep transactionType as balances, show same menu
                        return "CON Invalid option, please try again " + newline +
                               "1. M-Wallet Balance " + newline +
                               "2. BOSA Balance " + newline +
                               "0. Back";
                    }
            }
        }

        // LOANS HANDLER
        private static string HandleLoans(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            switch (lastInput)
            {
                case "1":
                    {
                        try
                        {
                            using (var transdb = new SwizzKash_Mobile_DBEntities())
                            {
                                MTransaction mTransaction = new MTransaction
                                {
                                    TransactionDate = DateTime.Now,
                                    ClientCode = request.customer.Client,
                                    SessionID = request.SESSIONID,
                                    AccountNo = request.MSISDN,
                                    SaccoCode= request.SACCO_CODE,
                                    AccountName = "",
                                    DocumentNo = "LB" + request.SESSIONID,
                                    TransactionType = (int)TRANSACTION_TYPE.LOANBALANCE_ENQUIRY,
                                    Telephone = request.MSISDN,
                                    Posted = false,
                                    DatePosted = null,
                                    DestinationAccount = "",
                                    LoanNo = "",
                                    Status = 0,
                                    Comments = "",
                                    Amount = 0,
                                    Charge = 10,
                                    Description = TRANSACTION_TYPE.LOANBALANCE_ENQUIRY.ToString(),
                                    ApplicationType = "USSD",
                                    SentToCBS = false
                                };

                                transdb.MTransactions.Add(mTransaction);
                                transdb.SaveChanges();
                                int entryNo = mTransaction.EntryNo;
                                NextMenu(request);

                                using (var db = new SwizzKash_Mobile_DBEntities())
                                {
                                    var thistrans = db.MTransactions.FirstOrDefault(b =>
                                        b.EntryNo == entryNo &&
                                        b.TransactionType == (int)TRANSACTION_TYPE.LOANBALANCE_ENQUIRY);

                                    if (thistrans != null)
                                    {
                                        EnquireLoanBalanceResponse enquireLoanBalanceResponse = null;
                                        var client = new System.Net.Http.HttpClient();
                                        var payload = JsonConvert.SerializeObject(new
                                        {
                                            PhoneNumber = request.MSISDN.Replace("+254", "0"),
                                            SaccoCode = request.SACCO_CODE,
                                            AccountId = thistrans.AccountNo
                                        });

                                        var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                                        var httpResponse = client.PostAsync($"{API_BASE_URL}/EnquireLoanBalances", content).Result;
                                        var balanceLoan = httpResponse.Content.ReadAsStringAsync().Result;

                                        if (!string.IsNullOrEmpty(balanceLoan))
                                            enquireLoanBalanceResponse = JsonConvert.DeserializeObject<EnquireLoanBalanceResponse>(balanceLoan);

                                        if (enquireLoanBalanceResponse == null || enquireLoanBalanceResponse.StatusCode != "000")
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.FAILED;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireLoanBalanceResponse?.StatusDescription ?? "Balance request failed.";
                                            db.SaveChanges();
                                            return "END " + (enquireLoanBalanceResponse?.StatusDescription ?? "Your Loan balance Request could not be processed. Please try again later.");
                                        }
                                        else
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.SUCCESS;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireLoanBalanceResponse.StatusDescription;
                                            db.SaveChanges();
                                            return "CON " + enquireLoanBalanceResponse.StatusDescription + "\n0. BACK";
                                        }
                                    }
                                    return "END Could not process your request. Kindly try again later or contact Rubani Sacco.";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                            return "END An error occurred while processing your request. Please try again later.";
                        }
                    }

                case "2":
                    {
                        request.transactionType = TRANSACTION_TYPE.LOAN_REPAYMENT;
                        return "CON Select payment source" + newline +
                               "1. Own Account" + newline +
                               "2. M-PESA Paybill" + newline +
                               "0. Back";
                    }

                case "3":
                    {
                        request.transactionType = TRANSACTION_TYPE.LOAN_MOBILE;
                        return "CON Select Repayment Period Below" + newline +
                               " 1. One Month." + newline +
                               " 0. Back";
                    }

                case "0":
                    {
                        request.transactionType = TRANSACTION_TYPE.RUBANIMenu;
                        return MainMenu();
                    }

                default:
                    {
                        return "CON Invalid option, please try again " + newline +
                               "1. Loan Balances " + newline +
                               "2. Loan Repayment " + newline +
                               "3. M-Loan " + newline +
                               "0. Back";
                    }
            }
        }

        // CASH WITHDRAW HANDLER
        private static string HandleCashWithdraw(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            // depth 2: user selects "1" (From M-Wallet) -> resolve account, ask amount
            // depth 3: user enters amount -> validate and process withdrawal

            if (lastInput == "0")
            {
                request.transactionType = TRANSACTION_TYPE.RUBANIMenu;
                return MainMenu();
            }

            int depth = ussd_string.Length;

            try
            {
                List<Account> accountsList = GetWSSAccounts(request);
                if (accountsList == null || accountsList.Count == 0)
                    return "END Account not found, Kindly contact Rubani Sacco";

                var account = accountsList.First();

                // ---------- depth 2: user just chose "1" From M-Wallet ----------
                if (depth == 2)
                {
                    if (lastInput != "1")
                    {
                        return "CON Invalid option, please try again " + newline +
                               "1. From M-Wallet" + newline +
                               "0. Back";
                    }

                    bool hasPendingWithdrawRequests = HasPendingWithdrawRequests(request.MSISDN);
                    if (hasPendingWithdrawRequests)
                    {
                        NextMenu(request);
                        return "CON Request could not be completed, you have a pending cash withdrawal request. Rubani MOBILE." + newline + "0. BACK";
                    }

                    return $"CON Withdraw from {account.AccountName}." + newline + "Enter Amount." + newline;
                }

                // ---------- depth 3: amount entered -> validate and process ----------
                else if (depth == 3)
                {
                    if (!decimal.TryParse(lastInput, out decimal amount) || amount <= 0)
                        return "CON Invalid amount, please enter a valid amount" + newline;

                    bool hasPendingWithdrawRequests = HasPendingWithdrawRequests(request.MSISDN);
                    if (hasPendingWithdrawRequests)
                    {
                        NextMenu(request);
                        return "CON Request could not be completed, you have a pending cash withdrawal request. Rubani MOBILE." + newline + "0. BACK";
                    }

                    GetBalanceResponse balanceResponse = GetWSSAccountBalance(request);
                    if (balanceResponse == null)
                        return "END Could not retrieve account balance, Kindly contact Rubani Sacco";

                    decimal accBalance = balanceResponse.AccountBalance;
                    decimal? acumulated = GetAccumulatedWithdrawalAmount(account.AccountNo) ?? 0;
                    decimal? totalWithdrawCharges = GetAmountWithdrawalCharges(amount) ?? 0;
                    decimal finalWithLimit = 50000 - (decimal)acumulated;

                    string Status = "Failed";
                    string Text_File_Created = string.Empty;
                    string withdrawRes = string.Empty;

                    if (amount > 50000)
                    {
                        Status = "Failed";
                        Text_File_Created = "Max 50000 MPESA Limit Per Trans";
                        withdrawRes = "CON Your Cash withdrawal Request has been declined. The maximum amount allowed per transaction is KES 20,000. Thank you, Rubani MOBILE" + newline + "0. BACK";
                    }
                    else if (finalWithLimit - amount < 0)
                    {
                        NextMenu(request);
                        Status = "Failed";
                        Text_File_Created = "Reached Daily Limit 50000";
                        withdrawRes = "CON Request could not be completed, you have reached your daily withdrawal limit, kindly try again tomorrow. Thank you, Rubani MOBILE" + newline + "0. BACK";
                    }
                    else if (amount > finalWithLimit)
                    {
                        NextMenu(request);
                        Status = "Failed";
                        Text_File_Created = "Maximum withdrawal Limit 50000";
                        withdrawRes = "END Request could not be completed, you can only withdraw up to a maximum of KSH " + finalWithLimit + " Per day. Thank you, Rubani MOBILE" + newline;
                    }
                    else if ((amount + totalWithdrawCharges) > accBalance)
                    {
                        Status = "Failed";
                        Text_File_Created = "insufficient funds";
                        withdrawRes = "CON Your account balance is insufficient for withdrawal KES " + amount + " + " + ((decimal)totalWithdrawCharges).ToString("#0.00") + " charges. Thank you, Rubani MOBILE" + newline + "0. BACK";
                    }
                    else
                    {
                        Status = "Pending";
                        Text_File_Created = "NULL";
                        withdrawRes = "CON Your Cash withdrawal Request has been received and is being processed. Thank you, Rubani MOBILE" + newline + "0. BACK";
                    }

                    using (var transdb = new SwizzKash_Mobile_DBEntities())
                    {
                        MTransaction mTransaction = new MTransaction
                        {
                            TransactionDate = DateTime.Now,
                            ClientCode = request.customer.Client,
                            SessionID = request.SESSIONID,
                            AccountNo = request.MSISDN,
                            SaccoCode = request.SACCO_CODE,
                            AccountName = "",
                            DocumentNo = "SB" + request.SESSIONID,
                            TransactionType = (int)TRANSACTION_TYPE.SHARESBALANCES,
                            Telephone = request.MSISDN,
                            Posted = false,
                            DatePosted = null,
                            DestinationAccount = "",
                            LoanNo = "",
                            Status = 0,
                            Comments = "",
                            Amount = amount,
                            Charge = 10,
                            Description = TRANSACTION_TYPE.SHARESBALANCES.ToString(),
                            ApplicationType = "USSD",
                            SentToCBS = false
                        };

                        transdb.MTransactions.Add(mTransaction);
                        transdb.SaveChanges();
                    }

                    Transaction tr = new Transaction
                    {
                        saccoId = request.customer.Client,
                        saccosource = 3012451,
                        accountNo = account.AccountNo,
                        accountName = account.AccountName,
                        Telephone = request.MSISDN,
                        SaccoCode = request.SACCO_CODE,
                         amount = (double)amount,
                        Time_file_created = DateTime.Now,
                        SMS_Sent = "No",
                        SESSIN_ID = "WD" + request.SESSIONID,
                        Sent_to_journal = "No",
                        account_balance = (double)accBalance,
                        Date_file_created = DateTime.Today,
                        transTime = DateTime.Now,
                        transType = (int)request.transactionType,
                        phone = request.MSISDN,
                        posted = Transaction.Posted.notPosted,
                        datePosted = DateTime.Today,
                        acc2 = "",
                        loanNo = "",
                        status = 0,
                        comments = Text_File_Created,
                        RequestConfirmed = "No",
                        charge = 10,
                        amount2 = Convert.ToDouble(accBalance),
                        description = "Withdraw Cash"
                    };

                    Insertwithdrawals(tr, Status);
                    NextMenu(request);
                    return withdrawRes;
                }

                return "END Invalid option selected";
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return "END An error occurred. Please try again later.";
            }
        }


        // PAYBILL STK HANDLER
        private static string HandlePaybillSTK(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            // Flow for type 2 (BOSA) and 3 (Loan):
            // depth 2: user just selected service code "4" -> show type menu (1/2/3)
            // depth 3: user entered type (2/3) -> show account/loan list
            // depth 4: user entered account/loan index -> VALIDATE immediately -> ask amount
            // depth 5: user entered amount -> validate -> show confirmation
            // depth 6: user confirmed (1/2) -> process
            //
            // Flow for type 1 (M-Wallet) — account step is SKIPPED, single wallet account auto-resolved:
            // depth 2: user just selected service code "4" -> show type menu (1/2/3)
            // depth 3: user entered "1" -> ask amount directly
            // depth 4: user entered amount -> validate -> show confirmation
            // depth 5: user confirmed (1/2) -> process

            if (lastInput == "0")
            {
                request.transactionType = TRANSACTION_TYPE.RUBANIMenu;
                return MainMenu();
            }

            int depth = ussd_string.Length;

            try
            {
                // show menu if depth is 2
                if (depth == 2)
                {
                    switch (lastInput)
                    {
                        case "1":
                        case "2":
                        case "3":
                            return BuildTypeSubMenu(request, lastInput);

                        default:
                            return "CON Invalid option, please try again " + newline +
                                   "1. Deposit to M-Wallet. " + newline +
                                   "2. Deposit to BOSA. " + newline +
                                   "3. Loan Repayment" + newline +
                                   "0. BACK";
                    }
                }

                else if (depth == 3)
                {
                    string typeSelection = lastInput;
                    return BuildTypeSubMenu(request, typeSelection);
                }

                else if (depth == 4)
                {
                    string typeSelection = ussd_string[ussd_string.Length - 2];

                    // type "1" (M-Wallet) has no account
                    if (typeSelection == "1")
                    {
                        if (!decimal.TryParse(lastInput, out decimal walletAmount) || walletAmount <= 0)
                            return "CON Invalid amount, please enter a valid amount";

                        string err = ValidateAccountSelection(request, "1", null, out string destAcc, out string destNm);
                        if (err != null) return err;

                        //string walletLabel = destNm + " (Account " + destAcc + ")";
                        return $"CON Pay to M-Wallet Account KES {walletAmount.ToString("#0")} via M-PESA?{newline}1. Yes{newline}2. Cancel";
                    }

                    // type 2/3 — lastInput is the account/loan index, validate now
                    string accountSelection = lastInput;
                    string validationError = ValidateAccountSelection(request, typeSelection, accountSelection, out _, out _);
                    if (validationError != null)
                        return validationError;

                    return "CON Enter Amount";
                }

                // ---------- depth 5 ----------
                else if (depth == 5)
                {
                    string typeSelection = ussd_string[ussd_string.Length - 3];

                    // type "1" (M-Wallet) — at depth 5, lastInput is the confirm (1/2)
                    if (typeSelection == "1")
                    {
                        if (lastInput == "2")
                            return "END You have cancelled the request. Rubani MOBILE";
                        if (lastInput != "1")
                            return "CON Invalid option " + newline + "1. Yes" + newline + "2. Cancel";

                        string walletAmountStr = ussd_string[ussd_string.Length - 2];
                        if (!decimal.TryParse(walletAmountStr, out decimal walletAmount))
                            return "END Invalid amount";

                        string err = ValidateAccountSelection(request, "1", null, out string strDestAcc, out string strDestNm);
                        if (err != null) return err;

                        return ProcessSTKPush(request, "1", strDestAcc, strDestNm, walletAmount);
                    }

                    // type 2/3 — lastInput is the amount, validate -> show confirmation
                    string accountSelection = ussd_string[ussd_string.Length - 2];

                    if (!decimal.TryParse(lastInput, out decimal theAmount) || theAmount <= 0)
                        return "CON Invalid amount, please enter a valid amount";

                    string validationError = ValidateAccountSelection(request, typeSelection, accountSelection,
                                                                        out string destAccount, out string destName);
                    if (validationError != null)
                        return validationError;

                    string label = typeSelection == "2" ? "BOSA account " + destName
                                  : destName + " (Loan " + destAccount + ")";

                    return $"CON Pay to {label} KES {theAmount.ToString("#0")} via M-PESA?{newline}1. Yes{newline}2. Cancel";
                }

                // ---------- depth 6: confirmation (type 2/3 only) ----------
                else if (depth == 6)
                {
                    string typeSelection = ussd_string[ussd_string.Length - 4];

                    if (lastInput == "2")
                        return "END You have cancelled the request. Rubani MOBILE";

                    if (lastInput != "1")
                        return "CON Invalid option " + newline + "1. Yes" + newline + "2. Cancel";

                    string accountSelection = ussd_string[ussd_string.Length - 3];
                    string amountStr = ussd_string[ussd_string.Length - 2];

                    if (!decimal.TryParse(amountStr, out decimal theAmount))
                        return "END Invalid amount";

                    string validationError = ValidateAccountSelection(request, typeSelection, accountSelection,
                                                                        out string strDestinationAccount,
                                                                        out string strDestinationAccountName);
                    if (validationError != null)
                        return validationError;

                    return ProcessSTKPush(request, typeSelection, strDestinationAccount, strDestinationAccountName, theAmount);
                }

                return "END Invalid option selected";
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return "END An error occurred. Please try again later.";
            }
        }

        /// <summary>
        /// Shared transaction-saving logic for the final confirmed STK push, used by both
        /// the M-Wallet (type 1, shorter flow) and BOSA/Loan (type 2/3, longer flow) paths.
        /// </summary>
        private static string ProcessSTKPush(SwizzRequest request, string typeSelection,
                                              string strDestinationAccount, string strDestinationAccountName,
                                              decimal theAmount)
        {
            decimal theCharge = typeSelection == "3" ? 10 : 0;
            string strTypeAccronym = typeSelection == "1" ? "DF" : typeSelection == "2" ? "DB" : "PL";

            // special-case BOSA member lookup, mirrors old code
            if (typeSelection == "2")
            {
                var memberdetails = GeMemberNumber(request);
                if (memberdetails != null && memberdetails.StatusCode == "000")
                {
                    strDestinationAccount = strDestinationAccount.Substring(0, 3) + memberdetails.MemberNo;
                    strDestinationAccountName = memberdetails.MemberName;
                }
            }

            using (var dbC2b = new SwizzKash_Mobile_DBEntities())
            {
                try
                {
                    var tran = new STKPushTransaction
                    {
                        SaccoID = request.customer.Client,
                        Telephone = request.MSISDN,
                        SaccoCode = request.SACCO_CODE,
                        AccountNo = request.MSISDN.Replace("+", ""),
                        AccountName = strDestinationAccountName,
                        DocumentNo = strTypeAccronym + request.SESSIONID,
                        DocumentDate = DateTime.Now,
                        Transaction_Type = (int)TRANSACTION_TYPE.PAYBILL_STK,
                        Posted = false,
                        Date_Posted = null,
                        Account_2 = strDestinationAccount,
                        Status = 0,
                        Comments = "",
                        Amount = theAmount,
                        Charge = theCharge,
                        Application_Type = "USSD",
                        Description = TRANSACTION_TYPE.PAYBILL_STK.ToString()
                    };
                    dbC2b.STKPushTransactions.Add(tran);
                    dbC2b.SaveChanges();
                    return "END Please wait for safaricom to complete request. Rubani MOBILE";
                }


                catch (Exception ex)
                {
                    _log.Error(ex.Message);
                    return "END A problem has been encountered, please retry after 10 minutes or contact Rubani SACCO";
                }
            }
        }

        /// <summary>
        /// Builds the list menu (accounts or loans) right after the user picks a type (1/2/3).
        /// </summary>
        private static string BuildTypeSubMenu(SwizzRequest request, string typeSelection)
        {
            switch (typeSelection)
            {
                case "1":
                    return "CON Enter Amount";

                case "2":
                    return "CON Select Option Below" + newline +
                           " 1. Deposit Contribution." + newline +
                           " 2. Share Capital" + newline +
                           " 0. Back";

                case "3":
                    {
                        List<LoanBalances> loansList = GetOutstandingLoans(request);
                        if (loansList == null || loansList.Count == 0)
                            return "END Loans not found, Kindly contact Rubani Sacco";

                        string strloans = string.Empty;
                        int menupos = 0;
                        foreach (var loan in loansList)
                        {
                            menupos += 1;
                            strloans += $"{menupos}. {loan.LoanName} {loan.BalanceAmount.ToString("#0")}." + newline;
                        }
                        return "CON Select Loan below " + newline + strloans + newline + "0. Back";
                    }

                default:
                    return "CON Invalid option, please try again " + newline +
                           "1. Deposit to M-Wallet. " + newline +
                           "2. Deposit to BOSA. " + newline +
                           "3. Loan Repayment" + newline +
                           "0. BACK";
            }
        }
        /// <summary>
        /// Validates that accountSelection is a valid index into the relevant list for typeSelection.
        /// Returns null if valid (and sets outDestAccount/outDestName), or an "END ..." error string if invalid.
        /// </summary>
        private static string ValidateAccountSelection(SwizzRequest request, string typeSelection, string accountSelection,
                                                      out string outDestAccount, out string outDestName)
        {
            outDestAccount = string.Empty;
            outDestName = string.Empty;

            switch (typeSelection)
            {
                case "1":
                    {
                        // M-Wallet:
                        List<Account> accountsList = GetWSSAccounts(request);
                        if (accountsList is null || accountsList.Count == 0)
                            return "END Accounts not found, Kindly contact Rubani Sacco";

                        var account = accountsList.First();
                        outDestAccount = account.AccountNo;
                        outDestName = account.AccountName;
                        return null;
                    }

                case "2":
                    {
                        if (accountSelection != "1" && accountSelection != "2")
                            return "END Invalid option selected";

                        string destinationAccount = accountSelection == "1" ? "DEPOSIT CONTRIBUTION" : "SHARE CAPITAL";
                        outDestAccount = destinationAccount;
                        outDestName = destinationAccount;
                        return null;
                    }

                case "3":
                    {
                        List<LoanBalances> loansList = GetOutstandingLoans(request);
                        if (loansList == null || loansList.Count == 0)
                            return "END Loans not found, Kindly contact Rubani Sacco";

                        if (!int.TryParse(accountSelection, out int loanIdx) || loanIdx < 1 || loanIdx > loansList.Count)
                            return "END Invalid loan selected";

                        var loan = loansList.ElementAt(loanIdx - 1);
                        outDestAccount = loan.LoanNo;
                        outDestName = loan.LoanName;
                        return null;
                    }

                default:
                    return "END Invalid option selected";
            }
        }

        // STATEMENT HANDLER
        private static string HandleStatement(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            switch (lastInput)
            {
                case "1":
                    {
                        request.transactionType = TRANSACTION_TYPE.MINISTATEMENT;
                        try
                        {
                            using (var transdb = new SwizzKash_Mobile_DBEntities())
                            {
                                MTransaction mTransaction = new MTransaction
                                {
                                    TransactionDate = DateTime.Now,
                                    ClientCode = request.customer.Client,
                                    SessionID = request.SESSIONID,
                                    AccountNo = request.MSISDN,
                                    SaccoCode = request.SACCO_CODE,
                                    AccountName = "",
                                    DocumentNo = "MN" + request.SESSIONID,
                                    TransactionType = (int)TRANSACTION_TYPE.MINISTATEMENT,
                                    Telephone = request.MSISDN,
                                    Posted = false,
                                    DatePosted = null,
                                    DestinationAccount = "",
                                    LoanNo = "",
                                    Status = 0,
                                    Comments = "",
                                    Amount = 0,
                                    Charge = 10,
                                    Description = TRANSACTION_TYPE.MINISTATEMENT.ToString(),
                                    ApplicationType = "USSD",
                                    SentToCBS = false
                                };

                                transdb.MTransactions.Add(mTransaction);
                                transdb.SaveChanges();
                                int entryNo = mTransaction.EntryNo;

                                using (var db = new SwizzKash_Mobile_DBEntities())
                                {
                                    var thistrans = db.MTransactions.FirstOrDefault(b =>
                                        b.EntryNo == entryNo &&
                                        b.TransactionType == (int)TRANSACTION_TYPE.MINISTATEMENT);

                                    if (thistrans != null)
                                    {
                                        EnquireMinistatementResponse enquireMinistatementResponse = null;
                                        var client = new System.Net.Http.HttpClient();
                                        var payload = JsonConvert.SerializeObject(new
                                        {
                                            EntryNo = thistrans.EntryNo.ToString(),
                                            PhoneNumber = request.MSISDN.Replace("254", "0"),
                                            SaccoCode = request.SACCO_CODE,
                                            DocumentNo = thistrans.DocumentNo,
                                            AccountId = thistrans.AccountNo,
                                            TransactionDate = DateTime.Now,
                                        });


                                        var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                                        var httpResponse = client.PostAsync($"{API_BASE_URL}/GetWalletMiniStatement", content).Result;
                                        var statementWallet = httpResponse.Content.ReadAsStringAsync().Result;

                                        if (!string.IsNullOrEmpty(statementWallet))
                                            enquireMinistatementResponse = JsonConvert.DeserializeObject<EnquireMinistatementResponse>(statementWallet);

                                        if (enquireMinistatementResponse == null || enquireMinistatementResponse.StatusCode != "0000")
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.FAILED;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireMinistatementResponse?.StatusDescription ?? "Statement request failed.";
                                            db.SaveChanges();
                                            return "END " + (enquireMinistatementResponse?.StatusDescription ?? "Could not process request.");
                                        }
                                        else
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.SUCCESS;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireMinistatementResponse.StatusDescription;
                                            db.SaveChanges();
                                            return "CON " + enquireMinistatementResponse.StatusDescription + "\n0. BACK";
                                        }
                                    }
                                    return "END Could not process your request. Kindly try again later.";
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                            return "END An error occurred. Please try again later.";
                        }
                    }

                case "2":
                    {
                        try
                        {
                            using (var transdb = new SwizzKash_Mobile_DBEntities())
                            {
                                MTransaction mTransaction = new MTransaction
                                {
                                    TransactionDate = DateTime.Now,
                                    ClientCode = request.customer.Client,
                                    SessionID = request.SESSIONID,
                                    AccountNo = request.MSISDN,
                                    SaccoCode = request.SACCO_CODE,
                                    AccountName = "",
                                    DocumentNo = "MD" + request.SESSIONID,
                                    TransactionType = (int)TRANSACTION_TYPE.MINISTATEMENT_DEPOSITS,
                                    Telephone = request.MSISDN,
                                    Posted = false,
                                    DatePosted = null,
                                    DestinationAccount = "",
                                    LoanNo = "",
                                    Status = 0,
                                    Comments = "",
                                    Amount = 0,
                                    Charge = 10,
                                    Description = TRANSACTION_TYPE.MINISTATEMENT_DEPOSITS.ToString(),
                                    ApplicationType = "USSD",
                                    SentToCBS = false
                                };


                                transdb.MTransactions.Add(mTransaction);
                                transdb.SaveChanges();
                                int entryNo = mTransaction.EntryNo;
                                NextMenu(request);

                                using (var db = new SwizzKash_Mobile_DBEntities())
                                {
                                    var thistrans = db.MTransactions.FirstOrDefault(b =>
                                        b.EntryNo == entryNo &&
                                        b.TransactionType == (int)TRANSACTION_TYPE.MINISTATEMENT_DEPOSITS);

                                    if (thistrans != null)
                                    {
                                        EnquireMinistatementResponse enquireMinistatementResponse = null;
                                        var client = new System.Net.Http.HttpClient();
                                        var payload = JsonConvert.SerializeObject(new
                                        {
                                            PhoneNumber = request.MSISDN.Replace("+254", "0"),
                                            SaccoCode = request.SACCO_CODE,
                                            AccountId = thistrans.AccountNo,
                                            DateTime.Now
                                        });
                                        var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                                        var httpResponse = client.PostAsync($"{API_BASE_URL}/GetBOSAMiniStatement", content).Result;
                                        var statementWallet = httpResponse.Content.ReadAsStringAsync().Result;

                                        if (!string.IsNullOrEmpty(statementWallet))
                                            enquireMinistatementResponse = JsonConvert.DeserializeObject<EnquireMinistatementResponse>(statementWallet);

                                        if (enquireMinistatementResponse == null || enquireMinistatementResponse.StatusCode != "0000")
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.FAILED;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireMinistatementResponse?.StatusDescription ?? "Statement request failed.";
                                            db.SaveChanges();
                                            return "END " + (enquireMinistatementResponse?.StatusDescription ?? "Your Deposits Statement Request could not be processed. Please try again later.");
                                        }
                                        else
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.SUCCESS;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireMinistatementResponse.StatusDescription;
                                            db.SaveChanges();
                                            return "CON " + enquireMinistatementResponse.StatusDescription + "\n0. BACK";
                                        }
                                    }
                                    return "END Could not process your request. Kindly try again later or contact Rubani Sacco.";
                                }
                            }
                        }

                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                            return "END An error occurred while processing your request. Please try again later.";
                        }
                    }

                case "3":
                    {
                        try
                        {
                            using (var transdb = new SwizzKash_Mobile_DBEntities())
                            {
                                MTransaction mTransaction = new MTransaction
                                {
                                    TransactionDate = DateTime.Now,
                                    ClientCode = request.customer.Client,
                                    SessionID = request.SESSIONID,
                                    AccountNo = request.MSISDN,
                                    SaccoCode = request.SACCO_CODE,
                                    AccountName = "",
                                    DocumentNo = "MD" + request.SESSIONID,
                                    TransactionType = (int)TRANSACTION_TYPE.MINISTATEMENT_SHARES,
                                    Telephone = request.MSISDN,
                                    Posted = false,
                                    DatePosted = null,
                                    DestinationAccount = "",
                                    LoanNo = "",
                                    Status = 0,
                                    Comments = "",
                                    Amount = 0,
                                    Charge = 10,
                                    Description = TRANSACTION_TYPE.MINISTATEMENT_SHARES.ToString(),
                                    ApplicationType = "USSD",
                                    SentToCBS = false
                                };


                                transdb.MTransactions.Add(mTransaction);
                                transdb.SaveChanges();
                                int entryNo = mTransaction.EntryNo;
                                NextMenu(request);

                                using (var db = new SwizzKash_Mobile_DBEntities())
                                {
                                    var thistrans = db.MTransactions.FirstOrDefault(b =>
                                        b.EntryNo == entryNo &&
                                        b.TransactionType == (int)TRANSACTION_TYPE.MINISTATEMENT_SHARES);

                                    //if (thistrans != null)
                                    //{
                                    //    EnquireMinistatementResponse enquireMinistatementResponse = null;
                                    //    var statementWallet = Config.service.MINISTATEMENT_SHARES(
                                    //        thistrans.EntryNo.ToString(),
                                    //        request.MSISDN.Replace("+254", "0"),
                                    //        thistrans.DocumentNo,
                                    //        thistrans.AccountNo,
                                    //        DateTime.Now);
                                    if (thistrans != null)
                                    {
                                        EnquireMinistatementResponse enquireMinistatementResponse = null;
                                        var client = new System.Net.Http.HttpClient();
                                        var payload = JsonConvert.SerializeObject(new
                                        {
                                            PhoneNumber = request.MSISDN.Replace("+254", "0"),
                                            SaccoCode = request.SACCO_CODE,
                                            AccountId = thistrans.AccountNo,
                                            DateTime.Now
                                        });
                                        var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                                        var httpResponse = client.PostAsync($"{API_BASE_URL}/GetBOSAMiniStatement", content).Result;
                                        var statementWallet = httpResponse.Content.ReadAsStringAsync().Result;

                                        if (!string.IsNullOrEmpty(statementWallet))
                                            enquireMinistatementResponse = JsonConvert.DeserializeObject<EnquireMinistatementResponse>(statementWallet);

                                        if (enquireMinistatementResponse == null || enquireMinistatementResponse.StatusCode != "0000")
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.FAILED;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireMinistatementResponse?.StatusDescription ?? "Statement request failed.";
                                            db.SaveChanges();
                                            return "END " + (enquireMinistatementResponse?.StatusDescription ?? "Your Shares Statement Request could not be processed. Please try again later.");
                                        }
                                        else
                                        {
                                            thistrans.Posted = true;
                                            thistrans.DatePosted = DateTime.Now;
                                            thistrans.Status = (int)STATUS.SUCCESS;
                                            thistrans.SentToCBS = true;
                                            thistrans.DateSentToCBS = DateTime.Now;
                                            thistrans.Comments = enquireMinistatementResponse.StatusDescription;
                                            db.SaveChanges();
                                            return "CON " + enquireMinistatementResponse.StatusDescription + "\n0. BACK";
                                        }
                                    }
                                    return "END Could not process your request. Kindly try again later or contact Rubani Sacco.";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                            return "END An error occurred while processing your request. Please try again later.";
                        }
                    }

                case "4":
                    {
                        List<LoanBalances> loansList = GetOutstandingLoans(request);
                        if (loansList == null)
                            return "END Loans not found, Kindly contact Rubani Sacco";

                        string strloans = string.Empty;
                        int menupos = 0;
                        foreach (var loan in loansList)
                        {
                            menupos += 1;
                            strloans += $"{menupos}. {loan.LoanName}." + newline;
                        }
                        return strloans == string.Empty
                            ? "END Loans not found, Kindly contact Rubani Sacco"
                            : "CON Select Loan below " + newline + strloans + newline;
                    }

                case "0":
                    {
                        request.transactionType = TRANSACTION_TYPE.RUBANIMenu;
                        return MainMenu();
                    }

                default:
                    {
                        return "CON Invalid option, please try again " + newline +
                               "1. M-Wallet Statement" + newline +
                               "2. Deposit Contribution. " + newline +
                               "3. Share Capital" + newline +
                               "4. Loans" + newline +
                               "0. BACK";
                    }
            }
        }

        // MINI STATEMENT LOAN SELECTION HANDLER
        private static string HandleMiniStatement(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            if (lastInput == "0")
            {
                request.transactionType = TRANSACTION_TYPE.STATEMENT;
                return "CON Select Option. " + newline +
                       "1. M-Wallet Statement" + newline +
                       "2. Deposit Contribution. " + newline +
                       "3. Share Capital" + newline +
                       "4. Loans" + newline +
                       "0. BACK";
            }

            try
            {
                string resp = null;
                using (var client = new System.Net.Http.HttpClient())
                {
                    var payload = JsonConvert.SerializeObject(new
                    {
                        PhoneNumber = request.MSISDN.Replace("254", "0"),
                        SaccoCode = request.SACCO_CODE
                    });
                    var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                    var httpResponse = client.PostAsync($"{API_BASE_URL}/GetOutstandingLoans", content).Result;
                    resp = httpResponse.Content.ReadAsStringAsync().Result;
                }

                if (string.IsNullOrEmpty(resp)) return "END No Loans found";

                LoanBalanceResponse loanBalanceResponse = JsonConvert.DeserializeObject<LoanBalanceResponse>(resp);
                if (loanBalanceResponse.StatusCode != "000")
                    return "CON You have no pending Loans";

                if (!int.TryParse(lastInput, out int loanIdx) || loanIdx < 1 || loanIdx > loanBalanceResponse.LoanBalances.Count)
                {
                    string strloans = string.Empty;
                    int menupos = 0;
                    foreach (var ln in loanBalanceResponse.LoanBalances)
                    {
                        menupos += 1;
                        strloans += $"{menupos}. {ln.LoanName}." + newline;
                    }
                    return "CON Invalid option, please try again " + newline + strloans + newline;
                }

                LoanBalances currloan = loanBalanceResponse.LoanBalances.ElementAt(loanIdx - 1);

                using (var transdb = new SwizzKash_Mobile_DBEntities())
                {
                    MTransaction mTransaction = new MTransaction
                    {
                        TransactionDate = DateTime.Now,
                        ClientCode = request.customer.Client,
                        SessionID = request.SESSIONID,
                        AccountNo = currloan.LoanNo,
                        AccountName = currloan.LoanName,
                        DocumentNo = "ML" + request.SESSIONID,
                        TransactionType = (int)TRANSACTION_TYPE.MINISTATEMENT_LOAN,
                        Telephone = request.MSISDN,
                        SaccoCode = request.SACCO_CODE,
                        Posted = false,
                        DatePosted = null,
                        DestinationAccount = currloan.LoanNo,
                        LoanNo = currloan.LoanNo,
                        Status = 0,
                        Comments = "",
                        Amount = 0,
                        Charge = 10,
                        Description = TRANSACTION_TYPE.MINISTATEMENT_LOAN.ToString(),
                        ApplicationType = "USSD",
                        SentToCBS = false
                    };


                    transdb.MTransactions.Add(mTransaction);
                    transdb.SaveChanges();
                    int entryNo = mTransaction.EntryNo;
                    NextMenu(request);

                    using (var db = new SwizzKash_Mobile_DBEntities())
                    {
                        var thistrans = db.MTransactions.FirstOrDefault(b =>
                            b.EntryNo == entryNo &&
                            b.TransactionType == (int)TRANSACTION_TYPE.MINISTATEMENT_LOAN);

                        if (thistrans != null)
                        {
                            EnquireMinistatementResponse enquireMinistatementResponse = null;
                            var statementWallet = Config.service.MINISTATEMENT_LOAN(
                                thistrans.EntryNo.ToString(),
                                request.MSISDN.Replace("+254", "0"),
                                thistrans.DocumentNo,
                                thistrans.AccountNo,
                                DateTime.Now);

                            if (!string.IsNullOrEmpty(statementWallet))
                                enquireMinistatementResponse = JsonConvert.DeserializeObject<EnquireMinistatementResponse>(statementWallet);

                            if (enquireMinistatementResponse == null || enquireMinistatementResponse.StatusCode != "0000")
                            {
                                thistrans.Posted = true;
                                thistrans.DatePosted = DateTime.Now;
                                thistrans.Status = (int)STATUS.FAILED;
                                thistrans.SentToCBS = true;
                                thistrans.DateSentToCBS = DateTime.Now;
                                thistrans.Comments = enquireMinistatementResponse?.StatusDescription ?? "Statement request failed.";
                                db.SaveChanges();
                                return "END " + (enquireMinistatementResponse?.StatusDescription ?? "Your Loan Statement Request could not be processed. Please try again later.");
                            }
                            else
                            {
                                thistrans.Posted = true;
                                thistrans.DatePosted = DateTime.Now;
                                thistrans.Status = (int)STATUS.SUCCESS;
                                thistrans.SentToCBS = true;
                                thistrans.DateSentToCBS = DateTime.Now;
                                thistrans.Comments = enquireMinistatementResponse.StatusDescription;
                                db.SaveChanges();
                                return "CON " + enquireMinistatementResponse.StatusDescription + "\n0. BACK";
                            }
                        }
                        return "END Could not process your request. Kindly try again later or contact Rubani Sacco.";
                    }
                }
            }

            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return "END An error occurred while processing your request. Please try again later.";
            }
        }

        // TRANSFER HANDLER
        private static string HandleTransfer(SwizzRequest request, string lastInput)
        {
            switch (lastInput)
            {
                case "1":
                    {
                        request.transactionType = TRANSACTION_TYPE.fosatransfer;
                        return "CON Select Option Below" + newline +
                               "1. Own Account." + newline +
                               "2. Other Account." + newline +
                               "0. Back";
                    }
                case "2":
                    {
                        request.transactionType = TRANSACTION_TYPE.bosatransfer;
                        return "CON Select Option Below" + newline +
                               "1. Transfer to Deposit Contribution." + newline +
                               "2. Transfer to Holiday Contribution." + newline +
                               "0. Back";
                    }
                case "0":
                    {
                        request.transactionType = TRANSACTION_TYPE.RUBANIMenu;
                        return MainMenu();
                    }
                default:
                    {
                        return "CON Invalid option, please try again " + newline +
                               "1. M-Wallet Transfer " + newline +
                               "2. BOSA Transfer " + newline +
                               "0. Back";
                    }
            }
        }

        // FOSA TRANSFER HANDLER
        private static string HandleFosaTransfer(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            if (lastInput == "0")
            {
                request.transactionType = TRANSACTION_TYPE.transfer;
                return "CON Select Option " + newline +
                       "1. M-Wallet Transfer " + newline +
                       "2. BOSA Transfer " + newline +
                       "0. Back";
            }

            int depth = ussd_string.Length;

            try
            {
                List<Account> accountsList = GetWSSAccounts(request);
                if (accountsList is null) return "END Accounts not found, Kindly contact Rubani Sacco";

                if (depth == 2)
                {
                    // own vs other
                    switch (lastInput)
                    {
                        case "1":
                            return ChoiceTransfer(request, "1");
                        case "2":
                            request.transactionType = TRANSACTION_TYPE.FOSA_TO_OTHER_MEMBER;
                            return ChoiceTransfer(request, "1");
                        default:
                            return "CON Invalid option, please try again " + newline +
                                   "1. Own Account." + newline +
                                   "2. Other Account." + newline +
                                   "0. Back";
                    }
                }
                else if (depth == 3)
                {
                    // source account selected
                    if (!int.TryParse(lastInput, out int accIdx) || accIdx < 1 || accIdx > accountsList.Count)
                    {
                        return ChoiceTransfer(request, "1").Replace("CON Select Source account", "CON Invalid option, please try again " + newline + "Select Source account");
                    }
                    // get destination accounts (excluding source)
                    var sourceAccount = accountsList.ElementAt(accIdx - 1);
                    var destAccounts = accountsList.Where(a => a.AccountNo != sourceAccount.AccountNo).ToList();
                    string straccounts = string.Empty;
                    int menupos = 0;
                    foreach (var acc in destAccounts)
                    {
                        menupos += 1;
                        straccounts += $"{menupos}. {acc.AccountName}." + newline;
                    }
                    return straccounts == string.Empty
                        ? "END No destination accounts available"
                        : "CON Select Destination account " + newline + straccounts + newline;
                }
                else if (depth == 4)
                {
                    // destination account selected, ask for amount
                    if (!int.TryParse(lastInput, out int destIdx))
                        return "CON Invalid option, please enter destination account number" + newline;
                    return "CON Enter Amount";
                }
                else if (depth == 5)
                {
                    // amount entered - process transfer
                    if (!decimal.TryParse(lastInput, out decimal transferAmount))
                        return "CON Invalid amount, please enter a valid amount" + newline;

                    string sourceAccStr = ussd_string[ussd_string.Length - 3];
                    string destAccStr = ussd_string[ussd_string.Length - 2];

                    if (!int.TryParse(sourceAccStr, out int sourceIdx) || sourceIdx < 1 || sourceIdx > accountsList.Count)
                        return "END Invalid source account";

                    var sourceAccount = accountsList.ElementAt(sourceIdx - 1);
                    var destAccounts = accountsList.Where(a => a.AccountNo != sourceAccount.AccountNo).ToList();

                    if (!int.TryParse(destAccStr, out int destIdx) || destIdx < 1 || destIdx > destAccounts.Count)
                        return "END Invalid destination account";

                    var destAccount = destAccounts.ElementAt(destIdx - 1);

                    using (var transdb = new SwizzKash_Mobile_DBEntities())
                    {
                        MTransaction mTransaction = new MTransaction
                        {
                            TransactionDate = DateTime.Now,
                            ClientCode = request.customer.Client,
                            SessionID = request.SESSIONID,
                            AccountNo = sourceAccount.AccountNo,
                            AccountName = sourceAccount.AccountName,
                            DestinationAccount = destAccount.AccountNo,
                            DocumentNo = "FF" + request.SESSIONID,
                            TransactionType = (int)TRANSACTION_TYPE.FOSA_TO_FOSA,
                            Telephone = request.MSISDN,
                            SaccoCode = request.SACCO_CODE,
                            Posted = false,
                            DatePosted = null,
                            LoanNo = "",
                            Status = 0,
                            Comments = "",
                            Amount = transferAmount,
                            Charge = 10,
                            Description = TRANSACTION_TYPE.FOSA_TO_FOSA.ToString(),
                            ApplicationType = "USSD"
                        };

                        transdb.MTransactions.Add(mTransaction);
                        transdb.SaveChanges();
                        NextMenu(request);
                        return "END Your request to transfer KSH " + transferAmount + " from account " + sourceAccount.AccountNo +
                               " to " + destAccount.AccountNo + " is being processed. Rubani MOBILE";
                    }
                }
                return "END Invalid option selected";
            }

            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return "END An error occurred. Please try again later.";
            }
        }

        // FOSA TO OTHER MEMBER HANDLER// galastian 5: 16-22: collosia 3 and 4 chepter
        private static string HandleFosaToOtherMember(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            if (lastInput == "0")
            {
                request.transactionType = TRANSACTION_TYPE.transfer;
                return "CON Select Option " + newline +
                       "1. M-Wallet Transfer " + newline +
                       "2. BOSA Transfer " + newline +
                       "0. Back";
            }

            int depth = ussd_string.Length;

            try
            {
                List<Account> accountsList = GetWSSAccounts(request);
                if (accountsList is null) return "END Accounts not found, Kindly contact Rubani Sacco";

                if (depth == 3)
                {
                    // source account selected
                    if (!int.TryParse(lastInput, out int accIdx) || accIdx < 1 || accIdx > accountsList.Count)
                        return ChoiceTransfer(request, "1").Replace("CON Select Source account", "CON Invalid option " + newline + "Select Source account");
                    return "CON Enter Destination Account";
                }
                else if (depth == 4)
                {
                    // destination account entered
                    string destinationAccount = lastInput.Trim();
                    var resp = Config.service.GetAccountDetails(destinationAccount);
                    if (resp == null) return $"END Could not verify destination account {destinationAccount}";

                    WSAccountsResponse wSAccountsResponse = JsonConvert.DeserializeObject<WSAccountsResponse>(resp);
                    if (wSAccountsResponse.StatusCode != "000")
                        return $"END Destination account {destinationAccount} not found";

                    return "CON Enter Amount";
                }
                else if (depth == 5)
                {
                    // amount entered - process
                    if (!decimal.TryParse(lastInput, out decimal transferAmount))
                        return "CON Invalid amount, please enter a valid amount" + newline;

                    string sourceAccStr = ussd_string[ussd_string.Length - 3];
                    string destinationAccount = ussd_string[ussd_string.Length - 2].Trim();

                    if (!int.TryParse(sourceAccStr, out int sourceIdx) || sourceIdx < 1 || sourceIdx > accountsList.Count)
                        return "END Invalid source account";

                    var sourceAccount = accountsList.ElementAt(sourceIdx - 1);

                    // verify destination account
                    var resp = Config.service.GetAccountDetails(destinationAccount);
                    if (resp == null) return $"END Could not verify destination account {destinationAccount}";

                    WSAccountsResponse wSAccountsResponse = JsonConvert.DeserializeObject<WSAccountsResponse>(resp);
                    if (wSAccountsResponse.StatusCode != "000")
                        return $"END Destination account {destinationAccount} not found";

                    string destinationAccName = wSAccountsResponse.Accounts?.ElementAt(0).AccountName ?? destinationAccount;

                    using (var transdb = new SwizzKash_Mobile_DBEntities())
                    {
                        MTransaction mTransaction = new MTransaction
                        {
                            TransactionDate = DateTime.Now,
                            ClientCode = request.customer.Client,
                            SessionID = request.SESSIONID,
                            AccountNo = sourceAccount.AccountNo,
                            AccountName = sourceAccount.AccountName,
                            DestinationAccount = destinationAccount,
                            DocumentNo = "FO" + request.SESSIONID,
                            SaccoCode = request.SACCO_CODE,
                            TransactionType = (int)TRANSACTION_TYPE.FOSA_TO_OTHER_MEMBER,
                            Telephone = request.MSISDN,
                            Posted = false,
                            DatePosted = null,
                            LoanNo = "",
                            Status = 0,
                            Comments = "",
                            Amount = transferAmount,
                            Charge = 10,
                            Description = TRANSACTION_TYPE.FOSA_TO_OTHER_MEMBER.ToString(),
                            ApplicationType = "USSD",
                            SentToCBS = false
                        };


                        transdb.MTransactions.Add(mTransaction);
                        transdb.SaveChanges();
                        int entryNo = mTransaction.EntryNo;
                        NextMenu(request);

                        using (var db = new SwizzKash_Mobile_DBEntities())
                        {
                            var thistrans = db.MTransactions.FirstOrDefault(b =>
                                b.EntryNo == entryNo &&
                                b.TransactionType == (int)TRANSACTION_TYPE.FOSA_TO_OTHER_MEMBER);

                            if (thistrans != null)
                            {
                                LoanApplicationResponse loanApplicationResponse = null;
                                var applicationResponse = Config.service.FundsTransferFOSAOTHER(
                                    thistrans.EntryNo.ToString(),
                                    thistrans.DocumentNo,
                                    request.MSISDN.Replace("+254", "0"),
                                    thistrans.AccountNo,
                                    thistrans.DestinationAccount,
                                    Convert.ToDecimal(thistrans.Amount),
                                    DateTime.Now);

                                if (!string.IsNullOrEmpty(applicationResponse))
                                    loanApplicationResponse = JsonConvert.DeserializeObject<LoanApplicationResponse>(applicationResponse);

                                if (loanApplicationResponse == null || loanApplicationResponse.StatusCode != "000")
                                {
                                    thistrans.Posted = true;
                                    thistrans.DatePosted = DateTime.Now;
                                    thistrans.Status = (int)STATUS.FAILED;
                                    thistrans.SentToCBS = true;
                                    thistrans.DateSentToCBS = DateTime.Now;
                                    thistrans.Comments = loanApplicationResponse?.StatusDescription ?? "M-Wallet Transfer request failed.";
                                    db.SaveChanges();
                                    return "END " + (loanApplicationResponse?.StatusDescription ?? "Your M-Wallet Transfer Request could not be processed. Please try again later.");
                                }
                                else
                                {
                                    thistrans.Posted = true;
                                    thistrans.DatePosted = DateTime.Now;
                                    thistrans.Status = (int)STATUS.SUCCESS;
                                    thistrans.SentToCBS = true;
                                    thistrans.DateSentToCBS = DateTime.Now;
                                    thistrans.Comments = loanApplicationResponse.StatusDescription;
                                    db.SaveChanges();
                                    return "END " + loanApplicationResponse.StatusDescription;
                                }
                            }
                            return "END Could not process your request. Kindly try again later or contact Rubani Sacco.";
                        }
                    }
                }
                return "END Invalid option selected";
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return "END An error occurred. Please try again later.";
            }
        }

        // BOSA TRANSFER HANDLER
        private static string HandleBosaTransfer(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            if (lastInput == "0")
            {
                request.transactionType = TRANSACTION_TYPE.transfer;
                return "CON Select Option " + newline +
                       "1. M-Wallet Transfer " + newline +
                       "2. BOSA Transfer " + newline +
                       "0. Back";
            }

            int depth = ussd_string.Length;

            try
            {
                if (depth == 2)
                {
                    switch (lastInput)
                    {
                        case "1":
                        case "2":
                            return ChoiceTransfer(request, "1");
                        default:
                            return "CON Invalid option, please try again " + newline +
                                   "1. Transfer to Deposit Contribution." + newline +
                                   "2. Transfer to Holiday Contribution." + newline +
                                   "0. Back";
                    }
                }
                else if (depth == 3)
                {
                    // source account selected
                    List<Account> accountsList = GetWSSAccounts(request);
                    if (accountsList == null) return "END Accounts not found, Kindly contact Rubani Sacco";
                    if (!int.TryParse(lastInput, out int accIdx) || accIdx < 1 || accIdx > accountsList.Count)
                        return ChoiceTransfer(request, "1").Replace("CON Select Source account", "CON Invalid option " + newline + "Select Source account");
                    return "CON Enter Amount";
                }
                else if (depth == 4)
                {
                    // amount entered - process BOSA transfer
                    if (!decimal.TryParse(lastInput, out decimal transferAmount))
                        return "CON Invalid amount, please enter a valid amount" + newline;

                    string bosaTypeStr = ussd_string[ussd_string.Length - 3];
                    string sourceAccStr = ussd_string[ussd_string.Length - 2];

                    string destinationAccount = string.Empty;
                    switch (bosaTypeStr)
                    {
                        case "1": destinationAccount = "Deposit Contribution"; break;
                        case "2": destinationAccount = "Xmas Contribution"; break;
                        default: return "END Invalid BOSA account type";
                    }

                    List<Account> accountsList = GetWSSAccounts(request);
                    if (accountsList == null) return "END Accounts not found, Kindly contact Rubani Sacco";
                    if (!int.TryParse(sourceAccStr, out int sourceIdx) || sourceIdx < 1 || sourceIdx > accountsList.Count)
                        return "END Invalid source account";

                    var account = accountsList.ElementAt(sourceIdx - 1);

                    using (var transdb = new SwizzKash_Mobile_DBEntities())
                    {
                        MTransaction mTransaction = new MTransaction
                        {
                            TransactionDate = DateTime.Now,
                            ClientCode = request.customer.Client,
                            SessionID = request.SESSIONID,
                            AccountNo = account.AccountNo,
                            AccountName = account.AccountName,
                            DocumentNo = "FB" + request.SESSIONID,
                            TransactionType = (int)TRANSACTION_TYPE.FOSA_TO_BOSA,
                            Telephone = request.MSISDN,
                            SaccoCode= request.SACCO_CODE,
                            Posted = false,
                            DatePosted = null,
                            DestinationAccount = destinationAccount,
                            LoanNo = "",
                            Status = 0,
                            Comments = "",
                            Amount = transferAmount,
                            Charge = 10,
                            Description = TRANSACTION_TYPE.FOSA_TO_BOSA.ToString(),
                            ApplicationType = "USSD",
                            SentToCBS = false
                        };

                        transdb.MTransactions.Add(mTransaction);
                        transdb.SaveChanges();
                        int entryNo = mTransaction.EntryNo;
                        NextMenu(request);

                        using (var db = new SwizzKash_Mobile_DBEntities())
                        {
                            var thistrans = db.MTransactions.FirstOrDefault(b =>
                                b.EntryNo == entryNo &&
                                b.TransactionType == (int)TRANSACTION_TYPE.FOSA_TO_BOSA);

                            if (thistrans != null)
                            {
                                FundsTransferFOSAResponse fundsTransferFOSAResponse = null;
                                var applicationResponse = Config.service.FundsTransferBOSA(
                                    thistrans.EntryNo.ToString(),
                                    thistrans.DocumentNo,
                                    request.MSISDN.Replace("+254", "0"),
                                    thistrans.AccountNo,
                                    thistrans.DestinationAccount,
                                    Convert.ToDecimal(thistrans.Amount),
                                    DateTime.Now);

                                if (!string.IsNullOrEmpty(applicationResponse))
                                    fundsTransferFOSAResponse = JsonConvert.DeserializeObject<FundsTransferFOSAResponse>(applicationResponse);

                                if (fundsTransferFOSAResponse == null || fundsTransferFOSAResponse.StatusCode != "000")
                                {
                                    thistrans.Posted = true;
                                    thistrans.DatePosted = DateTime.Now;
                                    thistrans.Status = (int)STATUS.FAILED;
                                    thistrans.SentToCBS = true;
                                    thistrans.DateSentToCBS = DateTime.Now;
                                    thistrans.Comments = fundsTransferFOSAResponse?.StatusDescription ?? "FOSA Transfer request failed.";
                                    db.SaveChanges();
                                    return "END " + (fundsTransferFOSAResponse?.StatusDescription ?? "Your M-Wallet Transfer Request could not be processed. Please try again later.");
                                }
                                else
                                {
                                    thistrans.Posted = true;
                                    thistrans.DatePosted = DateTime.Now;
                                    thistrans.Status = (int)STATUS.SUCCESS;
                                    thistrans.SentToCBS = true;
                                    thistrans.DateSentToCBS = DateTime.Now;
                                    thistrans.Comments = fundsTransferFOSAResponse.StatusDescription;
                                    db.SaveChanges();
                                    return "END " + fundsTransferFOSAResponse.StatusDescription;
                                }
                            }
                            return "END Could not process your request. Kindly try again later or contact Rubani Sacco.";
                        }
                    }
                }
                return "END Invalid option selected";
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return "END An error occurred. Please try again later.";
            }
        }

        // LOAN REPAYMENT HANDLER
        private static string HandleLoanRepayment(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            if (lastInput == "0")
            {
                request.transactionType = TRANSACTION_TYPE.LOANS;
                return "CON Select Option " + newline +
                       "1. Loan Balances " + newline +
                       "2. Loan Repayment " + newline +
                       "3. M-Loan " + newline +
                       "0. Back";
            }

            int depth = ussd_string.Length;

            try
            {
                if (depth == 2)
                {
                    // payment source selection
                    switch (lastInput)
                    {
                        case "1":
                            {
                                List<Account> accountsList = GetWSSAccounts(request);
                                if (accountsList is null) return "END Accounts not found, Kindly contact Rubani Sacco";
                                string straccounts = string.Empty;
                                int menupos = 0;
                                foreach (var acc in accountsList)
                                {
                                    menupos += 1;
                                    straccounts += $"{menupos}. {acc.AccountName}." + newline;
                                }
                                return straccounts == string.Empty
                                    ? "END Accounts not found, Kindly contact Rubani Sacco"
                                    : "CON Select source account below " + newline + straccounts + newline;
                            }
                        case "2":
                            {
                                List<LoanBalances> loansList = GetOutstandingLoans(request);
                                if (loansList == null) return "END Loans not found, Kindly contact Rubani Sacco";
                                string strloans = string.Empty;
                                int menupos = 0;
                                foreach (var loan in loansList)
                                {
                                    menupos += 1;
                                    strloans += $"{menupos}. {loan.LoanName} {loan.BalanceAmount.ToString("#0")}." + newline;
                                }
                                return strloans == string.Empty
                                    ? "END Loans not found, Kindly contact Rubani Sacco"
                                    : "CON Select Loan below " + newline + strloans + newline;
                            }
                        default:
                            return "CON Invalid option, please try again " + newline +
                                   "1. Own Account" + newline +
                                   "2. M-PESA Paybill" + newline +
                                   "0. Back";
                    }
                }
                else if (depth == 3)
                {
                    string paymentSource = ussd_string[ussd_string.Length - 2];
                    switch (paymentSource)
                    {
                        case "1":
                            {
                                // own account - loan selection
                                List<LoanBalances> loansList = GetOutstandingLoans(request);
                                if (loansList == null) return "END Loans not found, Kindly contact Rubani Sacco";
                                if (!int.TryParse(lastInput, out int accIdx))
                                {
                                    List<Account> accountsList = GetWSSAccounts(request);
                                    string straccounts = string.Empty;
                                    int menupos = 0;
                                    foreach (var acc in accountsList)
                                    {
                                        menupos += 1;
                                        straccounts += $"{menupos}. {acc.AccountName}." + newline;
                                    }
                                    return "CON Invalid option, please try again " + newline + straccounts + newline;
                                }
                                string strloans = string.Empty;
                                int lmenupos = 0;
                                foreach (var loan in loansList)
                                {
                                    lmenupos += 1;
                                    strloans += $"{lmenupos}. {loan.LoanName} {loan.BalanceAmount.ToString("#0")}." + newline;
                                }
                                return strloans == string.Empty
                                    ? "END Loans not found, kindly contact Rubani Sacco"
                                    : "CON Select Loan below " + newline + strloans + newline;
                            }

                        case "2":
                            {
                                // mpesa - amount entry
                                if (!int.TryParse(lastInput, out int loanIdx))
                                {
                                    List<LoanBalances> loansList2 = GetOutstandingLoans(request);
                                    string strloans2 = string.Empty;
                                    int m2 = 0;
                                    foreach (var ln in loansList2)
                                    {
                                        m2 += 1;
                                        strloans2 += $"{m2}. {ln.LoanName} {ln.BalanceAmount.ToString("#0")}." + newline;
                                    }
                                    return "CON Invalid option, please try again " + newline + strloans2 + newline;
                                }
                                return "CON Enter Amount";
                            }
                        default:
                            return "END Invalid option selected";
                    }
                }
                else if (depth == 4)
                {
                    string paymentSource = ussd_string[ussd_string.Length - 3];
                    switch (paymentSource)
                    {
                        case "1":
                            {
                                // own account - amount entry
                                if (!int.TryParse(lastInput, out int loanIdx))
                                    return "CON Invalid option, please select a valid loan" + newline;
                                return "CON Enter Amount";
                            }

                        case "2":
                            {
                                // mpesa - confirmation
                                string loanIdxStr = ussd_string[ussd_string.Length - 2];
                                List<LoanBalances> loansList = GetOutstandingLoans(request);
                                if (loansList == null) return "END Loans not found, Kindly contact Rubani Sacco";
                                if (!int.TryParse(loanIdxStr, out int loanIdx) || loanIdx < 1 || loanIdx > loansList.Count)
                                    return "END Invalid loan selected";
                                var selectedloan = loansList.ElementAt(loanIdx - 1);
                                return $"CON Repay loan number {selectedloan.LoanNo} KES {lastInput}?{newline}1. Yes{newline}2. Cancel.";
                            }
                        default:
                            return "END Invalid option selected";
                    }
                }
                else if (depth == 5)
                {
                    string paymentSource = ussd_string[ussd_string.Length - 4];
                    switch (paymentSource)
                    {
                        case "1":
                            {
                                // own account - confirmation
                                string sourceAccStr = ussd_string[ussd_string.Length - 3];
                                string loanIdxStr = ussd_string[ussd_string.Length - 2];
                                string amountStr = lastInput;

                                List<Account> accountsList = GetWSSAccounts(request);
                                if (accountsList is null) return "END Source Account not found, Kindly contact Rubani Sacco";
                                if (!int.TryParse(sourceAccStr, out int sourceIdx) || sourceIdx < 1 || sourceIdx > accountsList.Count)
                                    return "END Invalid source account";
                                var selectedSourceAccount = accountsList.ElementAt(sourceIdx - 1);

                                List<LoanBalances> loansList = GetOutstandingLoans(request);
                                if (loansList == null) return "END Loans not found, Kindly contact Rubani Sacco";
                                if (!int.TryParse(loanIdxStr, out int loanIdx) || loanIdx < 1 || loanIdx > loansList.Count)
                                    return "END Invalid loan selected";
                                var selectedloan = loansList.ElementAt(loanIdx - 1);

                                if (!decimal.TryParse(amountStr, out decimal amountEntered))
                                    return "CON Invalid amount entered" + newline;

                                GetBalanceResponse balanceResponse = GetWSSAccountBalance(request);
                                decimal accBalance = balanceResponse.AccountBalance;
                                if (accBalance < amountEntered)
                                    return "END Insufficient funds in the source account to complete loan repayment. Rubani MOBILE";

                                return $"CON Repay loan number {selectedloan.LoanNo} KES {amountStr} from {selectedSourceAccount.AccountNo}?{newline}1. Yes{newline}2. Cancel.";
                            }

                        case "2":
                            {
                                // mpesa confirmation result
                                if (lastInput == "2") return "END You have canceled loan repayment request. Rubani MOBILE";
                                if (lastInput != "1") return "CON Invalid option " + newline + "1. Yes" + newline + "2. Cancel.";

                                string loanIdxStr = ussd_string[ussd_string.Length - 3];
                                string amountStr = ussd_string[ussd_string.Length - 2];

                                List<LoanBalances> loansList = GetOutstandingLoans(request);
                                if (loansList == null) return "END Loans not found, Kindly contact Rubani SACCO";
                                if (!int.TryParse(loanIdxStr, out int loanIdx) || loanIdx < 1 || loanIdx > loansList.Count)
                                    return "END Invalid loan selected";
                                var selectedloan = loansList.ElementAt(loanIdx - 1);

                                if (!decimal.TryParse(amountStr, out decimal repaymentAmount))
                                    return "END Invalid amount";

                                using (var dbC2b = new SwizzKash_Mobile_DBEntities())
                                {
                                    STKPushTransaction tran = new STKPushTransaction();
                                    try
                                    {
                                        tran.SaccoID = request.customer.Client;
                                        tran.AccountNo = request.MSISDN.Replace("+", "");
                                        tran.AccountName = selectedloan.LoanName;
                                        tran.DocumentNo = "PL" + request.SESSIONID;
                                        tran.DocumentDate = DateTime.Now;
                                        tran.Transaction_Type = (int)TRANSACTION_TYPE.PAYBILL_STK;
                                        tran.Telephone = request.MSISDN;
                                        tran.SaccoCode = request.SACCO_CODE;
                                        tran.Posted = false;
                                        tran.Date_Posted = null;
                                        tran.Account_2 = selectedloan.LoanNo;
                                        tran.Status = 0;
                                        tran.Comments = "";
                                        tran.Amount = repaymentAmount;
                                        tran.Charge = 0;
                                        tran.Application_Type = "USSD";
                                        tran.Description = TRANSACTION_TYPE.PAYBILL_STK.ToString();
                                        dbC2b.STKPushTransactions.Add(tran);
                                        dbC2b.SaveChanges();
                                        return "END Please wait for safaricom to complete request. Rubani MOBILE";
                                    }
                                    catch (Exception ex)
                                    {
                                        _log.Error(ex.Message);
                                        return "END A problem has been encountered, please retry after 10 minutes or contact Rubani SACCO";
                                    }
                                }
                            }
                        default:
                            return "END Invalid option selected";
                    }
                }
                else if (depth == 6)
                {
                    // own account final confirmation
                    string paymentSource = ussd_string[ussd_string.Length - 5];
                    if (paymentSource != "1") return "END Invalid option selected";

                    if (lastInput == "2") return "END You have canceled loan repayment request. Rubani MOBILE";
                    if (lastInput != "1") return "CON Invalid option " + newline + "1. Yes" + newline + "2. Cancel.";

                    string sourceAccStr = ussd_string[ussd_string.Length - 4];
                    string loanIdxStr = ussd_string[ussd_string.Length - 3];
                    string amountStr = ussd_string[ussd_string.Length - 2];

                    List<Account> accountsList = GetWSSAccounts(request);
                    if (accountsList is null) return "END Source Account not found, Kindly contact Rubani Sacco";
                    if (!int.TryParse(sourceAccStr, out int sourceIdx) || sourceIdx < 1 || sourceIdx > accountsList.Count)
                        return "END Invalid source account";
                    var selectedSourceAccount = accountsList.ElementAt(sourceIdx - 1);

                    List<LoanBalances> loansList = GetOutstandingLoans(request);
                    if (loansList == null) return "END Loans not found, Kindly contact Rubani Sacco";
                    if (!int.TryParse(loanIdxStr, out int loanIdx) || loanIdx < 1 || loanIdx > loansList.Count)
                        return "END Invalid loan selected";
                    var selectedloan = loansList.ElementAt(loanIdx - 1);

                    if (!decimal.TryParse(amountStr, out decimal repaymentAmount))
                        return "END Invalid amount";

                    using (var transdb = new SwizzKash_Mobile_DBEntities())
                    {
                        MTransaction mTransaction = new MTransaction
                        {
                            TransactionDate = DateTime.Now,
                            ClientCode = request.customer.Client,
                            SessionID = request.SESSIONID,
                            AccountNo = selectedSourceAccount.AccountNo,
                            AccountName = selectedSourceAccount.AccountName,
                            DocumentNo = "PL" + request.SESSIONID,
                            TransactionType = (int)TRANSACTION_TYPE.FOSA_TO_LOAN,
                            Telephone = request.MSISDN,
                            SaccoCode = request.SACCO_CODE,
                            Posted = false,
                            DatePosted = null,
                            DestinationAccount = selectedloan.LoanNo,
                            LoanPeriod = 0,
                            LoanNo = selectedloan.LoanNo,
                            Status = 0,
                            Comments = "",
                            Amount = repaymentAmount,
                            Charge = 10,
                            Description = TRANSACTION_TYPE.FOSA_TO_LOAN.ToString(),
                            ApplicationType = "USSD",
                            SentToCBS = false
                        };

                        transdb.MTransactions.Add(mTransaction);
                        transdb.SaveChanges();
                        int entryNo = mTransaction.EntryNo;
                        NextMenu(request);

                        using (var db = new SwizzKash_Mobile_DBEntities())
                        {
                            var thistrans = db.MTransactions.FirstOrDefault(b =>
                                b.EntryNo == entryNo &&
                                b.TransactionType == (int)TRANSACTION_TYPE.FOSA_TO_LOAN);

                            if (thistrans != null)
                            {
                                FundsTransferFOSAResponse fundsTransferFOSAResponse = null;
                                string fosaLoanResponse = null;
                                using (var client = new System.Net.Http.HttpClient())
                                {
                                    var payload = JsonConvert.SerializeObject(new
                                    {
                                        PhoneNumber = thistrans.Telephone.Replace("+254", "0"),
                                        SaccoCode = request.SACCO_CODE,
                                        AmountPaid = (decimal)thistrans.Amount
                                    });
                                    var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                                    var httpResponse = client.PostAsync($"{API_BASE_URL}/RepayInstantLoan", content).Result;
                                    fosaLoanResponse = httpResponse.Content.ReadAsStringAsync().Result;
                                }

                                if (!string.IsNullOrEmpty(fosaLoanResponse))
                                    fundsTransferFOSAResponse = JsonConvert.DeserializeObject<FundsTransferFOSAResponse>(fosaLoanResponse);

                                if (fundsTransferFOSAResponse == null || fundsTransferFOSAResponse.StatusCode != "200")
                                {
                                    thistrans.Posted = true;
                                    thistrans.DatePosted = DateTime.Now;
                                    thistrans.Status = (int)STATUS.FAILED;
                                    thistrans.SentToCBS = true;
                                    thistrans.DateSentToCBS = DateTime.Now;
                                    thistrans.Comments = fundsTransferFOSAResponse?.StatusDescription ?? "Loan repayment request failed.";
                                    db.SaveChanges();
                                    return "END " + (fundsTransferFOSAResponse?.StatusDescription ?? "Your Loan repayment could not be processed. Please try again later.");
                                }
                                else
                                {
                                    thistrans.Posted = true;
                                    thistrans.DatePosted = DateTime.Now;
                                    thistrans.Status = (int)STATUS.SUCCESS;
                                    thistrans.SentToCBS = true;
                                    thistrans.DateSentToCBS = DateTime.Now;
                                    thistrans.Comments = fundsTransferFOSAResponse.StatusDescription;
                                    db.SaveChanges();
                                    return "END " + fundsTransferFOSAResponse.StatusDescription;
                                }
                            }
                            return "END Could not process your request. Kindly try again later or contact Rubani Sacco.";
                        }
                    }
                }
                return "END Invalid option selected";
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return "END An error occurred. Please try again later.";
            }
        }


        // LOAN MOBILE HANDLER
        private static string HandleLoanMobile(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            if (lastInput == "0")
            {
                request.transactionType = TRANSACTION_TYPE.LOANS;
                return "CON Select Option " + newline +
                       "1. Loan Balances " + newline +
                       "2. Loan Repayment " + newline +
                       "3. M-Loan " + newline +
                       "0. Back";
            }

            //int depth = ussd_string.Length;
            int depth = ussd_string.Length - 2;

            try
            {
                if (depth == 2)
                {
                    // repayment period selected
                    switch (lastInput)
                    {
                        case "1":
                            {
                                LoanLimitResponse loanLimitResponse = GetLoanAmountLimit(request);
                                if (loanLimitResponse == null)
                                    return "END Unable to retrieve your loan limit.";
                                if (loanLimitResponse.StatusCode != "000" || loanLimitResponse.LimitAmount < 50)
                                    return "END " + loanLimitResponse.Reason;

                                double qual = loanLimitResponse.LimitAmount;
                                if (qual < 500)
                                    return "CON You do not qualify for this loan product." + newline + "0. Back";

                                return "CON You qualify for a loan of up to Ksh." + qual +
                                       " at 7.5% interest per month." + newline +
                                       "Enter Amount:";
                            }
                        default:
                            return "CON Invalid option, please try again " + newline +
                                   " 1. One Month." + newline +
                                   " 0. Back";
                    }
                }
                else if (depth == 3)
                {
                    // amount entered - show confirmation
                    if (!double.TryParse(lastInput, out double amount))
                        return "CON Invalid amount, please enter a valid amount" + newline;

                    LoanLimitResponse loanLimitResponse = GetLoanAmountLimit(request);
                    if (loanLimitResponse == null) return "END Could not get your loan limit";
                    if (loanLimitResponse.StatusCode != "000") return "END " + loanLimitResponse.StatusDescription;

                    double qual = loanLimitResponse.LimitAmount;
                    if (amount > qual)
                        return "CON Request declined, please enter an amount less than or equal to " + qual + ". Rubani MOBILE." + newline + "0. BACK";
                    if (amount < 500)
                        return "CON Request declined, please enter an amount more than or equal to Ksh.500. Rubani MOBILE." + newline + "0. BACK";

                    string period = ussd_string[ussd_string.Length - 2];
                    return $"CON Apply for a M-LOAN of KES {amount} repayable in {period} Month(s) at 7.5% Interest per month.{newline}1. Yes {newline}2. No";
                }
                else if (depth == 4)
                {
                    // confirmation
                    if (lastInput == "2") return "CON You have cancelled your E-Loan application. Rubani MOBILE" + newline + "0. BACK";
                    if (lastInput != "1") return "CON Invalid option " + newline + "1. Yes" + newline + "2. No";

                    string periodStr = ussd_string[ussd_string.Length - 3];
                    string amountStr = ussd_string[ussd_string.Length - 2];

                    if (!double.TryParse(amountStr, out double amount))
                        return "END Invalid amount";

                    List<Account> accountsList = GetWSSAccounts(request);
                    if (accountsList == null) return "END Account not found, Kindly contact Rubani Sacco";
                    var account = accountsList.ElementAt(0);
                    //var account = new Account { AccountNo = request.MSISDN, AccountName = "TEMP" };

                    using (var transdb = new SwizzKash_Mobile_DBEntities())
                    {
                        MTransaction mTransaction = new MTransaction
                        {
                            TransactionDate = DateTime.Now,
                            ClientCode = request.customer.Client,
                            SessionID = request.SESSIONID,
                            AccountNo = account.AccountNo,
                            AccountName = account.AccountName,
                            DocumentNo = "LNM" + request.SESSIONID,
                            TransactionType = (int)TRANSACTION_TYPE.LOAN_MOBILE,
                            Telephone = request.MSISDN,
                            SaccoCode = request.SACCO_CODE,
                            Posted = false,
                            DatePosted = null,
                            DestinationAccount = "",
                            LoanNo = "",
                            LoanPeriod = int.TryParse(periodStr, out int period) ? period : 1,
                            Status = 0,
                            Comments = "",
                            Amount = Convert.ToDecimal(amount),
                            Charge = 10,
                            Description = TRANSACTION_TYPE.LOAN_MOBILE.ToString(),
                            ApplicationType = "USSD",
                            SentToCBS = false
                        };
                        transdb.MTransactions.Add(mTransaction);
                        transdb.SaveChanges();
                        int entryNo = mTransaction.EntryNo;
                        NextMenu(request);

                        using (var db = new SwizzKash_Mobile_DBEntities())
                        {
                            var thistrans = db.MTransactions.FirstOrDefault(b =>
                                b.EntryNo == entryNo &&
                                b.TransactionType == (int)TRANSACTION_TYPE.LOAN_MOBILE);

                            if (thistrans != null)
                            {
                                LoanApplicationResponse loanApplicationResponse = null;
                                string applicationResponse = null;
                                using (var client = new System.Net.Http.HttpClient())
                                {
                                    var payload = JsonConvert.SerializeObject(new
                                    {
                                        PhoneNumber = request.MSISDN.Replace("+254", "0"),
                                        SaccoCode = request.SACCO_CODE,
                                        Amount = Convert.ToDecimal(amount)
                                    });
                                    var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                                    var httpResponse = client.PostAsync($"{API_BASE_URL}/RequestInstantLoan", content).Result;
                                    applicationResponse = httpResponse.Content.ReadAsStringAsync().Result;
                                }

                                if (!string.IsNullOrEmpty(applicationResponse))
                                    loanApplicationResponse = JsonConvert.DeserializeObject<LoanApplicationResponse>(applicationResponse);

                                if (loanApplicationResponse == null || loanApplicationResponse.StatusCode != "000")
                                {
                                    thistrans.Posted = true;
                                    thistrans.DatePosted = DateTime.Now;
                                    thistrans.Status = (int)STATUS.FAILED;
                                    thistrans.SentToCBS = true;
                                    thistrans.DateSentToCBS = DateTime.Now;
                                    thistrans.Comments = loanApplicationResponse?.StatusDescription ?? "M-LOAN request failed.";
                                    db.SaveChanges();
                                    return "END " + (loanApplicationResponse?.StatusDescription ?? "Your M-LOAN Request could not be processed. Please try again later.");
                                }
                                else
                                {
                                    thistrans.Posted = true;
                                    thistrans.DatePosted = DateTime.Now;
                                    thistrans.Status = (int)STATUS.SUCCESS;
                                    thistrans.SentToCBS = true;
                                    thistrans.DateSentToCBS = DateTime.Now;
                                    thistrans.Comments = loanApplicationResponse.StatusDescription;
                                    db.SaveChanges();
                                    return "END " + loanApplicationResponse.StatusDescription;
                                }
                            }
                            return "END Could not process your request. Kindly try again later or contact Rubani Sacco.";
                        }
                    }
                }
                return "END Invalid option selected";
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return "END An error occurred. Please try again later.";
            }
        }

        // LOAN SALARY ADVANCE HANDLER
        private static string HandleLoanSalaryAdvance(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            if (lastInput == "0")
            {
                request.transactionType = TRANSACTION_TYPE.LOANS;
                return "CON Select Option " + newline +
                       "1. Loan Balances " + newline +
                       "2. Loan Repayment " + newline +
                       "3. M-Loan " + newline +
                       "0. Back";
            }

            int depth = ussd_string.Length;

            try
            {
                LoanLimitResponse loanLimitResponse = GetLoanAmountLimit(request);
                if (loanLimitResponse == null)
                    return "CON Could not get your loan limit" + newline + "0. BACK";
                if (loanLimitResponse.StatusCode != "00")
                    return "CON " + loanLimitResponse.StatusDescription + newline + "0. BACK";

                double qual = loanLimitResponse.LimitAmount;
                if (qual < 500)
                    return "CON You do not qualify for this product" + newline + "0. BACK";

                if (depth == 2)
                {
                    // amount entry stage
                    return "CON " + loanLimitResponse.StatusDescription + newline + "Enter Amount";
                }
                else if (depth == 3)
                {
                    // amount entered - show confirmation
                    if (!double.TryParse(lastInput, out double amount))
                        return "CON Invalid amount, please enter a valid amount" + newline;

                    if (amount > qual)
                        return "CON Request declined, please enter an amount less than or equal to " + qual + ". FINSAFE MOBILE." + newline + "0. BACK";
                    if (amount < 500)
                        return "CON Request declined, please enter an amount more than or equal to Ksh.500. Rubani MOBILE." + newline + "0. BACK";

                    return "CON Apply for Salary advance of KES " + amount + " at 10% interest." + newline +
                           "1. Yes" + newline + "2. No";
                }
                else if (depth == 4)
                {
                    // confirmation
                    if (lastInput == "2") return "CON You have cancelled your E-loan application. Rubani MOBILE" + newline + "0. BACK";
                    if (lastInput != "1") return "CON Invalid option " + newline + "1. Yes" + newline + "2. No";

                    string amountStr = ussd_string[ussd_string.Length - 2];
                    if (!double.TryParse(amountStr, out double amount))
                        return "END Invalid amount";

                    List<Account> accountsList = GetWSSAccounts(request);
                    if (accountsList == null) return "END Account not found, Kindly contact Rubani Sacco";
                    var account = accountsList.ElementAt(0);

                    using (transdb = new SwizzKash_Mobile_DBEntities())
                    {
                        MTransaction mTransaction = new MTransaction
                        {
                            TransactionDate = DateTime.Now,
                            ClientCode = request.customer.Client,
                            SessionID = request.SESSIONID,
                            AccountNo = account.AccountNo,
                            AccountName = account.AccountName,
                            DocumentNo = "LNA" + request.SESSIONID,
                            TransactionType = (int)TRANSACTION_TYPE.LOAN_MOBILE,
                            Telephone = request.MSISDN,
                            SaccoCode = request.SACCO_CODE,
                            Posted = false,
                            DatePosted = null,
                            DestinationAccount = "",
                            LoanNo = "",
                            Status = 0,
                            Comments = "",
                            Amount = Convert.ToDecimal(amount),
                            Charge = 10,
                            Description = TRANSACTION_TYPE.LOAN_MOBILE.ToString(),
                            ApplicationType = "USSD"
                        };

                        transdb.MTransactions.Add(mTransaction);
                        transdb.SaveChanges();
                    }
                    NextMenu(request);
                    return "CON Your Salary Advance loan request has been received and is being processed. Rubani MOBILE" + newline + "0. BACK";
                }
                return "END Invalid option selected";
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return "END An error occurred. Please try again later.";
            }
        }


        // LOAN DIVIDEND ADVANCE HANDLER
        private static string HandleLoanDividendAdvance(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            if (lastInput == "0")
            {
                request.transactionType = TRANSACTION_TYPE.LOANS;
                return "CON Select Option " + newline +
                       "1. Loan Balances " + newline +
                       "2. Loan Repayment " + newline +
                       "3. M-Loan " + newline +
                       "0. Back";
            }

            int depth = ussd_string.Length;

            try
            {
                LoanLimitResponse loanLimitResponse = GetLoanAmountLimit(request);
                if (loanLimitResponse == null)
                    return "CON Could not get your loan limit" + newline + "0. BACK";
                if (loanLimitResponse.StatusCode != "00")
                    return "CON " + loanLimitResponse.StatusDescription + newline + "0. BACK";

                double qual = loanLimitResponse.LimitAmount;
                if (qual < 500)
                    return "CON You do not qualify for this product" + newline + "0. BACK";

                if (depth == 2)
                {
                    return "CON " + loanLimitResponse.StatusDescription + newline + "Enter Amount";
                }
                else if (depth == 3)
                {
                    if (!double.TryParse(lastInput, out double amount))
                        return "CON Invalid amount, please enter a valid amount" + newline;

                    if (amount > qual)
                        return "CON Request declined, please enter an amount less than or equal to " + qual + ". FINSAFE MOBILE." + newline + "0. BACK";
                    if (amount < 500)
                        return "CON Request declined, please enter an amount more than or equal to Ksh.500. Rubani MOBILE." + newline + "0. BACK";

                    return "CON Apply for Special Salary advance of KES " + amount + " at 15% interest." + newline +
                           "1. Yes" + newline + "2. No";
                }
                else if (depth == 4)
                {
                    if (lastInput == "2") return "CON You have cancelled your Special salary advance application. Rubani MOBILE" + newline + "0. BACK";
                    if (lastInput != "1") return "CON Invalid option " + newline + "1. Yes" + newline + "2. No";

                    string amountStr = ussd_string[ussd_string.Length - 2];
                    if (!double.TryParse(amountStr, out double amount)) return "END Invalid amount";

                    List<Account> accountsList = GetWSSAccounts(request);
                    if (accountsList == null) return "END Account not found, Kindly contact Rubani Sacco";
                    var account = accountsList.ElementAt(0);

                    using (transdb = new SwizzKash_Mobile_DBEntities())
                    {
                        MTransaction mTransaction = new MTransaction
                        {
                            TransactionDate = DateTime.Now,
                            ClientCode = request.customer.Client,
                            SessionID = request.SESSIONID,
                            AccountNo = account.AccountNo,
                            AccountName = account.AccountName,
                            DocumentNo = "LNS" + request.SESSIONID,
                            TransactionType = (int)TRANSACTION_TYPE.LOAN_DIVIDENDADVANCE,
                            Telephone = request.MSISDN,
                            SaccoCode = request.SACCO_CODE,
                            Posted = false,
                            DatePosted = null,
                            DestinationAccount = "",
                            LoanNo = "",
                            Status = 0,
                            Comments = "",
                            Amount = Convert.ToDecimal(amount),
                            Charge = 10,
                            Description = TRANSACTION_TYPE.LOAN_DIVIDENDADVANCE.ToString(),
                            ApplicationType = "USSD"
                        };

                        transdb.MTransactions.Add(mTransaction);
                        transdb.SaveChanges();
                    }
                    NextMenu(request);
                    return "CON Your Special salary advance request has been received and is being processed. Rubani MOBILE" + newline + "0. BACK";
                }
                return "END Invalid option selected";
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
                return "END An error occurred. Please try again later.";
            }
        }


        // PIN CHANGE HANDLER
        private static string HandlePinChange(SwizzRequest request, string lastInput, string[] ussd_string)
        {
            if (lastInput == "0")
            {
                request.transactionType = TRANSACTION_TYPE.RUBANIMenu;
                return MainMenu();
            }
            // PIN change logic placeholder - implement as needed
            return "CON Enter Current PIN" + newline;
        }


        // HELPER METHODS
        public static string ChoiceMemberInfo(SwizzRequest request, string choice)
        {
            string reslt = string.Empty;
            switch (choice)
            {
                case "1":
                    {
                        try
                        {
                            using (var client = new System.Net.Http.HttpClient())
                            {
                                var payload = JsonConvert.SerializeObject(new { PhoneNumber = request.MSISDN.Replace("254", "0"), SaccoCode = request.SACCO_CODE });
                                var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                                var httpResponse = client.PostAsync($"{API_BASE_URL}/GetMemberInfo", content).Result;
                                var json = httpResponse.Content.ReadAsStringAsync().Result;
                                _log.Error("API Response: " + json);
                                var apiResponse = JsonConvert.DeserializeObject<MemberNumberResponse>(json);

                                if (apiResponse == null || apiResponse.StatusCode != "000")
                                {
                                    reslt = "CON Could not get your member Information " + newline +
                                            "Kindly try again after 20 minutes or contact Rubani Sacco" +
                                            newline + "0. Back";
                                }
                                else
                                {
                                    string accounts = newline +
                                        "Member No: " + apiResponse.MemberNo +
                                        newline +
                                        "Name: " + apiResponse.MemberName;
                                    reslt = "CON Your Information " + accounts + newline + "0. Back";
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error(ex.Message);
                            reslt = "CON Could not get your member Information " + newline +
                                    "Kindly try again after 20 minutes or contact Rubani Sacco" +
                                    newline + "0. Back";
                        }
                    }
                    break;
            }
            return reslt;
        }

        public static bool Insertwithdrawals(Transaction tran, string statu)
        {
            bool inserted = false;
            SqlConnection connToSMobileDb = null;
            try
            {
                using (connToSMobileDb = new SqlConnection(Properties.Settings.Default.ConnString))
                {
                    string SQL = string.Format(@"INSERT INTO [SwizzKash USSD].[dbo].[MPESA Withdrawals](
                        [Status],[Account No],[Account Name],[Telephone],[Amount],[Text File Created],
                        [Date File Created],[Time File Created],[SMS Sent],[SESSION ID],[Sent to Journal],
                        [Sacco ID],[Account Balance],[Request Confirmed],[Money Received],[Transaction Type],
                        [Sacco Source],[Transaction Date],[Application Type],[Charge]
                        ) VALUES (
                        @status,@accNo,@accName,@tel,@amnt,@textfilecreated,@datefilecreated,@timefilecreated,
                        @smssent,@sessionid,@senttojournal,@saccoid,@accountbal,@requestconfirmed,@mneyreceived,
                        @transatype,@saccosource,@Transactiondate,@apptype,@charge)");

                    SqlCommand cmdRt = new SqlCommand(SQL, connToSMobileDb);
                    cmdRt.Parameters.AddWithValue("@saccoid", tran.saccoId);
                    cmdRt.Parameters.AddWithValue("@accNo", tran.accountNo);
                    cmdRt.Parameters.AddWithValue("@accName", tran.accountName);
                    cmdRt.Parameters.AddWithValue("@status", statu);
                    cmdRt.Parameters.AddWithValue("@tel", tran.Telephone);
                    cmdRt.Parameters.AddWithValue("@amnt", tran.amount);
                    cmdRt.Parameters.AddWithValue("@datefilecreated", tran.Date_file_created);
                    cmdRt.Parameters.AddWithValue("@timefilecreated", tran.Time_file_created);
                    cmdRt.Parameters.AddWithValue("@smssent", tran.SMS_Sent);
                    cmdRt.Parameters.AddWithValue("@sessionid", tran.SESSIN_ID);
                    cmdRt.Parameters.AddWithValue("@senttojournal", tran.Sent_to_journal);
                    cmdRt.Parameters.AddWithValue("@textfilecreated", tran.comments);
                    cmdRt.Parameters.AddWithValue("@accountbal", tran.account_balance);
                    cmdRt.Parameters.AddWithValue("@requestconfirmed", tran.RequestConfirmed);
                    cmdRt.Parameters.AddWithValue("@mneyreceived", 0);
                    cmdRt.Parameters.AddWithValue("@transatype", tran.transType);
                    cmdRt.Parameters.AddWithValue("@saccosource", tran.saccosource);
                    cmdRt.Parameters.AddWithValue("@Transactiondate", DateTime.Today);
                    cmdRt.Parameters.AddWithValue("@apptype", "USSD");
                    cmdRt.Parameters.AddWithValue("@charge", tran.charge);
                    if (connToSMobileDb.State != ConnectionState.Open)
                        connToSMobileDb.Open();
                    int a = cmdRt.ExecuteNonQuery();
                    inserted = a > 0;
                    connToSMobileDb.Close();
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return inserted;
        }

        public static string ChoiceTransfer(SwizzRequest request, string choice)
        {
            string reslt = string.Empty;
            switch (choice)

            {
                case "1":
                    {
                        List<Account> accountsList = GetWSSAccounts(request);
                        if (accountsList is null)
                        {
                            res = "END Accounts not found, Kindly contact Rubani Sacco";
                        }
                        else
                        {
                            string straccounts = string.Empty;
                            int menupos = 0;
                            foreach (var account in accountsList)
                            {
                                menupos += 1;
                                straccounts += $"{menupos}. {account.AccountName}." + newline;
                            }
                            reslt = straccounts == string.Empty
                                ? "END Accounts not found, Kindly contact Rubani Sacco"
                                : "CON Select Source account " + newline + straccounts + newline;
                        }
                    }
                    break;


                case "2":
                    {
                        List<Account> accountsList = GetWSSAccounts(request);
                        if (accountsList is null)
                        {
                            res = "END Accounts not found, Kindly contact Rubani Sacco";
                        }
                        else
                        {
                            string straccounts = string.Empty;
                            int menupos = 0;
                            foreach (var account in accountsList)
                            {
                                menupos += 1;
                                straccounts += $"{menupos}. {account.AccountName}." + newline;
                            }
                            reslt = straccounts == string.Empty
                                ? "END Accounts not found, Kindly contact Rubani Sacco"
                                : "CON Select Destination account " + straccounts + newline;
                        }
                    }
                    break;
            }
            return reslt;
        }


        public static string PaybillAccount(SwizzRequest request, string choice)
        {
            string reslt = string.Empty;
            switch (choice)
            {
                case "3":
                    {
                        request.transactionType = TRANSACTION_TYPE.PAYBILL_STK;
                        List<Account> accountsList = GetWSSAccounts(request);
                        if (accountsList is null) return "END Accounts not found, Kindly contact Rubani Sacco";
                        string straccounts = string.Empty;
                        int menupos = 0;
                        foreach (var account in accountsList)
                        {
                            menupos += 1;
                            straccounts += $"{menupos}. {account.AccountName}." + newline;
                        }
                        if (straccounts == string.Empty) return "END Accounts not found, Kindly contact Rubani Sacco";
                        reslt = "CON Select Source account " + newline + straccounts + newline + "0. Back";
                        break;
                    }
            }
            return reslt;
        }

        public static void NextMenu(SwizzRequest request)
        {
            request.active = true;
            request.Currentoption = request.USSDSTRING;
            request.menucount = request.session.Menu_Count + 1;
        }

        private static bool HasPendingWithdrawRequests(string msisdn)
        {
            try
            {
                var resp = Config.service.HasPendingWithdrawRequests(msisdn.Replace("254", "0"));
                TransExist balanceResponse = JsonConvert.DeserializeObject<TransExist>(resp);
                return balanceResponse.StatusCode != "200";
            }
            catch (Exception e)
            {
                return true;
            }
        }

        public static decimal? GetAccumulatedWithdrawalAmount(string selectedAccount)
        {
            decimal acumulated = 0;
            using (SqlConnection connToSMobileDb = new SqlConnection(Properties.Settings.Default.ConnString))
            {
                string SQLtxt = @"SELECT * FROM [SwizzKash USSD].[dbo].[MPESA Withdrawals] WHERE [Account No]=@accfromm and [Transaction Date]=@transdated and [MPESA Result Code]=0";
                using (SqlCommand cmd = new SqlCommand(SQLtxt, connToSMobileDb))
                {
                    cmd.Parameters.AddWithValue("@accfromm", selectedAccount);
                    cmd.Parameters.AddWithValue("@transdated", DateTime.Today);
                    if (connToSMobileDb.State != System.Data.ConnectionState.Open)
                        connToSMobileDb.Open();
                    using (SqlDataReader sqlReader1 = cmd.ExecuteReader())
                    {
                        if (sqlReader1.HasRows)
                            while (sqlReader1.Read())
                                acumulated += Convert.ToDecimal(sqlReader1[5]);
                    }
                }
            }
            return acumulated;
        }

        public static decimal? GetAmountWithdrawalCharges(decimal withdrawAmount)
        {
            decimal totalwithdrawcharges = 0;
            using (SqlConnection connToSMobileDb = new SqlConnection(Properties.Settings.Default.ConnString))
            {
                string SQLtxt = @"select 20+[chargeAmount] TOTALCHARGES from [b2cdb].[dbo].[mpesaB2Ctariffs] a where (clientCode=@clientcode) AND ((@amount>=[lowerAmount]) and (@amount<=[upperAmount]))";
                using (SqlCommand cmd = new SqlCommand(SQLtxt, connToSMobileDb))
                {
                    cmd.Parameters.AddWithValue("@clientcode", "00084");
                    cmd.Parameters.AddWithValue("@amount", withdrawAmount);
                    if (connToSMobileDb.State != System.Data.ConnectionState.Open)
                        connToSMobileDb.Open();
                    using (SqlDataReader sqlReader1 = cmd.ExecuteReader())
                    {
                        if (sqlReader1.HasRows && sqlReader1.Read())
                            totalwithdrawcharges = Convert.ToDecimal(sqlReader1["TOTALCHARGES"]);
                    }
                }
            }
            return totalwithdrawcharges;
        }
        public static List<Account> GetWSSAccounts(SwizzRequest request)
        {
            List<Account> rst = null;
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    var payload = JsonConvert.SerializeObject(new { PhoneNumber = request.MSISDN.Replace("254", "0"), SaccoCode = request.SACCO_CODE });
                    var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                    var httpResponse = client.PostAsync($"{API_BASE_URL}/GetMemberInfo ", content).Result;
                    var json = httpResponse.Content.ReadAsStringAsync().Result;
                    _log.Error("GetWSSAccounts Response: " + json);

                    var apiResponse = JsonConvert.DeserializeObject<WSAccountsResponse>(json);
                    if (apiResponse != null && apiResponse.StatusCode == "000")
                    {
                        rst = new List<Account>
                {
                    new Account
                    {
                        AccountNo = apiResponse.MemberNumber,
                        AccountName = apiResponse.MemberName
                    }
                };
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
            return rst;
        }

        //public static List<Account> GetWSSAccounts(SwizzRequest request)
        //{
        //    List<Account> rst = null;
        //    try
        //    {
        //        using (var client = new System.Net.Http.HttpClient())
        //        {
        //            var payload = JsonConvert.SerializeObject(new { PhoneNumber = request.MSISDN.Replace("254", "0"), SaccoCode = request.SACCO_CODE });
        //            var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
        //            var httpResponse = client.PostAsync($"{API_BASE_URL}/GetMemberInfo ", content).Result;
        //            var json = httpResponse.Content.ReadAsStringAsync().Result;
        //            _log.Error("GetWSSAccounts Response: " + json);
        //            var apiResponse = JsonConvert.DeserializeObject<WSAccountsResponse>(json);
        //            if (apiResponse != null && apiResponse.StatusCode == "000")
        //                rst = apiResponse.Accounts;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Error(ex.Message);
        //    }
        //    return rst;
        //}

        public static MemberNumberResponse GeMemberNumber(SwizzRequest request)
        {
            MemberNumberResponse rst = null;
            try
            {
                var resp = Config.service.GetMemberNumber(request.MSISDN.Replace("+254", "0"));
                if (resp != null)
                {
                    MemberNumberResponse wSAccountsResponse = JsonConvert.DeserializeObject<MemberNumberResponse>(resp);
                    if (wSAccountsResponse.StatusCode == "000")
                        rst = wSAccountsResponse;
                }
            }
            catch (Exception ex)
            {
                rst = new MemberNumberResponse { StatusCode = "304", StatusDescription = ex.Message, MemberNo = string.Empty, MemberName = string.Empty };
            }
            return rst;
        }

        public static List<LoanBalances> GetOutstandingLoans(SwizzRequest request)
        {
            List<LoanBalances> rst = null;
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    var payload = JsonConvert.SerializeObject(new { PhoneNumber = request.MSISDN.Replace("254", "0"), SaccoCode = request.SACCO_CODE });
                    var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");
                    var httpResponse = client.PostAsync($"{API_BASE_URL}/GetOutstandingLoans", content).Result;
                    var json = httpResponse.Content.ReadAsStringAsync().Result;
                    _log.Error("GetOutstandingLoans Response: " + json);
                    var loanBalanceResponse = JsonConvert.DeserializeObject<LoanBalanceResponse>(json);
                    if (loanBalanceResponse != null && loanBalanceResponse.StatusCode == "000")
                        rst = loanBalanceResponse.LoanBalances;
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
            return rst;
        }

        public static LoanLimitResponse GetLoanAmountLimit(SwizzRequest request)
        {
            LoanLimitResponse rst = null;
            try
            {
                string phoneNumber = request.MSISDN.Replace("+254", "0");
                using (var client = new System.Net.Http.HttpClient())
                {
                    string url = $"{API_BASE_URL}/CheckInstantLoanEligibility?phoneNumber={Uri.EscapeDataString(phoneNumber)}&saccoCode={Uri.EscapeDataString(request.SACCO_CODE)}";
                    var httpResponse = client.GetAsync(url).Result;
                    var resp = httpResponse.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrEmpty(resp))
                        rst = JsonConvert.DeserializeObject<LoanLimitResponse>(resp);
                    SwizzRequest.LogEntryOnFile(request.SESSIONID, $"M-Loan response: {resp}");
                }
            }
            catch (Exception ex) { }
            return rst;
        }

        public static GetBalanceResponse GetWSSAccountBalance(SwizzRequest request)
        {
            GetBalanceResponse rst = null;
            try
            {
                var resp = Config.service.GetWSSAccountBalance(request.MSISDN.Replace("+254", "0"));
                GetBalanceResponse balanceResponse = JsonConvert.DeserializeObject<GetBalanceResponse>(resp);
                if (balanceResponse.StatusCode == "000")
                    rst = balanceResponse;
            }
            catch (Exception ex) { }
            return rst;
        }

        // RESPONSE MODELS 
        public class FundsTransferFOSAResponse
        {
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
        }
        public class TransExist
        {
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
            public string TraceId { get; set; }
        }
        public class Account
        {
            public string AccountName { get; set; }
            public string AccountNo { get; set; }
        }
        public class WSAccountsResponse
        {
            public List<Account> Accounts { get; set; }
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
            public string MemberName { get; set; }
            public string MemberNumber { get; set; }
        }
        public class Member_Info
        {
            public string MemberNo { get; set; }
            public string MemberName { get; set; }
        }
        public class MemberNumberResponse
        {
            [JsonProperty("MemberNumber")]
            public string MemberNo { get; set; }
            public string MemberName { get; set; }
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
        }
        public class MemberInfoAPIResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string MemberName { get; set; }
            public string Phone { get; set; }
            public string StaffNumber { get; set; }
            public string MemberNumber { get; set; }
        }
        public class GetMemberInformationResponse
        {
            public List<Member_Info> MemberInfo { get; set; }
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
        }
        public class GetBalanceResponse
        {
            public decimal AccountBalance { get; set; }
            public string AccountNo { get; set; }
            public string AccountType { get; set; }
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
        }
        public class InsertTransactionResponse
        {
            public string DocumentNo { get; set; }
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
            public string TraceId { get; set; }
        }
        public class LoanApplicationResponse
        {
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
            public string DocumentNo { get; set; }
        }
        public class EnquireBalanceResponse
        {
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
            public string DocumentNo { get; set; }
            public string AccountNo { get; set; }
            
            public decimal AccountBalance { get; set; }
        }
        public class EnquireLoanBalanceResponse
        {
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
            public string DocumentNo { get; set; }
            public string Phone { get; set; }
            public List<LoanBalancesResponse> LoanBalances { get; set; }
        }
        public class LoanBalancesResponse
        {
            public string LoanNo { get; set; }
            public string ProductType { get; set; }
            public string BalanceAmount { get; set; }
        }
        public class LoanBalances
        {
            public decimal BalanceAmount { get; set; }
            public string LoanNo { get; set; }
            public string LoanName { get; set; }
        }
        public class LoanBalanceResponse
        {
            public string DocumentNo { get; set; }
            public List<LoanBalances> LoanBalances { get; set; }
            public string Phone { get; set; }
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
        }
        public class EnquireMinistatementResponse
        {
            public string AccountNo { get; set; }
            public string DocumentNo { get; set; }
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
            public List<TransactionLines> TransactionLine { get; set; }
        }
        public class TransactionLines
        {
            public double Amount { get; set; }
            public string Description { get; set; }
            public string PostingDate { get; set; }
        }
        public class WithdrawRequestResponse
        {
            public int StatusCode { get; set; }
            public string StatusDescription { get; set; }
        }
        public class WithdrawUpdateResponse
        {
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }
            public string TraceId { get; set; }
        }
        public class LoanLimitResponse
        {
            public string StatusCode { get; set; }
            public string StatusDescription { get; set; }

            [JsonProperty("MaxAmount")]
            public double LimitAmount { get; set; }

            public bool IsEligible { get; set; }
            public int MembershipMonths { get; set; }
            public string Reason { get; set; }
            public string InterestRate { get; set; }
            public string CommissionRate { get; set; }
            public string MemberName { get; set; }
            public string MemberNo { get; set; }
        }
        //public class LoanLimitResponse
        //{
        //    public string StatusCode { get; set; }
        //    public string StatusDescription { get; set; }
        //    public double LimitAmount { get; set; }
        //    public string Message { get; set; }
        //}
        public class RubaniUSSD
        {
            public string MemberName { get; set; }
        }
    }
}