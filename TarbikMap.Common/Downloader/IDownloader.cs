namespace TarbikMap.Common.Downloader
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDownloader
    {
        Task<byte[]> HttpGet(Uri url);

        Task<byte[]> HttpPost(Uri url, byte[] postData);

        Task<byte[]> HttpPost(Uri url, Dictionary<string, string> formData);
    }
}