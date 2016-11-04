using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BTOFindrScraper
{
    /// <summary>
    /// UI class to scrap data from HDB's website.
    /// Uses a WebBrowser control to navigate through the website.
    /// Note: Flat type is a HDB term similar to our UnitType object.
    /// 
    /// Author: Calvin Che Zi Yi
    /// </summary>
    public partial class ScrapingForm : Form
    {
        /// <summary>
        /// New instance of ParsingController.
        /// </summary>
        ParsingController pc = new ParsingController();

        /// <summary>
        /// To keep track of visited urls history.
        /// </summary>
        List<string> visited = new List<string>();

        /// <summary>
        /// To keep track of crawling level
        /// </summary>
        int action = 0;

        /// <summary>
        /// For storing of navigable urls
        /// </summary>
        List<string> flatTypeUris;
        List<string> blockUris;

        /// <summary>
        /// For tracking progress of navigable urls
        /// </summary>
        int progress;

        /// <summary>
        /// The class constructor.
        /// </summary>
        public ScrapingForm()
        {
            InitializeComponent();
            // Initialize uri lists
            flatTypeUris = new List<string>();
            blockUris = new List<string>();
        }

        /// <summary>
        /// Event Handler for "Initiate Scraping" button click.
        /// </summary>
        /// <param name="sender">"Initiate Scraping" Button</param>
        /// <param name="e">Event arguments</param>
        private void btnScrape_Click(object sender, EventArgs e)
        {
            // Disable button
            btnScrape.Enabled = false;
            // Start crawling through all the flat types
            CrawlFlatTypes();
        }

        /// <summary>
        /// Initiate crawling through all the available flat types in all the towns.
        /// Gets all the links from the main page:
        /// https://services2.hdb.gov.sg/webapp/BP13AWFlatAvail/BP13SEstateSummary?sel=BTO
        /// Saves the links in flatTypeUris.
        /// </summary>
        private void CrawlFlatTypes()
        {
            // Set crawling level to 1 (Flat Types)
            action = 1;
            // Clear url history
            visited.Clear();
            // Reset progress
            progress = 0;
            // Navigate to main page
            webBrowser1.Navigate("https://services2.hdb.gov.sg/webapp/BP13AWFlatAvail/BP13SEstateSummary?sel=BTO");
            lblStatus.Text = "Navigating...";
        }

        /// <summary>
        /// Initiate crawling through all the available blocks in each flat type.
        /// Gets all the links in each of the flatTypeUris pages.
        /// Saves the links in blockUris.
        /// </summary>
        private void CrawlBlocks()
        {
            // Set crawling level to 2 (Blocks)
            action = 2;
            // Clear url history
            visited.Clear();
            // Reset progress
            progress = 0;
            // Navigate to first flat type url
            webBrowser1.Navigate(flatTypeUris[progress]);
            lblStatus.Text = "Navigating...";
        }

        /// <summary>
        /// Initiate scraping of all the units in each block. 
        /// Navigates through all of the blockUris pages.
        /// Parse and save each unit into database.
        /// </summary>
        private void ScrapeUnits()
        {
            // Set crawling level to 3 (Units)
            action = 3;
            // Clear url history
            visited.Clear();
            // Reset progress
            progress = 0;
            // Navigate to first flat type url, after which navigate to blockUris.
            // HDB's website does not allow direct navigation to the block uris.
            webBrowser1.Navigate(flatTypeUris[0]);
            lblStatus.Text = "Navigating...";
        }

        /// <summary>
        /// Event handler when a web page has finished loading.
        /// </summary>
        /// <param name="sender">WebBrowser control</param>
        /// <param name="e">Event arguments</param>
        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            lblStatus.Text = "Done.";
            // If crawling level is 1 (Flat Types)
            // Save the url to flatTypeUris
            if (action == 1)
            {
                SaveFlatTypeUri();
            }

            // If crawling level is 2 (Blocks)
            // Navigate to the next block url
            if (action == 2)
            {
                NavigateBlockUri();
            }

            // If crawling level is 3 (Units)
            if (action == 3)
            {
                // If url not a block url (flat type url)
                if (!webBrowser1.Url.AbsoluteUri.Contains("Neighbourhood"))
                {
                    // Navigate to a block url
                    webBrowser1.Navigate(blockUris[progress]);
                }
                else
                {
                    // Process, parse and save units
                    ProcessUnits();
                }
            }
        }

        /// <summary>
        /// Event handler when the WebBrower control has navigated to a page.
        /// </summary>
        /// <param name="sender">WebBrowser control</param>
        /// <param name="e">Event arguments</param>
        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            // If crawling level is 2 (Blocks)
            // Save the url to blockUris
            if (action == 2)
            {
                SaveBlockUri();
            }
        }

        /// <summary>
        /// Saves the current url to flatTypeUris, and navigate to the next flat type url.
        /// </summary>
        private void SaveFlatTypeUri()
        {
            // If current url is a flat type url
            if (webBrowser1.Url.AbsoluteUri.Contains("BP13EBSFlatSearch"))
            {
                // Save to url list
                flatTypeUris.Add(webBrowser1.Url.AbsoluteUri);

                // Navigate back to main page
                webBrowser1.Navigate("https://services2.hdb.gov.sg/webapp/BP13AWFlatAvail/BP13SEstateSummary?sel=BTO");
                lblStatus.Text = "Navigating...";
            }
            // If current url is the main page
            else
            {
                // Get all the links in main page
                HtmlElementCollection links = webBrowser1.Document.GetElementsByTagName("a");

                // Completed flag to know if all the links in main page has been visited
                bool completed = true;

                // Loop through all the links in the main page
                foreach (HtmlElement link in links)
                {
                    // If link is a flat type url and has not been visited
                    if (link.OuterHtml.Contains("BP13EBSFlatSearch") && !visited.Contains(link.OuterHtml))
                    {
                        // Set completed flag to false
                        completed = false;
                        // Add url to link history
                        visited.Add(link.OuterHtml);
                        // Navigate to flat type url
                        link.InvokeMember("Click");
                        lblStatus.Text = "Navigating...";
                        break;
                    }
                }

                // If all the links in main page has been visited
                if (completed)
                {
                    // Initiate crawling of block urls
                    CrawlBlocks();
                }
            }
        }

        /// <summary>
        /// Navigates to the next flat type url.
        /// </summary>
        private void NavigateBlockUri()
        {
            // If url not a block url (flat type url)
            if (!webBrowser1.Url.AbsoluteUri.Contains("Neighbourhood"))
            {
                // Gets all the links in the flat type page
                HtmlElementCollection links = webBrowser1.Document.GetElementsByTagName("a");

                // Completed flag to know if all the links has been visited
                bool completed = true;

                // Loop through all the links
                foreach (HtmlElement link in links)
                {
                    // If block is still available, and has not been visited
                    if (link.OuterHtml.Contains("#000099") && !visited.Contains(link.OuterHtml))
                    {
                        // Set completed flag to false
                        completed = false;
                        // Add url to link history
                        visited.Add(link.OuterHtml);
                        // Navigate to block url
                        link.InvokeMember("Click");
                        lblStatus.Text = "Navigating...";
                        break;
                    }
                }

                // If all the links in flat type page has been visited
                if (completed)
                {
                    // Clear url history
                    visited.Clear();
                    // Increment flat type url index
                    progress++;
                    // If index is equals to the flat type url list size (completed)
                    if (progress == flatTypeUris.Count)
                    {
                        // Initiate scraping of units
                        ScrapeUnits();
                    }
                    else
                    {
                        // Navigate to the next flat type url
                        webBrowser1.Navigate(flatTypeUris[progress]);
                        lblStatus.Text = "Navigating...";
                    }
                }
            }
        }

        /// <summary>
        /// Saves the current url to blockUris, and navigate to the next block url.
        /// </summary>
        private void SaveBlockUri()
        {
            // If url is a block url
            if (webBrowser1.Url.AbsoluteUri.Contains("Neighbourhood"))
            {
                // Save to url list
                blockUris.Add(webBrowser1.Url.AbsoluteUri);
                // Navigate to the next block url
                webBrowser1.Navigate(flatTypeUris[progress]);
                lblStatus.Text = "Navigating...";
            }
        }

        /// <summary>
        /// Initiates parsing and saving of units to database.
        /// </summary>
        private void ProcessUnits()
        {
            // For the time being, do not include short lease flats
            // as they are fundamentally very different from normal flats
            if (!webBrowser1.Url.AbsoluteUri.Contains("Short+Lease"))
            {
                // Get html string and send it to parser
                var html = webBrowser1.DocumentText;
                pc.ParseUnit(html);
            }
            // Increment block urls index
            progress++;
            // If index is equals to the block url list size (completed)
            if (progress == blockUris.Count)
            {
                // Re-enable scraping button
                btnScrape.Enabled = true;
                // Show notification
                MessageBox.Show("Scraping Completed!");
            }
            else
            {
                // Navigate to the next block url
                webBrowser1.Navigate(blockUris[progress]);
                lblStatus.Text = "Navigating...";
            }
        }
    }
}
