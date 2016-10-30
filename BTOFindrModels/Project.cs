using System.Collections.Generic;

namespace BTOFindr.Models
{
    /// <summary>
    /// This describes a HDB BTO Project that contains one or more Blocks.
    /// A Project is a group of Blocks built together with the same concept.
    /// </summary>
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