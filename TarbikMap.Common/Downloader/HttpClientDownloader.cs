namespace TarbikMap.Common.Downloader
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public sealed class HttpClientDownloader : IDownloader, IDisposable
    {
        private HttpClient client = new HttpClient();

        public HttpClientDownloader()
        {
            this.client.DefaultRequestHeaders.Add("User-Agent", "TarbikMap");
        }

        public void Dispose()
        {
            this.client.Dispose();
        }

        public async Task<byte[]> HttpGet(Uri url)
        {
            using (HttpResponseMessage response = await this.client.GetAsync(url).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
        }

        public async Task<byte[]> HttpPost(Uri url, byte[] postData)
        {
            using var content = new ByteArrayContent(postData);
            using (HttpResponseMessage response = await this.client.PostAsync(url, content).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
        }

        public async Task<byte[]> HttpPost(Uri url, Dictionary<string, string> formData)
        {
            using var content = new FormUrlEncodedContent(formData);
            using (HttpResponseMessage response = await this.client.PostAsync(url, content).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
        }
    }
}