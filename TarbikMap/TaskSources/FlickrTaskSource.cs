namespace TarbikMap.TaskSources
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Shape.Random;
    using TarbikMap.Common.Downloader;
    using TarbikMap.DataSources.FlickrMetadata;
    using TarbikMap.Storage;

    internal class FlickrTaskSource : ITaskSource
    {
        private const string Label = "Flickr";

        private IDownloader downloader;
        private EnvironmentConfig environmentConfig;

        public FlickrTaskSource(IDownloader downloader, EnvironmentConfig environmentConfig)
        {
            this.downloader = downloader;
            this.environmentConfig = environmentConfig;
        }

        public Task<List<GameType>> Search(string query)
        {
            return Task.FromResult(new List<GameType> { new GameType("default", Label, "Photos") });
        }

        public Task<string> GetLabel(string gameTypeKey)
        {
            return Task.FromResult(Label);
        }

        public bool IsFinite(string gameTypeKey)
        {
            return false;
        }

        public async Task<GameTask> CreateTask(string gameTypeKey, string areaKey, Geometry geometry, IList<GameTask> existingTasks)
        {
            GeometryFactory factory = new GeometryFactory();

            List<FoundItem> foundItems = new List<FoundItem>();

            for (int i = 0; i <= 20; ++i)
            {
                RandomPointsBuilder randomPointsBuilder = new RandomPointsBuilder(factory);
                randomPointsBuilder.SetExtent(geometry);
                randomPointsBuilder.NumPoints = 1;
                var randomPoint = randomPointsBuilder.GetGeometry().Centroid;

                int radius = (int)(Math.Pow(2, i) * 50);
                if (radius > 5000)
                {
                    radius = 5000;
                }

                FlickrMetadataApiResult metadata = await FlickrMetadataMain.Get(this.downloader, this.environmentConfig, randomPoint.Y, randomPoint.X, radius).ConfigureAwait(false);

                if (metadata.Status == "ok")
                {
                    foreach (var item in metadata.CurrentPageItems)
                    {
                        if (geometry.Contains(new Point(new Coordinate(double.Parse(item.Lng, CultureInfo.InvariantCulture), double.Parse(item.Lat, CultureInfo.InvariantCulture)))))
                        {
                            foundItems.Add(new FoundItem(item, randomPoint.Y, randomPoint.X));
                        }
                    }

                    if (foundItems.Count > 0)
                    {
                        break;
                    }
                }
                else
                {
                    throw new ExternalDataException("Invalid status");
                }
            }

            {
                var selectedItem = foundItems.OrderBy(item => CalculationUtils.DistanceBetween(double.Parse(item.Item.Lat, CultureInfo.InvariantCulture), double.Parse(item.Item.Lng, CultureInfo.InvariantCulture), item.QueriedLat, item.QueriedLng)).First();

                var images = new List<TaskImage>();

                images.Add(new TaskImage(TaskImage.AccessType.HTTP, new Uri(selectedItem.Item.Url)));

                string description = string.Empty;

                var envelope = geometry.EnvelopeInternal;

                return new GameTask(
                    new TaskQuestion(images, envelope.MinY, envelope.MaxY, envelope.MinX, envelope.MaxX),
                    new TaskAnswer(double.Parse(selectedItem.Item.Lat, CultureInfo.InvariantCulture), double.Parse(selectedItem.Item.Lng, CultureInfo.InvariantCulture), description),
                    new TaskCreationDetails(selectedItem.QueriedLat, selectedItem.QueriedLng));
            }
        }

        public Task<List<GameTask>> CreateTasks(string gameTypeKey, string areaKey, Geometry geometry, int maxCount)
        {
            throw new InvalidOperationException();
        }

        private class FoundItem
        {
            public FoundItem(FlickrMetadataApiItem item, double queriedLat, double queriedLng)
            {
                this.Item = item;
                this.QueriedLat = queriedLat;
                this.QueriedLng = queriedLng;
            }

            public FlickrMetadataApiItem Item { get; private set; }

            public double QueriedLat { get; private set; }

            public double QueriedLng { get; private set; }
        }
    }
}
