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

            Console.Write("Ange antal gäster: ");
            if (!int.TryParse(Console.ReadLine(), out int guests))
            {
                Console.WriteLine("Felaktig inmatning. Antal gäster måste vara en siffra.");
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

            using (MySqlConnection conn = db.GetConnection())
            {
                conn.Open();

                string sql = @"
                SELECT Hotels.name AS hotel_name, Hotels.city, Rooms.id AS room_id, Rooms.room_number, Rooms.max_capacity, Rooms.price_per_night
                FROM Rooms 
                INNER JOIN Hotels ON Rooms.hotels_id = Hotels.id
                WHERE Hotels.city = @city
                AND Rooms.max_capacity >= @guests
                AND NOT EXISTS (
                SELECT 1 FROM Bookings
                WHERE Bookings.room_id = Rooms.id
                AND (Bookings.check_in_date < @checkOutDate AND Bookings.check_out_date > @checkInDate));";

                //Creates the command to run SQL query
                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    //Parameterize user input to prevent SQL-injection
                    cmd.Parameters.AddWithValue("@city", city);
                    cmd.Parameters.AddWithValue("@guests", guests);

                    //Format Datetime objects to strings that MySQL understands.
                    cmd.Parameters.AddWithValue("@checkInDate", checkInDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@checkOutDate", checkOutDate.ToString("yyyy-MM-dd"));

                    //reader reads the results row by row
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine($"Sökresultat för {guests} gäster i {city} ({checkInDate:yyyy-MM-dd} till {checkOutDate:yyyy-MM-dd})");


                        //will be true if rooms have been found matching the search
                        bool foundRooms = false;

                        while (reader.Read())
                        {
                            foundRooms = true;

                            Console.WriteLine($"Rum-ID: {reader["room_id"]} | Hotell: {reader["hotel_name"]} | Rum: {reader["room_number"]} | Max gäster: {reader["max_capacity"]} | Pris: {reader["price_per_night"]} kr/natt");
                        }
                        if (!foundRooms)
                        {
                            Console.WriteLine("Tyvärr matchade inga rum din sökning för dessa datum.");
                        }
                    }
                }
            }
        }
        //Creates new booking for active customer
        public void CreateBooking()
        {
            Console.WriteLine("Skapa en bokning");

            //checks if costumer is selected, error message is given if not.
            if (ActiveCostumerId == 0)
            {
                Console.WriteLine("Ingen kund är vald. Vänligen välj eller registrera en kund i menyval 1 först.");
                return;
            }

            Console.WriteLine($"Bokar för kund: {ActiveCostumerName}");

            //Below are different inputs being asked for such as room ID, amount of people, check-in and check-out.
            Console.Write("Ange Rum-ID: ");
            if (!long.TryParse(Console.ReadLine(), out long roomId))
            {
                Console.WriteLine("Felaktig inmatning. Rum-ID måste vara en siffra.");
                return;
            }

            Console.Write("Ange antal gäster: ");
            if (!int.TryParse(Console.ReadLine(), out int guests))
            {
                Console.WriteLine("Felaktig inmatning. Antal gäster måste vara en siffra.");
                return;
            }

            Console.Write("Ange incheckningsdatum (ÅÅÅÅ-MM-DD): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkInDate))
            {
                Console.WriteLine("Felaktigt datumformat. Vänligen använd ÅÅÅÅ-MM-DD.");
                return;
            }

            Console.Write("Ange utcheckningsdatum (ÅÅÅÅ-MM-DD): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime checkOutDate))
            {
                Console.WriteLine("Felaktigt datumformat. Vänligen använd ÅÅÅÅ-MM-DD.");
                return;
            }

            if (checkOutDate <= checkInDate)
            {
                Console.WriteLine("Utcheckningsdatumet måste vara minst en dag efter incheckningsdatumet.");
                return;
            }
            //Calculates the number of nights the costumer is staying
            int totalNights = (checkOutDate - checkInDate).Days;
            using (MySqlConnection conn = db.GetConnection())
            {
                conn.Open();

                //We ask the database for the room's price and capacity if it is free during the given dates.
                string checkSql = @"
                SELECT price_per_night, max_capacity
                FROM Rooms
                WHERE id = @roomId
                AND max_capacity >= @guests
                AND NOT EXISTS (
                SELECT 1 FROM Bookings
                WHERE room_id = @roomId
                AND (check_in_date < @checkOutDate AND check_out_date > @checkInDate));";
                int pricePerNight = 0;
                bool isAvailable = false;

                using (MySqlCommand checkCmd = new MySqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@roomId", roomId);
                    checkCmd.Parameters.AddWithValue("@guests", guests);
                    checkCmd.Parameters.AddWithValue("@checkInDate", checkInDate.ToString("yyyy-MM-dd"));
                    checkCmd.Parameters.AddWithValue("@checkOutDate", checkOutDate.ToString("yyyy-MM-dd"));

                    using (MySqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        //if Read() is true, the room is both available and big enough
                        if (reader.Read())
                        {
                            isAvailable = true;

                            //grabs the price from the database and converts it to integer.
                            pricePerNight = Convert.ToInt32(reader["price_per_night"]);
                        }
                    }
                }
                //If room isn't available for any of the reasons given below, the booking is aborted.
                if (!isAvailable)
                {
                    Console.WriteLine("Bokningen misslyckades. Antingen existerar rummet inte, är för litet eller upptagen under dessa datum.");
                    return;
                }

                // Calculates the total price
                int totalPrice = pricePerNight * totalNights;

                //Query to insert new booking and retrieve auto-generated ID
                string insertSql = @"
                INSERT INTO Bookings (costumer_id, room_id, check_in_date, check_out_date, guests_amount, total_price)
                VALUES (@costumerId, @roomId, @checkInDate, @checkOutDate, @guests, @totalPrice);
                SELECT LAST_INSERT_ID();";

                //Execute insert command using parameterized queries for security
                using (MySqlCommand insertCmd = new MySqlCommand(insertSql, conn))
                {
                    insertCmd.Parameters.AddWithValue("@costumerId", ActiveCostumerId);
                    insertCmd.Parameters.AddWithValue("@roomId", roomId);
                    insertCmd.Parameters.AddWithValue("@checkInDate", checkInDate.ToString("yyyy-MM-dd"));
                    insertCmd.Parameters.AddWithValue("@checkOutDate", checkOutDate.ToString("yyyy-MM-dd"));
                    insertCmd.Parameters.AddWithValue("@guests", guests);
                    insertCmd.Parameters.AddWithValue("@totalPrice", totalPrice);

                    //runs the query and returns new booking ID
                    long bookingId = Convert.ToInt64(insertCmd.ExecuteScalar());

                    //prints booking receipt.
                    Console.WriteLine("Bokningsbekräftelse");
                    Console.WriteLine($"Bokningsnummer: {bookingId}");
                    Console.WriteLine($"kund: {ActiveCostumerName}");
                    Console.WriteLine($"Rum-ID: {roomId}");
                    Console.WriteLine($"Period: {checkInDate:yyyy-MM-dd} till {checkOutDate:yyyy-MM-dd} ({totalNights} nätter)");
                    Console.WriteLine($"Totalpris: {totalPrice} kr");
                }
            }
        }

        //fetches and displays bookings for active costumer.
        public void ShowCostumerBookings()
        {
            Console.WriteLine("Visa kundens bokningar");

            //makes sure costumer is selected.
            if (ActiveCostumerId == 0)
            {
                Console.WriteLine("Ingen kund är vald. Vänligen välj en kund i menyn.");
                return;
            }

            Console.WriteLine($"Bokningar för: {ActiveCostumerName} (Kund-ID: {ActiveCostumerId})");

            using (MySqlConnection conn = db.GetConnection())
            {
                conn.Open();

                //Selects all bookings tied to active costumer.
                string sql = @"
                SELECT id, room_id, check_in_date, check_out_date, guests_amount, total_price
                FROM Bookings
                WHERE costumer_id = @costumerId;";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@costumerId", ActiveCostumerId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool hasBookings = false;

                        //prints all found bookings
                        while (reader.Read())
                        {
                            hasBookings = true;
                            DateTime checkIn = Convert.ToDateTime(reader["check_in_date"]);
                            DateTime checkOut = Convert.ToDateTime(reader["check_out_date"]);

                            Console.WriteLine($"Boknings-ID: {reader["id"]} | Rum: {reader["room_id"]} | Period: {checkIn:yyyy-MM-dd} -> {checkOut:yyyy-MM-dd} | Pris: {reader["total_price"]} kr");
                        }
                        //if no booking is found
                        if (!hasBookings)
                        {
                            Console.WriteLine("Kunden har inga registrerade bokningar.");
                        }
                    }
                }
            }
        }

        //allows user to update guest count on one of their existing bookings.
        public void UpdateBookingGuests()
        {
            Console.WriteLine("Ändra antal gäster i en bokning");

            if (ActiveCostumerId == 0)
            {
                Console.WriteLine("Ingen kund är vald. Vänligen välj eller registrera en kund i menyval 1 först.");
                return;
            }

            //ask for specific Booking ID to update
            Console.Write("Ange det Boknings-ID du vill ändra: ");
            if (!long.TryParse(Console.ReadLine(), out long bookingId))
            {
                Console.WriteLine("Felaktig inmatning. Boknings-ID;n måste vara en siffra.");
                return;
            }

            //ask for new guest count
            Console.Write("Ange det nya antalet gäster: ");
            if (!int.TryParse(Console.ReadLine(), out int newGuestAmount))
            {
                Console.WriteLine("Felaktig inmatning. Antal gäster måste vara en siffra.");
                return;
            }

            using (MySqlConnection conn = db.GetConnection())
            {
                conn.Open();

                //UPDATE query. WHERE prevents updating other costumers bookings.
                string sql = @"
                UPDATE Bookings
                SET guests_amount = @newGuests
                WHERE id = @bookingId AND costumer_id = @costumerId;";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@newGuests", newGuestAmount);
                    cmd.Parameters.AddWithValue("@bookingId", bookingId);
                    cmd.Parameters.AddWithValue("@costumerId", ActiveCostumerId);

                    //returns how many rows were updated in the database
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"Lyckades! Bokning #{bookingId} har nu uppdaterats till {newGuestAmount} gäster.");
                    }
                    else
                    {
                        Console.WriteLine("Misslyckades. Hittade ingen bokning med ID:t för denna kund.");
                    }
                }
            }
        }
    }
}
        