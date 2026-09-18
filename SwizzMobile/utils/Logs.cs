using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SwizzMobile.Utils
{
    public class Logs
    {
        public static string logpath = @"D:\HOSTED\USSD";

        public static string LogFileName
        {
            get
            {
                if (!Directory.Exists(logpath))
                    Directory.CreateDirectory(logpath);
                return String.Format("{0}{1}{2}{3}.txt", logpath, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }
        }


        public static void LogEntryOnFile(string clientRequest)
        {
            File.AppendAllText(LogFileName, String.Format("{0}: {1}\n", DateTime.Now, clientRequest));

        }
    }
}