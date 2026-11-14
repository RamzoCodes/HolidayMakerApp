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
            
        }
    }
}