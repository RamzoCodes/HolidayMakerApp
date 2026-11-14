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
                RegisterCostumer();
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
                                Console.WriteLine("Felaktig inmatning. ingen kund med det ID:t hittades..");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Felaktig inmatning. Du måste skriva en siffra");
                }
            }
        }
        // Method to register a new costumer into the database
        public void RegisterCostumer()
        {
            Console.WriteLine("Registrera ny kund");

            Console.Write("Förnamn: ");
            string firstName = Console.ReadLine();

            Console.Write("Efternamn: ");
            string lastName = Console.ReadLine();

            Console.Write("Telefonnummer: ");
            string phoneNumber = Console.ReadLine();

            Console.Write("Email: ");
            string email = Console.ReadLine();

            //Error message if fist and/or last name is left empty.
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                Console.WriteLine("Förnamn och efternamn måste fyllas i.");
                return;

            }
            using (MySqlConnection conn = db.GetConnection())
            {
                conn.Open();
                //INSERT query which also pulls the new ID which was created
                string sql = @"INSERT INTO Costumers (first_name, last_name, phone_number, email) 
                VALUES (@firstName, @lastName, @phone, @email); 
                SELECT LAST_INSERT_ID();";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@firstName", firstName);
                    cmd.Parameters.AddWithValue("@lastName", lastName);
                    cmd.Parameters.AddWithValue("@phone", phoneNumber);
                    cmd.Parameters.AddWithValue("@email", email);

                    //ExecuteScalar runs the command and returns the new ID
                    long newId = Convert.ToInt64(cmd.ExecuteScalar());

                    //Sets the new costumer as "Active"
                    ActiveCostumerId = newId;
                    ActiveCostumerName = $"{firstName} {lastName}";

                    Console.WriteLine($"Kunden {ActiveCostumerName} har registrerats och valts. (Kund-ID: {ActiveCostumerId})");
                }
            }
        }

        //Method which searches for avaliable rooms based on city, dates and capacity.
        public void SearchRooms()
        {
            Console.WriteLine("Sök efter lediga rum");

            Console.Write("Vilken ort vill du välja?: ");
            string city = Console.ReadLine();

            Console.Write("Ange antal personer: ");
            if (!int.TryParse(Console.ReadLine(), out int guests))
            {
                Console.WriteLine("Felaktig inmatning. Antal personer måste vara en siffra.");
                return;

            }
            Console.Write("Incheckningsdatum (ÅÅÅÅ-MM-DD): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkInDate))
            {
                Console.WriteLine("Felaktigt datumformat. Vänligen använd ÅÅÅ-MM-DD");
                return;
            }

             Console.Write("utcheckningsdatum (ÅÅÅÅ-MM-DD): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkOutDate))
            {
                Console.WriteLine("Felaktigt datumformat. Vänligen använd ÅÅÅ-MM-DD");
                return;
            }

            //Error message in case the check-out date is on the same day or prior to check-in date.
            if (checkOutDate <= checkInDate)
            {
                Console.WriteLine("Utcheckningsdatumet måste vara minst en dag efter incheckningen.");
                return;
            }
        }
    }
}
        