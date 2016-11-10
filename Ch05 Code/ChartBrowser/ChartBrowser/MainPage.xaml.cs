using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Microsoft.SharePoint.Client;
using System.ServiceModel.Syndication;
using System.IO;
using System.Windows.Browser;
using System.Xml;

namespace ChartBrowser
{
    public partial class MainPage : UserControl
    {
        ListItemCollection docs;
        ClientContext context;
        List list;
        Workbook workbook;
        string siteRoot = "http://sharepoint/sites/chadwach/";


        private delegate void UpdateUI(bool success);


        public struct Workbook
        {
            public string Name { get; set; }
            public string FileName { get; set; }
        }

        public MainPage()
        {
            InitializeComponent();
            LoadSpreadsheets();
        }

        private void LoadSpreadsheets()
        {
            context = new ClientContext(siteRoot);
            list = context.Web.Lists.GetByTitle("EventBudgets");

            CamlQuery query = new CamlQuery();
            query.ViewXml = "<View><Query><Where><Contains><FieldRef Name='FileLeafRef' />" +
                "<Value Type='Text'>Budget</Value></Contains></Where></Query><ViewFields>" +
                "<FieldRef Name='FileLeafRef' /></ViewFields></View>";

            docs = list.GetItems(query);
            context.Load(docs);
            context.ExecuteQueryAsync(QuerySucceded, QueryFailed);
        }

        private void QuerySucceded(object sender, ClientRequestSucceededEventArgs args)
        {
            UpdateUI updateUI = QueryComplete;
            this.Dispatcher.BeginInvoke(updateUI, true);
        }

        private void QueryFailed(object sender, ClientRequestFailedEventArgs args)
        {
            UpdateUI updateUI = QueryComplete;
            this.Dispatcher.BeginInvoke(updateUI, false);
        }

        private void QueryComplete(bool success)
        {
            if (success && docs.Count > 0)
            {
                List<Workbook> workbooks = new List<Workbook>();
                foreach (ListItem li in docs)
                {
                    workbooks.Add(new Workbook
                    {
                        Name = li["FileLeafRef"].ToString().Replace(".xlsx", ""),
                        FileName = li["FileLeafRef"].ToString()
                    });
                }
                workbookList.ItemsSource = workbooks;
                workbookList.SelectedIndex = 0;
            }
            else
                workbookList.Items.Clear();
        }

        private void WorkbookSelected(object sender, SelectionChangedEventArgs e)
        {
            workbook = ((Workbook)((ListBox)sender).SelectedItem);
            chart.Source = null;
            WebClient request = new WebClient();
            request.DownloadStringCompleted += 
                new DownloadStringCompletedEventHandler(LoadChartsComplete);
            request.DownloadStringAsync(new Uri(siteRoot + 
                "_vti_bin/ExcelRest.aspx/EventBudgets/" + workbook.FileName + "/model/Charts"));
        }

        private void LoadChartsComplete(object sender, DownloadStringCompletedEventArgs e)
        {
            var reader = XmlReader.Create(new StringReader(e.Result));
            var feed = SyndicationFeed.Load(reader);
            chartList.ItemsSource = feed.Items;
        }

        private void ChartSelected(object sender, MouseButtonEventArgs e)
        {
            chart.Source = ((Image)sender).Source;
        }

        private void ViewWorkbook(object sender, RoutedEventArgs ev)
        {
            var options = HtmlPage.Window.CreateInstance("SP.UI.DialogOptions");
            options.SetProperty("showMaximized", true);
            options.SetProperty("url", siteRoot + "SitePages/EventBudgetComment.aspx?workbook=" + 
                siteRoot + "EventBudgets/" + workbook.FileName);
            options.SetProperty("autoSize", false);
            options.SetProperty("title", workbook.Name);
            HtmlPage.Window.CreateInstance("SP.UI.ModalDialog.showModalDialog", options);
        }
    }
}
