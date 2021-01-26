using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Earth2.io.Data
{
    public class ErrorRepository
    {
        private static SqlConnection SqlConnection { get; set; }
        private static string ConnectionString { get; set; }

        //this is called on start up
        public static void SetConnection(string connectionString)
        {
            SqlConnection = new SqlConnection();
            ConnectionString = connectionString;
        }

        public string LogError(string referralCode, string error)
        {
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var insertErrorCommand = $@"insert into ErrorLogs
                                             values(NEWID(), @referralCode, '{error}', GETDATE())";

                    using (var command = new SqlCommand(insertErrorCommand, SqlConnection))
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
                return $"DB Call Failed in LogError function in the insertErrorCommand: {e}";
            }

            return "Error Logged Successfully";
        }

        public bool LogIpAndCheckForDDOS(string ipAddress)
        {
            var tooManyRequests = false;
            try
            {
                using (SqlConnection)
                {
                    SqlConnection.ConnectionString = ConnectionString;
                    SqlConnection.Open();

                    var insertErrorCommand = $@"insert into IpLog
                                            values(@ipAddress,GETDATE())

                                            select count(IpAddress) from IpLog 
                                            where IpAddress = '' 
                                            and DATEDIFF(minute, CreatedOn, GETDATE()) < 5";

                    using (var command = new SqlCommand(insertErrorCommand, SqlConnection))
                    {
                        command.Parameters.Add("@ipAddress", SqlDbType.VarChar);
                        command.Parameters["@ipAddress"].Value = ipAddress;

                        using (var reader = command.ExecuteReader())
                        {
                            var requests = reader[0].ToString();

                            while (reader.Read())
                            {
                                //this returns number of requests per 5 minutes
                                //we check for less than 75 since the UI will send a request about every 5 seconds
                                if (int.Parse(requests) > 75)
                                {
                                    tooManyRequests = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogError("DDoS Check", $"DB Call Failed in LogError function in the insertErrorCommand: {e}");
                return tooManyRequests;
            }

            return tooManyRequests;
        }
    }
}
