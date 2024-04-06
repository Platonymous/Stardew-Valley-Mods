using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artista.Online
{
    public class OnlineData
    {
        public User User { get; set; } = null;

        public List<string> Downloads { get; set; } = new List<string>();
    }
}
