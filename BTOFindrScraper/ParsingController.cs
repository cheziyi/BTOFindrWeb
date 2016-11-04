using AngleSharp.Parser.Html;
using BTOFindr.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BTOFindrScraper
{
    /// <summary>
    /// Class to support parsing of HTML markup to Unit objects
    /// and saving it to database via the web service.
    /// 
    /// Author: Calvin Che Zi Yi
    /// </summary>
    class ParsingController
    {
        /// <summary>
        /// HttpClient for connecting to web service.
        /// </summary>
        static public HttpClient client;

        /// <summary>
        /// The class constructor. 
        /// </summary>
        public ParsingController()
        {
            // Initialize HttpClient with our web service's url and headers.
            client = new HttpClient();
            client.BaseAddress = new Uri("http://btofindr.cczy.io/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        /// <summary>
        /// Parse a HTML string into Unit objects
        /// and save it to database via the web service.
        /// </summary>
        /// <param name="html">The HTML body of the web page.</param>
        public void ParseUnit(string html)
        {
            // Create new AngleSharp.HtmlParser
            var parser = new HtmlParser();

            // Parse HTML into DOM
            var document = parser.Parse(html);

            // Find all the units in DOM
            var units = document.QuerySelectorAll("font.e-more-info.has-tip.tip-bottom");

            // Create new empty UnitType and child objects
            UnitType unitType = new UnitType();
            unitType.block = new Block();
            unitType.block.project = new Project();

            // Find and set selected town name
            var select = document.QuerySelector("#Town");
            foreach (var item in select.Children)
            {
                if (item.HasAttribute("selected"))
                {
                    unitType.block.project.townName = item.InnerHtml;
                }
            }

            // Find and set selected flat type name
            select = document.QuerySelector("#Flat");
            unitType.unitTypeName = "";
            foreach (var item in select.Children)
            {
                if (item.HasAttribute("selected"))
                {
                    unitType.unitTypeName = item.InnerHtml;
                }
            }

            // Find and set block specifications and images uri
            unitType.block.sitePlan = document.QuerySelector("#vendor1").GetAttribute("src");
            unitType.block.townMap = document.QuerySelector("#vendor2").GetAttribute("src");
            unitType.block.blockPlan = document.QuerySelector("#vendor3").GetAttribute("src");
            unitType.block.unitDist = document.QuerySelector("#vendor4").GetAttribute("src");
            unitType.block.floorPlan = document.QuerySelector("#vendor5").GetAttribute("src");
            unitType.block.layoutIdeas = document.QuerySelector("#vendor6").GetAttribute("src");
            unitType.block.specs = document.QuerySelector("#vendor7").GetAttribute("src");

            // Find and set block address and delivery date
            var blkDetails = document.QuerySelector("#blockDetails");
            unitType.block.blockNo = blkDetails.Children[1].Children[1].InnerHtml.Replace("&nbsp;", " ").Trim();
            unitType.block.street = blkDetails.Children[1].Children[3].InnerHtml.Replace("&nbsp;", " ").Trim();
            unitType.block.deliveryDate = blkDetails.Children[2].Children[1].InnerHtml.Replace("(Estimated)", "").Replace("&nbsp;", " ").Trim();

            // Find and set ethnic qouta of unit type
            string quota = blkDetails.Children[3].Children[1].InnerHtml;
            unitType.quotaMalay = Int32.Parse(GetStringBetween(quota, "Malay-", ", Chinese-").Replace("&nbsp;", " ").Trim());
            unitType.quotaChinese = Int32.Parse(GetStringBetween(quota, ", Chinese-", ", Indian/Other Races-").Replace("&nbsp;", " ").Trim());
            unitType.quotaOthers = Int32.Parse(GetStringBetween(quota, ", Indian/Other Races-", "").Replace("&nbsp;", " ").Trim());

            // Loop through all the units
            foreach (var unit in units)
            {
                // If unit is still available
                // (In the HDB website, if the unit is not available, it is static and therefore does not have an id attribute)
                if (unit.HasAttribute("id"))
                {
                    // Create new Unit and set our UnitType created previously to the Unit
                    Unit u = new Unit();
                    u.unitType = unitType;

                    // Find and set details of Unit
                    u.unitNo = unit.GetAttribute("id");
                    string title = unit.GetAttribute("title");
                    u.price = Int32.Parse(GetStringBetween(title, "$", "<br/>_").Replace(",", ""));
                    u.floorArea = Int32.Parse(GetStringBetween(title, "_<br/>", " Sqm"));

                    // Post unit to web service to save in database
                    HttpResponseMessage response = client.PostAsJsonAsync("Unit/AddUnit", u).Result;
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        /// <summary>
        /// Get the middle substring between 2 substrings from a string token.
        /// </summary>
        /// <param name="token">The full string</param>
        /// <param name="first">The first substring</param>
        /// <param name="second">The second substring</param>
        /// <returns></returns>
        private string GetStringBetween(string token, string first, string second)
        {
            // If string does not contain first substring, return
            if (!token.Contains(first)) return "";

            // String after first substring
            var afterFirst = token.Split(new[] { first }, StringSplitOptions.None)[1];

            // If string does not contain second substring, return
            if (!afterFirst.Contains(second)) return "";

            // String after first substring and before second substring
            var result = afterFirst.Split(new[] { second }, StringSplitOptions.None)[0];

            // Return middle substring
            return result;
        }
    }
}
