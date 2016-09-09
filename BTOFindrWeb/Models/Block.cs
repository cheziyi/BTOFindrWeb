using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindrWeb.Models
{
    public class Block
    {
        public int blockId { get; set; }
        public string blockNo { get; set; }
        public string street { get; set; }
        public DateTime deliveryDate { get; set; }
        public decimal locLat { get; set; }
        public decimal locLong { get; set; }
        public string sitePlan { get; set; }
        public string townMap { get; set; }
        public string blockPlan { get; set; }
        public string unitDist { get; set; }
        public string floorPlan { get; set; }
        public string layoutIdeas { get; set; }
        public string specs { get; set; }
        public Project project { get; set; }
        public List<UnitType> unitTypes { get; set; }
    }
}