using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwizzMobile.models
{
    public class PaybillRequest
    {
        public string TraceId { get; set; }
        public string DocumentNo { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public string PhoneNumber { get; set; }
        public decimal AmountPaidIn { get; set; }
        public decimal SaccoPaybillBal { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string Narration { get; set; }
    }
}
