using System;
using System.Data;
using pokeDigiSample;

internal class Program
{
    private static void Main()
    {
        Console.WriteLine("Welcome to the Pokemon-Digimon Sample Application!");
        Console.WriteLine("Creating database...");
        DB database = new DB();
        int creatureCount = database.GetRecordCount("creatures");
        API api;
        if (creatureCount > 0)
        {
            api = new API();
        }
        else
        {
            api = new API(database);
        }
        Console.WriteLine($"Total creatures: {database.GetRecordCount("creatures")}");
        // Now get input
        int input = 0;
        while (input != 5)
        {
            Console.WriteLine("Please enter a number that corresponds to the list item of the information you would like to retrieve:\n1 - List All Monsters\n2 - List All Digimon By Level\n3 - List All Pokemon By Type\n4 - Reload data\n5 - Exit program");
            string userInput = Console.ReadLine();
            if (int.TryParse(userInput, out input))
            {
                switch (input)
                {
                    case 1:
                        api.PrintMonsters(database);
                        break;
                    case 2:
                        List<string> levels = database.GetLevels();
                        bool printd = api.PrintMonsters(database, true, $"Please enter the level you would like to use to filter Digimon (possible levels - {string.Join(", ",levels)}):");
                        if (!printd)
                        {
                            Console.WriteLine("Failure occurred in digimon process");
                            input = 5;
                        }
                        break;
                    case 3:
                        List<string> types = database.GetTypes();
                        bool printp = api.PrintMonsters(database, false, $"Please enter the type you would like to use to filter Pokemon (possible types - {string.Join(", ", types)}):");
                        if (!printp)
                        {
                            Console.WriteLine("Failure occurred in pokemon process");
                            input = 5;
                        }
                        break;
                    case 4:
                        api.FetchAndStoreCreatures(database);
                        break;
                    default:
                        input = 5;
                        break;
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Exiting program by default.");
                break;
            }
        }
        Console.WriteLine("Exiting program...");
    }
}