namespace TarbikMap.DataSources.OpenStreetCamMetadata
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using TarbikMap.Common.Downloader;

    internal static class OpenStreetCamMetadataMain
    {
        public static async Task<OpenStreetCamMetadataApiResult> Get(IDownloader downloader, double lat, double lng, int radius)
        {
            Uri metadataUrl = new Uri("http://openstreetcam.org/1.0/list/nearby-photos/");

            var formData = new Dictionary<string, string>
            {
                { "lat", lat.ToString(CultureInfo.InvariantCulture) },
                { "lng", lng.ToString(CultureInfo.InvariantCulture) },
                { "radius", radius.ToString(CultureInfo.InvariantCulture) },
            };

            string metadataStr = Encoding.UTF8.GetString(await downloader.HttpPost(metadataUrl, formData).ConfigureAwait(false));
            return JsonSerializer.Deserialize<OpenStreetCamMetadataApiResult>(metadataStr)!;
        }
    }
}