using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindrWeb.Models
{
    public class Block
    {
        public int BlockId { get; set; }
        public string BlockNo { get; set; }
        public string Street { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal LocLat { get; set; }
        public decimal LocLong { get; set; }
        public string SitePlan { get; set; }
        public string TownMap { get; set; }
        public string BlockPlan { get; set; }
        public string UnitDist { get; set; }
        public string FloorPlan { get; set; }
        public string LayoutIdeas { get; set; }
        public string Specs { get; set; }
        public Project Project { get; set; }
        public List<UnitType> UnitTypes { get; set; }

    }
}