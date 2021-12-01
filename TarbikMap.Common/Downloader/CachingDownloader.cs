namespace TarbikMap.Common.Downloader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class CachingDownloader : IDownloader
    {
        private IDownloader orig;
        private string cachePath;

        public CachingDownloader(IDownloader orig, string cachePath)
        {
            this.orig = orig;
            this.cachePath = cachePath;

            Directory.CreateDirectory(cachePath);
        }

        public async Task<byte[]> HttpGet(Uri url)
        {
            string path = this.CreateFilePath(url, null);

            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }

            var result = await this.orig.HttpGet(url).ConfigureAwait(false);
            File.WriteAllBytes(path + ".bk", result);
            File.Move(path + ".bk", path);

            return result;
        }

        public async Task<byte[]> HttpPost(Uri url, byte[] postData)
        {
            string path = this.CreateFilePath(url, Encoding.UTF8.GetString(postData));

            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }

            var result = await this.orig.HttpPost(url, postData).ConfigureAwait(false);
            File.WriteAllBytes(path + ".bk", result);
            File.Move(path + ".bk", path);

            return result;
        }

        public async Task<byte[]> HttpPost(Uri url, Dictionary<string, string> formData)
        {
            return await this.orig.HttpPost(url, formData).ConfigureAwait(false);
        }

        private string CreateFilePath(Uri url, string? additionalData)
        {
            return Path.Combine(this.cachePath, NameGenerator.CreateName(url + (additionalData != null ? "#" + additionalData : string.Empty)));
        }
    }
}