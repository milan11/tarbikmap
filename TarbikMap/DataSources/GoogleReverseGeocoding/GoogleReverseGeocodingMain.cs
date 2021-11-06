namespace TarbikMap.DataSources.GoogleReverseGeocoding
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using TarbikMap.Common.Downloader;

    internal static class GoogleReverseGeocodingMain
    {
        public static async Task<GoogleReverseGeocodingApiResult> Get(IDownloader downloader, EnvironmentConfig environmentConfig, double lat, double lng)
        {
            Uri reverseGeocodingUrl = new Uri($"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lng}&radius=10000&key={environmentConfig.GetPrivateConfigValue("GoogleGeocodingApiKey")}");
            string reverseGeocodingStr = Encoding.UTF8.GetString(await downloader.HttpGet(reverseGeocodingUrl).ConfigureAwait(false));
            return JsonSerializer.Deserialize<GoogleReverseGeocodingApiResult>(reverseGeocodingStr)!;
        }
    }
}