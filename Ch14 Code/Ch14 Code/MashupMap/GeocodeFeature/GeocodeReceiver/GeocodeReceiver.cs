using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
//added
using GeocodeFeature.GeocodeService;
using System.ServiceModel;


namespace GeocodeFeature.GeocodeReceiver
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class GeocodeReceiver : SPItemEventReceiver
    {
       /// <summary>
       /// An item is being added.
       /// </summary>
       public override void ItemAdding(SPItemEventProperties properties)
       {
           GeoCodeItem(properties);
           base.ItemAdding(properties);
       }

       /// <summary>
       /// An item is being updated.
       /// </summary>
       public override void ItemUpdating(SPItemEventProperties properties)
       {
           GeoCodeItem(properties);
           base.ItemUpdating(properties);
       }

       private void GeoCodeItem(SPItemEventProperties properties)
       {
           string bingMapsKey = "YOURKEY";
           GeocodeRequest geocodeRequest = new GeocodeRequest();

           // Set the credentials using a valid Bing Maps key
           geocodeRequest.Credentials = new GeocodeService.Credentials();
           geocodeRequest.Credentials.ApplicationId = bingMapsKey;

           // Set the full address query
           string addressTemplate = "{0},{1},{2},{3}";
           geocodeRequest.Query = string.Format(addressTemplate, properties.AfterProperties["WorkAddress"].ToString(),
                                                                 properties.AfterProperties["WorkCity"].ToString(),
                                                                 properties.AfterProperties["WorkState"].ToString(),
                                                                 properties.AfterProperties["WorkZip"].ToString());

           // Set the options to only return high confidence results 
           ConfidenceFilter[] filters = new ConfidenceFilter[1];
           filters[0] = new ConfidenceFilter();
           filters[0].MinimumConfidence = GeocodeService.Confidence.High;

           GeocodeOptions geocodeOptions = new GeocodeOptions();
           geocodeOptions.Filters = filters;

           geocodeRequest.Options = geocodeOptions;

           // Make the geocode request
           BasicHttpBinding binding1 = new BasicHttpBinding();
           EndpointAddress addr = new EndpointAddress("http://dev.virtualearth.net/webservices/v1/geocodeservice/GeocodeService.svc");
           GeocodeServiceClient geocodeService = new GeocodeServiceClient(binding1, addr);

           GeocodeResponse geocodeResponse = geocodeService.Geocode(geocodeRequest);

           // The result is a GeocodeResponse object
           if (geocodeResponse.Results.Length > 0)
           {
               properties.AfterProperties["Latitude"] = geocodeResponse.Results[0].Locations[0].Latitude;
               properties.AfterProperties["Longitude"] = geocodeResponse.Results[0].Locations[0].Longitude;
               properties.AfterProperties["GeocodeStatus"] = "Geocoded";
           }
       }


    }
}
