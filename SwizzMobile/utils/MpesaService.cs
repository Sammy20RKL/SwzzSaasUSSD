using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwizzMobile.Utils
{
    public class MpesaService
    {
        [System.ServiceModel.ServiceContractAttribute(Namespace = "tns:ns", ConfigurationName = "MpesaService.LNMO_portType")]
        public interface LNMO_portType
        {

            // CODEGEN: Generating message contract since the operation processCheckOut is neither RPC nor document wrapped.
            [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
            [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
            processCheckOutResponse1 processCheckOut(processCheckOutRequest1 request);

            [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
            System.Threading.Tasks.Task<MpesaService.processCheckOutResponse1> processCheckOutAsync(MpesaService.processCheckOutRequest1 request);

            // CODEGEN: Generating message contract since the operation transactionStatusQuery is neither RPC nor document wrapped.
            [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
            [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
            transactionStatusQueryResponse transactionStatusQuery(MpesaService.transactionStatusQueryRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
            System.Threading.Tasks.Task<MpesaService.transactionStatusQueryResponse> transactionStatusQueryAsync(MpesaService.transactionStatusQueryRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
            [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
            MpesaService.confirmTransactionResponse confirmTransaction(MpesaService.confirmTransactionRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
            System.Threading.Tasks.Task<MpesaService.confirmTransactionResponse> confirmTransactionAsync(MpesaService.confirmTransactionRequest request);
            [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
            [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
            MpesaService.LNMOResultResponse LNMOResult(MpesaService.LNMOResultRequest request);

            [System.ServiceModel.OperationContractAttribute(Action = "", ReplyAction = "*")]
            System.Threading.Tasks.Task<MpesaService.LNMOResultResponse> LNMOResultAsync(MpesaService.LNMOResultRequest request);
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1586.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "tns:ns")]
        public partial class CheckOutHeader : object, System.ComponentModel.INotifyPropertyChanged
        {

            private string mERCHANT_IDField;

            private string pASSWORDField;

            private string tIMESTAMPField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
            public string MERCHANT_ID
            {
                get
                {
                    return this.mERCHANT_IDField;
                }
                set
                {
                    this.mERCHANT_IDField = value;
                    this.RaisePropertyChanged("MERCHANT_ID");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
            public string PASSWORD
            {
                get
                {
                    return this.pASSWORDField;
                }
                set
                {
                    this.pASSWORDField = value;
                    this.RaisePropertyChanged("PASSWORD");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 2)]
            public string TIMESTAMP
            {
                get
                {
                    return this.tIMESTAMPField;
                }
                set
                {
                    this.tIMESTAMPField = value;
                    this.RaisePropertyChanged("TIMESTAMP");
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged(string propertyName)
            {
                System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
                if ((propertyChanged != null))
                {
                    propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1586.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "tns:ns")]
        public partial class processCheckOutRequest : object, System.ComponentModel.INotifyPropertyChanged
        {

            private string mERCHANT_TRANSACTION_IDField;

            private string rEFERENCE_IDField;

            private double aMOUNTField;

            private string mSISDNField;

            private string eNC_PARAMSField;

            private string cALL_BACK_URLField;

            private string cALL_BACK_METHODField;

            private string tIMESTAMPField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
            public string MERCHANT_TRANSACTION_ID
            {
                get
                {
                    return this.mERCHANT_TRANSACTION_IDField;
                }
                set
                {
                    this.mERCHANT_TRANSACTION_IDField = value;
                    this.RaisePropertyChanged("MERCHANT_TRANSACTION_ID");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
            public string REFERENCE_ID
            {
                get
                {
                    return this.rEFERENCE_IDField;
                }
                set
                {
                    this.rEFERENCE_IDField = value;
                    this.RaisePropertyChanged("REFERENCE_ID");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 2)]
            public double AMOUNT
            {
                get
                {
                    return this.aMOUNTField;
                }
                set
                {
                    this.aMOUNTField = value;
                    this.RaisePropertyChanged("AMOUNT");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 3)]
            public string MSISDN
            {
                get
                {
                    return this.mSISDNField;
                }
                set
                {
                    this.mSISDNField = value;
                    this.RaisePropertyChanged("MSISDN");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 4)]
            public string ENC_PARAMS
            {
                get
                {
                    return this.eNC_PARAMSField;
                }
                set
                {
                    this.eNC_PARAMSField = value;
                    this.RaisePropertyChanged("ENC_PARAMS");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 5)]
            public string CALL_BACK_URL
            {
                get
                {
                    return this.cALL_BACK_URLField;
                }
                set
                {
                    this.cALL_BACK_URLField = value;
                    this.RaisePropertyChanged("CALL_BACK_URL");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 6)]
            public string CALL_BACK_METHOD
            {
                get
                {
                    return this.cALL_BACK_METHODField;
                }
                set
                {
                    this.cALL_BACK_METHODField = value;
                    this.RaisePropertyChanged("CALL_BACK_METHOD");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 7)]
            public string TIMESTAMP
            {
                get
                {
                    return this.tIMESTAMPField;
                }
                set
                {
                    this.tIMESTAMPField = value;
                    this.RaisePropertyChanged("TIMESTAMP");
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged(string propertyName)
            {
                System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
                if ((propertyChanged != null))
                {
                    propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1586.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "tns:ns")]
        public partial class processCheckOutResponse : object, System.ComponentModel.INotifyPropertyChanged
        {

            private string rETURN_CODEField;

            private string dESCRIPTIONField;

            private string tRX_IDField;

            private string eNC_PARAMSField;

            private string cUST_MSGField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
            public string RETURN_CODE
            {
                get
                {
                    return this.rETURN_CODEField;
                }
                set
                {
                    this.rETURN_CODEField = value;
                    this.RaisePropertyChanged("RETURN_CODE");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
            public string DESCRIPTION
            {
                get
                {
                    return this.dESCRIPTIONField;
                }
                set
                {
                    this.dESCRIPTIONField = value;
                    this.RaisePropertyChanged("DESCRIPTION");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 2)]
            public string TRX_ID
            {
                get
                {
                    return this.tRX_IDField;
                }
                set
                {
                    this.tRX_IDField = value;
                    this.RaisePropertyChanged("TRX_ID");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 3)]
            public string ENC_PARAMS
            {
                get
                {
                    return this.eNC_PARAMSField;
                }
                set
                {
                    this.eNC_PARAMSField = value;
                    this.RaisePropertyChanged("ENC_PARAMS");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 4)]
            public string CUST_MSG
            {
                get
                {
                    return this.cUST_MSGField;
                }
                set
                {
                    this.cUST_MSGField = value;
                    this.RaisePropertyChanged("CUST_MSG");
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged(string propertyName)
            {
                System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
                if ((propertyChanged != null))
                {
                    propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                }
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class processCheckOutRequest1
        {

            [System.ServiceModel.MessageHeaderAttribute(Namespace = "tns:ns")]
            public MpesaService.CheckOutHeader CheckOutHeader;

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 0)]
            public MpesaService.processCheckOutRequest processCheckOutRequest;

            public processCheckOutRequest1()
            {
            }

            public processCheckOutRequest1(MpesaService.CheckOutHeader CheckOutHeader, MpesaService.processCheckOutRequest processCheckOutRequest)
            {
                this.CheckOutHeader = CheckOutHeader;
                this.processCheckOutRequest = processCheckOutRequest;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class processCheckOutResponse1
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 0)]
            public MpesaService.processCheckOutResponse processCheckOutResponse;

            public processCheckOutResponse1()
            {
            }

            public processCheckOutResponse1(MpesaService.processCheckOutResponse processCheckOutResponse)
            {
                this.processCheckOutResponse = processCheckOutResponse;
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1586.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "tns:ns")]
        public partial class transactionStatusRequest : object, System.ComponentModel.INotifyPropertyChanged
        {

            private string tRX_IDField;

            private string mERCHANT_TRANSACTION_IDField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
            public string TRX_ID
            {
                get
                {
                    return this.tRX_IDField;
                }
                set
                {
                    this.tRX_IDField = value;
                    this.RaisePropertyChanged("TRX_ID");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
            public string MERCHANT_TRANSACTION_ID
            {
                get
                {
                    return this.mERCHANT_TRANSACTION_IDField;
                }
                set
                {
                    this.mERCHANT_TRANSACTION_IDField = value;
                    this.RaisePropertyChanged("MERCHANT_TRANSACTION_ID");
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged(string propertyName)
            {
                System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
                if ((propertyChanged != null))
                {
                    propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                }
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1586.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "tns:ns")]
        public partial class transactionStatusResponse : object, System.ComponentModel.INotifyPropertyChanged
        {

            private string mSISDNField;

            private string aMOUNTField;

            private string mPESA_TRX_DATEField;

            private string mPESA_TRX_IDField;

            private string tRX_STATUSField;

            private string rETURN_CODEField;

            private string dESCRIPTIONField;

            private string mERCHANT_TRANSACTION_IDField;

            private string eNC_PARAMSField;

            private string tRX_IDField;

            private string cALLBACK_STATUSField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
            public string MSISDN
            {
                get
                {
                    return this.mSISDNField;
                }
                set
                {
                    this.mSISDNField = value;
                    this.RaisePropertyChanged("MSISDN");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
            public string AMOUNT
            {
                get
                {
                    return this.aMOUNTField;
                }
                set
                {
                    this.aMOUNTField = value;
                    this.RaisePropertyChanged("AMOUNT");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 2)]
            public string MPESA_TRX_DATE
            {
                get
                {
                    return this.mPESA_TRX_DATEField;
                }
                set
                {
                    this.mPESA_TRX_DATEField = value;
                    this.RaisePropertyChanged("MPESA_TRX_DATE");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 3)]
            public string MPESA_TRX_ID
            {
                get
                {
                    return this.mPESA_TRX_IDField;
                }
                set
                {
                    this.mPESA_TRX_IDField = value;
                    this.RaisePropertyChanged("MPESA_TRX_ID");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 4)]
            public string TRX_STATUS
            {
                get
                {
                    return this.tRX_STATUSField;
                }
                set
                {
                    this.tRX_STATUSField = value;
                    this.RaisePropertyChanged("TRX_STATUS");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 5)]
            public string RETURN_CODE
            {
                get
                {
                    return this.rETURN_CODEField;
                }
                set
                {
                    this.rETURN_CODEField = value;
                    this.RaisePropertyChanged("RETURN_CODE");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 6)]
            public string DESCRIPTION
            {
                get
                {
                    return this.dESCRIPTIONField;
                }
                set
                {
                    this.dESCRIPTIONField = value;
                    this.RaisePropertyChanged("DESCRIPTION");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 7)]
            public string MERCHANT_TRANSACTION_ID
            {
                get
                {
                    return this.mERCHANT_TRANSACTION_IDField;
                }
                set
                {
                    this.mERCHANT_TRANSACTION_IDField = value;
                    this.RaisePropertyChanged("MERCHANT_TRANSACTION_ID");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 8)]
            public string ENC_PARAMS
            {
                get
                {
                    return this.eNC_PARAMSField;
                }
                set
                {
                    this.eNC_PARAMSField = value;
                    this.RaisePropertyChanged("ENC_PARAMS");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 9)]
            public string TRX_ID
            {
                get
                {
                    return this.tRX_IDField;
                }
                set
                {
                    this.tRX_IDField = value;
                    this.RaisePropertyChanged("TRX_ID");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 10)]
            public string CALLBACK_STATUS
            {
                get
                {
                    return this.cALLBACK_STATUSField;
                }
                set
                {
                    this.cALLBACK_STATUSField = value;
                    this.RaisePropertyChanged("CALLBACK_STATUS");
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged(string propertyName)
            {
                System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
                if ((propertyChanged != null))
                {
                    propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                }
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class transactionStatusQueryRequest
        {

            [System.ServiceModel.MessageHeaderAttribute(Namespace = "tns:ns")]
            public MpesaService.CheckOutHeader CheckOutHeader;

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 0)]
            public MpesaService.transactionStatusRequest transactionStatusRequest;

            public transactionStatusQueryRequest()
            {
            }

            public transactionStatusQueryRequest(MpesaService.CheckOutHeader CheckOutHeader, MpesaService.transactionStatusRequest transactionStatusRequest)
            {
                this.CheckOutHeader = CheckOutHeader;
                this.transactionStatusRequest = transactionStatusRequest;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class transactionStatusQueryResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 0)]
            public MpesaService.transactionStatusResponse transactionStatusResponse;

            public transactionStatusQueryResponse()
            {
            }

            public transactionStatusQueryResponse(MpesaService.transactionStatusResponse transactionStatusResponse)
            {
                this.transactionStatusResponse = transactionStatusResponse;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ServiceModel.MessageContractAttribute(WrapperName = "transactionConfirmRequest", WrapperNamespace = "tns:ns", IsWrapped = true)]
        public partial class confirmTransactionRequest
        {

            [System.ServiceModel.MessageHeaderAttribute(Namespace = "tns:ns")]
            public MpesaService.CheckOutHeader CheckOutHeader;

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 0)]
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
            public string TRX_ID;

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 1)]
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
            public string MERCHANT_TRANSACTION_ID;

            public confirmTransactionRequest()
            {
            }

            public confirmTransactionRequest(MpesaService.CheckOutHeader CheckOutHeader, string TRX_ID, string MERCHANT_TRANSACTION_ID)
            {
                this.CheckOutHeader = CheckOutHeader;
                this.TRX_ID = TRX_ID;
                this.MERCHANT_TRANSACTION_ID = MERCHANT_TRANSACTION_ID;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ServiceModel.MessageContractAttribute(WrapperName = "transactionConfirmResponse", WrapperNamespace = "tns:ns", IsWrapped = true)]
        public partial class confirmTransactionResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 0)]
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
            public string RETURN_CODE;

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 1)]
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
            public string DESCRIPTION;

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 2)]
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
            public string MERCHANT_TRANSACTION_ID;

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 3)]
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
            public string TRX_ID;

            public confirmTransactionResponse()
            {
            }

            public confirmTransactionResponse(string RETURN_CODE, string DESCRIPTION, string MERCHANT_TRANSACTION_ID, string TRX_ID)
            {
                this.RETURN_CODE = RETURN_CODE;
                this.DESCRIPTION = DESCRIPTION;
                this.MERCHANT_TRANSACTION_ID = MERCHANT_TRANSACTION_ID;
                this.TRX_ID = TRX_ID;
            }
        }

        /// <remarks/>
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1586.0")]
        [System.SerializableAttribute()]
        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "tns:ns")]
        public partial class ResultMsg : object, System.ComponentModel.INotifyPropertyChanged
        {

            private string mSISDNField;

            private string aMOUNTField;

            private string mPESA_TRX_DATEField;

            private string mPESA_TRX_IDField;

            private string tRX_STATUSField;

            private string rETURN_CODEField;

            private string dESCRIPTIONField;

            private string mERCHANT_TRANSACTION_IDField;

            private string eNC_PARAMSField;

            private string tRX_IDField;

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
            public string MSISDN
            {
                get
                {
                    return this.mSISDNField;
                }
                set
                {
                    this.mSISDNField = value;
                    this.RaisePropertyChanged("MSISDN");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
            public string AMOUNT
            {
                get
                {
                    return this.aMOUNTField;
                }
                set
                {
                    this.aMOUNTField = value;
                    this.RaisePropertyChanged("AMOUNT");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 2)]
            public string MPESA_TRX_DATE
            {
                get
                {
                    return this.mPESA_TRX_DATEField;
                }
                set
                {
                    this.mPESA_TRX_DATEField = value;
                    this.RaisePropertyChanged("MPESA_TRX_DATE");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 3)]
            public string MPESA_TRX_ID
            {
                get
                {
                    return this.mPESA_TRX_IDField;
                }
                set
                {
                    this.mPESA_TRX_IDField = value;
                    this.RaisePropertyChanged("MPESA_TRX_ID");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 4)]
            public string TRX_STATUS
            {
                get
                {
                    return this.tRX_STATUSField;
                }
                set
                {
                    this.tRX_STATUSField = value;
                    this.RaisePropertyChanged("TRX_STATUS");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 5)]
            public string RETURN_CODE
            {
                get
                {
                    return this.rETURN_CODEField;
                }
                set
                {
                    this.rETURN_CODEField = value;
                    this.RaisePropertyChanged("RETURN_CODE");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 6)]
            public string DESCRIPTION
            {
                get
                {
                    return this.dESCRIPTIONField;
                }
                set
                {
                    this.dESCRIPTIONField = value;
                    this.RaisePropertyChanged("DESCRIPTION");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 7)]
            public string MERCHANT_TRANSACTION_ID
            {
                get
                {
                    return this.mERCHANT_TRANSACTION_IDField;
                }
                set
                {
                    this.mERCHANT_TRANSACTION_IDField = value;
                    this.RaisePropertyChanged("MERCHANT_TRANSACTION_ID");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 8)]
            public string ENC_PARAMS
            {
                get
                {
                    return this.eNC_PARAMSField;
                }
                set
                {
                    this.eNC_PARAMSField = value;
                    this.RaisePropertyChanged("ENC_PARAMS");
                }
            }

            /// <remarks/>
            [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 9)]
            public string TRX_ID
            {
                get
                {
                    return this.tRX_IDField;
                }
                set
                {
                    this.tRX_IDField = value;
                    this.RaisePropertyChanged("TRX_ID");
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            protected void RaisePropertyChanged(string propertyName)
            {
                System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
                if ((propertyChanged != null))
                {
                    propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                }
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class LNMOResultRequest
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 0)]
            public MpesaService.ResultMsg ResultMsg;

            public LNMOResultRequest()
            {
            }

            public LNMOResultRequest(MpesaService.ResultMsg ResultMsg)
            {
                this.ResultMsg = ResultMsg;
            }
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
        public partial class LNMOResultResponse
        {

            [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "tns:ns", Order = 0)]
            public string ResponseMsg;

            public LNMOResultResponse()
            {
            }

            public LNMOResultResponse(string ResponseMsg)
            {
                this.ResponseMsg = ResponseMsg;
            }
        }

        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        public interface LNMO_portTypeChannel : MpesaService.LNMO_portType, System.ServiceModel.IClientChannel
        {
        }

        [System.Diagnostics.DebuggerStepThroughAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
        public partial class LNMO_portTypeClient : System.ServiceModel.ClientBase<MpesaService.LNMO_portType>, MpesaService.LNMO_portType
        {

            public LNMO_portTypeClient()
            {
            }

            public LNMO_portTypeClient(string endpointConfigurationName) :
                    base(endpointConfigurationName)
            {
            }

            public LNMO_portTypeClient(string endpointConfigurationName, string remoteAddress) :
                    base(endpointConfigurationName, remoteAddress)
            {
            }

            public LNMO_portTypeClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
                    base(endpointConfigurationName, remoteAddress)
            {
            }

            public LNMO_portTypeClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
                    base(binding, remoteAddress)
            {
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            MpesaService.processCheckOutResponse1 MpesaService.LNMO_portType.processCheckOut(MpesaService.processCheckOutRequest1 request)
            {
                return base.Channel.processCheckOut(request);
            }

            public MpesaService.processCheckOutResponse processCheckOut(MpesaService.CheckOutHeader CheckOutHeader, MpesaService.processCheckOutRequest processCheckOutRequest)
            {
                MpesaService.processCheckOutRequest1 inValue = new MpesaService.processCheckOutRequest1();
                inValue.CheckOutHeader = CheckOutHeader;
                inValue.processCheckOutRequest = processCheckOutRequest;
                MpesaService.processCheckOutResponse1 retVal = ((MpesaService.LNMO_portType)(this)).processCheckOut(inValue);
                return retVal.processCheckOutResponse;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<MpesaService.processCheckOutResponse1> MpesaService.LNMO_portType.processCheckOutAsync(MpesaService.processCheckOutRequest1 request)
            {
                return base.Channel.processCheckOutAsync(request);
            }

            public System.Threading.Tasks.Task<MpesaService.processCheckOutResponse1> processCheckOutAsync(MpesaService.CheckOutHeader CheckOutHeader, MpesaService.processCheckOutRequest processCheckOutRequest)
            {
                MpesaService.processCheckOutRequest1 inValue = new MpesaService.processCheckOutRequest1();
                inValue.CheckOutHeader = CheckOutHeader;
                inValue.processCheckOutRequest = processCheckOutRequest;
                return ((MpesaService.LNMO_portType)(this)).processCheckOutAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            MpesaService.transactionStatusQueryResponse MpesaService.LNMO_portType.transactionStatusQuery(MpesaService.transactionStatusQueryRequest request)
            {
                return base.Channel.transactionStatusQuery(request);
            }

            public MpesaService.transactionStatusResponse transactionStatusQuery(MpesaService.CheckOutHeader CheckOutHeader, MpesaService.transactionStatusRequest transactionStatusRequest)
            {
                MpesaService.transactionStatusQueryRequest inValue = new MpesaService.transactionStatusQueryRequest();
                inValue.CheckOutHeader = CheckOutHeader;
                inValue.transactionStatusRequest = transactionStatusRequest;
                MpesaService.transactionStatusQueryResponse retVal = ((MpesaService.LNMO_portType)(this)).transactionStatusQuery(inValue);
                return retVal.transactionStatusResponse;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<MpesaService.transactionStatusQueryResponse> MpesaService.LNMO_portType.transactionStatusQueryAsync(MpesaService.transactionStatusQueryRequest request)
            {
                return base.Channel.transactionStatusQueryAsync(request);
            }

            public System.Threading.Tasks.Task<MpesaService.transactionStatusQueryResponse> transactionStatusQueryAsync(MpesaService.CheckOutHeader CheckOutHeader, MpesaService.transactionStatusRequest transactionStatusRequest)
            {
                MpesaService.transactionStatusQueryRequest inValue = new MpesaService.transactionStatusQueryRequest();
                inValue.CheckOutHeader = CheckOutHeader;
                inValue.transactionStatusRequest = transactionStatusRequest;
                return ((MpesaService.LNMO_portType)(this)).transactionStatusQueryAsync(inValue);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            MpesaService.confirmTransactionResponse MpesaService.LNMO_portType.confirmTransaction(MpesaService.confirmTransactionRequest request)
            {
                return base.Channel.confirmTransaction(request);
            }

            public string confirmTransaction(MpesaService.CheckOutHeader CheckOutHeader, ref string TRX_ID, ref string MERCHANT_TRANSACTION_ID, out string DESCRIPTION)
            {
                MpesaService.confirmTransactionRequest inValue = new MpesaService.confirmTransactionRequest();
                inValue.CheckOutHeader = CheckOutHeader;
                inValue.TRX_ID = TRX_ID;
                inValue.MERCHANT_TRANSACTION_ID = MERCHANT_TRANSACTION_ID;
                MpesaService.confirmTransactionResponse retVal = ((MpesaService.LNMO_portType)(this)).confirmTransaction(inValue);
                DESCRIPTION = retVal.DESCRIPTION;
                MERCHANT_TRANSACTION_ID = retVal.MERCHANT_TRANSACTION_ID;
                TRX_ID = retVal.TRX_ID;
                return retVal.RETURN_CODE;
            }

            public System.Threading.Tasks.Task<MpesaService.confirmTransactionResponse> confirmTransactionAsync(MpesaService.confirmTransactionRequest request)
            {
                return base.Channel.confirmTransactionAsync(request);
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            MpesaService.LNMOResultResponse MpesaService.LNMO_portType.LNMOResult(MpesaService.LNMOResultRequest request)
            {
                return base.Channel.LNMOResult(request);
            }

            public string LNMOResult(MpesaService.ResultMsg ResultMsg)
            {
                MpesaService.LNMOResultRequest inValue = new MpesaService.LNMOResultRequest();
                inValue.ResultMsg = ResultMsg;
                MpesaService.LNMOResultResponse retVal = ((MpesaService.LNMO_portType)(this)).LNMOResult(inValue);
                return retVal.ResponseMsg;
            }

            [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
            System.Threading.Tasks.Task<MpesaService.LNMOResultResponse> MpesaService.LNMO_portType.LNMOResultAsync(MpesaService.LNMOResultRequest request)
            {
                return base.Channel.LNMOResultAsync(request);
            }

            public System.Threading.Tasks.Task<MpesaService.LNMOResultResponse> LNMOResultAsync(MpesaService.ResultMsg ResultMsg)
            {
                MpesaService.LNMOResultRequest inValue = new MpesaService.LNMOResultRequest();
                inValue.ResultMsg = ResultMsg;
                return ((MpesaService.LNMO_portType)(this)).LNMOResultAsync(inValue);
            }
        }
    }
}