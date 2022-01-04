namespace TarbikMap.DataSources.FlickrMetadata
{
    internal class FlickrMetadataApiItem
    {
        public FlickrMetadataApiItem(string lat, string lng, string url, string ownerName)
        {
            this.Lat = lat;
            this.Lng = lng;
            this.Url = url;
            this.OwnerName = ownerName;
        }

        public string Lat { get; private set; }

        public string Lng { get; private set; }

        public string Url { get; private set; }

        public string OwnerName { get; private set; }
    }
}