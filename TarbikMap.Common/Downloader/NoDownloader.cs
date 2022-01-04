namespace TarbikMap.Common.Downloader
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public sealed class NoDownloader : IDownloader
    {
        public Task<byte[]> HttpGet(Uri url)
        {
            throw CreateException();
        }

        public Task<byte[]> HttpPost(Uri url, byte[] postData)
        {
            throw CreateException();
        }

        public Task<byte[]> HttpPost(Uri url, Dictionary<string, string> formData)
        {
            throw CreateException();
        }

        private static Exception CreateException()
        {
            return new InvalidOperationException("Downloading is not supported");
        }
    }
}