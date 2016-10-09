using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindr.Models
{
    public class Profile
    {
        public String postalCode { get; set; }
        public decimal income { get; set; }
        public decimal currentCpf { get; set; }
        public decimal monthlyCpf { get; set; }
        public int loanTenure { get; set; }
    }
}