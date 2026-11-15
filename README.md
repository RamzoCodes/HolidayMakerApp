#Holiday Maker - Bokningssystem

En C#-konsolapplikation kopplad till en MySQL-databas för att hantera kunder, rumssökningar och rumsbokningar.

## Instruktioner för att skapa databasen och köra programmet

1. **Databasen (`holidaymaker.sql`)**
   Kör SQL-skriptet i valfri databasklient. Skriptet skapar automatiskt databasen `HolidayMaker`, tabellstrukturen samt all nödvändig testdata (inklusive 25 bokningar för december 2026).

2. **Databasanslutning (`Database.cs`)**
   Programmet förväntar sig en lokal MySQL-instans (port 3306). 
   Uppdatera `uid` och `pwd` i anslutningssträngen så det matchar din lokala miljö.

3. **Kör programmet**
   ```bash
   dotnet run