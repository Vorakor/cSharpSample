using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace pokeDigiSample
{
    public class API
    {
        public int pokeCount, digiCount;
        public API(DB database)
        {
            FetchAndStoreCreatures(database);
        }

        public List<APIDigimon> FetchDigimon(string api_prefix)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            List<APIDigimon> creatures = client.GetFromJsonAsync<List<APIDigimon>>(api_prefix).Result;
            client.Dispose();
            digiCount = creatures.Count();
            return creatures;
        }

        public List<DefaultAttributes> FetchPokemonURLs(HttpClient client, string api_prefix)
        {
            List<DefaultAttributes> pokeNameURLs = new List<DefaultAttributes>();
            // Initial call here...
            IPokeResults results = client.GetFromJsonAsync<IPokeResults>(api_prefix + "?offset=0&limit=200").Result;
            pokeCount = results.Count;
            while (results.Next != null)
            {
                // When next is empty then we've reached the end of the list
                pokeNameURLs.AddRange(results.Results);
                results = client.GetFromJsonAsync<IPokeResults>(results.Next).Result;
            }
            return pokeNameURLs;
        }

        public List<IPokemon> FetchPokemon(HttpClient client, List<DefaultAttributes> pokeNameURLs)
        {
            List<IPokemon> pokemon = new List<IPokemon>();
            double count = 0;
            foreach (DefaultAttributes attr in pokeNameURLs)
            {
                APIPokemon result = client.GetFromJsonAsync<APIPokemon>(attr.Url).Result;
                pokemon.Add(result.MapAPIToDB(attr.Url));
                double perc = (count / pokeCount) * 100;
                double percent = Math.Round(perc, 0);
                Console.WriteLine($"{percent}% pokemon fetched...");
                count++;
            }
            return pokemon;
        }

        public List<IPokemon> FetchPokemon(string api_prefix)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(api_prefix);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            List<DefaultAttributes> pokeNameURLs = FetchPokemonURLs(client, api_prefix);
            List<IPokemon> pokemon = FetchPokemon(client, pokeNameURLs);
            client.Dispose();
            return pokemon;
        }

        public void FetchAndStoreCreatures(DB database)
        {
            Console.WriteLine("Please wait one moment while we load data from APIs...");
            List<APIDigimon> aDigi = FetchDigimon(APIDigimon.GetAPIPrefix());
            List<IPokemon> aPoke = FetchPokemon(APIPokemon.GetAPIPrefix());
            Console.WriteLine("Now storing data from APIs in database...");
            database.InsertDigimon(aDigi, digiCount);
            database.InsertPokemon(aPoke, pokeCount);
            return;
        }

        public void PrintMonsters(DB database)
        {
            List<string> userFriendly = new List<string>();
            List<string> data = new List<string>();
            List<IMonster> monsters = database.GetMonsters();
            foreach (IMonster monster in monsters)
            {
                userFriendly.Add($"Fetched {monster.Name}.  {monster.Name}'s image url: {monster.Image}");
                data.Add($"{monster.Name}, {monster.Image}");
            }
            PrettyPrint(userFriendly, data);
            return;
        }

        public bool PrintMonsters(DB database, bool digimon, string userMsg)
        {
            Console.WriteLine(userMsg);
            var userInput = Console.ReadLine();
            if (userInput != null)
            {
                List<string> userFriendly = new List<string>();
                List<string> data = new List<string>();
                if (digimon)
                {
                    bool valid = database.ValidateDInput(userInput);
                    if (valid)
                    {
                        List<IMonster> monsters = database.GetMonsters(userInput);
                        foreach (IMonster monster in monsters)
                        {
                            userFriendly.Add($"Fetched {monster.Name}.  {monster.Name}'s image url: {monster.Image}");
                            data.Add($"{monster.Name}, {monster.Image}");
                        }
                        return PrettyPrint(userFriendly, data);
                    }
                    else
                    {
                        Console.WriteLine($"Sorry, but that was not a valid {(digimon ? "digimon level" : "pokemon type")}. Terminating program.");
                        return false;
                    }
                }
                else
                {
                    int valid = database.ValidatePInput(userInput);
                    if (valid > 0)
                    {
                        List<IMonster> monsters = database.GetMonsters(userInput, valid);
                        foreach (IMonster monster in monsters)
                        {
                            userFriendly.Add($"Fetched {monster.Name}.  {monster.Name}'s image url: {monster.Image}");
                            data.Add($"{monster.Name}, {monster.Image}");
                        }
                        return PrettyPrint(userFriendly, data);
                    }
                    else
                    {
                        Console.WriteLine($"Sorry, but that was not a valid {(digimon ? "digimon level" : "pokemon type")}. Terminating program.");
                        return false;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Invalid {(digimon ? "digimon level" : "pokemon type")}. Exiting program by default.");
                return false;
            }
        }

        public bool PrettyPrint(List<string> userFriendly, List<string> data)
        {
            Console.WriteLine("Would you like a user friendly version of the data printed? Enter 0 for no, 1 for yes.");
            var userInput = Console.ReadLine();
            int input;
            if (int.TryParse(userInput, out input))
            {
                if (input == 1)
                {
                    foreach (string uf in userFriendly)
                    {
                        Console.WriteLine(uf);
                    }
                }
                else
                {
                    foreach (string d in data)
                    {
                        Console.WriteLine(d);
                    }
                }
                return true;
            }
            Console.WriteLine("The input you entered was not valid. Exiting program by default.");
            return false;
        }
    }
}
