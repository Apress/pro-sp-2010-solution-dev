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
//added
using SP = Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client;
using Microsoft.Maps.MapControl;
using System.Xml;
using System.ServiceModel.Syndication;
using System.IO;

namespace MashupMap
{
    public partial class MainPage : UserControl
    {
        SP.ClientContext context = null;
        SP.Web web = null;
        SP.List campgroundList = null;
        SP.ListItemCollection campgroundItems = null;


        private bool alertsLoaded = false;
        private delegate void UpdateUIMethod();

        public MainPage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            context = SP.ClientContext.Current;
            web = context.Web;
            campgroundList = web.Lists.GetByTitle("Campgrounds");
            SP.CamlQuery query = new SP.CamlQuery();
            query.ViewXml = "<View><Query><Where><Eq>" +
                            "<FieldRef Name='GeocodeStatus'/><Value Type='Choice'>Geocoded</Value>" +
                            "</Eq></Where></Query></View>";


            campgroundItems = campgroundList.GetItems(query);

            context.Load(campgroundItems);

            context.ExecuteQueryAsync(requestSucceeded, requestFailed); 
        }
        private void requestSucceeded(object sender, SP.ClientRequestSucceededEventArgs e)
        {
            UpdateUIMethod updateUI = AddCampgroundPins;
            this.Dispatcher.BeginInvoke(updateUI);
        }

        private void requestFailed(object sender, SP.ClientRequestFailedEventArgs e)
        {
            UpdateUIMethod updateUI = CampgroundsNotFound;
            this.Dispatcher.BeginInvoke(updateUI);
        }

        private void AddCampgroundPins()
        {
            foreach (SP.ListItem item in campgroundItems)
            {
                Pushpin pin = new Pushpin();
                pin.Tag = item["Company"].ToString();
                double latitude = (double)item["Latitude"];
                double longitude = (double)item["Longitude"];
                Location loc = new Location(latitude, longitude);
                pin.Location = loc;
                pin.MouseEnter += new MouseEventHandler(pin_MouseEnter);
                pin.MouseLeave += new MouseEventHandler(pin_MouseLeave);

                PinLayer.Children.Add(pin);
            }
        }

        private void CampgroundsNotFound()
        {
            ShowContacts.IsChecked = false;
            MessageBox.Show("Error Retrieving Campground contacts");
        }
        void pin_MouseLeave(object sender, MouseEventArgs e)
        {

            infoBox.Visibility = Visibility.Collapsed;

        }

        void pin_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement pin = sender as FrameworkElement;
            MapLayer.SetPosition(infoBox, MapLayer.GetPosition(pin));
            MapLayer.SetPositionOffset(infoBox, new Point(20, -15));
            pinInfo.Text = pin.Tag as string;
            infoBox.Visibility = Visibility.Visible;
        }


        private void ShowParks_Checked(object sender, RoutedEventArgs e)
        {
            ParkLayer.Visibility = Visibility.Visible;
        }

        private void ShowParks_Unchecked(object sender, RoutedEventArgs e)
        {
            ParkLayer.Visibility = Visibility.Collapsed;
        }

        private void ShowContacts_Unchecked(object sender, RoutedEventArgs e)
        {
            PinLayer.Visibility = Visibility.Collapsed;
        }

        private void ShowContacts_Checked(object sender, RoutedEventArgs e)
        {
            PinLayer.Visibility = Visibility.Visible;
        }

        private void ShowWildlifeAlerts_Checked(object sender, RoutedEventArgs e)
        {
            if (!alertsLoaded)
            {
                MessageBox.Show("Please load the wildlife alerts feed");
                ShowWildlifeAlerts.IsChecked = false;
            }
            else
            {
                AlertLayer.Visibility = Visibility.Visible;
            }
        }

        private void ShowWildlifeAlerts_Unchecked(object sender, RoutedEventArgs e)
        {
            AlertLayer.Visibility = Visibility.Collapsed;
        }

        private void Park_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement pin = sender as FrameworkElement;
            MapLayer.SetPosition(infoBox, MapLayer.GetPosition(pin));
            MapLayer.SetPositionOffset(infoBox, new Point(20, -15));
            pinInfo.Text = pin.Tag as string;
            infoBox.Visibility = Visibility.Visible;
        }

        private void Park_MouseLeave(object sender, MouseEventArgs e)
        {
            infoBox.Visibility = Visibility.Collapsed;
        }

        private void loadFeed_Click(object sender, RoutedEventArgs e)
        {
            if (alertsLoaded) AlertLayer.Children.Clear();

            WebClient wc = new WebClient();
            wc.OpenReadCompleted += new OpenReadCompletedEventHandler(wc_OpenReadCompleted);

            Uri feedUri = new Uri("http://feeds.feedburner.com/wdinNewsDigestGeoRSS", UriKind.Absolute);
            wc.OpenReadAsync(feedUri);
        }
        void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Error: " + e.Error.Message.ToString());
                return;
            }
            using (Stream s = e.Result)
            {
                SyndicationFeed feed;


                using (XmlReader reader = XmlReader.Create(s))
                {
                    feed = SyndicationFeed.Load(reader);
                    foreach (SyndicationItem feedItem in feed.Items)
                    {
                        Ellipse ell = new Ellipse();
                        ell.Height = 10;
                        ell.Width = 10;
                        ell.Fill = new SolidColorBrush(Colors.Red);

                        string point = feedItem.ElementExtensions[0].GetObject<String>();
                        string[] pointParts = point.Split(' ');
                        Location loc = new Location(double.Parse(pointParts[0]), double.Parse(pointParts[1]));

                        AlertLayer.AddChild(ell, loc);
                    }
                }
            }
            alertsLoaded = true;
            ShowWildlifeAlerts.IsChecked = true;
        }
    }
}
