using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MushyMu.Model
{
    public class Game
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public Guid ID { get; set; }
        public string CharName { get; set; }
        public string Password { get; set; }
        public string Font { get; set; }
        public int FontSize { get; set; }


    }
}
