using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindrWeb.Models
{
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
