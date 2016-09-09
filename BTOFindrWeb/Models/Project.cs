using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BTOFindrWeb.Models
{
    public class Project
    {
        public string projectId { get; set; }
        public string projectName { get; set; }
        public string townName { get; set; }
        public string ballotDate { get; set; }
        public string projectImage { get; set; }
        public List<Block> blocks { get; set; }
    }
}