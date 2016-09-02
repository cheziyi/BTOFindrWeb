using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindrWeb.Models
{
    public class Floor
    {
        public int FloorNumber { get; set; }
        public List<Unit> Units { get; set; }
        public Block Block { get; set; }
        public UnitType UnitType { get; set; }
    }
}