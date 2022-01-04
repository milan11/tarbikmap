namespace TarbikMap.DataSources.Wikimedia
{
    internal class WikimediaAttributionResult
    {
        public WikimediaAttributionResult(string? artistHtml, string? licenseShortName, string? licenseUrl)
        {
            this.ArtistHtml = artistHtml;
            this.LicenseShortName = licenseShortName;
            this.LicenseUrl = licenseUrl;
        }

        public string? ArtistHtml { get; private set; }

        public string? LicenseShortName { get; private set; }

        public string? LicenseUrl { get; private set; }
    }
}