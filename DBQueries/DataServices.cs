using System;
using System.Collections.Generic;
using System.Text;
using WeatherProject_v1.Models;
using WeatherProject_v1.DataDB;
using System.Linq;
using System.IO;

/*
 * This Class provides the functionality and creates the DBContext conection to the Database.
 */

namespace WeatherProject_v1.DBQueries
{
    /*
     * Dobles (struct):
     * At first I did use tupples as a returning value, but after I implemented
     * Database functionality with EF Code first, for some reason I couldn't use
     * them no more. This simple struct worked perfect and solved the problem.
     * My guess is that it has something to do with the IEnumerable vs IQueriable objects.
     */
    public struct Dobles
    {
        public DateTime valueDateTime;
        public double valueDouble;
    }


    internal class DataServices
    {
        RegistriesContext database = new RegistriesContext();

        /*
         * BlackHoles():
         * First method I wrote to have a visual representation of the missing information
         * Green days are actual registries.
         * Red Days are missing data.
         */
        public void BlackHoles()
        {
            Console.Clear();
            Console.WriteLine("list av svarta hål:".ToUpper());

            var Records = database.Registros;

            var result = Records
                .GroupBy(r => r.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g => g.Key);

            DateTime lastDay = result.First();
            foreach (DateTime d in result)
            {
                if (d.Date == lastDay.Date || d.Date == lastDay.Date.AddDays(1))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(d.Date.ToString().Substring(0, 10));
                    Console.ResetColor();
                }
                else if (d.Date != lastDay.Date)
                {
                    for (int i = 0; i < (d.Date - lastDay.Date).Days; i++)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(lastDay.Date.AddDays(i).ToString().Substring(0, 10));
                        Console.ResetColor();
                    }
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(d.Date.ToString().Substring(0, 10));
                    Console.ResetColor();
                    lastDay = d.Date;
                }

