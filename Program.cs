using System;

namespace HolidayMaker
{
    class Program
    {
        static void Main(string[] args)
        {
            //Creates object of Database class so we can use methods
            Database db = new Database();

            // Stops program if TestConnection is false
            if (!db.TestConnection())
            {
                return;
            }
        }
    }
}