using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using PENISON_MANAGEMENT_SYSTEM.Models;
using System.Configuration;

namespace PENISON_MANAGEMENT_SYSTEM.DA
{
    public class PPAAccountDA
    {

        public static string GetConfig()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            return connectionString;
        }

        


        public DataTable GetTodaysUploads(string staffNo)
        {
            var connectionString = GetConfig();
            DataTable dt = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT PPA_ACNO, PPA_PPONO, PPA_NAME, PPA_CREATED_BY, 
                           FORMAT(PPA_CREATED_DATE, 'dd-MMM-yyyy HH:mm') AS PPA_CREATED_DATE,
                           PPA_DPCD, PPA_Type 
                           FROM PPA_DTLS 
                           WHERE CONVERT(DATE, PPA_CREATED_DATE) = CONVERT(DATE, GETDATE())
                           AND PPA_CREATED_BY = @StaffNo
                           ORDER BY PPA_CREATED_DATE DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StaffNo", staffNo);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in GetTodaysUploads: {ex.Message}");
                        throw;
                    }
                }
            }

            return dt;
        }
        public void INSERTEXCELDATEINPPA_DTLSTABBLE(DataTable dt)
        {
            var conString = GetConfig();
            using (SqlConnection con = new SqlConnection(conString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_INSERT_PPA_DETAILS", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@PPA_DETAILS", dt);
                    tvpParam.SqlDbType = SqlDbType.Structured;

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertExcelData(List<EXCELFILEUPLOADMODEL> items)
        {
            var conString = GetConfig();
            using (SqlConnection conn = new SqlConnection(conString))
            {
                conn.Open();

                foreach (var item in items)
                {
                    string query = @"INSERT INTO PPA_DETAILS_1 
                            (PPA_ACNO, PPA_PPONO, ppA_name, PPA_DPCD, PPA_Type, PPA_CREATED_BY, PPA_CREATED_DATE) 
                            VALUES 
                            (@PPA_ACNO, @PPA_PPONO, @ppA_name, @PPA_DPCD, @PPA_Type, @PPA_CREATED_BY, @PPA_CREATED_DATE)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PPA_ACNO", item.PPA_ACNO);
                        cmd.Parameters.AddWithValue("@PPA_PPONO", item.PPA_PPONO);
                        cmd.Parameters.AddWithValue("@ppA_name", item.ppA_name);
                        cmd.Parameters.AddWithValue("@PPA_DPCD", item.PPA_DPCD);
                        cmd.Parameters.AddWithValue("@PPA_Type", item.PPA_Type);
                        cmd.Parameters.AddWithValue("@PPA_CREATED_BY", item.PPA_CREATED_BY);
                        cmd.Parameters.AddWithValue("@PPA_CREATED_DATE", item.PPA_CREATED_DATE);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        public List<EXCELFILEUPLOADMODEL> GetAllPPADetails()
        {
            var conString = GetConfig();
            List<EXCELFILEUPLOADMODEL> ppaDetails = new List<EXCELFILEUPLOADMODEL>();

            using (SqlConnection connection = new SqlConnection(conString))
            {
                string query = "SELECT PPA_ACNO, PPA_PPONO, ppA_name, PPA_DPCD, PPA_Type, PPA_CREATED_BY, PPA_CREATED_DATE FROM PPA_DETAILS_1";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ppaDetails.Add(new EXCELFILEUPLOADMODEL
                        {
                            PPA_ACNO = reader["PPA_ACNO"].ToString(),
                            PPA_PPONO = reader["PPA_PPONO"].ToString(),
                            ppA_name = reader["ppA_name"].ToString(),
                            PPA_DPCD = reader["PPA_DPCD"].ToString(),
                            PPA_Type = reader["PPA_Type"].ToString(),
                            PPA_CREATED_BY = reader["PPA_CREATED_BY"].ToString(),
                            PPA_CREATED_DATE = Convert.ToDateTime(reader["PPA_CREATED_DATE"])
                        });
                    }
                }
            }

            return ppaDetails;
        }

        public List<PPADETAILSWITHIMAGE> SP_PPADETAILSWITHIMAGE(string accountNumber)
        {
            var accounts = new List<PPADETAILSWITHIMAGE>();
            var conString = GetConfig();
            using (var connection = new SqlConnection(conString))
            using (var command = new SqlCommand("SP_PPADETAILSWITHIMAGE", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                if (!string.IsNullOrWhiteSpace(accountNumber))
                {
                    command.Parameters.AddWithValue("@ACCOUNTNUMBER", accountNumber);
                }
                else
                {
                    command.Parameters.AddWithValue("@ACCOUNTNUMBER", DBNull.Value);
                }

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        accounts.Add(new PPADETAILSWITHIMAGE
                        {
                            PPA_ACNO = reader["PPA_ACNO"].ToString(),
                            PPA_PPONO = reader["PPA_PPONO"].ToString(),
                            PPA_name = reader["PPA_name"].ToString(),
                            PPA_DPCD = reader["PPA_DPCD"].ToString(),
                            PPA_Type = reader["PPA_Type"].ToString()
                        });
                    }
                }
            }

            return accounts;
        }
    }
}