using System.Collections.Generic;

namespace BTOFindr.Models
{
    /// <summary>
    ///  This describes a HDB BTO Block in a Project that contains one or more Unit Types.
    /// </summary>
    public class Block
    {
        public int blockId { get; set; }
        public string blockNo { get; set; }
        public string street { get; set; }
        public string deliveryDate { get; set; }
        public decimal locLat { get; set; }
        public decimal locLong { get; set; }
        public int travelTime { get; set; }
        public int travelDist { get; set; }
        public string sitePlan { get; set; }
        public string townMap { get; set; }
        public string blockPlan { get; set; }
        public string unitDist { get; set; }
        public string floorPlan { get; set; }
        public string layoutIdeas { get; set; }
        public string specs { get; set; }
        public decimal minPrice { get; set; }
        public decimal maxPrice { get; set; }
        public Project project { get; set; }
        public List<UnitType> unitTypes { get; set; }
    }
}