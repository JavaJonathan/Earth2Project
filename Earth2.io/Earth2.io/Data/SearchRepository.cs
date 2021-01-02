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

        public static string InsertSearchingRecord()
        {

        }

        public static string FindAnotherUserSearching()
        {

        }

        public static string RemoveBothSearchRecords()
        {

        }

        public static string GetNumberOfUsersSearching()
        {

        }

        public static string CheckIfUserIsAlreadySearching()
        {

        }
    }
}
