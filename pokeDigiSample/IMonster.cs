using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pokeDigiSample
{
    public class IMonster : ICreature
    {
        public string Image { get; set; }
        public IMonster() {
            Image = string.Empty;
        }
    }
}
