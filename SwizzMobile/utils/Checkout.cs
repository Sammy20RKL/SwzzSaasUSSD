using System;
using System.Text;
using System.Security.Cryptography;
using System.ServiceModel;

namespace SwizzMobile.Utils
{
    public class Checkout
    {
        private string PASSWORD { get; set; }
        private string TIMESTAMP
        {
            get
            {
                return "20160510161908";
            }
        }
        MpesaService.LNMO_portTypeClient client;
        private string RealTimestamp = string.Empty;
        double amount;
        string phone;

        string MERCHANT_ID = string.Empty;
        string REFERENCE_ID = string.Empty;
        string PASSKEY = string.Empty;
        MpesaService.CheckOutHeader header = null;
        MpesaService.processCheckOutRequest request = null;
        MpesaService.processCheckOutResponse checkoutresponse = null;
        string TRX_ID = string.Empty;
        string DESCRIPRION = string.Empty;
        string MERCHANT_TRANSACTION_ID = string.Empty;
        public Checkout(int amount, string phone, string referenceId, string MERCHANT_ID, string PASSKEY)
        {
            this.amount = amount;
            this.phone = phone;
            this.PASSKEY = PASSKEY;
            this.MERCHANT_ID = MERCHANT_ID;
            REFERENCE_ID = referenceId;
            MERCHANT_TRANSACTION_ID = new Random().Next(0, 100000).ToString();
        }
        public string pay()
        {
            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
                checkoutresponse = getClient(getCheckOutHeader(), getCheckOutRequest()).processCheckOut(header, request);
                if (checkoutresponse.RETURN_CODE == "00")
                {
                    return confirm(checkoutresponse);
                }
                return checkoutresponse.RETURN_CODE;
            }
            catch (EndpointNotFoundException ex)
            {
                string message = ex.Message;
                return null;
            }
        }
        public static string sha256_hash(string value)
        {
            StringBuilder Sb = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }
        private string generatePassword()
        {
            HashAlgorithm algorithm = new SHA256CryptoServiceProvider();
            RealTimestamp = generateTimestamp();
            string toHash = MERCHANT_ID + PASSKEY + RealTimestamp;
            PASSWORD = Convert.ToBase64String(Encoding.UTF8.GetBytes(sha256_hash(toHash)));
            return PASSWORD.Replace("+", "");

        }
        private string generateTimestamp()
        {
            DateTime dt = DateTime.Now;
            string formattedDate = dt.ToString("yyyyMMddHHmmss");
            return formattedDate;

        }
        private MpesaService.CheckOutHeader getCheckOutHeader()
        {
            header = new MpesaService.CheckOutHeader();
            header.PASSWORD = generatePassword();
            header.TIMESTAMP = RealTimestamp;
            header.MERCHANT_ID = MERCHANT_ID;
            return header;
        }
        private MpesaService.processCheckOutRequest getCheckOutRequest()
        {
            request = new MpesaService.processCheckOutRequest();
            request.AMOUNT = amount;
            request.CALL_BACK_METHOD = "POST";
            request.CALL_BACK_URL = "http://62.12.113.67:8063/lnmo_checkout_Service.asmx";
            request.MERCHANT_TRANSACTION_ID = this.MERCHANT_TRANSACTION_ID;
            request.MSISDN = this.phone;
            request.REFERENCE_ID = REFERENCE_ID;
            request.TIMESTAMP = TIMESTAMP;
            return request;
        }
        private MpesaService.LNMO_portTypeClient getClient(MpesaService.CheckOutHeader coHeader, MpesaService.processCheckOutRequest coRequest)
        {
            // BasicHttpBinding binding = new BasicHttpBinding();
            BasicHttpsBinding binding = new BasicHttpsBinding();
            binding.SendTimeout = new TimeSpan(0, 1, 30);
            binding.MaxReceivedMessageSize = 20000000;
            EndpointAddress address = new EndpointAddress("https://safaricom.co.ke/mpesa_online/lnmo_checkout_server.php?wsdl");
            client = new MpesaService.LNMO_portTypeClient(binding, address);
            return client;
        }
        private string confirm(MpesaService.processCheckOutResponse res)
        {
            string TRX_ID = res.TRX_ID;
            string DESCRIPRION = res.DESCRIPTION;
            string MERCHANT_T_ID = MERCHANT_TRANSACTION_ID;
            string status = client.confirmTransaction(header, ref TRX_ID, ref MERCHANT_TRANSACTION_ID, out DESCRIPRION);
            return status;
        }
        public string checkForErrors(string code)
        {
            string message = String.Empty;
            switch (code)
            {
                case "00":
                    message = "Success: Request completed";
                    break;
                case "01":
                    message = "Failure: Authentication failed";
                    break;
                case "03":
                    message = "Amount less than the minimum single transfer allowed on the system";
                    break;
                case "04":
                    message = "Amount more than the maximum single transfer amount allowed";
                    break;
                case "05":
                    message = "Transaction expired before it was processed, please try again";
                    break;
                case "06":
                    message = " Transaction could not be confirmed possibly due to confirm operation failure";
                    break;
                case "08":
                    message = "Paybill provided does not exist";
                    break;
                case "09":
                    message = "The customer has reached transaction limit for the day";
                    break;
                case "10":
                    message = "Your customer is not a registered customer of MPESA";
                    break;
                case "11":
                    message = "Sorry the system could't process your request, try again later";
                    break;
                case "12":
                    message = "Corrupt transaction details, details differ from ones provided in initial request";
                    break;
                case "29":
                    message = "System is down! Sorry for the inconvinience";
                    break;
                case "30":
                    message = "Reference ID is missing";
                    break;
                case "31":
                    message = "Invalid amount to be paid";
                    break;
                case "32":
                    message = "Your customer's MPESA account has not been activated";
                    break;
                case "33":
                    message = "Your customer's is not approved to transact";

                    break;
                case "34":
                    message = "Your request took longer than expected";
                    break;
                case "35":
                    message = "Duplicate transaction detected";
                    break;
                default:
                    message = "Unknown error, try again later";
                    break;
            }
            return message;
        }
    }
}