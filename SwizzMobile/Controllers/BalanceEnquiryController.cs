using MobileRestAPI.acumenWR;
using MobileRestAPI.Clients;
using MobileRestAPI.Models;
using MobileRestAPI.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace SwizzMobile.Controllers
{

    [Authorize]
    public class BalanceEnquiryController : ApiController
    {
        private MobileEntities mobileEntities;

        private CLOUDPESA cloud = new CLOUDPESA();
        private HAZINASACCO hazina = new HAZINASACCO();
        public ProxyService acumen;
        private UNITEDWOMENSACCO unitedwomen = new UNITEDWOMENSACCO();
        private FINSAFE finsafe = new FINSAFE();
        private UMOJAWENDANI umojawendani = new UMOJAWENDANI();
        private MAGADI magadi = new MAGADI();
        private VISION vision = new VISION();
        private POLYTECH polytech = new POLYTECH();

        [ResponseType(typeof(void))]
        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            BalanceEnquiryController enquiryController = this;
            string userHostAddress = HttpContext.Current.Request.UserHostAddress;
            string Resultresponse = "";
            SuccessResponse successresponse = null;
            HttpRequestHeaders headers = enquiryController.Request.Headers;
            HttpResponseMessage jsonresponse = enquiryController.Request.CreateResponse();
            GetTokenController genController = new GetTokenController();
            BalanceEnquiryRequest balanceEnquiryRequest = new BalanceEnquiryRequest();
            List<AccountBalance> balances = new List<AccountBalance>();
            string Token = string.Empty;
            string sessionId = string.Empty;
            string phoneNumber = string.Empty;

            if (headers.Contains("token"))
            {
                Token = headers.GetValues("token").First();
                sessionId = Guid.NewGuid().ToString();
                TokenStatus tokenStatus = genController.Checktoken(Token);
                if (!tokenStatus.statusCode)
                {
                    WrongHeadersResponse wrongHeadersResponse = new WrongHeadersResponse()
                    {
                        Description = tokenStatus.Description,
                        statusCode = "401",
                        Success = false
                    };
                    jsonresponse.StatusCode = HttpStatusCode.Unauthorized;
                    Resultresponse = JsonConvert.SerializeObject(wrongHeadersResponse);
                }
                else
                {
                    string requestContent = await enquiryController.Request.Content.ReadAsStringAsync();
                    balanceEnquiryRequest = JsonConvert.DeserializeObject<BalanceEnquiryRequest>(requestContent);
                    if (balanceEnquiryRequest != null)
                    {
                        string clientId = genController.GetClientId(Token);
                        if (clientId != null)
                        {
                            string[] separator1 = new string[1] { "|" };
                            string[] separator2 = new string[1] { ":::" };
                            string[] separator3 = new string[1] { ";" };
                            try
                            {
                                if (balanceEnquiryRequest.mobile_no.StartsWith("0"))
                                {
                                    phoneNumber = "+254" + balanceEnquiryRequest.mobile_no.TrimStart('0');
                                }
                                else if (balanceEnquiryRequest.mobile_no.StartsWith("254"))
                                {
                                    phoneNumber = "+" + balanceEnquiryRequest.mobile_no;
                                }
                                else if (balanceEnquiryRequest.mobile_no.StartsWith("7"))
                                {
                                    phoneNumber = "+254" + balanceEnquiryRequest.mobile_no;
                                }
                                else if (balanceEnquiryRequest.mobile_no.StartsWith("1"))
                                {
                                    phoneNumber = "+254" + balanceEnquiryRequest.mobile_no;
                                }
                                else
                                {
                                    phoneNumber = balanceEnquiryRequest.mobile_no;
                                }

                                using(MobileEntities mobileEntities = new MobileEntities())
                                {
                                    MobileAppLog mobileLogs = new MobileAppLog
                                    {
                                        Token = Token,
                                        Telephone = phoneNumber,
                                        ClientId = clientId,
                                        ClientName = clientId,
                                        SessionId = sessionId,
                                        RequestTimestamp = DateTime.Now,
                                        RequestType = "Balance Enquiry",
                                        RequestMethod = "POST",
                                        RequestContent = requestContent
                                    };
                                    mobileEntities.MobileAppLogs.Add(mobileLogs);
                                    mobileEntities.SaveChanges();

                                    if (clientId == "00001")
                                    {
                                        string[] array1 = hazina.Balance(phoneNumber, sessionId.Substring(1,18)).ToArray();
                                        if (array1.Length != 0)
                                        {
                                            if (array1[0] != "INSUFFICIENT")
                                            {
                                                for (int index = 0; index < array1.Length; ++index)
                                                {
                                                    AccountBalance accountBalance = new AccountBalance();
                                                    string[] strArray = array1[index].Split(separator2, StringSplitOptions.None);
                                                    accountBalance.accName = strArray[0];
                                                    accountBalance.accBal = strArray[1];
                                                    balances.Add(accountBalance);
                                                }
                                                if (balances.Count != 0)
                                                {
                                                    BalanceEnquiryResponse balanceEnquiryResponse = new BalanceEnquiryResponse()
                                                    {
                                                        accountbalance_list = balances.ToArray()
                                                    };
                                                    jsonresponse.StatusCode = HttpStatusCode.OK;
                                                    Resultresponse = JsonConvert.SerializeObject(balanceEnquiryResponse);
                                                }
                                                else
                                                {
                                                    SuccessResponse successResponse = new SuccessResponse()
                                                    {
                                                        status_code = "400",
                                                        Description = "Response is empty"
                                                    };
                                                    jsonresponse.StatusCode = HttpStatusCode.BadRequest;
                                                    Resultresponse = JsonConvert.SerializeObject(successResponse);
                                                }
                                            }
                                            else
                                            {
                                                SuccessResponse successResponse1 = new SuccessResponse()
                                                {
                                                    status_code = "500",
                                                    Description = "Insufficient funds!"
                                                };
                                                jsonresponse.StatusCode = HttpStatusCode.InternalServerError;
                                                Resultresponse = JsonConvert.SerializeObject(successResponse1);
                                            }
                                        }
                                        else
                                        {
                                            SuccessResponse successResponse = new SuccessResponse()
                                            {
                                                status_code = "502",
                                                Description = "An error occurred while fetching your data!"
                                            };
                                            jsonresponse.StatusCode = HttpStatusCode.BadGateway;
                                            Resultresponse = JsonConvert.SerializeObject(successResponse);
                                        }
                                    }
                                    else if (clientId == "00008")
                                    {
                                        string response = polytech.BalanceEnquiry(phoneNumber, balanceEnquiryRequest.accountType);
                                        if (response != null)
                                        {
                                            GetBalanceResponse getBalanceResponse = JsonConvert.DeserializeObject<GetBalanceResponse>(response);
                                            foreach (var balanceList in getBalanceResponse.accountsList)
                                            {
                                                balances.Add(new AccountBalance()
                                                {
                                                    accName = balanceList.accountName,
                                                    accBal = balanceList.balance.ToString()
                                                });
                                            }

                                            if (balances.Count != 0)
                                            {
                                                BalanceEnquiryResponse balanceEnquiryResponse = new BalanceEnquiryResponse()
                                                {
                                                    accountbalance_list = balances.ToArray()
                                                };
                                                jsonresponse.StatusCode = HttpStatusCode.OK;
                                                Resultresponse = JsonConvert.SerializeObject(balanceEnquiryResponse);
                                            }
                                            else
                                            {
                                                successresponse = new SuccessResponse
                                                {
                                                    status_code = "400",
                                                    Description = "Response is empty"
                                                };
                                                jsonresponse.StatusCode = HttpStatusCode.BadRequest;
                                                Resultresponse = JsonConvert.SerializeObject(successresponse);
                                            }
                                        }
                                    }
                                    else if (clientId == "00026")
                                    {
                                        bool inquiry = false;
                                        foreach (string str4 in unitedwomen.BalanceEnquiry(phoneNumber, sessionId, inquiry, DateTime.Today, "").Split(separator3, StringSplitOptions.RemoveEmptyEntries))
                                        {
                                            AccountBalance accountBalance = new AccountBalance();
                                            string[] strArray = str4.Split(separator1, StringSplitOptions.None);
                                            accountBalance.accName = strArray[0];
                                            accountBalance.accBal = strArray[1];
                                            balances.Add(accountBalance);
                                        }
                                        if (balances.Count != 0)
                                        {
                                            BalanceEnquiryResponse balanceEnquiryResponse = new BalanceEnquiryResponse()
                                            {
                                                accountbalance_list = balances.ToArray()
                                            };
                                            jsonresponse.StatusCode = HttpStatusCode.OK;
                                            Resultresponse = JsonConvert.SerializeObject(balanceEnquiryResponse);
                                        }
                                        else
                                        {
                                            successresponse = new SuccessResponse
                                            {
                                                status_code = "400",
                                                Description = "Response is empty"
                                            };
                                            jsonresponse.StatusCode = HttpStatusCode.BadRequest;
                                            Resultresponse = JsonConvert.SerializeObject(successresponse);
                                        }
                                    }
                                    else if (clientId == "00032")
                                    {
                                        foreach (string str5 in umojawendani.Balance(phoneNumber, sessionId.Substring(1, 20)).ToArray())
                                        {
                                            AccountBalance accountBalance = new AccountBalance();
                                            string[] strArray = str5.Split(separator2, StringSplitOptions.None);
                                            accountBalance.accName = strArray[0];
                                            accountBalance.accBal = strArray[1].ToString();
                                            balances.Add(accountBalance);
                                        }
                                        if (balances != null)
                                        {
                                            BalanceEnquiryResponse balanceEnquiryResponse = new BalanceEnquiryResponse()
                                            {
                                                accountbalance_list = balances.ToArray()
                                            };
                                            jsonresponse.StatusCode = HttpStatusCode.OK;
                                            Resultresponse = JsonConvert.SerializeObject(balanceEnquiryResponse);
                                        }
                                        else
                                        {
                                            successresponse = new SuccessResponse
                                            {
                                                status_code = "400",
                                                Description = "Response is empty"
                                            };
                                            jsonresponse.StatusCode = HttpStatusCode.BadRequest;
                                            Resultresponse = JsonConvert.SerializeObject(successresponse);
                                        }
                                    }
                                    else if (clientId == "00043")
                                    {
                                        string[] array2 = enquiryController.magadi.Balance(phoneNumber, sessionId.Substring(1, 20)).ToArray();
                                        if (array2[0] != "INSUFFICIENT")
                                        {
                                            for (int index = 0; index < array2.Length; ++index)
                                            {
                                                AccountBalance accountBalance = new AccountBalance();
                                                string[] strArray = array2[index].Split(separator2, StringSplitOptions.None);
                                                accountBalance.accName = strArray[0];
                                                accountBalance.accBal = strArray[1];
                                                balances.Add(accountBalance);
                                            }
                                            if (balances.Count != 0)
                                            {
                                                BalanceEnquiryResponse balanceEnquiryResponse = new BalanceEnquiryResponse()
                                                {
                                                    accountbalance_list = balances.ToArray()
                                                };
                                                jsonresponse.StatusCode = HttpStatusCode.OK;
                                                Resultresponse = JsonConvert.SerializeObject(balanceEnquiryResponse);
                                            }
                                            else
                                            {
                                                successresponse = new SuccessResponse
                                                {
                                                    status_code = "400",
                                                    Description = "Response is empty"
                                                };
                                                jsonresponse.StatusCode = HttpStatusCode.BadRequest;
                                                Resultresponse = JsonConvert.SerializeObject(successresponse);
                                            }
                                        }
                                        else
                                        {
                                            successresponse = new SuccessResponse
                                            {
                                                status_code = "500",
                                                Description = "Insufficient funds!"
                                            };
                                            jsonresponse.StatusCode = HttpStatusCode.InternalServerError;
                                            Resultresponse = JsonConvert.SerializeObject(successresponse);
                                        }
                                    }
                                    else if (clientId == "00048")
                                    {
                                        foreach (string str6 in enquiryController.cloud.BalanceEnquiry(phoneNumber).Split(separator3, StringSplitOptions.RemoveEmptyEntries))
                                        {
                                            AccountBalance accountBalance = new AccountBalance();
                                            string[] strArray = str6.Split(separator1, StringSplitOptions.None);
                                            accountBalance.accName = strArray[0];
                                            accountBalance.accBal = strArray[1];
                                            balances.Add(accountBalance);
                                        }
                                        if (balances.Count != 0)
                                        {
                                            BalanceEnquiryResponse balanceEnquiryResponse = new BalanceEnquiryResponse()
                                            {
                                                accountbalance_list = balances.ToArray()
                                            };
                                            jsonresponse.StatusCode = HttpStatusCode.OK;
                                            Resultresponse = JsonConvert.SerializeObject(balanceEnquiryResponse);
                                        }
                                        else
                                        {
                                            successresponse = new SuccessResponse
                                            {
                                                status_code = "400",
                                                Description = "Response is empty"
                                            };
                                            jsonresponse.StatusCode = HttpStatusCode.BadRequest;
                                            Resultresponse = JsonConvert.SerializeObject(successresponse);
                                        }
                                    }
                                    else if (clientId == "00057")
                                    {
                                        string response = finsafe.BalanceEnquiry(phoneNumber, balanceEnquiryRequest.accountType);
                                        if (response != null)
                                        {
                                            GetBalanceResponse getBalanceResponse = JsonConvert.DeserializeObject<GetBalanceResponse>(response);
                                            foreach (var balanceList in getBalanceResponse.accountsList)
                                            {
                                                balances.Add(new AccountBalance()
                                                {
                                                    accName = balanceList.accountName,
                                                    accBal = balanceList.balance.ToString()
                                                });
                                            }

                                            if (balances.Count != 0)
                                            {
                                                BalanceEnquiryResponse balanceEnquiryResponse = new BalanceEnquiryResponse()
                                                {
                                                    accountbalance_list = balances.ToArray()
                                                };
                                                jsonresponse.StatusCode = HttpStatusCode.OK;
                                                Resultresponse = JsonConvert.SerializeObject(balanceEnquiryResponse);
                                            }
                                            else
                                            {
                                                successresponse = new SuccessResponse
                                                {
                                                    status_code = "400",
                                                    Description = "Response is empty"
                                                };
                                                jsonresponse.StatusCode = HttpStatusCode.BadRequest;
                                                Resultresponse = JsonConvert.SerializeObject(successresponse);
                                            }
                                        }
                                    }
                                    else if (clientId == "00076")
                                    {
                                        string response = vision.BalanceEnquiry(phoneNumber, balanceEnquiryRequest.accountType);
                                        if (response != null)
                                        {
                                            GetBalanceResponse getBalanceResponse = JsonConvert.DeserializeObject<GetBalanceResponse>(response);
                                            foreach (var balanceList in getBalanceResponse.accountsList)
                                            {
                                                balances.Add(new AccountBalance()
                                                {
                                                    accName = balanceList.accountName,
                                                    accBal = balanceList.balance.ToString()
                                                });
                                            }

                                            if (balances.Count != 0)
                                            {
                                                BalanceEnquiryResponse balanceEnquiryResponse = new BalanceEnquiryResponse()
                                                {
                                                    accountbalance_list = balances.ToArray()
                                                };
                                                jsonresponse.StatusCode = HttpStatusCode.OK;
                                                Resultresponse = JsonConvert.SerializeObject(balanceEnquiryResponse);
                                            }
                                            else
                                            {
                                                successresponse = new SuccessResponse
                                                {
                                                    status_code = "400",
                                                    Description = "Response is empty"
                                                };
                                                jsonresponse.StatusCode = HttpStatusCode.BadRequest;
                                                Resultresponse = JsonConvert.SerializeObject(successresponse);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                SuccessResponse successResponse = new SuccessResponse()
                                {
                                    status_code = "502",
                                    Description = "An error occurred at " + ex.Message
                                };
                                jsonresponse.StatusCode = HttpStatusCode.BadGateway;
                                Resultresponse = JsonConvert.SerializeObject(successResponse);
                            }
                        }
                        else
                        {
                            SuccessResponse successResponse = new SuccessResponse()
                            {
                                status_code = "409",
                                Description = "An error occurred"
                            };
                            jsonresponse.StatusCode = HttpStatusCode.Conflict;
                            Resultresponse = JsonConvert.SerializeObject(successResponse);
                        }
                    }
                    else
                    {
                        SuccessResponse successResponse = new SuccessResponse()
                        {
                            status_code = "400",
                            Description = "Bad Request"
                        };
                        jsonresponse.StatusCode = HttpStatusCode.BadRequest;
                        Resultresponse = JsonConvert.SerializeObject(successResponse);
                    }
                }
            }
            else
            {
                WrongHeadersResponse wrongHeadersResponse = new WrongHeadersResponse()
                {
                    Description = "Request Headers does not exist",
                    statusCode = "400",
                    Success = false
                };
                jsonresponse.StatusCode = HttpStatusCode.BadRequest;
                Resultresponse = JsonConvert.SerializeObject(wrongHeadersResponse);
            }
            jsonresponse.Content = new StringContent(Resultresponse, Encoding.UTF8, "application/json");

            SqlConnection connection;
            using (connection = new SqlConnection(Settings.Default.ConnString))
            {
                SqlCommand sqlCommand = new SqlCommand(string.Format("UPDATE [Mobile].[dbo].[MobileAppLogs] SET [ResponseContent]=@respcontent, [ResponseTimestamp]=@restimestamp WHERE [Token]=@token AND [SessionId]=@session"), connection);
                sqlCommand.Parameters.AddWithValue("@respcontent", Resultresponse);
                sqlCommand.Parameters.AddWithValue("@restimestamp", DateTime.Now);
                sqlCommand.Parameters.AddWithValue("@token", Token);
                sqlCommand.Parameters.AddWithValue("@session", sessionId);
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                sqlCommand.ExecuteNonQuery();
            }

            return jsonresponse;
        }
    }
}
