using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindrWeb.Models
{
   public class UnitType
    {
        public int UnitTypeId { get; set; }
        public string UnitTypeName { get; set; }
        public int QuotaMalay { get; set; }
        public int QuotaChinese { get; set; }
        public int QuotaOthers { get; set; }
        public List<Unit> Units { get; set; }
        public Block Block { get; set; }
    }
}
