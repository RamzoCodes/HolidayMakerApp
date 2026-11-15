#Holiday Maker - Bokningssystem

En C#-konsolapplikation kopplad till en MySQL-databas för att hantera kunder, rumssökningar och rumsbokningar.

## Förutsättningar
- .NET SDK installerat
- MySQL Server igång lokalt
- En databasklient

##Instruktioner för att skapa databasen och köra programmet

1. **Skapa databas och lägg in testdata**
Öppna filen `holidaymaker.sql` i din databasklient. Kör skriptet för att skapa databasen `HolidayMaker`, de fyra tabellerna (`Costumers`, `Hotels`, `Rooms`, `Bookings`) samt all testdata (5 hotell, 20 rum, 5 kunder och 25 bokningar i december 2026).
   *(Om skriptet inte har `CREATE DATABASE IF NOT EXISTS HolidayMaker; USE HolidayMaker;` högst upp, kör de två raderna först innan tabellerna skapas).*

2. **Konfigurera databaskopplingen i C#:**
   Öppna filen `Database.cs` och kontrollera `connectionString`:
   `server=127.0.0.1;port=3306;uid=root;pwd=kurslosenord;database=HolidayMaker;...`
   Ändra `uid` och `pwd` om din lokala MySQL-server använder ett annat användarnamn eller lösenord.

3. **Starta applikationen:**
   Öppna en terminal i projektmappen och kör:
   ```bash
   dotnet run