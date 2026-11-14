using System;
using MySqlConnector;

namespace HolidayMaker
{
    public class BookingService
    {
        private Database db = new Database();

        //Keeps track of which costumer is currently selected
        public long ActiveCostumerId = 0;
        public string ActiveCostumerName = "Ingen vald";

        //Handles costumer selection and registration. This is the first option of the menu.
        public void HandleCostumer()
        {
            Console.WriteLine("Kundhantering");
            Console.WriteLine("1. Välj befintlig kund");
            Console.WriteLine("2. Registrera ny kund");
            Console.WriteLine("Välj ett alternativ (1-2)");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                SelectCostumer();
            }
            else if (choice == "2")
            {
                Console.WriteLine("Registrera ny kund");
            }
            else
            {
                Console.WriteLine("Ogiltigt val. Vänligen välj en av valen som visas.");
                return;
            }

        }
        // Fetches all costumers from the database and allows us to select individuals by ID
        public void SelectCostumer()
        {
            using (MySqlConnection conn = db.GetConnection())
            {
                conn.Open();

                //pulls all costumers from the sql table "Costumers"
                string sql = "SELECT id, first_name, last_name, phone_number, email FROM Costumers;";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    Console.WriteLine("Existerande kunder");

                    //Loops through results and prints each costumer out.
                    while (reader.Read())
                    {
                        Console.WriteLine($"ID: {reader["id"]} | Namn:{reader["first_name"]} {reader["last_name"]} | Telefon: {reader["phone_number"]}");
                    }
                } //printer closes here so we can run a new query

                Console.Write("Skriv in ID på kundern du vill välja:");
                string input = Console.ReadLine();

                //Attempts to Convert string input to a number to prevent crashes
                if (long.TryParse(input, out long chosenID))
                {
                    //prepares SQL query to find the specific costumer.
                    string checkSql = "SELECT first_name, last_name FROM Costumers WHERE id = @id;";
                    using (MySqlCommand checkCmd = new MySqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@id", chosenID);

                        using (MySqlDataReader checkReader = checkCmd.ExecuteReader())
                        {
                            //If the ID exists in the database, set them as the active costumer
                            if (checkReader.Read())
                            {
                                ActiveCostumerId = chosenID;
                                ActiveCostumerName = $"{checkReader["first_name"]} {checkReader["last_name"]}";
                                Console.WriteLine($"Du har valt kund: {ActiveCostumerName} (ID: {ActiveCostumerId})");
                            }
                            else
                            {
                                Console.WriteLine("Felaktig inmatning. Du måste skriva en siffra.");
                            }
                        }
                    }
                }
            }
        }
    }
}