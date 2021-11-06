namespace TarbikMap.DataSources.GoogleStreetViewMetadata
{
    using System;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using TarbikMap.Common.Downloader;

    internal static class GoogleStreetViewMetadataMain
    {
        public static async Task<GoogleStreetViewMetadataApiResult> Get(IDownloader downloader, EnvironmentConfig environmentConfig, double lat, double lng, int radius)
        {
            Uri metadataUrl = new Uri($"https://maps.googleapis.com/maps/api/streetview/metadata?location={lat},{lng}&radius={radius}&source=outdoor&key={environmentConfig.GetPrivateConfigValue("GoogleStreetViewStaticApiKey")}");
            string metadataStr = Encoding.UTF8.GetString(await downloader.HttpGet(metadataUrl).ConfigureAwait(false));
            return JsonSerializer.Deserialize<GoogleStreetViewMetadataApiResult>(metadataStr)!;
        }
    }
}