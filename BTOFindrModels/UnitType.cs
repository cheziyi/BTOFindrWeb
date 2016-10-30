using System.Collections.Generic;

namespace BTOFindr.Models
{
    /// <summary>
    /// This describes a HDB BTO Unit Type in a Block that contains one or more Units.
    /// A Unit Type is a group of Units with the same number of rooms, e.g. 3-Room, 4-Room.
    /// Also contains the quota of ethnic buyers that HDB has allocated for each Unit Type.
    /// </summary>
    public class UnitType
    {
        public int unitTypeId { get; set; }
        public string unitTypeName { get; set; }
        public int quotaMalay { get; set; }
        public int quotaChinese { get; set; }
        public int quotaOthers { get; set; }
        public List<Unit> units { get; set; }
        public Block block { get; set; }
    }
}