                lastDay = lastDay.AddDays(1);
            }
            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }

        public IQueryable<Register> Up_01_SelectByDay(DateTime daySelected, char typeOfRegistry)
        {
            IQueryable<Register> retValue = null;

            var Records = database.Registros;

            retValue = Records.Where(r => r.Date.Date == daySelected.Date);

            if (retValue.Count() == 0)
            {
                retValue = null;
            }
            else
            {
                switch (char.ToLower(typeOfRegistry))
                {
                    case 'e':
                        {
                            retValue = Records.Where(r => r.Date.Date == daySelected.Date && r.IsExterior);
                            break;
                        }
                    case 'i':
                        {
                            retValue = Records.Where(r => r.Date.Date == daySelected.Date && !r.IsExterior);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            return retValue;
        }

        public IOrderedQueryable<Dobles> Up_02_WarmestTillColdest(char typeOfRegistry)
        {
            var Records = database.Registros;
            var retValue = (object)null;

            switch (char.ToLower(typeOfRegistry))
            {
                case 'e':
                    {
                        retValue = Records
                            .Where(r => r.IsExterior)
                            .GroupBy(r => r.Date.Date)
                            .Select(g => new Dobles() { valueDateTime = g.Key, valueDouble = g.Average(r => r.Temperature) })
                            .OrderByDescending(s => s.valueDouble);
                        break;
                    }
                case 'i':
                    {
                        retValue = Records
                            .Where(r => !r.IsExterior)
                            .GroupBy(r => r.Date.Date)
                            .Select(g => new Dobles() { valueDateTime = g.Key, valueDouble = g.Average(r => r.Temperature) })
                            .OrderByDescending(s => s.valueDouble);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return (IOrderedQueryable<Dobles>)retValue;
        }

        public IOrderedQueryable<Dobles> Up_03_DriestToWettest(char typeOfRegistry)
        {
            var retValue = (object)null;

            var Records = database.Registros;

            switch (char.ToLower(typeOfRegistry))
            {
                case 'e':
                    {
                        retValue = Records
                            .Where(r => r.IsExterior)
                            .GroupBy(r => r.Date.Date)
                            .Select(g => new Dobles() { valueDateTime = g.Key, valueDouble = g.Average(r => r.Humidity) })
                            .OrderBy(s => s.valueDouble);
                        break;
                    }
                case 'i':
                    {
                        retValue = Records
                            .Where(r => !r.IsExterior)
                            .GroupBy(r => r.Date.Date)
                            .Select(g => new Dobles() { valueDateTime = g.Key, valueDouble = g.Average(r => r.Humidity) })
                            .OrderBy(s => s.valueDouble);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return (IOrderedQueryable < Dobles >)retValue;
        }

        /*
         * Up_04_MoldRisk(char typeOfRegistry):
         * This method handles the petition of "MoldIndex List" by using the next method:
         * MoldIndexOrRisk(double temperature, int humidity)
         */
        public IOrderedEnumerable<Dobles> Up_04_MoldRisk(char typeOfRegistry)
        {
            var Records = database.Registros;

            IOrderedEnumerable<Dobles> retValue = null;

            switch (char.ToLower(typeOfRegistry))
            {
                case 'e':
                    {
                        /*
                         * If I don´t do .AsEnumerable() in the .Where(), I would not be able to use my local method.
                         * The solution would be to create a local funtion in the SQL Server and call it locally.
                         * 
                         * IEnumerable executes the filters locally while,
                         * IQueriable does it at Server level.
                         * 
                         * OBS! It took me very long to figure this out! OUCH!!!!
                         */
                        retValue = Records
                            .Where(r => r.IsExterior).AsEnumerable()
                            .GroupBy(r => r.Date.Date)
                            .Select(g => new Dobles() { valueDateTime = g.Key, valueDouble = MoldIndexOrRisk(g.Average(r => r.Temperature), (int)Math.Round(g.Average(r => r.Humidity))) })
                            .OrderBy(s => s.valueDouble);

                        break;
                    }
                case 'i':
                    {
                        retValue = Records
                            .Where(r => !r.IsExterior).AsEnumerable()
                            .GroupBy(r => r.Date.Date)
                            .Select(g => new Dobles() { valueDateTime = g.Key, valueDouble = MoldIndexOrRisk(g.Average(r => r.Temperature), (int)Math.Round(g.Average(r => r.Humidity))) })
                            .OrderBy(s => s.valueDouble);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            return retValue;
        }

        /*
         * MoldIndexOrRisk(double temperature, int humidity):
         * This is my own formula to calculate Mold Index. It was a lot of fun to
         * work with and I didn't want to use Micke's formula.
         * I wrote a little document explaining the creation process that
         * can be found in the "About" folder.
         */
        public double MoldIndexOrRisk(double temperature, int humidity)
        {

            if (temperature < 0) temperature = 0; // Dealing with possible errors in the data
            if (humidity > 100 ) humidity = 100;  // Dealing with possible errors in the data

            double res = ((humidity - 75 - ((1.0 / 35000) *
                         (Math.Pow(temperature - 30, 4)))) * 100) /
                         (100 - 75 - ((1.0 / 35000) *
                         (Math.Pow(temperature - 30, 4))));

            //Negatives values from the formula should be interpreted as 0 (No risk)
            if (res <= 0) res = 0;

            return Math.Ceiling(res * 0.06); //Return value after applying the Mold Index Multiplier and rounding the result.
        }

        /*
         * Up_05_Autumn():
         * This method uses an algorithm to calculate the "Meteorologisk Höst"
         * that can be found also in the "About" folder. It is a Flowchart with
         * the name of "FLOWCHART-MeteorologiskHost.png"
         */
        public DateTime Up_05_Autumn()
        {
            var Records = database.Registros;
            var result = Records
                .Where(r => r.IsExterior)
                .GroupBy(r => r.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g => new Dobles() { valueDateTime = g.Key, valueDouble = g.Average(r => r.Temperature) }).ToList();

            DateTime autumn = new DateTime();

            int dayCount = 0;
            for (int i = 0; i < result.Count; i++)
            {
                /*
                * Look at the Flowchart in the "About" folder for more details.
                * 
                * "...Enligt SMHI:s meteorologiska definition råder höst,
                * då en dygnsmedeltemperatur på mellan 0 och + 10 grader Celsius varat i fem dygn.
                * Så länge detta inträffar efter 1 augusti. Vi Säger att vintern anlände det första av dessa dygn."
                */
                DateTime today = result[i].valueDateTime;
                if (dayCount < 5)                                                       // If the 5 days are not reached yet...
                {
                    if (result[i].valueDouble >= 0 && result[i].valueDouble <= 10)                  // ...and if the current Temp is between the range,
                    {
                        if (CheckDay(today.AddDays(-1)))                                // ...and if yesterday exists in the registries,
                        {
                            if (result[i - 1].valueDouble >= 0 && result[i - 1].valueDouble <= 10)  // ...and if yesterday's temp is between the range,
                            {
                                dayCount++;                                             // ...then we are one day closer to determine the Autumn.
                            }
                            else
                            {
                                dayCount = 1;
                            }
                        }
                        else
                        {
                            dayCount = 1;
                        }
                    }
                    else
                    {
                        dayCount = 0;
                    }
                }
                else                                                                    // If we have already have 5 days in a row,
                {
                    if (today.Date.AddDays(-5) > DateTime.Parse("16/08/01"))            // ...and if the first of those 5 days is posterior to 16/08/01
                    {
                        autumn = today.Date.AddDays(-5);                                // EUREKA!!!!!
                        break;
                    }
                } 
            }

            return autumn;
        }

        /*
         * Up_06_Winter():
         * This function has a similar functionality than the previous one.
         * No point in going throw it.
         * 
         * Returns:
         * Datetime object if the Winter is found or
         * Null if is undetermined.
         */
        public object Up_06_Winter()
        {
            var Records = database.Registros;

            var result = Records
                .Where(r => r.IsExterior)
                .GroupBy(r => r.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g => new Dobles() { valueDateTime = g.Key, valueDouble = g.Average(r => r.Temperature) }).ToList();

            object autumn = null;

            /*
             * "...Enligt SMHI:s meteorologiska definition,
             * om dygnsmedeltemperaturen är 0,0°C eller lägre fem dygn i följd,
             * säger vi att vintern anlände det första av dessa dygn."
             */
            int dayCount = 0;
            for (int i = 0; i < result.Count; i++)
            {
                DateTime today = result[i].valueDateTime;
                if (dayCount < 5)
                {
                    if (result[i].valueDouble < 0)
                    {
                        if (CheckDay(today.AddDays(-1)))
                        {
                            if (result[i - 1].valueDouble < 0)
                            {
                                dayCount++;
                            }
                            else
                            {
                                dayCount = 1;
                            }
                        }
                        else
                        {
                            dayCount = 1;
                        }
                    }
                    else
                    {
                        dayCount = 0;
                    }
                }
                else
                {
                    autumn = today.Date.AddDays(-5);
                    break;
                }
            }

            return autumn;
        }

        /*
         * CheckDay(DateTime dayToCheck):
         * Simply check if the record exist.
         */
        public bool CheckDay(DateTime dayToCheck)
        {
            var Records = database.Registros;

            var result = Records
                .GroupBy(r => r.Date.Date)
                .Select(g => g.Key)
                .ToList();

            bool ret = result.Contains(dayToCheck);

            return ret;
        }

        /*
         * LoadDataFromFile(string fileName):
         * This method load the data from the given file into the Database
         * created by EntityFramework code first approach.
         */
        public void LoadDataFromFile(string fileName)
        {
            // Control question to avoid unwanted loading of data.
            Console.Clear();
            Console.Write("Are you sure that you want to load the Data to the SQL Server? ( Y / N ) : ");
            ConsoleKeyInfo k = Console.ReadKey(true);
            if (char.ToLower(k.KeyChar) == 'n') return; // If "NO", we exit the method without changes.

            try
            {
                var lineas = File.ReadLines(fileName);

                foreach (string s in lineas)
                {
                    string[] datos = s.Split(',');

                    Register register = new Register();

                    register.Date = DateTime.Parse(datos[0]);
                    register.IsExterior = datos[1].ToLower() == "ute";
                    register.Temperature = double.Parse(datos[2].Replace('.', ',')); // Problem with Regional preferences in Operating system? Maybe Encoding. Check Later.
                    register.Humidity = int.Parse(datos[3]);

                    database.Registros.Add(register);
                }
                database.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                Environment.Exit(1);
            }
        }

        public void Close()
        {
            database.Dispose(); // Close database's DBContext object.
        }
    }
}
