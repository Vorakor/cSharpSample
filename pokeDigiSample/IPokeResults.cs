using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pokeDigiSample
{
    public class IPokeResults
    {
        public int Count { get; set; }
        public string? Next { get; set; }
        public string Previous { get; set; }
        public List<DefaultAttributes> Results { get; set; }
        public IPokeResults() {
            Next = string.Empty;
            Previous = string.Empty;
            Results = new List<DefaultAttributes>();
        }
    }
}
