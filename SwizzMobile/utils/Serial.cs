using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SwizzMobile.Utils
{
    public static class Serial
    {
        public static string Reference(string saccoid)
        {
            int _min1 = 100000000;
            int _max1 = 999999999;

            int _min2 = 100000000;
            int _max2 = 999999999;

            Random _rdm = new Random();

            string number1 = _rdm.Next(_min1, _max1).ToString();
            string number2 = _rdm.Next(_min2, _max2).ToString();

            string finalNo = number1.Substring(0, 8) + number2.Substring(0,8);

            return finalNo;
        }
    }
}