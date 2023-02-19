using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pokeDigiSample
{
    public interface IDefaultAttributes
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class DefaultAttributes
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DefaultAttributes()
        {
            Name = string.Empty;
            Url = string.Empty;
        }
    }
}
