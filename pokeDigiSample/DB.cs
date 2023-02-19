using Dapper;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pokeDigiSample
{
    public class DB
    {
        MySqlConnection connection = new MySqlConnection("Server=localhost;User ID=pokedigi;Password=r@nd0mPas$word;Database=pokedigisample");

        public DB() {
            if (!CheckDatabase())
            {
                CreateDB();
            }
        }

        public bool ValidateDInput(string input)
        {
            int validInput;
            try
            {
                validInput = connection.QuerySingle<int>($"SELECT id FROM `digimon` WHERE '{input}' IN (SELECT DISTINCT `level` FROM `digimon`) LIMIT 1;");
                // Console.WriteLine($"Valid ID: {validInput}"); // For troubleshooting
            }
            catch
            {
                validInput = 0;
            }
            return validInput > 0 ? true : false;
        }

        public int ValidatePInput(string input)
        {
            int validInput;
            try
            {
                validInput = connection.QuerySingle<int>($"SELECT id FROM `poketypes` WHERE '{input}' LIKE type;");
                Console.WriteLine($"Valid ID: {validInput}"); // For troubleshooting
            }
            catch
            {
                validInput = 0;
            }
            return validInput;
        }

        public int GetRecordCount(string tablename) {
            string query = $"select count(*) from {tablename}";
            IEnumerable<int> count = connection.Query<int>(query);
            return count.FirstOrDefault();
        }

        public List<string> GetLevels()
        {
            string SQL = "SELECT level FROM `digimon` GROUP BY level;";
            List<string> levels = connection.Query<string>(SQL).ToList();
            return levels;
        }

        public List<string> GetTypes()
        {
            string SQL = "SELECT type from `poketypes`;";
            List<string> types = connection.Query<string>(SQL).ToList();
            return types;
        }

        public List<IMonster> GetMonsters()
        {
            string SQL = @"SELECT c.*, COALESCE(p.image, d.image) AS image FROM `creatures` AS c
                            LEFT JOIN `pokemon` as p ON p.creature_id = c.id
                            LEFT JOIN `digimon` as d ON d.creature_id = c.id;";
            List<IMonster> results = connection.Query<IMonster>(SQL).ToList();
            return results;
        }

        public List<IMonster> GetMonsters(string input, int typeId = 0)
        {
            string SQL;
            if (typeId == 0)
            {
                SQL = $@"SELECT c.*, d.image from `creatures` AS c
                            INNER JOIN `digimon` AS d ON d.creature_id = c.id
                            WHERE '{input}' LIKE d.level;";
            }
            else
            {
                SQL = $@"SELECT c.*, p.image from `creatures` AS c
                            INNER JOIN `pokemon` AS p ON p.creature_id = c.id
                            INNER JOIN `pokemontypes` AS pt ON pt.pokemon_id = p.id
                            INNER JOIN `poketypes` AS t ON t.id = pt.poketype_id
                            WHERE pt.poketype_id = {typeId};";
            }
            List<IMonster> results = connection.Query<IMonster>(SQL).ToList();
            return results;
        }

        public ICreature InsertCreature(ICreature creature)
        {
            // This function is inserting the creature and also adding the ID to it before returning the creature
            // This assumes we try to query the ID and if we fail, which means the record does not exist, then we go to insert
            string tablename = "creatures";
            string fields = "`name`,`url`,`api_prefix`";
            string values = $"'{creature.Name}', '{creature.Url}', '{creature.APIPrefix}'";
            string whereConditions = $"'{creature.Name}' LIKE `name` AND '{creature.Url}' LIKE `url` AND '{creature.APIPrefix}' LIKE `api_prefix`";
            creature = RunInsert(creature, tablename, fields, values, whereConditions);
            return creature;
        }

        public List<IDigimon> InsertDigimon(List<IDigimon> digimon, int totalCount)
        {
            List<IDigimon> creatures = new List<IDigimon>();
            double count = 0;
            foreach (IDigimon dig in digimon)
            {
                ICreature c = InsertCreature(dig);
                string tablename = "digimon";
                string fields = "`creature_id`,`image`,`level`";
                string values = $"'{c.Id}', '{dig.Image}', '{dig.Level}'";
                string whereConditions = $"`creature_id` = {c.Id} AND '{dig.Level.ToLower()}' LIKE `level` AND '{dig.Image}' LIKE `image`";
                IDigimon dCreature = RunInsert(dig, tablename, fields, values, whereConditions);
                creatures.Add(dCreature);
                double perc = (count / totalCount) * 100;
                double percent = Math.Round(perc, 0);
                Console.WriteLine($"{percent}% digimon inserted...");
                count++;
            }
            return creatures;
        }

        public List<IDigimon> InsertDigimon(List<APIDigimon> digimon, int totalCount)
        {
            List<IDigimon> creatures = new List<IDigimon>();
            double count = 0;
            foreach (APIDigimon dig in digimon)
            {
                dig.Url = $"/name/{dig.Name.ToLower()}";
                ICreature c = InsertCreature(dig);
                string tablename = "digimon";
                string fields = "`creature_id`,`image`,`level`";
                string values = $"'{c.Id}', '{dig.Img}', '{dig.Level.ToLower()}'";
                string whereConditions = $"`creature_id` = {c.Id} AND '{dig.Level.ToLower()}' LIKE `level` AND '{dig.Img}' LIKE `image`";
                IDigimon dCreature = RunInsert(dig.MapAPIToDB(), tablename, fields, values, whereConditions);
                creatures.Add(dCreature);
                double perc = (count / totalCount) * 100;
                double percent = Math.Round(perc, 0);
                Console.WriteLine($"{percent}% digimon inserted...");
                count++;
            }
            return creatures;
        }

        public List<IPokemon> InsertPokemon(List<IPokemon> pokemon, int totalCount)
        {
            List<IPokemon> creatures = new List<IPokemon>();
            double count = 0;
            foreach (IPokemon poke in pokemon)
            {
                ICreature c = InsertCreature(poke);
                string tablename = "pokemon";
                string fields = "`creature_id`,`image`";
                string values = $"'{c.Id}', '{poke.Image}'";
                string whereConditions = $"`creature_id` = {c.Id} AND '{poke.Image}' LIKE `image`";
                IPokemon pCreature = RunInsert(poke, tablename, fields, values, whereConditions);
                foreach (string type in poke.Types)
                {
                    // call LinkPokemonTypes after running InsertPokemonType on each pokemon type
                    int typeId = InsertPokemonType(type);
                    pCreature.TypeIds.Add(typeId);
                    LinkPokemonTypes(pCreature.Id, typeId);
                }
                creatures.Add(pCreature);
                double perc = (count / totalCount) * 100;
                double percent = Math.Round(perc, 0);
                Console.WriteLine($"{percent}% pokemon inserted...");
                count++;
            }
            return creatures;
        }

        public List<IPokemon> InsertPokemon(List<APIPokemon> pokemon, int totalCount)
        {
            List<IPokemon> creatures = new List<IPokemon>();
            double count = 0;
            foreach (APIPokemon poke in pokemon)
            {
                IPokemon p = poke.MapAPIToDB(poke.creatureUrl);
                ICreature c = InsertCreature(p);
                string tablename = "pokemon";
                string fields = "`creature_id`,`image`";
                string values = $"'{c.Id}', '{p.Image}'";
                string whereConditions = $"`creature_id` = {c.Id} AND '{p.Image}' LIKE `image`";
                IPokemon pCreature = RunInsert(p, tablename, fields, values, whereConditions);
                foreach (string type in p.Types)
                {
                    // call LinkPokemonTypes after running InsertPokemonType on each pokemon type
                    int typeId = InsertPokemonType(type);
                    pCreature.TypeIds.Add(typeId);
                    LinkPokemonTypes(pCreature.Id, typeId);
                }
                creatures.Add(pCreature);
                double perc = (count / totalCount) * 100;
                double percent = Math.Round(perc, 0);
                Console.WriteLine($"{percent}% pokemon inserted...");
                count++;
            }
            return creatures;
        }

        public int InsertPokemonType(string type)
        {
            // Needs to return either last inserted id if it inserted a type or the id of the selected type
            string tablename = "poketypes";
            string fields = "`type`";
            string values = $"'{type}'";
            string whereConditions = $"'{type}' LIKE `type`";
            return RunInsert(tablename, fields, values, whereConditions);
        }

        public void LinkPokemonTypes(int pokemon_id, int type_id)
        {
            string tablename = "pokemontypes";
            string fields = "`pokemon_id`,`poketype_id`";
            string values = $"{pokemon_id},{type_id}";
            string whereConditions = $"`pokemon_id` = {pokemon_id} AND `poketype_id` = {type_id}";
            RunInsert(tablename, fields, values, whereConditions);
            return;
        }

        public void LinkPokemonTypes(int pokemon_id, List<int> type_ids)
        {
            foreach (int id in type_ids)
            {
                string tablename = "pokemontypes";
                string fields = "`pokemon_id`,`poketype_id`";
                string values = $"{pokemon_id},{id}";
                string whereConditions = $"`pokemon_id` = {pokemon_id} AND `poketype_id` = {id}";
                RunInsert(tablename, fields, values, whereConditions);
            }
            return;
        }

        public T RunInsert<T>(T item, string tablename, string fields, string values, string whereConditions) where T : ICreature
        {
            // Console.WriteLine($"T {tablename}, F {fields}, V {values}, W {whereConditions}"); // Debugging purposes
            int id = connection.QuerySingle<int>(
                $"INSERT INTO `{tablename}` ({fields}) SELECT {values} WHERE NOT EXISTS (SELECT 1 FROM `{tablename}` WHERE {whereConditions}); SELECT `id` FROM `{tablename}` WHERE {whereConditions};"
            );
            item.Id = id;
            return item;
        }

        public int RunInsert(string tablename, string fields, string values, string whereConditions)
        {
            // Console.WriteLine($"T {tablename}, F {fields}, V {values}, W {whereConditions}"); // Debugging purposes
            int id = connection.QuerySingle<int>(
                $"INSERT INTO `{tablename}` ({fields}) SELECT {values} WHERE NOT EXISTS (SELECT 1 FROM `{tablename}` WHERE {whereConditions}); SELECT `id` FROM `{tablename}` WHERE {whereConditions};"
            );
            return id;
        }

        public bool CheckDatabase()
        {
            Console.WriteLine("Checking if database exists...");
            string dbExists = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'pokedigisample';";
            bool result = true;
            try
            {
                string database = connection.QueryFirstOrDefault<string>(dbExists);
                if (database == null || database?.Length < 14)
                {
                    result = false;
                }
            } catch
            {
                result = false;
            }
            return result;
        }

        public void CreateDB() {
            MySqlConnection conn = new MySqlConnection("Server=localhost;User ID=pokedigi;Password=r@nd0mPas$word");
            string creationQuery = @"CREATE DATABASE pokedigisample; USE pokedigisample;
                                     CREATE TABLE `creatures` (`id` INT NOT NULL AUTO_INCREMENT,`name` VARCHAR(255) NOT NULL,`url` TEXT NOT NULL,`api_prefix` TEXT NOT NULL,PRIMARY KEY (`id`));
                                     CREATE TABLE `pokemon` (`id` INT NOT NULL AUTO_INCREMENT,`creature_id` INT NOT NULL,`image` TEXT NOT NULL,PRIMARY KEY (`id`),CONSTRAINT `FK1` FOREIGN KEY (`creature_id`) REFERENCES `creatures` (`id`) ON UPDATE CASCADE ON DELETE CASCADE);
                                     CREATE TABLE `pokeTypes` (`id` INT NOT NULL AUTO_INCREMENT,`type` VARCHAR(255) NOT NULL,PRIMARY KEY (`id`));
                                     CREATE TABLE `digimon` (`id` INT NOT NULL AUTO_INCREMENT,`creature_id` INT NOT NULL,`image` TEXT NOT NULL,`level` VARCHAR(255) NOT NULL,PRIMARY KEY (`id`),CONSTRAINT `FK2` FOREIGN KEY (`creature_id`) REFERENCES `creatures` (`id`) ON UPDATE CASCADE ON DELETE CASCADE);
                                     CREATE TABLE `pokemonTypes` (`id` INT NOT NULL AUTO_INCREMENT,`pokemon_id` INT NOT NULL,`poketype_id` INT NOT NULL,PRIMARY KEY (`id`),CONSTRAINT `FK3` FOREIGN KEY (`pokemon_id`) REFERENCES `pokemon` (`id`) ON UPDATE CASCADE ON DELETE CASCADE,CONSTRAINT `FK4` FOREIGN KEY (`poketype_id`) REFERENCES `poketypes` (`id`) ON UPDATE CASCADE ON DELETE CASCADE);";
            conn.Execute(creationQuery);
            return;
        }
    }
}
