using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwizzMobile.Models
{
	public class Responses
	{
        public bool HasResults = true;
        public object ResponseData = null;
        public string ResponseErrors = string.Empty;
    }
    public enum STATUS
    {
        PENDING, COMPLETED, FAILED, SUCCESS
    }
}