using log4net;
using Newtonsoft.Json;
using SwizzMobile.Models;
using SwizzMobile.PolytechWebRef;

using SwizzMobile.Properties;

//using SwizzMobile.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Services;
using static SwizzMobile.Clients.BambuiWS;

namespace SwizzMobile
{
    /// <summary>
    /// Summary description for SwizzApp
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class SwizzApp : System.Web.Services.WebService
    {

        public SwizzApp()
        {
        }
        public static MobileService service
        {
            get
            {
                string url, username, password, domain;
                url = "http://polytech-svr:7047/BC240/WS/Test%20Company/Codeunit/MobileService";// Settings.Default.mobileService.ToString();
                username = Settings.Default.username.ToString();
                password = Settings.Default.password.ToString();
                domain = Settings.Default.domain.ToString();

                MobileService service_ = new MobileService();
                service_.Url = url;
                service_.PreAuthenticate = true;
                service_.Credentials = new NetworkCredential(username, password, domain);

                return service_;
            }

        }

        [WebMethod]
        public Responses GetLoanGuarantors(string phoneNumber)
        {
            Responses responses = new Responses();
            try
            {
                responses.ResponseData = service.GetLoanGuarantors(phoneNumber);
                responses.HasResults = true;
            }
            catch (Exception ex)
            {
                responses.ResponseErrors = ex.Message;
                responses.HasResults = false;
                throw;
            }
            return responses;
        }
        [WebMethod]
        public Responses GetLoansGuaranteed(string phoneNumber)
        {
            Responses responses = new Responses();
            try
            {
                responses.ResponseData = service.GetLoansGuaranteed(phoneNumber);
                responses.HasResults = true;
            }
            catch (Exception ex)
            {
                responses.ResponseErrors = ex.Message;
                responses.HasResults = false;
                throw;
            }
            return responses;
        }
        [WebMethod]
        public Responses GetOutstandingLoans(string phoneNumber)
        {
            Responses responses = new Responses();
            try
            {
                responses.ResponseData = service.GetOutstandingLoans(phoneNumber);
                responses.HasResults = true;
            }
            catch (Exception ex)
            {
                responses.ResponseErrors = ex.Message;
                responses.HasResults = false;
                throw;
            }
            return responses;
        }
        [WebMethod]
        public Responses MpolytechEligibility(string phoneNumber)
        {
            Responses responses = new Responses();
            try
            {
                responses.ResponseData = service.AdvanceEligibility(phoneNumber);
                responses.HasResults = true;
            }
            catch (Exception ex)
            {
                responses.ResponseErrors = ex.Message;
                responses.HasResults = false;
                throw;
            }
            return responses;
        }

        [WebMethod]
        public Responses ApplyMPolytech(string phoneNumber, decimal amount, int LoanPeriod)
        {
            Responses responses = new Responses();
            responses.HasResults = false; res = string.Empty;
            try
            {
                responses.HasResults = true;

                List<Account> accountsList = GetWSSAccounts(phoneNumber);
                if (accountsList == null)
                {
                    res = "END Account not found, Kindly contact Polytech Sacco";
                }
                else
                {
                    string straccounts = string.Empty;
                    var account = accountsList.ElementAt(0);

                    using (var transdb = new SwizzKash_Mobile_DBEntities())
                    {
                        MTransaction mTransaction = new MTransaction
                        {
                            TransactionDate = DateTime.Now,
                            ClientCode = phoneNumber,
                            SessionID = phoneNumber + DateTime.Now.ToString("u"),
                            AccountNo = account.AccountNo,
                            AccountName = account.AccountName,
                            DocumentNo = "LNM" + phoneNumber + DateTime.Now.ToString("u"),
                            TransactionType = (int)TRANSACTION_TYPE.LOAN_MOBILE,
                            Telephone = phoneNumber,
                            Posted = false,
                            DatePosted = null,
                            DestinationAccount = "",
                            LoanNo = "",
                            LoanPeriod = Convert.ToInt32(LoanPeriod),
                            Status = 0,
                            Comments = "",
                            Amount = Convert.ToDecimal(amount),
                            Charge = 10,
                            Description = TRANSACTION_TYPE.LOAN_MOBILE.ToString(),
                            ApplicationType = "APP",
                            SentToCBS = false
                        };

                        transdb.MTransactions.Add(mTransaction);
                        transdb.SaveChanges();

                        int entryNo = mTransaction.EntryNo;

                        using (var db = new SwizzKash_Mobile_DBEntities())
                        {
                            var thistrans = db.MTransactions.FirstOrDefault(b =>
                                b.EntryNo == entryNo &&
                                b.TransactionType == (int)TRANSACTION_TYPE.LOAN_MOBILE
                            );

                            if (thistrans != null)
                            {
                                LoanApplicationResponse loanApplicationResponse = null;
                                var applicationResponse = service.ApplyLoan(
                                    thistrans.EntryNo.ToString(),
                                    thistrans.DocumentNo,
                                    phoneNumber.Replace("+254", "0"),
                                    thistrans.AccountNo,
                                    "24",
                                    Convert.ToDecimal(amount),
                                    Convert.ToDecimal(thistrans.LoanPeriod),
                                    DateTime.Now,
                                    "M-LOAN Loan Application"
                                    );

                                if (!string.IsNullOrEmpty(applicationResponse))
                                {
                                    loanApplicationResponse = JsonConvert.DeserializeObject<LoanApplicationResponse>(applicationResponse);
                                }

                                if (loanApplicationResponse == null || loanApplicationResponse.StatusCode != "000")
                                {
                                    thistrans.Posted = true;
                                    thistrans.DatePosted = DateTime.Now;
                                    thistrans.Status = (int)STATUS.FAILED;
                                    thistrans.SentToCBS = true;
                                    thistrans.DateSentToCBS = DateTime.Now;
                                    thistrans.Comments = loanApplicationResponse?.StatusDescription ?? "M-LOAN request failed.";

                                    res = "" + (loanApplicationResponse?.StatusDescription ??
                                           "Your M-LOAN Request could not be processed. Please try again later.");
                                }
                                else
                                {
                                    thistrans.Posted = true;
                                    thistrans.DatePosted = DateTime.Now;
                                    thistrans.Status = (int)STATUS.SUCCESS;
                                    thistrans.SentToCBS = true;
                                    thistrans.DateSentToCBS = DateTime.Now;
                                    thistrans.Comments = loanApplicationResponse.StatusDescription;

                                    res = "" + loanApplicationResponse.StatusDescription;
                                }

                                db.SaveChanges();
                            }
                            else
                            {
                                res = "Could not process your request. Kindly try again later or contact Polytech Sacco.";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                res = "An error occurred while processing your request. Please try again later.";
                responses.HasResults = false;
                responses.ResponseErrors = ex.Message;
            }

            responses.ResponseData = res;
            return responses;
        }

        [WebMethod]
        public Responses CashWithdrawal(string phoneNumber, decimal amount, string walletAccount)
        {
            Responses responses = new Responses();
            try
            {
                //record the withdrawal request here
                using (Db = new SwizzKash_Mobile_DBEntities())
                {
                    try
                    {

                        List<Account> accountsList = GetWSSAccounts(phoneNumber);
                        if (accountsList == null)
                        {
                            res = "Account not found, Kindly contact Polytech Sacco";
                            return BuildErrorResponse(res);
                        }
                        else
                        {
                            var account = accountsList.FirstOrDefault(a =>
                                a.AccountNo.Equals(walletAccount, StringComparison.OrdinalIgnoreCase));

                            if (account == null)
                            {
                                res = "The account provided is invalid. Kindly contact Polytech Sacco";
                                return BuildErrorResponse(res);
                            }

                            DateTime transdate = DateTime.Today;

                            string Status = "Failed";
                            String Text_File_Created = string.Empty;

                            decimal? totalWithdrawCharges = 0;
                            decimal? acumulated = 0;
                            decimal? finalWithLimit = 0;

                            bool hasPendingWithdrawRequests = HasPendingWithdrawRequests(phoneNumber);
                            if (hasPendingWithdrawRequests)
                            {
                                res = "Request could not be completed, you have a pending cash withdrawal request.";
                                return BuildErrorResponse(res);
                            }

                            GetBalanceResponse balanceResponse = GetWSSAccountBalance(phoneNumber);
                            decimal accBalance = balanceResponse.AccountBalance;
                            acumulated = GetAccumulatedWithdrawalAmount(walletAccount);
                            totalWithdrawCharges = GetAmountWithdrawalCharges(amount); // -- get total withdraw charges
                            finalWithLimit = 50000 - acumulated;

                            if (amount > 50000)
                            {
                                Status = "Failed";
                                Text_File_Created = "Max 50000 MPESA Limit Per Trans ";
                                res = "Your Cash withdrawal Request has been declined.The maximum amount allowed per transaction is KES 20,000.";
                                return BuildErrorResponse(res);
                            }
                            else if (finalWithLimit - amount < 0)
                            {
                                Status = "Failed";
                                Text_File_Created = "Reached Daily Limit 50000";
                                res = "Request could not be completed, you have a reached your daily withdrawal limit,kindly try again tomorrow.";
                                return BuildErrorResponse(res);
                            }
                            else if (amount > finalWithLimit)
                            {
                                Status = "Failed";
                                Text_File_Created = "Maximum withdrawal Limit 50000";
                                res = "Request could not be completed, you can only withdraw up to a maximum of KSH " + finalWithLimit + " Per day.";
                                return BuildErrorResponse(res);
                            }
                            else if ((amount + totalWithdrawCharges) > accBalance)
                            {
                                Status = "Failed";
                                Text_File_Created = "insufficient funds";
                                res = "Your account balance is insufficient for withdrawal KES " + amount + " + " + ((decimal)totalWithdrawCharges).ToString("#0.00") + " charges.";
                                return BuildErrorResponse(res);
                            }
                            else
                            {
                                Status = "Pending";
                                Text_File_Created = "NULL";
                                res = "Your Cash withdrawal Request has been received and is being proccesed.";
                            }
                            using (var transdb = new SwizzKash_Mobile_DBEntities())
                            {
                                MTransaction mTransaction = new MTransaction
                                {
                                    TransactionDate = DateTime.Now,
                                    ClientCode = phoneNumber,
                                    SessionID = phoneNumber + DateTime.Now.ToString("u"),
                                    AccountNo = account.AccountNo,
                                    AccountName = account.AccountName,
                                    DocumentNo = "SB" + phoneNumber + DateTime.Now.ToString("u"),
                                    TransactionType = (int)TRANSACTION_TYPE.CASHWITHDRAW,
                                    Telephone = phoneNumber,
                                    Posted = false,
                                    DatePosted = null,
                                    DestinationAccount = "",
                                    LoanNo = "",
                                    Status = 0,
                                    Comments = "",
                                    Amount = amount,
                                    Charge = 10,
                                    Description = TRANSACTION_TYPE.CASHWITHDRAW.ToString(),
                                    ApplicationType = "USSD",
                                    SentToCBS = false
                                };

                                transdb.MTransactions.Add(mTransaction);
                                transdb.SaveChanges();
                            }
                            Transaction tr = new Transaction
                            {
                                saccoId = phoneNumber,
                                saccosource = 3012451,
                                accountNo = account.AccountNo,
                                accountName = account.AccountName,
                                Telephone = phoneNumber,
                                amount = (double)amount,
                                Time_file_created = DateTime.Now,
                                SMS_Sent = "No",
                                SESSIN_ID = "WD" + phoneNumber + DateTime.Now.ToString("u"),
                                Sent_to_journal = "No",
                                account_balance = (double)accBalance,
                                Date_file_created = DateTime.Today,
                                transTime = DateTime.Now,
                                transType = (int)TRANSACTION_TYPE.CASHWITHDRAW,
                                phone = phoneNumber,
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
                            bool withd = Insertwithdrawals(tr, Status);

                        }

                    }
                    catch (Exception ex)
                    {
                        responses.ResponseErrors = ex.Message;
                        responses.HasResults = false;
                        ex.Data.Clear();
                    }
                }

                responses.ResponseData = res;
                responses.HasResults = true;
            }
            catch (Exception ex)
            {
                responses.ResponseErrors = ex.Message;
                responses.HasResults = false;
            }
            return responses;
        }

        [WebMethod]
        public Responses CashDeposit(string phoneNumber, decimal amount, string destinationAccount, string transType)
        {
            var responses = new Responses();
            string res = string.Empty;

            try
            {
                decimal theAmount = amount;
                decimal theCharge = 0;
                string strDestinationAccount = string.Empty;
                string strDestinationAccountName = string.Empty;
                string strTypeAccronym = string.Empty;

                // --- Determine transaction type ---
                switch (transType)
                {
                    // ----------------------- 1. STK → FOSA -----------------------
                    case "1":
                        {
                            var accountsList = GetWSSAccounts(phoneNumber);

                            if (accountsList == null || !accountsList.Any())
                            {
                                res = "Accounts not found, Kindly contact Polytech Sacco";
                                return BuildErrorResponse(res);
                            }

                            var sourceAccount = accountsList.FirstOrDefault(a =>
                                a.AccountNo.Equals(destinationAccount, StringComparison.OrdinalIgnoreCase));

                            if (sourceAccount == null)
                            {
                                res = "The account provided is invalid. Kindly contact Polytech Sacco";
                                return BuildErrorResponse(res);
                            }

                            strDestinationAccount = sourceAccount.AccountNo;
                            strDestinationAccountName = sourceAccount.AccountName;
                            strTypeAccronym = "DF";
                            break;
                        }

                    // ----------------------- 2. STK → BOSA -----------------------
                    case "2":
                        {
                            string destinationAccountBosa = string.Empty;
                            switch (destinationAccount)
                            {
                                case "1":
                                    destinationAccountBosa = "DEPOSIT CONTRIBUTION";
                                    break;
                                case "2":
                                    destinationAccountBosa = "SHARE CAPITAL";
                                    break;
                                case "3":
                                    destinationAccountBosa = "DEPOSIT CONTRIBUTION";
                                    break;
                                case "4":
                                    destinationAccountBosa = "M-Wallet SHARES";
                                    break;
                                default:
                                    destinationAccountBosa = string.Empty;
                                    break;
                            }

                            var memberDetails = GeMemberNumber(phoneNumber);
                            if (memberDetails.StatusCode == "000")
                            {
                                strDestinationAccount = destinationAccountBosa.Substring(0, 3) + memberDetails.MemberNo;
                                strDestinationAccountName = memberDetails.MemberName;
                            }
                            else
                            {
                                strDestinationAccount = destinationAccountBosa;
                                strDestinationAccountName = destinationAccount;
                            }

                            strTypeAccronym = "DB";
                            theCharge = 0;
                            break;
                        }

                    // ----------------------- 3. STK → LOAN -----------------------
                    case "3":
                        {
                            var loansList = GetOutstandingLoansList(phoneNumber);
                            if (loansList == null || !loansList.Any())
                            {
                                res = "Loans not found, Kindly contact Polytech Sacco";
                                return BuildErrorResponse(res);
                            }

                            var destinationLoanAccount = loansList.FirstOrDefault(a =>
                                a.LoanNo.Equals(destinationAccount, StringComparison.OrdinalIgnoreCase));

                            if (destinationLoanAccount == null)
                            {
                                res = "The loan account provided is invalid. Kindly contact Polytech Sacco";
                                return BuildErrorResponse(res);
                            }

                            strDestinationAccount = destinationLoanAccount.LoanNo;
                            strDestinationAccountName = destinationLoanAccount.LoanName;
                            strTypeAccronym = "PL";
                            theCharge = 10;
                            break;
                        }

                    // ----------------------- Invalid Option -----------------------
                    default:
                        res = "Invalid transaction type selected";
                        return BuildErrorResponse(res);
                }

                // ----------------------- Save Transaction -----------------------
                using (var dbC2b = new SwizzKash_Mobile_DBEntities())
                {
                    try
                    {
                        var tran = new STKPushTransaction
                        {
                            SaccoID = "100",
                            Telephone = phoneNumber,
                            AccountNo = phoneNumber.Replace("+", ""),
                            AccountName = strDestinationAccountName,
                            DocumentNo = $"{strTypeAccronym}{phoneNumber}{DateTime.Now:yyyyMMddHHmmss}",
                            DocumentDate = DateTime.Now,
                            Transaction_Type = (int)TRANSACTION_TYPE.PAYBILL_STK,
                            Posted = false,
                            Date_Posted = null,
                            Account_2 = strDestinationAccount,
                            Status = 0,
                            Comments = "",
                            Amount = theAmount,
                            Charge = theCharge,
                            Application_Type = "APP",
                            Description = TRANSACTION_TYPE.PAYBILL_STK.ToString()
                        };

                        dbC2b.STKPushTransactions.Add(tran);
                        dbC2b.SaveChanges();

                        res = "Please wait for Safaricom to complete your request. POLYTECH MOBILE";
                    }
                    catch (Exception)
                    {
                        res = "A problem has been encountered. Please retry after 10 minutes or contact POLYTECH SACCO.";
                    }
                }

                // ----------------------- Build Success Response -----------------------
                responses.ResponseData = res;
                responses.HasResults = true;
            }
            catch (Exception ex)
            {
                responses.ResponseErrors = ex.Message;
                responses.HasResults = false;
            }

            return responses;
        }


        [WebMethod]
        public Responses ProfileInformation(string phoneNumber)
        {
            Responses responses = new Responses();
            try
            {
                responses.ResponseData = service.GetMemberInformationAPP(phoneNumber.Replace("+254", "0"));
                responses.HasResults = true;
            }
            catch (Exception ex)
            {
                responses.ResponseErrors = ex.Message;
                responses.HasResults = false;
            }
            return responses;
        }
        [WebMethod]
        public Responses Ministatements(string phoneNumber)
        {
            Responses responses = new Responses();
            try
            {
                responses.ResponseData = service.GetMinistatementApp(phoneNumber.Replace("+254", "0"));
                responses.HasResults = true;
            }
            catch (Exception ex)
            {
                responses.ResponseErrors = ex.Message;
                responses.HasResults = false;
            }
            return responses;
        }
        [WebMethod]
        public Responses GetWalletAccounts(string phoneNumber)
        {
            Responses responses = new Responses();
            try
            {
                responses.ResponseData = service.WSSAccount(phoneNumber.Replace("+254", "0"));
                responses.HasResults = true;
            }
            catch (Exception ex)
            {
                responses.ResponseErrors = ex.Message;
                responses.HasResults = false;
            }
            return responses;
        }
        [WebMethod]
        public Responses GetMemberAccounts(string phoneNumber)
        {
            Responses responses = new Responses();
            try
            {
                responses.ResponseData = service.GetMemberAccounts(phoneNumber.Replace("+254", "0"));
                responses.HasResults = true;
            }
            catch (Exception ex)
            {
                responses.ResponseErrors = ex.Message;
                responses.HasResults = false;
            }
            return responses;
        }

        #region HelperMethods
        private static bool HasPendingWithdrawRequests(string msisdn)
        {
            try
            {
                var resp = service.HasPendingWithdrawRequests(msisdn.Replace("+254", "0"));
                TransExist balanceResponse = JsonConvert.DeserializeObject<TransExist>(resp);
                if (balanceResponse.StatusCode == "200")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                return true; // -- defensive
            }
        }
        public static GetBalanceResponse GetWSSAccountBalance(string phoneNumber)
        {
            GetBalanceResponse rst = null;

            try
            {
                var resp = service.GetWSSAccountBalance(phoneNumber.Replace("+254", "0"));
                GetBalanceResponse balanceResponse = JsonConvert.DeserializeObject<GetBalanceResponse>(resp);
                if (balanceResponse.StatusCode == "000")
                {
                    rst = balanceResponse;
                }
            }
            catch (Exception ex)
            {

            }
            return rst;
        }
        public static List<Account> GetWSSAccounts(string phoneNumber)
        {
            List<Account> rst = null;
            try
            {
                var resp = service.WSSAccount(phoneNumber.Replace("+254", "0"));
                if (resp != null)
                {
                    WSAccountsResponse wSAccountsResponse = JsonConvert.DeserializeObject<WSAccountsResponse>(resp);
                    if (wSAccountsResponse.StatusCode == "000")
                    {
                        rst = wSAccountsResponse.Accounts;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return rst;
        }
        public static MemberNumberResponse GeMemberNumber(string phoneNumber)
        {
            MemberNumberResponse rst = null;
            try
            {
                var resp = service.GetMemberNumber(phoneNumber.Replace("+254", "0"));
                if (resp != null)
                {
                    MemberNumberResponse wSAccountsResponse = JsonConvert.DeserializeObject<MemberNumberResponse>(resp);
                    if (wSAccountsResponse.StatusCode == "000")
                    {
                        rst = wSAccountsResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                rst = new MemberNumberResponse { StatusCode = "304", StatusDescription = ex.Message, MemberNo = string.Empty, MemberName = string.Empty };
            }
            return rst;
        }
        public static List<LoanBalances> GetOutstandingLoansList(string phoneNumber)
        {
            List<LoanBalances> rst = null;
            try
            {
                var resp = service.GetOutstandingLoans(phoneNumber.Replace("+254", "0"));
                if (resp != null)
                {
                    LoanBalanceResponse loanBalanceResponse = JsonConvert.DeserializeObject<LoanBalanceResponse>(resp);
                    if (loanBalanceResponse.StatusCode == "000")
                    {
                        rst = loanBalanceResponse.LoanBalances;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return rst;
        }
        private Responses BuildErrorResponse(string message)
        {
            return new Responses
            {
                ResponseData = message,
                HasResults = false
            };
        }
        #endregion
    }
}
