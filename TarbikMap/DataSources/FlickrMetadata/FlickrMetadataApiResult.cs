namespace TarbikMap.DataSources.FlickrMetadata
{
    using System.Collections.Generic;

    internal class FlickrMetadataApiResult
    {
        public FlickrMetadataApiResult(IList<FlickrMetadataApiItem> currentPageItems, string status)
        {
            this.CurrentPageItems = currentPageItems;
            this.Status = status;
        }

        public IList<FlickrMetadataApiItem> CurrentPageItems { get; private set; }

        public string Status { get; private set; }
    }
}