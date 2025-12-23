using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.DA
{
    public class Repository
    {
        private readonly string conStr =
            ConfigurationManager.ConnectionStrings["conString"].ConnectionString;

        public void SavePdfDetails( string dpCode, string accountNumber, string category, string filePath, decimal fileSizeKB, string createdBy)
        {
            using (SqlConnection con = new SqlConnection(conStr))
            using (SqlCommand cmd = new SqlCommand("USP_SAVE_ACCOUNT_PDF", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@DPCode", SqlDbType.VarChar, 50).Value = dpCode;
                cmd.Parameters.Add("@AccountNumber", SqlDbType.VarChar, 50).Value = accountNumber;
                cmd.Parameters.Add("@Category", SqlDbType.VarChar, 50).Value = category;
                cmd.Parameters.Add("@FilePath", SqlDbType.NVarChar, 500).Value = filePath;
                cmd.Parameters.Add("@FileSizeKB", SqlDbType.Decimal).Value = fileSizeKB;
                cmd.Parameters.Add("@CreatedBy", SqlDbType.VarChar, 50).Value = createdBy;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<string> GetFullyConvertedDpCodes(string category)
        {
            var list = new List<string>();

            using (SqlConnection con = new SqlConnection(conStr))
            using (SqlCommand cmd = new SqlCommand("SP_GetFullyConvertedDpCodes", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Category", SqlDbType.VarChar).Value = category;

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(dr["DPCode"].ToString());
                    }
                }
            }
            return list;
        }

        public HashSet<string> GetConvertedAccounts(string category, string dpCode)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (SqlConnection con = new SqlConnection(conStr))
            using (SqlCommand cmd = new SqlCommand("SP_GetConvertedAccounts", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Category", SqlDbType.VarChar).Value = category;
                cmd.Parameters.Add("@DPCode", SqlDbType.VarChar).Value = dpCode;

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        set.Add(dr["AccountNumber"].ToString().Trim().ToUpper());
                    }
                }
            }
            return set;
        }


        public bool IsAccountAlreadyConverted(string category, string dpCode, string accountNumber)
        {
            using (SqlConnection con = new SqlConnection(conStr))
            using (SqlCommand cmd = new SqlCommand("SP_IsAccountAlreadyConverted", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Category", SqlDbType.VarChar).Value = category;
                cmd.Parameters.Add("@DPCode", SqlDbType.VarChar).Value = dpCode;
                cmd.Parameters.Add("@AccountNumber", SqlDbType.VarChar).Value = accountNumber;

                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }


        public bool IsAccountPdfExists(string dpCode, string accountNumber, string category)
        {
            using (SqlConnection con = new SqlConnection(conStr))
            using (SqlCommand cmd = new SqlCommand("SP_IsAccountPdfExists", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@DPCode", SqlDbType.VarChar).Value = dpCode;
                cmd.Parameters.Add("@AccountNumber", SqlDbType.VarChar).Value = accountNumber;
                cmd.Parameters.Add("@Category", SqlDbType.VarChar).Value = category;

                con.Open();
                return cmd.ExecuteScalar() != null;
            }
        }


    }

}