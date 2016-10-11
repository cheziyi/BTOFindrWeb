using AngleSharp.Parser.Html;
using BTOFindr.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTOFindrScraper
{
    public partial class Form1 : Form
    {
        static public HttpClient client;

        List<string> visited = new List<string>();

        int action = 0;

        string[] lvl1urls;
        string[] lvl2urls;

        int progress;
        public Form1()
        {
            InitializeComponent();
            client = new HttpClient();
#if DEBUG
            client.BaseAddress = new Uri("http://localhost:8827/api/");
#else
            client.BaseAddress = new Uri("http://btofindr.cczy.io/api/");
#endif
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        private void btnProjects_Click(object sender, EventArgs e)
        {
            DisableAllButtons();
            action = 1;
            visited.Clear();

            progress = 0;

            webBrowser1.Navigate("https://services2.hdb.gov.sg/webapp/BP13AWFlatAvail/BP13SEstateSummary?sel=BTO");
            lblStatus.Text = "Navigating...";
        }

        private void btnBlocks_Click(object sender, EventArgs e)
        {
            DisableAllButtons();
            action = 2;
            visited.Clear();

            lvl1urls = File.ReadAllLines("lvl1.txt");
            progress = 0;

            webBrowser1.Navigate(lvl1urls[progress]);
            lblStatus.Text = "Navigating...";
        }

        private void btnUnits_Click(object sender, EventArgs e)
        {
            DisableAllButtons();
            action = 3;
            visited.Clear();

            lvl1urls = File.ReadAllLines("lvl1.txt");
            lvl2urls = File.ReadAllLines("lvl2.txt");
            progress = 0;

            webBrowser1.Navigate(lvl1urls[0]);
            lblStatus.Text = "Navigating...";
        }


        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            lblStatus.Text = "Done.";

            if (action == 1)
            {
                lvl1_Crawl();
            }

            if (action == 2)
            {
                lvl2_Crawl();
            }

            if (action == 3)
            {
                if (!webBrowser1.Url.AbsoluteUri.Contains("Neighbourhood"))
                {
                    webBrowser1.Navigate(lvl2urls[progress]);
                }
                else
                {
                    lvl3_Crawl();
                }
            }
        }

        private void lvl1_Crawl()
        {
            if (webBrowser1.Url.AbsoluteUri.Contains("BP13EBSFlatSearch"))
            {
                //textBox1.Text = webBrowser1.Url.AbsoluteUri + Environment.NewLine;

                File.AppendAllText("lvl1.txt", webBrowser1.Url.AbsoluteUri + Environment.NewLine);

                webBrowser1.Navigate("https://services2.hdb.gov.sg/webapp/BP13AWFlatAvail/BP13SEstateSummary?sel=BTO");
                lblStatus.Text = "Navigating...";
            }
            else
            {
                HtmlElementCollection links = webBrowser1.Document.GetElementsByTagName("a");

                bool completed = true;
                foreach (HtmlElement link in links)
                {
                    if (link.OuterHtml.Contains("BP13EBSFlatSearch") && !visited.Contains(link.OuterHtml))
                    {
                        completed = false;
                        visited.Add(link.OuterHtml);
                        link.InvokeMember("Click");
                        lblStatus.Text = "Navigating...";
                        break;
                    }
                }

                if (completed)
                {
                    EnableAllButtons();
                    MessageBox.Show("Done!");
                }
            }
        }

        private void lvl2_Crawl()
        {
            if (!webBrowser1.Url.AbsoluteUri.Contains("Neighbourhood"))
            {
                HtmlElementCollection links = webBrowser1.Document.GetElementsByTagName("a");

                bool completed = true;
                foreach (HtmlElement link in links)
                {
                    if (link.OuterHtml.Contains("#000099") && !visited.Contains(link.OuterHtml))
                    {
                        completed = false;
                        visited.Add(link.OuterHtml);
                        link.InvokeMember("Click");
                        lblStatus.Text = "Navigating...";
                        break;
                    }
                }

                if (completed)
                {
                    visited.Clear();
                    progress++;
                    if (progress == lvl1urls.Length)
                    {
                        EnableAllButtons();
                        MessageBox.Show("Done!");
                    }
                    else
                    {
                        webBrowser1.Navigate(lvl1urls[progress]);
                        lblStatus.Text = "Navigating...";
                    }
                }
            }
        }

        private void lvl3_Crawl()
        {
            if (!webBrowser1.Url.AbsoluteUri.Contains("Short+Lease"))
            {

                var html = webBrowser1.DocumentText;
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
                        //InsertIntoDb(u);

                    }


                }

            }

            progress++;
            if (progress == lvl2urls.Length)
            {
                EnableAllButtons();
                MessageBox.Show("Done!");
            }
            else
            {
                webBrowser1.Navigate(lvl2urls[progress]);
                lblStatus.Text = "Navigating...";
            }

        }

        private void DisableAllButtons()
        {
            btnProjects.Enabled = false;
            btnBlocks.Enabled = false;
            btnUnits.Enabled = false;
        }

        private void EnableAllButtons()
        {
            btnProjects.Enabled = true;
            btnBlocks.Enabled = true;
            btnUnits.Enabled = true;
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
