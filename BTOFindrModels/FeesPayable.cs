using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindr.Models
{
    public class FeesPayable
    {
        public decimal grantAmt { get; set; }
        public decimal afterGrantAmt { get; set; }
        public decimal applFee { get; set; }
        public decimal optionFee { get; set; }
        public decimal signingFeesCash { get; set; }
        public decimal signingFeesCpf { get; set; }
        public decimal collectionFeesCash { get; set; }
        public decimal collectionFeesCpf { get; set; }
        public decimal monthlyCash { get; set; }
        public decimal monthlyCpf { get; set; }
    }
}