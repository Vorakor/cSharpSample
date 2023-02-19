using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace pokeDigiSample
{
    public class PokemonSprite
    {
        public string BackDefault { get; set; }
        public string BackFemale { get; set; }
        public string BackShiny { get; set; }
        public string BackShinyFemale { get; set; }
        public string FrontDefault { get; set; }
        public string FrontFemale { get; set; }
        public string FrontShiny { get; set; }
        public string FrontShinyFemale { get; set; }
        public PokemonSprite(string? backD = null, string? backF = null, string? backS = null, string? backSF = null, string? frontD = null, string? frontF = null, string? frontS = null, string? frontSF = null)
        {
            BackDefault = backD != null ? backD : string.Empty;
            BackFemale = backF != null ? backF : string.Empty;
            BackShiny = backS != null ? backS : string.Empty;
            BackShinyFemale = backSF != null ? backSF : string.Empty;
            FrontDefault = frontD != null ? frontD : string.Empty;
            FrontFemale = frontF != null ? frontF : string.Empty;
            FrontShiny = frontS != null ? frontS : string.Empty;
            FrontShinyFemale = frontSF != null ? frontSF : string.Empty;
        }

        public string FirstOrDefault()
        {
            if (ReferenceEquals(this, null))
            {
                return "";
            }
            // We're expecting FrontDefault to have a property every time, otherwise we'll grab the first URL and throw that back as the image to use.
            string? result = GetType().GetProperty("FrontDefault").GetValue(this) as string;
            if (string.IsNullOrEmpty(result))
            {
                foreach (FieldInfo field in GetType().GetFields())
                {
                    if (!string.IsNullOrEmpty(field.GetValue(this) as string))
                    {
                        result = field.GetValue(this) as string;
                        break;
                    }
                }
            }
            return string.IsNullOrEmpty(result) ? "" : result;
        }
    }
}
