using System;
using MySqlConnector;

namespace HolidayMaker
{
    class Program
    {
        static void Main(string[] args)
        {
            //Sets up the connection between the database and code. 
            string connectionString = "server=127.0.0.1;port=3306;uid=root;pwd=kurslosenord;database=HolidayMaker;SslMode=Disabled;AllowPublicKeyRetrieval=True;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    Console.WriteLine("Anslutningen till databasen lyckades!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Anslutningen Missyckades." + ex.Message);
                }
            }
        }
    }
}