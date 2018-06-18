using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomShirts
{
    public class SavedShirt
    {
        public long Id { get; set; }
        public string ShirtId { get; set; }

        public SavedShirt()
        {

        }

        public SavedShirt(long id, string shirtId)
        {
            Id = id;
            ShirtId = shirtId;
        }
    }
}
