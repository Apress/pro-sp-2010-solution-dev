using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Microsoft.Maps.MapControl;

namespace MashupMap
{
    /// <summary>
    /// A Simple KML parser (parses a small subset of KML)
    /// </summary>
    public class KMLElementCollection : ObservableCollection<KMLElement>
    {
        private Uri uri;

        public KMLElementCollection()
        {
        }

        public Uri Source
        {
            get
            {
                return uri;
            }
            set
            {
                uri = value;
                WebClient webClient = new WebClient();
                webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webClient_DownloadStringCompleted);
                webClient.DownloadStringAsync(uri);
            }
        }

        private void webClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ParseKML(e.Result);
            }
        }

        private void ParseKML(string kmlString)
        {
            XDocument xmlDoc = XDocument.Parse(kmlString);
            XNamespace xn = XNamespace.Get("http://earth.google.com/kml/2.0");
            var placemarks = from placemark in xmlDoc.Descendants(xn + "Placemark")
                             select new KMLElement
                             {
                                 Name = placemark.Element(xn + "name").Value,
                                 Description = placemark.Element(xn + "description").Value,
                                 Location = ParseKMLCoordinates(placemark.Element(xn + "Point").Element(xn + "coordinates").Value),
                             };

            foreach (KMLElement element in placemarks)
            {
                this.Add(element);
            }
        }

        private Location ParseKMLCoordinates(string coordinates)
        {
            Location location = new Location();
            string[] strArray = coordinates.Split(',');
            if (strArray.Length >= 2)
            {
                double latitude;
                if (double.TryParse(strArray[1], out latitude))
                {
                    location.Latitude = latitude;
                }
                double longitude;
                if (double.TryParse(strArray[0], out longitude))
                {
                    location.Longitude = longitude;
                }
            }
            return location;
        }
    }

    /// <summary>
    /// Simple KMLElement
    /// </summary>
    public class KMLElement
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Location Location { get; set; }
    }

}
