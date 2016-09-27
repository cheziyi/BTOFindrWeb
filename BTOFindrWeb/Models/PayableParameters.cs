using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindrWeb.Models
{
    public class PayableParameters
    {
        public int unitId { get; set; }
        //public int appl1Time { get; set; }
        //public int appl2Time { get; set; }
        public decimal income { get; set; }
        public decimal currentCpf { get; set; }
        public decimal monthlyCpf { get; set; }
        //public char loan { get; set; }
        //public int loanPercent { get; set; }
        public int loanTenure { get; set; }
    }
}