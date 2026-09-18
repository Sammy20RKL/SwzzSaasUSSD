using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwizzMobile.Models
{
    public partial class Customer
    {
        public string Telephone { get; set; }
        public bool Active { get; set; }
        public string Client { get; set; }
        public string Comments { get; set; }
        public Nullable<decimal> Daily_Limit { get; set; }
        public System.DateTime Date_Registered { get; set; }
        public string Language { get; set; }
        public string Name { get; set; }
        public bool PinChanged { get; set; }
        public string SaccoCode { get; set; }
        public short Status { get; set; }
        public Nullable<short> Subscribed { get; set; }
        public decimal Transaction_Limit { get; set; }
        public string Verification_Code { get; set; }
        public string CustomerIPAddress { get; set; }
        public string DateverificationRequested { get; set; }
    }

}