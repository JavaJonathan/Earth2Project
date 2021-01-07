using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Earth2.io.Data
{
    public class SearchRepository
    {
        private static SqlConnection SqlConnection { get; set; }
        private static string ConnectionString { get; set; }

        //this is called on start up
        public static void SetConnection(string connectionString)
        {
            SqlConnection = new SqlConnection();
            ConnectionString = connectionString;
        }

        public static string InsertSearchingRecord(string referralCode)
        {
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var insertSearchCommand = $@"DECLARE @UserId uniqueidentifier;

                                                Select @UserId = Id from AspNetUsers where Email = '{referralCode}'

                                                insert into UsersBuying
                                                values(@UserId, GETDATE())";

                    using (var command = new SqlCommand(insertSearchCommand, SqlConnection))
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
                return $"DB Call Failed in InsertSearchingRecord function in the insertSearchCommand: {e}";
            }

            return "Buying Record Insert Succeeded.";
        }

        public static string[] FindAnotherUserSearching(string referralCode)
        {
            var matchedUserReferralCode = "";
            var matchedUserUsername = "";

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var getSearchingCommand = $@"select top 1 Email, Username from UsersBuying 
                                                join AspNetUsers on AspNetUsers.Id = UsersBuying.UserId
                                                where AspNetUsers.Email <> '{referralCode}'
                                                order by CreatedOn desc";

                    using (var command = new SqlCommand(getSearchingCommand, SqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.HasRows)
                                {
                                    matchedUserReferralCode = reader[0].ToString();
                                    matchedUserUsername = reader[1].ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return new[] { $"DB Call Failed in FindAnotherUserSearching function in the getSearchingCommand: {e}" };
            }

            return new[] { matchedUserReferralCode, matchedUserUsername};
        }

        public static string GetNumberOfUsersSearching()
        {
            var numberOfUsersBuying = "";

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var getBuyingCountCommand = $@"select count(*) from UsersBuying";

                    using (var command = new SqlCommand(getBuyingCountCommand, SqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                numberOfUsersBuying = reader[0].ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return $"DB Call Failed in GetNumberOfUsersSearching function in the getBuyingCountCommand: {e}";
            }

            return numberOfUsersBuying;
        }

        public static string CheckIfUserIsAlreadySearching(string referralCode)
        {
            var alreadySearching = "false";

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var getAlreadySearchingCommand = $@"select * from AspNetUsers 
                                                join UsersBuying on UsersBuying.UserId = AspNetUsers.Id
                                                where Email = '{referralCode}'";

                    using (var command = new SqlCommand(getAlreadySearchingCommand, SqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.HasRows)
                                {
                                    alreadySearching = "true";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return $"DB Call Failed in CheckIfUserIsAlreadySearching function in the getAlreadySearchingCommand: {e}";
            }

            return alreadySearching;
        }

        public static string RemoveSearchingRecord(string referralCode)
        {
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var deleteSearchingCommand = $@"delete from UsersBuying where UserId in (select Id from AspNetUsers where Email = '{referralCode}')";

                    using (var command = new SqlCommand(deleteSearchingCommand, SqlConnection))
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
                return $"DB Call Failed in RemoveSearchingRecord function in the deleteSearchingCommand: {e}";
            }

            return "Record Deleted Successfully";
        }

        public static string RemoveBothSearchingRecords(string buyerActivationCode, string matchedBuyerActivationCode)
        {
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var deleteSearchingCommand = $@"delete from UsersBuying where UserId in (select Id from AspNetUsers where Email in ('{buyerActivationCode}', '{matchedBuyerActivationCode}'))";

                    using (var command = new SqlCommand(deleteSearchingCommand, SqlConnection))
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
                return $"DB Call Failed in RemoveBothSearchingRecords function in the deleteSearchingCommand: {e}";
            }

            return "Records Deleted Successfully";
        }

        public static string InsertUserMatchedRecord(string userId, string userMatchedId)
        {
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var insertMatchedCommand = $@"insert into UsersMatched
                                                    values('{userId}', '{userMatchedId}', GETDATE())";

                    using (var command = new SqlCommand(insertMatchedCommand, SqlConnection))
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
                return $"DB Call Failed in InsertUserMatchedRecord function in the insertMatchedCommand: {e}";
            }

            return "Matched Record Inserted Successfully";
        }

        //we will need to call this right beofre we start searching for users then another when we find a possible match 
        //we will need to check if thereis more than one record here, if so, there was a race con
        public static string CheckIfUserAlreadyMatched(string userId)
        {
            var alreadyMatched = "false";

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var getAlreadyMatchedCommand = $@"select * from UsersMatched where UserId = '{userId}' or UserMatchedWith = '{userId}'";

                    using (var command = new SqlCommand(getAlreadyMatchedCommand, SqlConnection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader.HasRows)
                                {
                                    alreadyMatched = "true";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return $"DB Call Failed in CheckIfUserAlreadyMatched function in the getAlreadyMatchedCommand: {e}";
            }

            return alreadyMatched;
        }

        //this is called when the matched user finds out they were matched
        public static string RemoveUserMatchedRecord(string userId)
        {
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var deleteMatchedCommand = $@"delete from UsersMatched where UserMatchedWith = '{userId}'";

                    using (var command = new SqlCommand(deleteMatchedCommand, SqlConnection))
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
                return $"DB Call Failed in RemoveUserMatchedRecord function in the deleteMatchedCommand: {e}";
            }

            return "Matched Record Deleted Successfully";
        }
    }
}
