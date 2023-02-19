using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace pokeDigiSample
{
    public class APIDigimon : ICreature
    {
        public string Img { get; set; }
        public string Level { get; set; }

        public APIDigimon()
        {
            APIPrefix = GetAPIPrefix();
            Img = string.Empty;
            Level = string.Empty;
        }

        public static string GetAPIPrefix() {
            return "https://digimon-api.vercel.app/api/digimon";
        }

        public static List<IDigimon> MapAPIToDB(List<APIDigimon> APICreatures, List<IDigimon> DBCreatures)
        {
            foreach (APIDigimon creature in APICreatures)
            {
                IDigimon digimon = new IDigimon();
                digimon.APIPrefix = creature.APIPrefix;
                digimon.Level = creature.Level;
                digimon.Url = creature.Url;
                digimon.Name = creature.Name;
                digimon.Image = creature.Img;
                DBCreatures.Add(digimon);
            }
            return DBCreatures;
        }

        public static IDigimon MapAPIToDB(APIDigimon creature, IDigimon digimon)
        {
            digimon.APIPrefix = creature.APIPrefix;
            digimon.Level = creature.Level;
            digimon.Url = creature.Url;
            digimon.Name = creature.Name;
            digimon.Image = creature.Img;
            return digimon;
        }

        public IDigimon MapAPIToDB()
        {
            IDigimon digimon = new IDigimon();
            digimon.APIPrefix = APIPrefix;
            digimon.Level = Level;
            digimon.Url = Url;
            digimon.Name = Name;
            digimon.Image = Img;
            return digimon;
        }
    }
}
