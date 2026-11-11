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

            //if the program is still running

            // Keeps the menu running until we exit it (false).
            bool running = true; 
            while(running)
            {
                Console.WriteLine("\n=== Välkommen till Holiday Maker");
                Console.WriteLine("1. Registrera/välj kund");
                Console.WriteLine("2. Sök efter lediga rum");
                Console.WriteLine("3. Skapa en bokning");
                Console.WriteLine("4. Visa kundens bokningar");
                Console.WriteLine("5. Ändra antal personer i en bokning");
                Console.WriteLine("6. Avboka");
                Console.WriteLine("0. Avsluta");
                Console.Write("\nVälj ett alternativ (0-6): ");

                string val = Console.ReadLine();
            }
        }
    }
}