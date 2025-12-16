using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;

public class ReportGenerator
{
    private readonly string _connectionString;
    private readonly string _reportDirectory;

    public ReportGenerator()
    {
        _connectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
        _reportDirectory = ConfigurationManager.AppSettings["ReportDirectory"] ?? @"D:\ConversionReports\";

        if (!Directory.Exists(_reportDirectory))
        {
            Directory.CreateDirectory(_reportDirectory);
        }
    }

    public string GenerateDailySummaryReport()
    {
        string reportFile = Path.Combine(_reportDirectory, $"DailySummary_{DateTime.Now:yyyyMMdd}.txt");

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();

            string summarySql = @"
                SELECT AccountType, 
                       SUM(TotalFiles) as TotalFiles,
                       SUM(ConvertedFiles) as Converted,
                       SUM(FailedFiles) as Failed,
                       COUNT(*) as TotalFolders
                FROM DummyConversionLog 
                WHERE CAST(StartTime AS DATE) = CAST(GETDATE() AS DATE)
                GROUP BY AccountType
                ORDER BY AccountType";

            var reportContent = new StringBuilder();
            reportContent.AppendLine("DAILY CONVERSION SUMMARY REPORT");
            reportContent.AppendLine("================================");
            reportContent.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            reportContent.AppendLine();

            using (var cmd = new SqlCommand(summarySql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                reportContent.AppendLine("SUMMARY BY ACCOUNT TYPE:");
                reportContent.AppendLine("-------------------------");
                while (reader.Read())
                {
                    string accountType = reader["AccountType"].ToString();
                    int totalFiles = Convert.ToInt32(reader["TotalFiles"]);
                    int converted = Convert.ToInt32(reader["Converted"]);
                    int failed = Convert.ToInt32(reader["Failed"]);
                    int folders = Convert.ToInt32(reader["TotalFolders"]);
                    double successRate = totalFiles > 0 ? (converted * 100.0 / totalFiles) : 0;

                    reportContent.AppendLine($"  {accountType}:");
                    reportContent.AppendLine($"    Folders Processed: {folders}");
                    reportContent.AppendLine($"    Total Files: {totalFiles}");
                    reportContent.AppendLine($"    Converted: {converted}");
                    reportContent.AppendLine($"    Failed: {failed}");
                    reportContent.AppendLine($"    Success Rate: {successRate:F2}%");
                    reportContent.AppendLine();
                }
            }

            string detailSql = @"
                SELECT DpCode, FolderPath, AccountType, 
                       TotalFiles, ConvertedFiles, FailedFiles, 
                       StartTime, EndTime, Status
                FROM DummyConversionLog 
                WHERE CAST(StartTime AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY AccountType, DpCode";

            reportContent.AppendLine("DETAILED FOLDER BREAKDOWN:");
            reportContent.AppendLine("---------------------------");

            using (var cmd = new SqlCommand(detailSql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string dpCode = reader["DpCode"].ToString();
                    string accountType = reader["AccountType"].ToString();
                    int totalFiles = Convert.ToInt32(reader["TotalFiles"]);
                    int converted = Convert.ToInt32(reader["ConvertedFiles"]);
                    int failed = Convert.ToInt32(reader["FailedFiles"]);
                    string status = reader["Status"].ToString();
                    DateTime startTime = Convert.ToDateTime(reader["StartTime"]);
                    DateTime? endTime = reader["EndTime"] as DateTime?;

                    reportContent.AppendLine($"  DP Code: {dpCode}");
                    reportContent.AppendLine($"  Account Type: {accountType}");
                    reportContent.AppendLine($"  Status: {status}");
                    reportContent.AppendLine($"  Files: {totalFiles} total, {converted} converted, {failed} failed");
                    reportContent.AppendLine($"  Duration: {(endTime.HasValue ? (endTime.Value - startTime).ToString(@"hh\:mm\:ss") : "In Progress")}");
                    reportContent.AppendLine($"  Folder: {reader["FolderPath"]}");
                    reportContent.AppendLine();
                }
            }

            string errorSql = @"
                SELECT fcl.AccountNumber, fcl.FileName, fcl.ErrorMessage, 
                       cl.DpCode, cl.AccountType
                FROM DummyFileConversionLog fcl
                INNER JOIN DummyConversionLog cl ON fcl.LogId = cl.LogId
                WHERE fcl.Status = 'Failed' 
                AND CAST(fcl.ConversionTime AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY cl.AccountType, cl.DpCode";

            reportContent.AppendLine("ERROR SUMMARY:");
            reportContent.AppendLine("--------------");

            using (var cmd = new SqlCommand(errorSql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                int errorCount = 0;
                while (reader.Read())
                {
                    errorCount++;
                    reportContent.AppendLine($"  Error #{errorCount}:");
                    reportContent.AppendLine($"    Account: {reader["AccountNumber"]}");
                    reportContent.AppendLine($"    File: {reader["FileName"]}");
                    reportContent.AppendLine($"    DP Code: {reader["DpCode"]}");
                    reportContent.AppendLine($"    Type: {reader["AccountType"]}");
                    reportContent.AppendLine($"    Error: {reader["ErrorMessage"]}");
                    reportContent.AppendLine();
                }

                if (errorCount == 0)
                {
                    reportContent.AppendLine("  No errors reported today.");
                    reportContent.AppendLine();
                }
            }

            File.WriteAllText(reportFile, reportContent.ToString());
            return reportFile;
        }
    }

    public string GenerateProgressSnapshot()
    {
        string snapshotFile = Path.Combine(_reportDirectory, $"ProgressSnapshot_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();

            string sql = @"
                SELECT FolderPath, DpCode, AccountType, TotalFiles, ProcessedFiles, 
                       CurrentFile, Status, LastUpdated,
                       CASE WHEN TotalFiles > 0 THEN 
                           CAST(ProcessedFiles AS FLOAT) / TotalFiles * 100 
                       ELSE 0 END as ProgressPercentage
                FROM DummyFolderProgress 
                WHERE CAST(LastUpdated AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY AccountType, DpCode";

            var snapshot = new StringBuilder();
            snapshot.AppendLine("CURRENT CONVERSION PROGRESS SNAPSHOT");
            snapshot.AppendLine("====================================");
            snapshot.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            snapshot.AppendLine();

            using (var cmd = new SqlCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    snapshot.AppendLine($"DP Code: {reader["DpCode"]}");
                    snapshot.AppendLine($"Account Type: {reader["AccountType"]}");
                    snapshot.AppendLine($"Status: {reader["Status"]}");
                    snapshot.AppendLine($"Progress: {reader["ProcessedFiles"]}/{reader["TotalFiles"]} files ({reader["ProgressPercentage"]:F1}%)");
                    snapshot.AppendLine($"Current File: {reader["CurrentFile"]}");
                    snapshot.AppendLine($"Last Updated: {Convert.ToDateTime(reader["LastUpdated"]):HH:mm:ss}");
                    snapshot.AppendLine($"Folder: {reader["FolderPath"]}");
                    snapshot.AppendLine("----------------------------------------");
                    snapshot.AppendLine();
                }
            }

            File.WriteAllText(snapshotFile, snapshot.ToString());
            return snapshotFile;
        }
    }
}