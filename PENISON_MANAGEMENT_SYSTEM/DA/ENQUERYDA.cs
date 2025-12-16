using PENISON_MANAGEMENT_SYSTEM.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.DA
{
    public class ENQUERYDA
    {
        public static string GetConfig()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            //string connectionString = "Data Source=192.168.1.46;Integrated Security=true;Initial Catalog=showdoc212012;TrustServerCertificate=True";
            return connectionString;
        }
        public static List<UploadInfo> ENQUERY(string keyword)
        {
            var connectionString = GetConfig();
            List<UploadInfo> list = new List<UploadInfo>();

            // Basic input validation
            if (string.IsNullOrWhiteSpace(keyword)
                || keyword.Length > 100
                || keyword.Contains(";")
                || keyword.Contains("--"))
            {
                return list;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT  * FROM PMSIMAGEUPLOAD
        WHERE DPCODE LIKE @kw OR
              Accountnumber LIKE @kw OR
              PPANO LIKE @kw OR
              FILENAME LIKE @kw OR
              UPLOADEDBY LIKE @kw OR
              APPROVEDBY LIKE @kw OR
              ACCOUNTTYPE LIKE @kw OR
              STATUS LIKE @kw
        ORDER BY UPLOADEDDATE DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@kw", "%" + keyword + "%");

                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new UploadInfo
                        {
                            ID = Convert.ToInt32(dr["ID"]),
                            DPCODE = dr["DPCODE"]?.ToString(),
                            Accountnumber = dr["Accountnumber"]?.ToString(),
                            PPANO = dr["PPANO"]?.ToString(),
                            FILENAME = dr["FILENAME"]?.ToString(),
                            UPLOADEDDATE = dr["UPLOADEDDATE"] != DBNull.Value ? Convert.ToDateTime(dr["UPLOADEDDATE"]) : DateTime.MinValue,
                            UPLOADEDBY = dr["UPLOADEDBY"]?.ToString(),
                            APPROVEDBY = dr["APPROVEDBY"]?.ToString(),
                            ACCOUNTTYPE = dr["ACCOUNTTYPE"]?.ToString(),
                            STATUS = dr["STATUS"]?.ToString()
                        });
                    }
                }
            }

            return list;
        }

        public List<PendingAccountStatus> GetPendingAccountStats()
        {
            var connectionString = GetConfig();
            var result = new List<PendingAccountStatus>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT 
                    P.PPA_Type AS AccountType,
                    COUNT(*) AS PendingCount
                FROM 
                    PPA_DETAILS_1 P
                LEFT JOIN 
                    PMSIMAGEUPLOAD M ON P.PPA_ACNO = M.Accountnumber
                WHERE 
                    M.Accountnumber IS NULL
                GROUP BY 
                    P.PPA_Type";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new PendingAccountStatus
                            {
                                AccountType = reader["AccountType"].ToString(),
                                PendingCount = Convert.ToInt32(reader["PendingCount"])
                            });
                        }
                    }
                }
            }

            return result;
        }

        public List<PendingAccountStatus> GetUploadedStatsByDate(DateTime uploadDate)
        {
            var formateddate = uploadDate.ToString("yyyy-MM-dd");
            var result = new List<PendingAccountStatus>();
            string connectionString = GetConfig();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_GET_UPLOADED_STATS_BY_DATE", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UploadDate", formateddate);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new PendingAccountStatus
                            {
                                AccountType = reader["AccountType"].ToString(),
                                CurrentDataUploadedCount = Convert.ToInt32(reader["Count"])
                            });
                        }
                    }
                }
            }

            return result;
        }
        //public static List<PensionFile> GetFiles(int? year = null, string month = null, int? categoryId = null, string section = null)
        //{
        //    var connectionString = GetConfig();
        //    var files = new List<PensionFile>();

        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        string query = "SELECT * FROM UploadFileForPensionFiles ";

        //        if (year.HasValue)
        //            query += " AND Year = @Year";
        //        if (!string.IsNullOrEmpty(month))
        //            query += " AND Month = @Month";
        //        if (!string.IsNullOrEmpty(section))
        //            query += " AND Section = @Section";

        //        query += " ORDER BY Year DESC, Month";

        //        using (SqlCommand cmd = new SqlCommand(query, conn))
        //        {
        //            if (year.HasValue)
        //                cmd.Parameters.AddWithValue("@Year", year.Value);
        //            if (!string.IsNullOrEmpty(month))
        //                cmd.Parameters.AddWithValue("@Month", month);
        //            if (!string.IsNullOrEmpty(section))
        //                cmd.Parameters.AddWithValue("@Section", section);

        //            conn.Open();
        //            using (SqlDataReader reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    files.Add(new PensionFile
        //                    {
        //                        FileId = Convert.ToInt32(reader["FileId"]),
        //                        FileName = reader["FileName"].ToString(),
        //                        FilePath = reader["FilePath"].ToString(),
        //                        FileSize = Convert.ToInt64(reader["FileSize"]),
        //                        UploadDate = Convert.ToDateTime(reader["UploadDate"]),
        //                        Year = Convert.ToInt32(reader["Year"]),
        //                        Month = reader["Month"].ToString(),
        //                        Section = reader["Section"].ToString(),
        //                        UploadedBy = reader["UploadedBy"].ToString()
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return files;
        //}


        //public static int InsertFile(PensionFile file)
        //{
        //    var connectionString = GetConfig();
        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        string query = @"INSERT INTO UploadFileForPensionFiles 
        //                    (FileName, FilePath, FileSize, Year, Month, Section, UploadedBy) 
        //                    VALUES (@FileName, @FilePath, @FileSize, @Year, @Month, @Section, @UploadedBy);
        //                    SELECT SCOPE_IDENTITY();";

        //        using (SqlCommand cmd = new SqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@FileName", file.FileName);
        //            cmd.Parameters.AddWithValue("@FilePath", file.FilePath);
        //            cmd.Parameters.AddWithValue("@FileSize", file.FileSize);
        //            cmd.Parameters.AddWithValue("@Year", file.Year);
        //            cmd.Parameters.AddWithValue("@Month", file.Month);
        //            cmd.Parameters.AddWithValue("@Section", file.Section);
        //            cmd.Parameters.AddWithValue("@UploadedBy", file.UploadedBy);

        //            conn.Open();
        //            return Convert.ToInt32(cmd.ExecuteScalar());
        //        }
        //    }
        //}


        //public static bool DeleteFile(int fileId)
        //{
        //    var connectionString = GetConfig();
        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        string query = "DELETE FROM UploadFileForPensionFiles WHERE FileId = @FileId";

        //        using (SqlCommand cmd = new SqlCommand(query, conn))
        //        {
        //            cmd.Parameters.AddWithValue("@FileId", fileId);

        //            conn.Open();
        //            return cmd.ExecuteNonQuery() > 0;
        //        }
        //    }
        //}

        public int InsertFile(PensionFile file)
        {
            var connectionString = GetConfig();
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("sp_InsertPensionFile", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FileName", file.FileName);
                cmd.Parameters.AddWithValue("@FilePath", file.FilePath);
                cmd.Parameters.AddWithValue("@FileSize", file.FileSize);
                cmd.Parameters.AddWithValue("@Year", file.Year);
                cmd.Parameters.AddWithValue("@Month", file.Month);
                cmd.Parameters.AddWithValue("@Section", file.Section);
                cmd.Parameters.AddWithValue("@UploadedBy", file.UploadedBy);

                var outputParam = new SqlParameter("@NewFileId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(outputParam);

                conn.Open();
                cmd.ExecuteNonQuery();

                return Convert.ToInt32(outputParam.Value);
            }
        }
        public int GetUploadedFileCount(string accountType)
        {
            var connectionString = GetConfig();
            int count = 0;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT COUNT(*) 
                     FROM PMSIMAGEUPLOAD 
                     WHERE ACCOUNTTYPE = @type 
                     AND MONTH(UPLOADEDDATE) = MONTH(GETDATE())
                     AND YEAR(UPLOADEDDATE) = YEAR(GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@type", accountType);
                    con.Open();
                    count = (int)cmd.ExecuteScalar();
                }
            }

            return count;
        }
        public List<PensionFile> GetFiles(int year, string section, string month = null)
        {
            var connectionString = GetConfig();
            var files = new List<PensionFile>();

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("sp_GetPensionFiles", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Year", year);
                cmd.Parameters.AddWithValue("@Section", section);
                cmd.Parameters.AddWithValue("@Month", (object)month ?? DBNull.Value);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        files.Add(new PensionFile
                        {
                            FileId = Convert.ToInt32(reader["FileId"]),
                            FileName = reader["FileName"].ToString(),
                            FilePath = reader["FilePath"].ToString(),
                            FileSize = Convert.ToInt64(reader["FileSize"]),
                            UploadDate = Convert.ToDateTime(reader["UploadDate"]),
                            Year = Convert.ToInt32(reader["Year"]),
                            Month = reader["Month"].ToString(),
                            Section = reader["Section"].ToString(),
                            UploadedBy = reader["UploadedBy"].ToString()
                        });
                    }
                }
            }

            return files;
        }

        public bool DeleteFile(int fileId)
        {
            var connectionString = GetConfig();
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("sp_DeletePensionFile", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FileId", fileId);

                conn.Open();
                var rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
    }
}