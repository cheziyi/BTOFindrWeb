using AngleSharp.Parser.Html;
using BTOFindr.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BTOFindrScraper
{
    class ParsingController
    {
        static public HttpClient client;

        public ParsingController()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri("http://btofindr.cczy.io/api/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    }
        public void ParseUnit(string html)
        {
            var parser = new HtmlParser();
            var document = parser.Parse(html);

            var units = document.QuerySelectorAll("font.e-more-info.has-tip.tip-bottom");
            //  var prices = document.QuerySelectorAll("span.tooltip.tip-bottom");

            UnitType unitType = new UnitType();
            unitType.block = new Block();

            var select = document.QuerySelector("#Town");

            foreach (var item in select.Children)
            {
                if (item.HasAttribute("selected"))
                {
                    unitType.block.project.townName = item.InnerHtml;
                }
            }

            select = document.QuerySelector("#Flat");

            unitType.unitTypeName = "";

            foreach (var item in select.Children)
            {
                if (item.HasAttribute("selected"))
                {
                    unitType.unitTypeName = item.InnerHtml;
                }
            }

            unitType.block.sitePlan = document.QuerySelector("#vendor1").GetAttribute("src");
            unitType.block.townMap = document.QuerySelector("#vendor2").GetAttribute("src");
            unitType.block.blockPlan = document.QuerySelector("#vendor3").GetAttribute("src");
            unitType.block.unitDist = document.QuerySelector("#vendor4").GetAttribute("src");
            unitType.block.floorPlan = document.QuerySelector("#vendor5").GetAttribute("src");
            unitType.block.layoutIdeas = document.QuerySelector("#vendor6").GetAttribute("src");
            unitType.block.specs = document.QuerySelector("#vendor7").GetAttribute("src");


            var blkDetails = document.QuerySelector("#blockDetails");

            unitType.block.blockNo = blkDetails.Children[1].Children[1].InnerHtml.Replace("&nbsp;", " ").Trim();
            unitType.block.street = blkDetails.Children[1].Children[3].InnerHtml.Replace("&nbsp;", " ").Trim();

            unitType.block.deliveryDate = blkDetails.Children[2].Children[1].InnerHtml.Replace("(Estimated)", "").Replace("&nbsp;", " ").Trim();

            string quota = blkDetails.Children[3].Children[1].InnerHtml;
            unitType.quotaMalay = Int32.Parse(GetStringBetween(quota, "Malay-", ", Chinese-").Replace("&nbsp;", " ").Trim());
            unitType.quotaChinese = Int32.Parse(GetStringBetween(quota, ", Chinese-", ", Indian/Other Races-").Replace("&nbsp;", " ").Trim());
            unitType.quotaOthers = Int32.Parse(GetStringBetween(quota, ", Indian/Other Races-", "").Replace("&nbsp;", " ").Trim());

            foreach (var unit in units)
            {
                if (unit.HasAttribute("id"))
                // if (unit.GetAttribute("id").Contains("#"))
                {
                    Unit u = new Unit();
                    u.unitType = unitType;

                    u.unitNo = unit.GetAttribute("id");
                    string title = unit.GetAttribute("title");
                    u.price = Int32.Parse(GetStringBetween(title, "$", "<br/>_").Replace(",", ""));
                    u.floorArea = Int32.Parse(GetStringBetween(title, "_<br/>", " Sqm"));

                    // textBox1.Text = u.FlatType.Block.BlockNo + Environment.NewLine + u.UnitNo;

                    HttpResponseMessage response = client.PostAsJsonAsync("Unit/AddUnit", u).Result;
                    response.EnsureSuccessStatusCode();

                    int content = response.Content.ReadAsAsync<int>().Result;
                }
            }
        }


        private string GetStringBetween(string token, string first, string second)
        {
            if (!token.Contains(first)) return "";

            var afterFirst = token.Split(new[] { first }, StringSplitOptions.None)[1];

            if (!afterFirst.Contains(second)) return "";

            var result = afterFirst.Split(new[] { second }, StringSplitOptions.None)[0];

            return result;
        }
    }
}
