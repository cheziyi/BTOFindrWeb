using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindr.Models
{
    public class SearchParameters
    {

        public string[] townNames { get; set; }
        public char ethnicGroup { get; set; }
        public string[] unitTypes { get; set; }
        public decimal maxPrice { get; set; }
        public decimal minPrice { get; set; }
        public char orderBy { get; set; }
        public string postalCode { get; set; }
    }
}