namespace TarbikMap
{
    using TarbikMap.Common.Downloader;

    internal static class Factories
    {
        private const string CacheDirectoryConfigKey = "DownloaderCacheDirectory";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2000", Justification = "Global variable")]
        public static IDownloader CreateDownloader(EnvironmentConfig environmentConfig)
        {
            if (environmentConfig.HasPrivateConfigValue(CacheDirectoryConfigKey))
            {
                string downloaderCacheDirectory = environmentConfig.GetPrivateConfigValue(CacheDirectoryConfigKey);
                return new CachingDownloader(new HttpClientDownloader(), downloaderCacheDirectory);
            }
            else
            {
                return new HttpClientDownloader();
            }
        }
    }
}
