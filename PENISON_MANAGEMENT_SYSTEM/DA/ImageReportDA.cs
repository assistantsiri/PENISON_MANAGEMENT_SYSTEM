using PENISON_MANAGEMENT_SYSTEM.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Configuration;

namespace PENISON_MANAGEMENT_SYSTEM.DA
{
    public class ImageReportDA
    {
        public static string GetConfig()
        {
            //string connectionString = "Data Source=192.168.1.46;Integrated Security=true;Initial Catalog=showdoc212012;TrustServerCertificate=True";
            string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            return connectionString;
        }
        public List<PMSImageUpload> GetDailyReport(DateTime fromDate, DateTime toDate, string accountType)
        {
            return ExecuteReport("SP_DAILY_UPLOAD_REPORT",
                new SqlParameter("@FromDate", fromDate),
                new SqlParameter("@ToDate", toDate),
                new SqlParameter("@AccountType", accountType));
        }
        public List<PMSImageUpload> GetAllAccounttypeReports(DateTime fromDate, DateTime toDate)
        {
            return ExecuteAllReport("SP_GENERATEALLUPLOADEDREPORT",
                new SqlParameter("@FromDate", fromDate),
                new SqlParameter("@ToDate", toDate));
                
        }


        #region

        //public List<PMSImageUpload> GetMonthlyReport(int month, int year, string accountType)
        //{
        //    return ExecuteReport("SP_MONTHLY_UPLOAD_REPORT", new SqlParameter("@Month", month),
        //                                                        new SqlParameter("@Year", year),
        //                                                        new SqlParameter("@AccountType", accountType));
        //}

        //public List<PMSImageUpload> GetYearlyReport(int year, string accountType)
        //{
        //    return ExecuteReport("SP_YEARLY_UPLOAD_REPORT", new SqlParameter("@Year", year),
        //                                                         new SqlParameter("@AccountType", accountType));
        //}


        #endregion
        private List<PMSImageUpload> ExecuteReport(string procedureName, params SqlParameter[] parameters)
        {
            var connectionString = GetConfig();
            List<PMSImageUpload> list = new List<PMSImageUpload>();
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(procedureName, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parameters);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new PMSImageUpload
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            //DPCODE = Convert.ToInt32(reader["DPCODE"]),
                            Accountnumber = reader["Accountnumber"].ToString(),
                            //PPANO = reader["PPANO"].ToString(),
                            FILENAME = reader["FILENAME"].ToString(),
                            UPLOADEDDATE = Convert.ToDateTime(reader["UPLOADEDDATE"]),
                            UPLOADEDBY = reader["UPLOADEDBY"].ToString(),
                            //APPROVEDBY = reader["APPROVEDBY"].ToString(),
                            ACCOUNTTYPE = reader["ACCOUNTTYPE"].ToString(),
                            FILESIZE = reader["FILESIZE"].ToString()
                        });
                    }
                }
            }

            return list;
        }

        private List<PMSImageUpload> ExecuteAllReport(string procedureName, params SqlParameter[] parameters)
        {
            var connectionString = GetConfig();
            List<PMSImageUpload> list = new List<PMSImageUpload>();
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(procedureName, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parameters);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new PMSImageUpload
                        {
                            ID = Convert.ToInt32(reader["ID"]),
                            DPCODE = Convert.ToInt32(reader["DPCODE"]),
                            Accountnumber = reader["Accountnumber"].ToString(),
                            PPANO = reader["PPANO"].ToString(),
                            FILENAME = reader["FILENAME"].ToString(),
                            UPLOADEDDATE = Convert.ToDateTime(reader["UPLOADEDDATE"]),
                            UPLOADEDBY = reader["UPLOADEDBY"].ToString(),
                            APPROVEDBY = reader["APPROVEDBY"].ToString(),
                            ACCOUNTTYPE = reader["ACCOUNTTYPE"].ToString()
                        });
                    }
                }
            }

            return list;
        }
    }
}