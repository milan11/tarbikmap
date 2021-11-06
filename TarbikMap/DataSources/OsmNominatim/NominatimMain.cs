namespace TarbikMap.DataSources.OsmNominatim
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using TarbikMap.Common.Downloader;

    internal static class NominatimMain
    {
        internal static async Task<List<NominatimApiResult>> QueryNominatim(IDownloader downloader, string name)
        {
            Uri url = new Uri("https://nominatim.openstreetmap.org/search?q=" + WebUtility.UrlEncode(name) + "&format=json");
            string content = Encoding.UTF8.GetString(await downloader.HttpGet(url).ConfigureAwait(false));
            return JsonSerializer.Deserialize<List<NominatimApiResult>>(content)!;
        }
    }
}
