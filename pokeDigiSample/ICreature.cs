using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pokeDigiSample
{
    public class ICreature: IDefaultAttributes
    {
        public string APIPrefix { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public ICreature()
        {
            APIPrefix = string.Empty;
            Name = string.Empty;
            Url = string.Empty;
        }
    }
}
