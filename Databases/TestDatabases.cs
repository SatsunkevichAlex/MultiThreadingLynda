using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Databases
{
    [TestClass]
    public class TestDatabases
    {
        const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        const string sqlSelect = "SELECT @@VERSION";

        [TestMethod]
        public void Test_Db_Sync()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sqlSelect, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string data = reader[0].ToString();
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Test_Db_BeginAsync()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(sqlSelect, connection);
                AsyncCallback callback = new AsyncCallback(DataAvailable);
                IAsyncResult ar = command.BeginExecuteReader(callback, command);

                ar.AsyncWaitHandle.WaitOne();
            }
        }
        private static void DataAvailable(IAsyncResult ar)
        {
            SqlCommand command = ar.AsyncState as SqlCommand;
            using (SqlDataReader reader = command.EndExecuteReader(ar))
            {
                while (reader.Read())
                {
                    string version = reader[0].ToString();
                }
            }
        }

        [TestMethod]
        public async Task Test_Db_AsyncAwait()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlCommand command = new SqlCommand(sqlSelect, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            string data = reader[0].ToString();
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Test_Db_AsyncTask()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            Task taskSqlConnection = connection.OpenAsync();

            taskSqlConnection.ContinueWith((Task tx, object state) =>
            {
                SqlConnection sc = state as SqlConnection;
                Assert.IsTrue(sc.State == ConnectionState.Open);

                SqlCommand command = new SqlCommand(sqlSelect, sc);
                Task<SqlDataReader> taskDataReader = command.ExecuteReaderAsync();
                Task taskProcessData = taskDataReader.ContinueWith((Task<SqlDataReader> txx) =>
                {
                    using (SqlDataReader reader = txx.Result)
                    {
                        while (reader.Read())
                        {
                            string data = reader[0].ToString();
                        }

                        mre.Set();
                    }
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            }, connection, TaskContinuationOptions.OnlyOnRanToCompletion);

            mre.WaitOne();
        }
        ManualResetEvent mre = new ManualResetEvent(false);
    }
}
