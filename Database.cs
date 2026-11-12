using System;
using MySqlConnector;

namespace HolidayMaker
{

    // Separate class for the sake of Separation of Concerns
    public class Database 
    {

        //Sets up the connection between the database and code. 
        string connectionString = "server=127.0.0.1;port=3306;uid=root;pwd=kurslosenord;database=HolidayMaker;SslMode=Disabled;AllowPublicKeyRetrieval=True;";
            
            public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
        //Method that returns true if connection works, otherwise catches any errors.
        public bool TestConnection()
        {
            //Returns new connection which BookingService can use for queries
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    Console.WriteLine("Anslutningen till databasen lyckades!");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Anslutningen Missyckades." + ex.Message);
                    return false;
                }
            }
        }
    }
}
