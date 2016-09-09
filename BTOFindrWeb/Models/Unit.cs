using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindrWeb.Models
{
    public class Unit
    {
        public int unitId { get; set; }
        public string unitNo { get; set; }
        public decimal price { get; set; }
        public int floorArea { get; set; }
        public Boolean avail { get; set; }
        public UnitType unitType { get; set; }
    }
}