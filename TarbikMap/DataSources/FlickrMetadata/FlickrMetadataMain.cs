namespace TarbikMap.DataSources.FlickrMetadata
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using TarbikMap.Common.Downloader;

    internal static class FlickrMetadataMain
    {
        public static async Task<FlickrMetadataApiResult> Get(IDownloader downloader, EnvironmentConfig environmentConfig, double lat, double lng, int radius)
        {
            double radius_km = (double)radius / 1000D;

            Uri metadataUrl = new Uri($"https://www.flickr.com/services/rest/?method=flickr.photos.search&api_key={environmentConfig.GetPrivateConfigValue("FlickrApiKey")}&has_geo=1&lat={lat}&lon={lng}&radius={radius_km}&radius_units=km&extras=url_c,geo&license=1,2,3,4,5,6,9,10&geo_context=2&media=photos");

            string metadataStr = Encoding.UTF8.GetString(await downloader.HttpGet(metadataUrl).ConfigureAwait(false));

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(metadataStr);

            var currentPageItems = new List<FlickrMetadataApiItem>();
            var rsp = doc.GetElementsByTagName("rsp")[0];
            var stat = rsp!.Attributes!["stat"];
            var status = stat!.Value;

            foreach (XmlNode node in doc.GetElementsByTagName("photo"))
            {
                if (node.Attributes!["url_c"] != null)
                {
                    var latitude = node.Attributes["latitude"];
                    var longitude = node.Attributes["longitude"];
                    var urlC = node.Attributes["url_c"];

                    currentPageItems.Add(new FlickrMetadataApiItem(
                        latitude!.Value,
                        longitude!.Value,
                        urlC!.Value));
                }
            }

            return new FlickrMetadataApiResult(currentPageItems, status);
        }
    }
}