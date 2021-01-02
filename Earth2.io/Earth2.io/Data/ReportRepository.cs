using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Earth2.io.Data
{
    public class ReportRepository
    {
        private static SqlConnection SqlConnection { get; set; }
        private static string ConnectionString { get; set; }

        //this is called on start up
        public static void SetConnection(string connectionString)
        {
            SqlConnection = new SqlConnection();
            ConnectionString = connectionString;
        }

        public static string InsertReportRecord(string userReportingId, string userBeingReportedId)
        {
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var insertReportCommand = $@"insert into UsersReported
                                                values('{userBeingReportedId}', '{userReportingId}', GETDATE())";

                    using (var command = new SqlCommand(insertReportCommand, SqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //do nothing
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return $"DB Call Failed in InsertReportRecord function in the insertReportCommand: {e}";
            }

            return "Record Insert Succeeded.";
        }

        public static string GetNumberOfReportsByUser(string userId)
        {
            var timesReported = "";

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var getReportsCommand = $@"select count(*) from UsersReported where UserReported = '{userId}'";

                    using (var command = new SqlCommand(getReportsCommand, SqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                timesReported = reader[0].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return $"DB Call Failed in GetNumberOfReportsByUser function in the getReportsCommand: {e}";
            }

            return timesReported;
        }

        public static string InsertBanRecord(string userId)
        {
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var banUserCommand = $@"insert into BannedUsers
                                            values('{userId}', GETDATE())";

                    using (var command = new SqlCommand(banUserCommand, SqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //do nothing
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return $"DB Call Failed in InsertBanRecord function in the banUserCommand: {e}";
            }

            return $"{userId} has successfully be inserted into the banned table.";
        }
    }    
}
