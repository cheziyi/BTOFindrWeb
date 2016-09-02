using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindrWeb.Models
{
    public class Project
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string TownName { get; set; }
        public string BallotDate { get; set; }
        public string ProjectImage { get; set; }
        public List<Block> Blocks { get; set; }
    }
}