using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FanShirts
{
    public class SavedJersey
    {
        public long Id { get; set; }
        public string JerseyID { get; set; }

        public SavedJersey()
        {

        }

        public SavedJersey(long id, string jerseyId)
        {
            Id = id;
            JerseyID = jerseyId;
        }
    }
}
