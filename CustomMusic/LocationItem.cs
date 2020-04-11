using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMusic
{
    public class LocationItem
    {
        public string Location { get; set; }
        public string MusicId { get; set; }
        public List<SoundEmitterPlacement> Emitters { get; set; } = new List<SoundEmitterPlacement>(); 
    }
}
