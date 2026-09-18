using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwizzMobile
{
    public class Transaction
    {
        public string saccoId = string.Empty;
        public string accountNo = string.Empty;
        public string accountName = string.Empty;
        public string docNo = string.Empty;
        public DateTime? docDate = null;
        public DateTime? transTime = null;
        public int transType = 0;
        public string phone = string.Empty;
        public Posted posted = (Int16)Transaction.Posted.notPosted;
        public DateTime? datePosted = null;
        public string acc2 = string.Empty;
        public string loanNo = string.Empty;
        public Status status = Status.Pending;
        public string comments = string.Empty;
        public double amount = 0.0;
        public double charge = 0.0;
        public int saccosource = 0;
        public string description = string.Empty;
        public DateTime? Date_file_created = null;
        public DateTime? Time_file_created = null;
        public string SMS_Sent = string.Empty;
        public string SESSIN_ID = string.Empty;
        public string Sent_to_journal = string.Empty;
        public double account_balance = 0.0;
        public string Telephone = string.Empty;
        public string SaccoCode = string.Empty;
        public string providerName = string.Empty;
        public string providerId = string.Empty;
        public string recepientPhone = string.Empty;
        public string RequestConfirmed = string.Empty;
        public int Loan_Period = 0;
        public string loanPeriod = string.Empty;
        public string loanType = string.Empty;
        public double amount2 = 0.0;


        public enum Status
        {
            Pending, Completed, Failed, pend, failpend, nnn, pendpay
        }
        public enum Posted
        {
            notPosted, Posted
        }

        public enum Transtype
        {
            ministatement = 0,
            balanceenquiry = 1,
            withdrawal = 2,
            bosatransfer = 3,
            fosatransfer = 4
        }
    }
}