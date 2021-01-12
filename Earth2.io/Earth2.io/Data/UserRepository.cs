using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Earth2.io.Data
{
    public class UserRepository
    {
        private static SqlConnection SqlConnection { get; set; }
        private static string ConnectionString { get; set; }

        //this is called on start up
        public static void SetConnection(string connectionString)
        {
            SqlConnection = new SqlConnection();
            ConnectionString = connectionString;
        }

        public static bool AddNewUser(string referralCode, string username)
        {
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var insertUserCommand = $@"insert into AspNetUsers
                                                values(NEWID(), @referralCode, 0, null, null, null, 0, 0, null, 0, 0, @userName)";

                    using (var command = new SqlCommand(insertUserCommand, SqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            command.Parameters.AddWithValue("@referralCode", referralCode);
                            command.Parameters.AddWithValue("@userName", username);

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
                ErrorRepository.LogError(referralCode, $"DB Call Failed in AddNewUser function in the insertUserCommand: {e}");
                return false;
            }

            return true;
        }

        //we have to return strings in these functions because we want to return string errors.
        public static string CheckIfUserExists(string referralCode)
        {
            var userExists = "false";
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var checkUserCommand = $@"select * from AspNetUsers where Email = @referralCode";

                    using (var command = new SqlCommand(checkUserCommand, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@referralCode", referralCode);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.HasRows)
                                {
                                    userExists = "true";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorRepository.LogError(referralCode, $"DB Call Failed in CheckIfUserExists function in the checkUserCommand: {e}");
                return "Error Occurred.";
            }

            return userExists;
        }

        public static bool CheckIfTwoUsersExists(string firstReferralCode, string secondReferralCode)
        {
            var bothUsersExist = false;
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var checkTwoUsersCommand = $@"select * from AspNetUsers where Email = @firstReferralCode and Email = @secondReferralCode";

                    using (var command = new SqlCommand(checkTwoUsersCommand, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@firstReferralCode", firstReferralCode);
                        command.Parameters.AddWithValue("@secondReferralCode", secondReferralCode);


                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.HasRows)
                                {
                                    bothUsersExist = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorRepository.LogError(firstReferralCode, $"DB Call Failed in CheckIfTwoUsersExists function in the checkTwoUsersCommand: {e}");
                return bothUsersExist;
            }

            return bothUsersExist;
        }

        public static bool InsertTrackingRecord(string referralCode, string trackingType, string description)
        {
            var trackingTypeId = "";

            switch (trackingType)
            {
                case "User Reported": trackingTypeId = "96999E58-ABB3-42FD-A0DE-5DD0D45D0E7D"; break;
                case "Started Searching": trackingTypeId = "032B2C5F-F983-48DD-9C89-7629BBC2C7F3"; break;
                case "Found Match": trackingTypeId = "3696C17B-451B-484A-9161-F8149FE3579E"; break;
                case "User Banned": trackingTypeId = "0ED601B8-6EC7-40FE-ABBE-5E737E9F88B1"; break;
                case "User Stopped Searching": trackingTypeId = "000A9275D-AEE5-41DA-B89E-E86E25C17895"; break;
            }

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var insertTrackerCommand = $@"insert into UserTracking
                                                values(NEWID(), @referralCode, '{trackingTypeId}', '{description}', GETDATE())";

                    using (var command = new SqlCommand(insertTrackerCommand, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@referralCode", referralCode);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorRepository.LogError(referralCode, $"DB Call Failed in InsertTrackingRecord function in the insertTrackerCommand: {e}");
                return false;
            }

            return true;
        }
    }
}
