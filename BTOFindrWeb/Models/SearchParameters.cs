using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindrWeb.Models
{
    public class SearchParameters
    {

        public string[] townNames { get; set; }
        public char ethnicGroup { get; set; }
        public string[] roomTypes { get; set; }
        public decimal maxPrice { get; set; }
        public decimal minPrice { get; set; }
        public char orderBy { get; set; }
        public string postalCode { get; set; }
    }
}