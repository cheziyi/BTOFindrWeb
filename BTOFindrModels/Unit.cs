using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindr.Models
{
    public class Unit
    {
        public int unitId { get; set; }
        public string unitNo { get; set; }
        public decimal price { get; set; }
        public int floorArea { get; set; }
        public Boolean avail { get; set; }
        public UnitType unitType { get; set; }
        public int faveCount { get; set; }
        public FeesPayable fees { get; set; }
    }
}