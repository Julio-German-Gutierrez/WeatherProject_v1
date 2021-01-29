using System;
using System.Collections.Generic;
using System.Linq;
using WeatherProject_v1.DBQueries;

namespace WeatherProject_v1
{
    class Program
    {
        static DataServices datos = new DataServices();

        static void Main(string[] args)
        {
            Console.Clear();

            /*
             * LoadDataFromFile() will do precise that, will read the data file ("Temperatur_Data.cvs")
             * and load its data to the EF created database.
             * In order to have a way to avoid the mistake of loading the data "unintentionally",
             * I commented the line and implemented a Method-Secure-Question inside the method.
             */

            //datos.LoadDataFromFile(@"RawData/Temperatur_Data.cvs"); //Uncomment to use.

            bool control = true;
            while (control)
            {
                control = Menu();
            }

            datos.Close();
        }

        
        private static bool Menu()
        {
            Console.Clear();

            Console.WriteLine("MENU");
            Console.WriteLine("0. Svarta hål");
            Console.WriteLine("1. Medeltemperatur för valt datum");
            Console.WriteLine("2. Sortering av varmast till kallaste dagen enligt medeltemperatur per dag");
            Console.WriteLine("3. Sortering av torrast till fuktigaste dagen enligt medelluftfuktighet per dag");
            Console.WriteLine("4. Sortering av minst till störst risk för mögel");
            Console.WriteLine("5. Datum för meteorologisk Höst");
            Console.WriteLine("6. Datum för meteorologisk Vinter (OBS! Vintern 2016 var mild)");
            Console.WriteLine();
            Console.WriteLine("7. Exit");
            Console.WriteLine();
            Console.Write("Ditt val: ");
            ConsoleKeyInfo k = Console.ReadKey();

            bool ret = true;
            switch (k.KeyChar)
            {
                case '0':
                    {
                        datos.BlackHoles();
                        break;
                    }
                case '1':
                    {
                        AskDay();
                        break;
                    }
                case '2':
                    {
                        WarmestToColdest();
                        break;
                    }
                case '3':
                    {
                        DriestToWettest();
                        break;
                    }
                case '4':
                    {
                        AscendingRiskMold();
                        break;
                    }
                case '5':
                    {
                        MeteorologistHost();
                        break;
                    }
                case '6':
                    {
                        MeteorologistWinter();
                        break;
                    }
                case '7':
                    {
                        ret = false;
                        break;
                    }
                default:
                    break;
            }
            return ret;
        }


        /*
         * The next 6 methods simply present the information obtained
         * from the DataServices object, "datos".
         * They have nothing in particular. All the magic happens in the DataServices object.
         */

