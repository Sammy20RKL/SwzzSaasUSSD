using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SwizzMobile.Utils
{
    public class FetchData
    {
        public DataTable RunQuery(string query)
        {
            DataTable dtResult = new DataTable();
            string Command = query;
            using (SqlConnection myConnection = new SqlConnection(Properties.Settings.Default.ConnString))
            {
                using (SqlDataAdapter myDataAdapter = new SqlDataAdapter(Command, myConnection))
                {
                    dtResult = new DataTable();
                    myDataAdapter.Fill(dtResult);
                }
            }
            return dtResult;
        }
    }
}