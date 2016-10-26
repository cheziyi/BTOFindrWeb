using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace BTOFindrScraper
{
    public partial class ScrapingForm : Form
    {
        ParsingController pc = new ParsingController();

        List<string> visited = new List<string>();

        int action = 0;

        string[] lvl1urls;
        string[] lvl2urls;

        int progress;

        public ScrapingForm()
        {
            InitializeComponent();
        }

        private void btnProjects_Click(object sender, EventArgs e)
        {
            btnScrape.Enabled = false;

            action = 1;
            visited.Clear();

            progress = 0;

            webBrowser1.Navigate("https://services2.hdb.gov.sg/webapp/BP13AWFlatAvail/BP13SEstateSummary?sel=BTO");
            lblStatus.Text = "Navigating...";
        }

        private void ScrapeBlocks()
        {
            action = 2;
            visited.Clear();

            lvl1urls = File.ReadAllLines("lvl1.txt");
            progress = 0;

            webBrowser1.Navigate(lvl1urls[progress]);
            lblStatus.Text = "Navigating...";
        }

        private void ScrapeUnits()
        {
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
                    ScrapeBlocks();
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
                        ScrapeUnits();
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
                pc.ParseUnit(html);
            }

            progress++;
            if (progress == lvl2urls.Length)
            {
                btnScrape.Enabled = true;
                MessageBox.Show("Scraping Completed!");
            }
            else
            {
                webBrowser1.Navigate(lvl2urls[progress]);
                lblStatus.Text = "Navigating...";
            }
        }
    }
}
