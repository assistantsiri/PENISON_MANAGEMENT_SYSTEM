//using Microsoft.Extensions.Configuration;
//using PENISON_MANAGEMENT_SYSTEM.Models;
//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Data;
//using System.Linq;
//using System.Web;

//namespace PENISON_MANAGEMENT_SYSTEM.DA
//{
//    public class PensionDataAccess
//    {
//        public static string GetConfig()
//        {
//            string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
//            //string connectionString = "Data Source=192.168.1.46;Integrated Security=true;Initial Catalog=showdoc212012;TrustServerCertificate=True";
//            return connectionString;
//        }

//        public List<PensionCategory> GetAllCategories()
//        {
//            var categories = new List<PensionCategory>();

//            using (var connection = new SqlConnection(_connectionString))
//            {
//                connection.Open();
//                using (var command = new SqlCommand("sp_GetAllCategories", connection))
//                {
//                    command.CommandType = CommandType.StoredProcedure;

//                    using (var reader = command.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            categories.Add(new PensionCategory
//                            {
//                                CategoryId = reader.GetInt32(0),
//                                CategoryName = reader.GetString(1),
//                                ParentCategoryId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
//                                IsActive = reader.GetBoolean(3)
//                            });
//                        }
//                    }
//                }
//            }

//            return categories;
//        }

//        public List<PensionFile> GetFiles(int? year = null, string month = null, int? categoryId = null, string section = null)
//        {
//            var files = new List<PensionFile>();

//            using (var connection = new SqlConnection(_connectionString))
//            {
//                connection.Open();
//                using (var command = new SqlCommand("sp_GetFiles", connection))
//                {
//                    command.CommandType = CommandType.StoredProcedure;

//                    if (year.HasValue)
//                        command.Parameters.AddWithValue("@Year", year);
//                    else
//                        command.Parameters.AddWithValue("@Year", DBNull.Value);

//                    command.Parameters.AddWithValue("@Month", string.IsNullOrEmpty(month) ? (object)DBNull.Value : month);

//                    if (categoryId.HasValue)
//                        command.Parameters.AddWithValue("@CategoryId", categoryId);
//                    else
//                        command.Parameters.AddWithValue("@CategoryId", DBNull.Value);

//                    command.Parameters.AddWithValue("@Section", string.IsNullOrEmpty(section) ? (object)DBNull.Value : section);

//                    using (var reader = command.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            files.Add(new PensionFile
//                            {
//                                FileId = reader.GetInt32(0),
//                                FileName = reader.GetString(1),
//                                FilePath = reader.GetString(2),
//                                FileSize = reader.GetInt64(3),
//                                UploadDate = reader.GetDateTime(4),
//                                Year = reader.GetInt32(5),
//                                Month = reader.GetString(6),
//                                CategoryId = reader.GetInt32(7),
//                                Section = reader.GetString(8),
//                                UploadedBy = reader.IsDBNull(9) ? null : reader.GetString(9),
//                                CategoryName = reader.GetString(10)
//                            });
//                        }
//                    }
//                }
//            }

//            return files;
//        }

//        public int InsertFile(PensionFile file)
//        {
//            using (var connection = new SqlConnection(_connectionString))
//            {
//                connection.Open();
//                using (var command = new SqlCommand("sp_InsertFile", connection))
//                {
//                    command.CommandType = CommandType.StoredProcedure;
//                    command.Parameters.AddWithValue("@FileName", file.FileName);
//                    command.Parameters.AddWithValue("@FilePath", file.FilePath);
//                    command.Parameters.AddWithValue("@FileSize", file.FileSize);
//                    command.Parameters.AddWithValue("@Year", file.Year);
//                    command.Parameters.AddWithValue("@Month", file.Month);
//                    command.Parameters.AddWithValue("@CategoryId", file.CategoryId);
//                    command.Parameters.AddWithValue("@Section", file.Section);
//                    command.Parameters.AddWithValue("@UploadedBy", file.UploadedBy);

//                    var result = command.ExecuteScalar();
//                    return Convert.ToInt32(result);
//                }
//            }
//        }

//        public bool DeleteFile(int fileId)
//        {
//            using (var connection = new SqlConnection(_connectionString))
//            {
//                connection.Open();
//                using (var command = new SqlCommand("sp_DeleteFile", connection))
//                {
//                    command.CommandType = CommandType.StoredProcedure;
//                    command.Parameters.AddWithValue("@FileId", fileId);

//                    var rowsAffected = command.ExecuteNonQuery();
//                    return rowsAffected > 0;
//                }
//            }
//        }
//    }

//}