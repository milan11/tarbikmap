namespace TarbikMap.DataSources.FlickrMetadata
{
    internal class FlickrMetadataApiItem
    {
        public FlickrMetadataApiItem(string lat, string lng, string url)
        {
            this.Lat = lat;
            this.Lng = lng;
            this.Url = url;
        }

        public string Lat { get; private set; }

        public string Lng { get; private set; }

        public string Url { get; private set; }
    }
}