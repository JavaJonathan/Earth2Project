using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Earth2.io.Data
{
    public class SearchRepository
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

        public bool InsertSearchingRecord(string referralCode)
        {
            errorRepository = new ErrorRepository();
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var insertSearchCommand = $@"DECLARE @UserId uniqueidentifier;

                                                Select @UserId = Id from AspNetUsers where Email = @referralCode

                                                insert into UsersBuying
                                                values(@UserId, GETDATE())";

                    using (var command = new SqlCommand(insertSearchCommand, SqlConnection))
                    {
                        command.Parameters.Add("@referralCode", SqlDbType.VarChar);
                        command.Parameters["@referralCode"].Value = referralCode;

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
                errorRepository.LogError(referralCode, $"DB Call Failed in InsertSearchingRecord function in the insertSearchCommand: {e}");
                return false;
            }

            return true;
        }

        public string[] FindAnotherUserSearching(string referralCode)
        {
            errorRepository = new ErrorRepository();
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
                                                where AspNetUsers.Email <> @referralCode
                                                order by CreatedOn desc";

                    using (var command = new SqlCommand(getSearchingCommand, SqlConnection))
                    {
                        command.Parameters.Add("@referralCode", SqlDbType.VarChar);
                        command.Parameters["@referralCode"].Value = referralCode;

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
                errorRepository.LogError(referralCode, $"DB Call Failed in FindAnotherUserSearching function in the getSearchingCommand: {e}");
                return new[] { "Error Occurred." };
            }

            return new[] { matchedUserReferralCode, matchedUserUsername};
        }

        public string GetNumberOfUsersSearching()
        {
            errorRepository = new ErrorRepository();
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
                errorRepository.LogError("Home Page", $"DB Call Failed in GetNumberOfUsersSearching function in the getBuyingCountCommand: {e}");
                return "Error Occurred.";
            }

            return numberOfUsersBuying;
        }

        public string CheckIfUserIsAlreadySearching(string referralCode)
        {
            errorRepository = new ErrorRepository();
            var alreadySearching = "false";

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var getAlreadySearchingCommand = $@"select * from AspNetUsers 
                                                    join UsersBuying on UsersBuying.UserId = AspNetUsers.Id
                                                    where Email = @referralCode";

                    using (var command = new SqlCommand(getAlreadySearchingCommand, SqlConnection))
                    {
                        command.Parameters.Add("@referralCode", SqlDbType.VarChar);
                        command.Parameters["@referralCode"].Value = referralCode;

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
                errorRepository.LogError(referralCode, $"DB Call Failed in CheckIfUserIsAlreadySearching function in the getAlreadySearchingCommand: { e }");
                return $"";
            }

            return alreadySearching;
        }

        public bool RemoveSearchingRecord(string referralCode)
        {
            errorRepository = new ErrorRepository();

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var deleteSearchingCommand = $@"delete from UsersBuying where UserId in (select Id from AspNetUsers where Email = @referralCode)";

                    using (var command = new SqlCommand(deleteSearchingCommand, SqlConnection))
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
                errorRepository.LogError(referralCode, $"DB Call Failed in RemoveSearchingRecord function in the deleteSearchingCommand: {e}");
                return false;
            }

            return true;
        }

        public string RemoveBothSearchingRecords(string buyerReferralCode, string matchedBuyerReferralCode)
        {
            errorRepository = new ErrorRepository();

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var deleteSearchingCommand = $@"delete from UsersBuying where UserId in (select Id from AspNetUsers where Email in (@buyerReferralCode, @matchedBuyerReferralCode))";

                    using (var command = new SqlCommand(deleteSearchingCommand, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@buyerReferralCode", buyerReferralCode);
                        command.Parameters.AddWithValue("@matchedBuyerReferralCode", matchedBuyerReferralCode);

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

        public string InsertUserMatchedRecord(string referralCode, string userMatchedReferralCode)
        {
            errorRepository = new ErrorRepository();

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var insertMatchedCommand = $@"DECLARE @UserId uniqueidentifier;
                                                DECLARE @UserMatchedId uniqueidentifier;

                                                Select @UserId = Id from AspNetUsers where Email = @referralCode
                                                Select @UserMatchedId = Id from AspNetUsers where Email = @userMatchedReferralCode

                                                insert into UsersMatched
                                                values(@UserId, @UserMatchedId, GETDATE())";

                    using (var command = new SqlCommand(insertMatchedCommand, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@referralCode", referralCode);
                        command.Parameters.AddWithValue("@userMatchedReferralCode", userMatchedReferralCode);

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
        public string[] CheckIfUserAlreadyMatched(string referralCode)
        {
            errorRepository = new ErrorRepository();

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var getAlreadyMatchedCommand = $@"DECLARE @UserId uniqueidentifier;
                                                   DECLARE @Count int;

                                                  select @UserId = UserId, @Count = @@ROWCOUNT from UsersMatched
                                                  join AspNetUsers on AspNetUsers.Id = UsersMatched.UserMatchedWith
                                                  where Email = '{referralCode}'

                                                  select @Count, Email, Username from AspNetUsers where Id = @UserId";

                    using (var command = new SqlCommand(getAlreadyMatchedCommand, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@referralCode", referralCode);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                //we wrote the wuery this way because we need the userId of the user searching 
                                //in order to get the user matched with userid and referral code
                                if (reader.HasRows)
                                {
                                    if (int.Parse(reader[0].ToString()) > 1)
                                    {
                                        return new string[] { "Error: User matched with more than one person." };
                                    }

                                    return new string[] { reader[1].ToString(), reader[2].ToString() };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errorRepository.LogError(referralCode, $"DB Call Failed in CheckIfUserAlreadyMatched function in the getAlreadyMatchedCommand: {e}");
                return new string[] { "Error Occurred." };
            }

            return new string[] { "User is not already matched." };
        }

        //this is called when the matched user finds out they were matched
        public bool RemoveUserMatchedRecord(string referralCode)
        {
            errorRepository = new ErrorRepository();

            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var deleteMatchedCommand = $@"DECLARE @UserId uniqueidentifier;

                                                  select @UserId = UserId from AspNetUsers
                                                  where Email = @referralCode

                                                  delete from UsersMatched where UserMatchedWith = @UserId";

                    using (var command = new SqlCommand(deleteMatchedCommand, SqlConnection))
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
                errorRepository.LogError(referralCode, $"DB Call Failed in RemoveUserMatchedRecord function in the deleteMatchedCommand: {e}");
                return false;
            }

            return true;
        }
    }
}
