namespace TarbikMap.DataSources.Wikimedia
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using TarbikMap.Common.Downloader;

    internal static class WikimediaApiMain
    {
        public static async Task<WikimediaAttributionResult> LoadAttribution(IDownloader downloader, string imageKey)
        {
            string url = $"https://en.wikipedia.org/w/api.php?format=json&action=query&prop=imageinfo&iiprop=extmetadata&iiextmetadatafilter=Artist|LicenseShortName|LicenseUrl&titles=File:{imageKey}";

            var apiResponse = await downloader.HttpGet(new Uri(url)).ConfigureAwait(false);

            var json = (JsonElement)JsonSerializer.Deserialize<object>(apiResponse)!;

            var extMetadata = json.GetProperty("query").GetProperty("pages").GetProperty("-1").GetProperty("imageinfo")[0].GetProperty("extmetadata");

            return new WikimediaAttributionResult(
                GetWikimediaProperty(extMetadata, "Artist"),
                GetWikimediaProperty(extMetadata, "LicenseShortName"),
                GetWikimediaProperty(extMetadata, "LicenseUrl"));
        }

        private static string? GetWikimediaProperty(JsonElement extMetadata, string key)
        {
            if (extMetadata.TryGetProperty(key, out JsonElement element))
            {
                return element.GetProperty("value").GetString();
            }
            else
            {
                return null;
            }
        }
    }
}