namespace BTOFindr.Models
{
    /// <summary>
    /// This describes Search Parameters that a user
    /// will pass to BTOFindr when searching for Blocks.
    /// </summary>
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