using iTextSharp.text;
using iTextSharp.text.pdf;
using PENISON_MANAGEMENT_SYSTEM.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace PENISON_MANAGEMENT_SYSTEM.DA
{
    public class Helper
    {
        //private const int SaltSize = 32;
        public static string GetConfig()
        {
            //string connectionString = "Data Source=192.168.1.46;Integrated Security=true;Initial Catalog=showdoc212012;TrustServerCertificate=True";
            string connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            return connectionString;
        }

        //public static string GenerateSalt()
        //{
        //    byte[] saltBytes = new byte[SaltSize];
        //    using (var rng = new RNGCryptoServiceProvider())
        //    {
        //        rng.GetBytes(saltBytes);
        //    }
        //    return Convert.ToBase64String(saltBytes);
        //}

        //public static string HashPassword(string password, string salt)
        //{
        //    using (var sha256 = SHA256.Create())
        //    {
        //        byte[] saltedPassword = Encoding.UTF8.GetBytes(password + salt);
        //        byte[] hash = sha256.ComputeHash(saltedPassword);
        //        return Convert.ToBase64String(hash);
        //    }
        //}

        //public LoginModel ValidateLogin(LoginModel model)
        //{
        //    var loginModel = new LoginModel();

        //    try
        //    {
        //        // Get stored password details
        //        var resultModel = GetLoginPassword(model.Username);
        //        if (resultModel == null)
        //        {
        //            return null;
        //        }

        //        string storedHashedPassword = resultModel.Password;
        //        string salt = resultModel.SALT;
        //        int Role = resultModel.RoleID;

        //        // Hash the entered password with the stored salt
        //        string enteredHashedPassword = HashPassword(model.Password, salt);

        //        // Compare the entered hashed password with the stored hashed password
        //        if (enteredHashedPassword == storedHashedPassword)
        //        {
        //            loginModel.Username = resultModel.Username;
        //            loginModel.RoleID = resultModel.RoleID;
        //            loginModel.StaffNo = resultModel.StaffNo;
        //            loginModel.StaffName = resultModel.StaffName;
        //            return loginModel;
        //        }
        //    }
        //    catch (Exception error)
        //    {
        //        Console.WriteLine(error.Message);
        //    }

        //    return null;
        //}

        //public static LoginModel User_Register(string Username, string Password, string SALT, int Roleid)
        //{
        //    LoginModel model = new LoginModel();
        //    var connectionString = GetConfig();
        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(connectionString))
        //        {
        //            con.Open();
        //            SqlCommand cmd = new SqlCommand("SP_REGISTER_LOGIN", con);
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.AddWithValue("@ACTION", "V_REGISTER");
        //            cmd.Parameters.AddWithValue("@USERNAME", Username);
        //            cmd.Parameters.AddWithValue("@PASSWORD", Password);
        //            cmd.Parameters.AddWithValue("@SALT", SALT);
        //            cmd.Parameters.AddWithValue("@ROLEID", Roleid);


        //            cmd.ExecuteNonQuery();


        //            model.Username = Username;
        //            model.Password = Password;
        //            model.SALT = SALT;
        //            model.RoleID = Roleid;

        //            con.Close();
        //        }
        //    }
        //    catch (Exception error)
        //    {
        //        Console.WriteLine(error.Message);
        //    }

        //    return model;
        //}


        //public LoginModel GetLoginPassword(string Username)
        //{
        //    LoginModel model = new LoginModel();
        //    var connectionString = GetConfig();
        //    try
        //    {
        //        using (SqlConnection con = new SqlConnection(connectionString))
        //        {
        //            con.Open();
        //            SqlCommand cmd = new SqlCommand("SP_REGISTER_LOGIN", con);
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.AddWithValue("@ACTION", "V_LOGIN");
        //            cmd.Parameters.AddWithValue("@USERNAME", Username);
        //            cmd.Parameters.AddWithValue("@PASSWORD", "");
        //            cmd.Parameters.AddWithValue("@SALT", "");
        //            cmd.Parameters.AddWithValue("@ROLEID", "");
        //            SqlDataReader rdr = cmd.ExecuteReader();
        //            if (rdr.Read())
        //            {
        //                model.Username = rdr["USERNAME"].ToString();
        //                model.Password = rdr["PSW"].ToString();
        //                model.SALT = rdr["SALT"].ToString();
        //                model.StaffName = rdr["StaffName"].ToString();
        //                model.StaffNo = rdr["StaffNo"].ToString();
        //                model.RoleID = Convert.ToInt32(rdr["Role"]);
        //                //model.RoleID = Convert.ToInt32(rdr["ROLEID"]);
        //            }

        //            con.Close();
        //        }
        //    }
        //    catch (Exception error)
        //    {
        //        Console.WriteLine(error.Message);
        //    }

        //    return model;
        //}

        public LoginModel ValidateLogin(LoginModel model)
        {
            try
            {
                string encryptedPassword = CryptoHelper.Encrypt(model.Password);

                var result = GetUserLoginStatus(model.Username, encryptedPassword);

                if (result == null) return null;

                if (result.Status == "LOCKED")
                {
                    model.LoginMessage = $"Account locked until {result.LockedUntil}";
                    model.Status = result.Status;
                    return model;
                }

                if (result.Status == "INVALID")
                {
                    model.LoginMessage = "Invalid credentials.";
                    return null;
                }

                if (result.Status == "SUCCESS")
                {
                    if (result.LastActivityTime == null)
                    {
                        string newSessionToken = Guid.NewGuid().ToString();
                        UpdateSessionToken(result.Username, newSessionToken);
                    }
                                
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return null;
        }
        public void UpdateSessionToken04112025(string username, string token)
        {
            using (SqlConnection con = new SqlConnection(GetConfig()))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SP_CPPC_REGISTER_LOGIN", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ACTION", "UPDATE_SESSION_TOKEN");
                cmd.Parameters.AddWithValue("@USERNAME", username);
                cmd.Parameters.AddWithValue("@SessionToken", token);
                cmd.ExecuteNonQuery();
            }
        }
        public void ClearSessionToken04112025(string username)
        {
            using (SqlConnection con = new SqlConnection(GetConfig()))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SP_CPPC_REGISTER_LOGIN", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ACTION", "CLEAR_SESSION_TOKEN");
                cmd.Parameters.AddWithValue("@USERNAME", username);
                cmd.ExecuteNonQuery();
            }
        }

        public LoginModel GetUserLoginStatus04112025(string username, string encryptedPassword)
        {
            LoginModel model = null;

            using (SqlConnection con = new SqlConnection(GetConfig()))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SP_CPPC_REGISTER_LOGIN", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Login action
                    cmd.Parameters.AddWithValue("@ACTION", "V_LOGIN");
                    cmd.Parameters.AddWithValue("@USERNAME", username);
                    cmd.Parameters.AddWithValue("@PASSWORD", encryptedPassword);

                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            model = new LoginModel();

                            // Status
                            model.Status = ColumnExists(rdr, "Status") && rdr["Status"] != DBNull.Value
                                ? rdr["Status"].ToString()
                                : null;

                            // Success: read user details
                            if (model.Status == "SUCCESS")
                            {
                                model.Username = ColumnExists(rdr, "USERNAME") && rdr["USERNAME"] != DBNull.Value
                                    ? rdr["USERNAME"].ToString() : null;

                                model.Password = ColumnExists(rdr, "PSW") && rdr["PSW"] != DBNull.Value
                                    ? rdr["PSW"].ToString() : null;

                                model.StaffName = ColumnExists(rdr, "StaffName") && rdr["StaffName"] != DBNull.Value
                                    ? rdr["StaffName"].ToString() : null;

                                model.StaffNo = ColumnExists(rdr, "StaffNo") && rdr["StaffNo"] != DBNull.Value
                                    ? rdr["StaffNo"].ToString() : null;

                                model.RoleID = ColumnExists(rdr, "Role") && rdr["Role"] != DBNull.Value
                                    ? Convert.ToInt32(rdr["Role"]) : 0;

                                model.SessionToken = ColumnExists(rdr, "ActiveSessionToken") && rdr["ActiveSessionToken"] != DBNull.Value
                                    ? rdr["ActiveSessionToken"].ToString() : null;

                                model.LastActivityTime = ColumnExists(rdr, "LastLoginTime") && rdr["LastLoginTime"] != DBNull.Value
                                    ? (DateTime?)Convert.ToDateTime(rdr["LastLoginTime"]) : null;

                                // Concurrent login check (30 minutes)
                                model.CanLogin = model.LastActivityTime.HasValue
                                    ? (DateTime.Now - model.LastActivityTime.Value).TotalMinutes >= 30
                                    : true;
                            }

                            // LOCKED
                            if (model.Status == "LOCKED")
                            {
                                model.LockedUntil = ColumnExists(rdr, "LockedUntil") && rdr["LockedUntil"] != DBNull.Value
                                    ? Convert.ToDateTime(rdr["LockedUntil"])
                                    : (DateTime?)null;

                                model.CanLogin = false; // locked users cannot login
                            }

                            // INVALID login attempts
                            if (model.Status == "INVALID" && ColumnExists(rdr, "Attempts"))
                            {
                                model.LoginAttempts = Convert.ToInt32(rdr["Attempts"]);
                                model.CanLogin = true; // still allowed to retry
                            }
                        }
                    }
                }
            }

            return model;
        }

        //=============================================================================
        /// <summary>
        /// Inserts a new PDF version (original or merged) into the database.
        /// </summary>
        public void InsertPdfVersion(string accountNumber, string fileName, byte[] pdfBytes,
                                     string uploadedBy, string accountType, string status, bool isMerged)
        {
            var _connectionString = GetConfig();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("dbo.InsertPdfVersion", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                cmd.Parameters.AddWithValue("@FileName", fileName);
                cmd.Parameters.AddWithValue("@PdfData", pdfBytes);
                cmd.Parameters.AddWithValue("@UploadedBy", uploadedBy);
                cmd.Parameters.AddWithValue("@AccountType", accountType);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@FileSize", pdfBytes.Length);
                cmd.Parameters.AddWithValue("@IsMerged", isMerged);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Fetches the latest merged PDF for an account.
        /// </summary>
        public byte[] GetLatestMergedPdf(string accountNumber)
        {
            var _connectionString = GetConfig();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"SELECT TOP 1 PdfData 
                               FROM CPPCIMAGEUPLOAD 
                               WHERE AccountNumber=@Acc 
                               ORDER BY ID DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 3600;
                    cmd.Parameters.AddWithValue("@Acc", accountNumber);
                      var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return (byte[])result;
                }
            }
            return null;
        }
        public static byte[] MergePdfBytes(byte[] pdf1, byte[] pdf2)
        {
            using (var ms = new MemoryStream())
            {
                Document doc = new Document();
                PdfCopy copy = new PdfCopy(doc, ms);
                doc.Open();

                foreach (var pdfBytes in new[] { pdf1, pdf2 })
                {
                    using (var reader = new PdfReader(pdfBytes))
                    {
                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            copy.AddPage(copy.GetImportedPage(reader, i));
                        }
                    }
                }

                doc.Close();
                return ms.ToArray();
            }
        }

        public LoginModel GetUserLoginStatus(string username, string encryptedPassword)
        {
            LoginModel model = null;

            using (SqlConnection con = new SqlConnection(GetConfig()))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SP_CPPC_REGISTER_LOGIN", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ACTION", "V_LOGIN");
                cmd.Parameters.AddWithValue("@USERNAME", username);
                cmd.Parameters.AddWithValue("@PASSWORD", encryptedPassword);

                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.Read())
                    {
                        model = new LoginModel();

                        
                        model.Status = rdr["STATUS"].ToString();
                        model.LoginAttempts = Convert.ToInt32(rdr["ATTEMPTS"]);

                      
                        if (model.Status == "LOCKED")
                        {
                            model.LockedUntil = rdr["LOCKEDUNTIL"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(rdr["LOCKEDUNTIL"]);

                            return model;
                        }

                      
                        if (model.Status == "INVALID")
                        {
                            return model;
                        }

                        
                        if (model.Status == "SUCCESS")
                        {
                            model.Username = rdr["USERNAME"] == DBNull.Value ? null : rdr["USERNAME"].ToString();
                            model.Password = rdr["PSW"] == DBNull.Value ? null : rdr["PSW"].ToString();
                            model.StaffNo = rdr["STAFFNO"] == DBNull.Value ? null : rdr["STAFFNO"].ToString();
                            model.StaffName = rdr["STAFFNAME"] == DBNull.Value ? null : rdr["STAFFNAME"].ToString();
                            model.RoleID = rdr["ROLE"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ROLE"]);

                            model.SessionToken = rdr["ACTIVESESSIONTOKEN"] == DBNull.Value
                                ? null
                                : rdr["ACTIVESESSIONTOKEN"].ToString();

                            model.LastActivityTime = rdr["LASTLOGINTIME"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(rdr["LASTLOGINTIME"]);
                        }
                    }
                }
            }

            return model;
        }


        public void UpdateSessionToken(string username, string token)
        {
            using (SqlConnection con = new SqlConnection(GetConfig()))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SP_CPPC_REGISTER_LOGIN", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ACTION", "UPDATE_SESSION_TOKEN");
                cmd.Parameters.AddWithValue("@USERNAME", username);
                cmd.Parameters.AddWithValue("@SESSIONTOKEN", token);

                cmd.ExecuteNonQuery();
            }
        }

        public void ClearSessionToken(string username)
        {
            using (SqlConnection con = new SqlConnection(GetConfig()))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SP_CPPC_REGISTER_LOGIN", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ACTION", "CLEAR_SESSION_TOKEN");
                cmd.Parameters.AddWithValue("@USERNAME", username);

                cmd.ExecuteNonQuery();
            }
        }


        //============================================================================

        private bool ColumnExists(SqlDataReader rdr, string columnName)
        {
            try
            {
                return rdr.GetOrdinal(columnName) >= 0;
            }
            catch (IndexOutOfRangeException)
            {
                return false;
            }
        }


        public LoginModel ValidateLogin1(LoginModel model)
        {
            try
            {
               
                string encryptedInputPassword = CryptoHelper.Encrypt(model.Password);

                var resultModel = GetUserByUsername1(model.Username);
                if (resultModel == null)
                    return null;

             
                if (encryptedInputPassword == resultModel.Password)
                {
                    return new LoginModel
                    {
                        Username = resultModel.Username,
                        RoleID = resultModel.RoleID,
                        StaffNo = resultModel.StaffNo,
                        StaffName = resultModel.StaffName
                    };
                }
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine($"Login validation error: {error.Message}");
            }
            return null;
        }

        public LoginModel GetUserByUsername1(string username)
        {
            var connectionString = GetConfig();
            LoginModel model = new LoginModel();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SP_CPPC_REGISTER_LOGIN", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ACTION", "V_LOGIN");
                    cmd.Parameters.AddWithValue("@USERNAME", username);
                    cmd.Parameters.AddWithValue("@PASSWORD", DBNull.Value);
                    cmd.Parameters.AddWithValue("@ROLEID", DBNull.Value);

                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        model.Username = rdr["USERNAME"].ToString();
                        model.Password = rdr["PSW"].ToString();
                        model.StaffName = rdr["StaffName"].ToString();
                        model.StaffNo = rdr["StaffNo"].ToString();
                        model.RoleID = Convert.ToInt32(rdr["Role"]);
                        return model;
                    }
                }
            }
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine($"Get user error: {error.Message}");
            }
            return null;
        }

        //public string RegisterUser(RegisterModel model)
        //{
        //    var connectionString = GetConfig();
        //    try
        //    {
        //        if (GetUserByUsername(model.Username) != null)
        //            return "Username already exists";

        //        string encryptedPassword = CryptoHelper.Encrypt(model.Password);

        //        using (SqlConnection con = new SqlConnection(connectionString))
        //        {
        //            con.Open();
        //            SqlCommand cmd = new SqlCommand("SP_CPPC_REGISTER_LOGIN", con);
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.AddWithValue("@ACTION", "INSERT");
        //            cmd.Parameters.AddWithValue("@USERNAME", model.Username);
        //            cmd.Parameters.AddWithValue("@PASSWORD", encryptedPassword);
        //            cmd.Parameters.AddWithValue("@ROLEID", model.RoleID);
        //            cmd.Parameters.AddWithValue("@STAFFNO", model.StaffNo);
        //            cmd.Parameters.AddWithValue("@STAFFNAME", model.StaffName);

        //            SqlDataReader rdr = cmd.ExecuteReader();
        //            if (rdr.Read())
        //            {
        //                return rdr["Result"].ToString();
        //            }
        //        }
        //    }
        //    catch (Exception error)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Registration error: {error.Message}");
        //        return "Error: " + error.Message;
        //    }
        //    return "Unknown error occurred";
        //}

        public int ChangeUserPassword(string username, string oldPassword, string newPassword)
        {
            var connectionString = GetConfig();
            try
            {
                string encryptedOldPassword = CryptoHelper.Encrypt(oldPassword);
                string encryptedNewPassword = CryptoHelper.Encrypt(newPassword);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_CPPC_ChangeUserPassword", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", username);
                    cmd.Parameters.AddWithValue("@OldPassword", encryptedOldPassword);
                    cmd.Parameters.AddWithValue("@NewPassword", encryptedNewPassword);

                    SqlParameter resultParam = new SqlParameter("@Result", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(resultParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    return (int)resultParam.Value;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Password change error: {ex.Message}");
                return -2;
            }
        }

        public void SaveToDatabase(string dpCode, string accountNumber, string fileName, string filePath)
        {
            var connectionString = GetConfig();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO ImageUploads (DPCode, AccountNumber, FileName, FilePath, UploadDate) VALUES (@DPCode, @AccountNumber, @FileName, @FilePath, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DPCode", dpCode);
                    cmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                    cmd.Parameters.AddWithValue("@FileName", fileName);
                    cmd.Parameters.AddWithValue("@FilePath", filePath);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public List<FileDetails> GetUploadedFiles()
        {
            var connectionString = GetConfig();
            List<FileDetails> filesList = new List<FileDetails>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT PPA_DPCD, PPA_ACNO FROM UploadFiles ORDER BY PPA_DPCD";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        filesList.Add(new FileDetails
                        {
                            DPCode = reader["PPA_DPCD"].ToString(),
                            AccountNumber = reader["PPA_ACNO"].ToString()
                        });
                    }
                }
            }

            return filesList;
        }

        public List<FileDetails> GetUploadedFilesPath()
        {
            var connectionString = GetConfig();
            List<FileDetails> filesList = new List<FileDetails>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT DPCODE, FilePath,Createddate FROM UploadedFilePath ORDER BY Createddate";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        filesList.Add(new FileDetails
                        {
                            DPCode = reader["DPCODE"].ToString(),
                            FilePath = reader["FilePath"].ToString()
                        });
                    }
                }
            }

            return filesList;
        }
        public void InsertFileUpload(string fileName, string folderPath)
        {
            var connectionString = GetConfig();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_InsertFileUpload", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FileName", fileName);
                    cmd.Parameters.AddWithValue("@FolderPath", folderPath);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void InsertIntoUploadedFilePath(int dpcode, string filePath, DateTime createddate)
        {
            var connectionString = GetConfig();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO UploadedFilePath (DPCODE, FilePath,Createddate) VALUES (@DPCODE, @FilePath,@createddate)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DPCODE", dpcode);
                    command.Parameters.AddWithValue("@FilePath", filePath);
                    command.Parameters.AddWithValue("@createddate", createddate);
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public void SaveFileRecord(string DPCode, string AccountNumber)
        {
            var connectionString = GetConfig();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @" INSERT INTO UploadFiles (PPA_DPCD, PPA_ACNO) VALUES (@DPCode, @AccountNumber)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DPCode", DPCode);
                    cmd.Parameters.AddWithValue("@AccountNumber", AccountNumber);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void SaveFileRecord(string AccountNumber, string DPCode, string type)
        {
            var connectionString = GetConfig();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                            IF NOT EXISTS (SELECT 1 FROM UploadFiles WHERE PPA_DPCD = @DPCode AND PPA_ACNO = @AccountNumber AND PPA_Type = @type)
                            INSERT INTO UploadFiles (PPA_ACNO, PPA_DPCD, PPA_Type) VALUES (@AccountNumber, @DPCode, @type)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@AccountNumber", AccountNumber);
                    cmd.Parameters.AddWithValue("@DPCode", DPCode);
                    cmd.Parameters.AddWithValue("@type", type);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public List<Imageupload> GetUploadedImages()
        {
            var connectionString = GetConfig();
            var uploads = new List<Imageupload>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT AccountNumber, FileName, UploadedDate, UploadedBy, AccountType, FileSize, IsMerged
                       FROM CPPCIMAGEUPLOAD
                       ORDER BY UploadedDate DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        uploads.Add(new Imageupload
                        {
                            Accountnumber = reader["AccountNumber"].ToString(),
                            FILENAME = reader["FileName"].ToString(),
                            UPLOADEDDATE = Convert.ToDateTime(reader["UploadedDate"]),
                            UPLOADEDBY = reader["UploadedBy"].ToString(),
                            AccountType = reader["AccountType"].ToString(),
                            FILESIZE = reader["FileSize"].ToString()

                        });
                    }
                }
            }

            return uploads;
        }

        public List<FileUpload> GetFileUploads(string fileName = null)
        {
            var connectionString = GetConfig();
            List<FileUpload> uploads = new List<FileUpload>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM FileUploads";
                if (!string.IsNullOrEmpty(fileName))
                    query += " WHERE FileName LIKE @FileName";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(fileName))
                        cmd.Parameters.AddWithValue("@FileName", "%" + fileName + "%");

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            uploads.Add(new FileUpload
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                FileName = reader["FileName"].ToString(),
                                FolderPath = reader["FolderPath"].ToString(),
                                UploadDate = Convert.ToDateTime(reader["UploadDate"])
                            });
                        }
                    }
                }
            }
            return uploads;
        }
        public List<Imageupload> GetUploadedImages1()
        {
            var images = new List<Imageupload>();
            var connectionString = GetConfig();
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand("sp_GetUploadedImages", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var image = new Imageupload
                            {
                                Accountnumber = reader["Accountnumber"].ToString(),
                                //PPANO = reader["PPANO"].ToString(),
                                UPLOADEDDATE = Convert.ToDateTime(reader["UPLOADEDDATE"]),
                                UPLOADEDBY = reader["UPLOADEDBY"].ToString(),
                                AccountType = reader["ACCOUNTTYPE"].ToString(),
                                FILESIZE = reader["FILESIZE"].ToString()
                            };

                            images.Add(image);
                        }
                    }
                }
            }

            return images;
        }
        public string InsertOrUpdatePMSImageUploadinjpg(Imageupload model, string action)
        {
            var connectionString = GetConfig();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_INSERT_UPDATE_PMSIMAGEUPLOAD", con);
                    cmd.CommandType = CommandType.StoredProcedure;


                    cmd.Parameters.AddWithValue("@ID", model.ID);

                    cmd.Parameters.AddWithValue("@Accountnumber", model.Accountnumber);
                    cmd.Parameters.AddWithValue("@FILENAME", model.FILENAME);
                    cmd.Parameters.AddWithValue("@UPLOADEDDATE", model.UPLOADEDDATE);

                    cmd.Parameters.AddWithValue("@STATUS", model.Status);

                    cmd.Parameters.AddWithValue("@Action", action);


                    if (!string.IsNullOrEmpty(model.PPANO))
                    {
                        cmd.Parameters.AddWithValue("@PPANO", model.PPANO);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@PPANO", DBNull.Value);
                    }
                    if (!string.IsNullOrEmpty(model.UPLOADEDBY))
                    {
                        cmd.Parameters.AddWithValue("@UPLOADEDBY", model.UPLOADEDBY);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@UPLOADEDBY", DBNull.Value);
                    }
                    if (!string.IsNullOrEmpty(model.APPROVEDBY))
                    {
                        cmd.Parameters.AddWithValue("@APPROVEDBY", model.APPROVEDBY);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@APPROVEDBY", DBNull.Value);
                    }

                    if (!string.IsNullOrEmpty(model.AccountType))
                    {
                        cmd.Parameters.AddWithValue("@ACCOUNTTYPE", model.AccountType);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ACCOUNTTYPE", DBNull.Value);
                    }


                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Success";
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }

        }
        public string InsertOrUpdatePMSImageUpload(Imageupload model, string action)
        {

            var connectionString = GetConfig();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("SP_INSERT_UPDATE_PMSIMAGEUPLOAD", con);
                    cmd.CommandType = CommandType.StoredProcedure;


                    cmd.Parameters.AddWithValue("@ID", model.ID);
                    cmd.Parameters.AddWithValue("@FILESIZE", model.FILESIZE);
                    cmd.Parameters.AddWithValue("@DPCODE", model.DPCODE);
                    cmd.Parameters.AddWithValue("@Accountnumber", model.Accountnumber);
                    cmd.Parameters.AddWithValue("@FILENAME", model.FILENAME);
                    cmd.Parameters.AddWithValue("@UPLOADEDDATE", model.UPLOADEDDATE);

                    cmd.Parameters.AddWithValue("@STATUS", model.Status);

                    cmd.Parameters.AddWithValue("@Action", action);


                    if (!string.IsNullOrEmpty(model.PPANO))
                    {
                        cmd.Parameters.AddWithValue("@PPANO", model.PPANO);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@PPANO", DBNull.Value);
                    }
                    if (!string.IsNullOrEmpty(model.UPLOADEDBY))
                    {
                        cmd.Parameters.AddWithValue("@UPLOADEDBY", model.UPLOADEDBY);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@UPLOADEDBY", DBNull.Value);
                    }
                    if (!string.IsNullOrEmpty(model.APPROVEDBY))
                    {
                        cmd.Parameters.AddWithValue("@APPROVEDBY", model.APPROVEDBY);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@APPROVEDBY", DBNull.Value);
                    }

                    if (!string.IsNullOrEmpty(model.AccountType))
                    {
                        cmd.Parameters.AddWithValue("@ACCOUNTTYPE", model.AccountType);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ACCOUNTTYPE", DBNull.Value);
                    }


                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Success";
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }

        }
        public List<PMSImageUpload> GetUploadedImagesByAccountType(string accountType)
        {
            List<PMSImageUpload> list = new List<PMSImageUpload>();
            var connectionString = GetConfig();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_GetUploadedImagesByAccountType", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@type", accountType);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
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
            return list;
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
        public List<AccountType> GETACCOUNTTYPE()
        {
            var connectionString = GetConfig();
            List<AccountType> empList = new List<AccountType>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GETACCOUNTTYPE", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            empList.Add(new AccountType
                            {
                                DESCRIPTION = dr["DESCRIPTION"].ToString(),
                                VALUE = dr["VALUE"].ToString()
                            });
                        }
                    }
                }
            }

            return empList;
        }
        public int ChangeUserPassword(string username, string oldPassword, string newPassword, string newSalt)
        {
            var connectionString = GetConfig();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SP_ChangeUserPassword", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserName", username);
                cmd.Parameters.AddWithValue("@OldPassword", oldPassword);
                cmd.Parameters.AddWithValue("@NewPassword", newPassword);
                cmd.Parameters.AddWithValue("@NewSalt", newSalt);

                SqlParameter resultParam = new SqlParameter("@Result", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(resultParam);

                conn.Open();
                cmd.ExecuteNonQuery();

                return (int)resultParam.Value;
            }
        }

        public bool IsUsernameExists(string username)
        {
            var connectionString = GetConfig();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(1) FROM Cppc_UserMaster WHERE USERNAME = @USERNAME";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@USERNAME", username);
                    connection.Open();
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public bool IsStaffNoExists(string staffNo)
        {
            var connectionString = GetConfig();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(1) FROM Cppc_UserMaster WHERE StaffNo = @StaffNo";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StaffNo", staffNo);
                    connection.Open();
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public bool RegisterUser(UserModel model, string encryptedPassword, string hashedPassword)
        {
            var connectionString = GetConfig();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Cppc_UserMaster 
                                (USERNAME, PSW, Role, StaffNo, StaffName, CreatedDate) 
                                VALUES 
                                (@USERNAME, @PSW, @Role, @StaffNo, @StaffName, GETDATE())";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@USERNAME", model.USERNAME);
                    command.Parameters.AddWithValue("@PSW", encryptedPassword);
                    command.Parameters.AddWithValue("@Role", model.Role);
                    command.Parameters.AddWithValue("@StaffNo", model.StaffNo);
                    command.Parameters.AddWithValue("@StaffName", model.StaffName);

                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }


        public class FileDetails
        {
            public string DPCode { get; set; }
            public string AccountNumber { get; set; }
            public string FolderName => $"{DPCode}-{AccountNumber}";
            public string FilePath { get; set; }
        }
    }

    public static class CryptoHelper
    {
        private static readonly byte[] aesKey = Encoding.UTF8.GetBytes("ThisIsA32ByteLongSecretKey123456");
        private static readonly byte[] aesIV = Encoding.UTF8.GetBytes("1234567890123456");

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesIV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Encryption failed", ex);
            }
        }

        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
                return string.Empty;

            try
            {
                string sanitized = HttpUtility.UrlDecode(encryptedText ?? "")
                    .Replace(" ", "+")
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Replace("%2F", "/")
                    .Replace("%2B", "+")
                    .Replace("%3D", "=")
                    .Replace("$", "")
                    .Trim();

                int mod4 = sanitized.Length % 4;
                if (mod4 > 0)
                    sanitized += new string('=', 4 - mod4);

                byte[] cipherBytes = Convert.FromBase64String(sanitized);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesIV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (MemoryStream ms = new MemoryStream(cipherBytes))
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (StreamReader reader = new StreamReader(cs, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        //public static class CryptoHelper
        //{
        //    private static readonly byte[] aesKey = Encoding.UTF8.GetBytes("ThisIsA32ByteLongSecretKey123456");
        //    private static readonly byte[] aesIV = Encoding.UTF8.GetBytes("1234567890123456");


        //    public static string Encrypt(string plainText)
        //    {
        //        if (string.IsNullOrEmpty(plainText))
        //            return string.Empty;

        //        try
        //        {
        //            using (Aes aes = Aes.Create())
        //            {
        //                aes.Key = aesKey;
        //                aes.IV = aesIV;
        //                aes.Mode = CipherMode.CBC;
        //                aes.Padding = PaddingMode.PKCS7;

        //                using (MemoryStream ms = new MemoryStream())
        //                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        //                {
        //                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        //                    cs.Write(plainBytes, 0, plainBytes.Length);
        //                    cs.FlushFinalBlock();

        //                    return Convert.ToBase64String(ms.ToArray());
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            throw new Exception("Encryption failed", ex);
        //        }
        //    }


        //    public static string Decrypt(string encryptedText)
        //    {
        //        if (string.IsNullOrWhiteSpace(encryptedText))
        //            throw new ArgumentNullException(nameof(encryptedText));

        //        try
        //        {

        //            string sanitized = HttpUtility.UrlDecode(encryptedText ?? "")
        //                .Replace(" ", "+")      // Space often replaces '+' in form/query POSTs
        //                .Replace("\n", "")      // Remove newlines
        //                .Replace("\r", "")      // Remove carriage returns
        //                .Replace("%2F", "/")    // Sometimes slashes get doubly encoded
        //                .Replace("%2B", "+")    // Sometimes '+' gets double-encoded
        //                .Replace("%3D", "=")    // '=' padding
        //                .Replace("$", "")       // Strip leading $
        //                .Trim();


        //            int mod4 = sanitized.Length % 4;
        //            if (mod4 > 0)
        //                sanitized += new string('=', 4 - mod4);

        //            byte[] cipherBytes = Convert.FromBase64String(sanitized);

        //            using (Aes aes = Aes.Create())
        //            {
        //                aes.Key = aesKey;
        //                aes.IV = aesIV;
        //                aes.Mode = CipherMode.CBC;
        //                aes.Padding = PaddingMode.PKCS7;

        //                using (MemoryStream ms = new MemoryStream(cipherBytes))
        //                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
        //                using (StreamReader reader = new StreamReader(cs, Encoding.UTF8))
        //                {
        //                    return reader.ReadToEnd(); 
        //                }
        //            }
        //        }
        //        catch (FormatException fe)
        //        {
        //            throw new Exception("The encrypted text is not a valid Base64 string.", fe);
        //        }
        //        catch (CryptographicException ce)
        //        {
        //            throw new Exception("Decryption failed. Possibly due to incorrect key/IV or corrupted input.", ce);
        //        }
        //    }


        //    public static void TestEncryption()
        //    {
        //        string testText = "HelloWorld123";

        //        string encrypted = Encrypt(testText);
        //        string decrypted = Decrypt(encrypted);

        //        Console.WriteLine($"Original: {testText}");
        //        Console.WriteLine($"Encrypted: {encrypted}");
        //        Console.WriteLine($"Decrypted: {decrypted}");
        //        Console.WriteLine($"Match: {testText == decrypted}");
        //    }
        //}
    }
}