using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindrWeb.Models
{
    public class Unit
    {
        public int UnitId { get; set; }
        public string UnitNo { get; set; }
        public decimal Price { get; set; }
        public int FloorArea { get; set; }
        public Boolean Avail { get; set; }
        public UnitType UnitType { get; set; }
        public Floor Floor { get; set; }
    }
}