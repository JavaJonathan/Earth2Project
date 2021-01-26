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
        ErrorRepository errorRepository;


        //this is called on start up
        public static void SetConnection(string connectionString)
        {
            SqlConnection = new SqlConnection();
            ConnectionString = connectionString;
        }

        public bool InsertReportRecord(string reportingReferralCode, string reportedReferralCode)
        {
            errorRepository = new ErrorRepository();
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var insertReportCommand = $@"DECLARE @ReportingUserId uniqueidentifier;
                                                DECLARE @ReportedUserId uniqueidentifier;

                                                Select @ReportingUserId = Id from AspNetUsers where Email = @reportingReferralCode
                                                Select @ReportedUserId = Id from AspNetUsers where Email = @reportedReferralCode

                                                insert into UsersReported
                                                values(@ReportingUserId, @ReportedUserId, GETDATE())";

                    using (var command = new SqlCommand(insertReportCommand, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@reportingReferralCode", reportingReferralCode);
                        command.Parameters.AddWithValue("@reportedReferralCode", reportedReferralCode);

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
                errorRepository.LogError(reportingReferralCode, $"DB Call Failed in InsertReportRecord function in the insertReportCommand: {e}");
                return false;
            }

            return true;
        }

        public string GetNumberOfReportsByUser(string referralCode)
        {
            var timesReported = "";
            errorRepository = new ErrorRepository();

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var getReportsCommand = $@"DECLARE @UserId uniqueidentifier;

                                            Select @UserId = Id from AspNetUsers where Email = @referralCode

                                            select count(*) from UsersReported where UserReported = @UserId";

                    using (var command = new SqlCommand(getReportsCommand, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@referralCode", referralCode);

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
                errorRepository.LogError(referralCode, $"DB Call Failed in GetNumberOfReportsByUser function in the getReportsCommand: {e}");
                return "Error Occurred.";
            }

            return timesReported;
        }

        public bool InsertBanRecord(string referralCode)
        {
            errorRepository = new ErrorRepository();

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var banUserCommand = $@"DECLARE @UserId uniqueidentifier;

                                            Select @UserId = Id from AspNetUsers where Email = @referralCode

                                            insert into BannedUsers
                                            values(@UserId, GETDATE())";

                    using (var command = new SqlCommand(banUserCommand, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@referralCode", referralCode);

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
                errorRepository.LogError(referralCode, $"DB Call Failed in InsertBanRecord function in the banUserCommand: {e}");
                return false;
            }

            return true;
        }

        //we need this function to ensure a user can only report another once
        public string CheckIfUserIsAlreadyReportedByUser(string reportingReferralCode, string reportedReferralCode)
        {
            errorRepository = new ErrorRepository();

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var checkReportCommand = $@"DECLARE @ReportingUserId uniqueidentifier;
                                                DECLARE @ReportedUserId uniqueidentifier;

                                                Select @ReportingUserId = Id from AspNetUsers where Email = @reportingReferralCode
                                                Select @ReportedUserId = Id from AspNetUsers where Email = @reportedReferralCode

                                                select * from UsersReported where ReportedBy = @reportingReferralCode and UserReported = @reportedReferralCode";

                    using (var command = new SqlCommand(checkReportCommand, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@reportingReferralCode", reportingReferralCode);
                        command.Parameters.AddWithValue("@reportedReferralCode", reportedReferralCode);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.HasRows)
                                {
                                    return "true";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errorRepository.LogError(reportingReferralCode, $"DB Call Failed in CheckIfUserIsAlreadyReportedByUser function in the checkReportCommand: {e}");
                return "Error Occurred.";
            }

            return "false";
        }

        public bool IsUserBanned(string referralCode)
        {
            errorRepository = new ErrorRepository();

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var getBannedUserCommand = $@"DECLARE @UserId uniqueidentifier;

                                            Select @UserId = Id from AspNetUsers where Email = @referralCode

                                            select * from BannedUsers where UserId = @UserId)";

                    using (var command = new SqlCommand(getBannedUserCommand, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@referralCode", referralCode);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.HasRows)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errorRepository.LogError(referralCode, $"DB Call Failed in IsUserBanned function in the getBannedUserCommand: {e}");
                return false;
            }

            return false;
        }
    }    
}