        private static void AscendingRiskMold()
        {
            Console.Clear();
            Console.WriteLine("minst till störst risk för mögel enligt mögelindex".ToUpper());
            Console.Write("(E)xterior / (I)nterior : ");
            char c = Console.ReadKey().KeyChar;
            Console.WriteLine("\n");

            //
            var result = datos.Up_04_MoldRisk(c);

            foreach (Dobles s in result)
            {
                Console.WriteLine($"DAY: {s.valueDateTime.Date}    MOLD INDEX: {s.valueDouble}");
            }

            
            switch (char.ToLower(c))
            {
                case 'e':
                    {
                        Console.WriteLine($"EXTERIOR REGISTRIES = {result.Count()}");
                        break;
                    }
                case 'i':
                    {
                        Console.WriteLine($"INTERIOR REGISTRIES = {result.Count()}");
                        break;
                    }
                default:
                    break;
            }
            
            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void MeteorologistHost()
        {
            Console.Clear();
            Console.WriteLine("Datum för meteorologisk Höst".ToUpper());
            Console.WriteLine();
            Console.WriteLine("Enligt SMHI:s meteorologiska definition råder höst då en dygnsmedeltemperatur\n" +
                              "på mellan 0 och + 10 grader Celsius varat i fem dygn.\n\n" +
                              "Så länge detta inträffar efter 1 augusti, säger vi att\n" +
                              "vintern anlände det första av dessa dygn.");
            Console.WriteLine();

            DateTime result = datos.Up_05_Autumn();

            if (result != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Den meteorologiska Hösten startade: {result.Date}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Den meteorologiska Hösten kunde inte beräknas.");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ReadKey(true);
        }

        private static void MeteorologistWinter()
        {
            Console.Clear();
            Console.WriteLine("Datum för meteorologisk Vinter".ToUpper());
            Console.WriteLine();
            Console.WriteLine("Enligt SMHI:s meteorologiska definition:\n" +
                              "Om dygnsmedeltemperaturen är 0,0°C eller lägre fem dygn i följd,\n" +
                              "säger vi att vintern anlände det första av dessa dygn.");
            Console.WriteLine();

            var result = datos.Up_06_Winter();

            if (result != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Den meteorologiska Vintern startade: {((DateTime)result).Date}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Den meteorologiska Vintern kunde inte beräknas.");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.ReadKey(true);
        }

        private static void DriestToWettest()
        {
            Console.Clear();
            Console.WriteLine("torrast till fuktigaste dagen enligt medelluftfuktighet per dag".ToUpper());
            Console.Write("(E)xterior / (I)nterior : ");
            char c = Console.ReadKey().KeyChar;
            Console.WriteLine("\n");

            var result = datos.Up_03_DriestToWettest(c);

            foreach (Dobles s in result)
            {
                Console.WriteLine($"DAY: {s.valueDateTime.Date,-12}    AVERAGE HUMIDITY: {s.valueDouble, 6:00.00}");
            }

            switch (char.ToLower(c))
            {
                case 'e':
                    {
                        Console.WriteLine($"EXTERIOR REGISTRIES = {result.Count()}");
                        break;
                    }
                case 'i':
                    {
                        Console.WriteLine($"INTERIOR REGISTRIES = {result.Count()}");
                        break;
                    }
                default:
                    break;
            }
            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void WarmestToColdest()
        {
            Console.Clear();
            Console.WriteLine("varmast till kallaste dagen enligt medeltemperatur per dag".ToUpper());
            Console.Write("(E)xterior / (I)nterior : ");
            char c = Console.ReadKey().KeyChar;
            Console.WriteLine("\n");

            var result = datos.Up_02_WarmestTillColdest(c);
            
            foreach (Dobles s in (IOrderedQueryable)result)
            {
                Console.WriteLine($"DAY: {s.valueDateTime.Date, -12}    AVERAGE TEMP: {s.valueDouble, 6:00.00}");
            }

            switch (char.ToLower(c))
            {
                case 'e':
                    {
                        Console.WriteLine($"EXTERIOR REGISTRIES = {result.Count()}");
                        break;
                    }
                case 'i':
                    {
                        Console.WriteLine($"INTERIOR REGISTRIES = {result.Count()}");
                        break;
                    }
                default:
                    break;
            }
            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        private static void AskDay()
        {
            Console.Clear();
            Console.Write("Valj Datum (YY/MM/DD) : ");
            DateTime daySelected = DateTime.Parse(Console.ReadLine());
            Console.Write("(E)xterior / (I)nterior : ");
            char c = Console.ReadKey().KeyChar;
            Console.WriteLine("\n");

            var result = datos.Up_01_SelectByDay(daySelected, c); // Ask information about the selected day.

            if (result == null) //If the day does not appear on the registries
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Det valda datumet finns inte i posterna.");
                Console.ResetColor();
                Console.Write("Press any key to continue...");
                Console.ReadKey(true);
            }
            else // But if the day is found...
            {
                switch (char.ToLower(c))
                {
                    case 'e':
                        {
                            Console.WriteLine($"DAY: {daySelected.Date} *** EXTERIOR REGISTRIES");
                            break;
                        }
                    case 'i':
                        {
                            Console.WriteLine($"DAY: {daySelected.Date} *** INTERIOR REGISTRIES");
                            break;
                        }
                    default:
                        break;
                }
                Console.WriteLine($"Num.Registries: {result.Count(),-6}Average Temp.: {result.Average(r => r.Temperature):.00}"); //Print the information about it.
                Console.ReadKey(true);
            }
        }
    }
}
