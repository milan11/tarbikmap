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
    using TarbikMap.DataSources.OpenStreetCamMetadata;
    using TarbikMap.Storage;

    internal class OpenStreetCamTaskSource : ITaskSource
    {
        private const string Label = "OpenStreetCam";

        private IDownloader downloader;

        public OpenStreetCamTaskSource(IDownloader downloader)
        {
            this.downloader = downloader;
        }

        public Task<List<GameType>> Search(string query)
        {
            return Task.FromResult(new List<GameType> { new GameType("default", Label, "Streets") });
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

                OpenStreetCamMetadataApiResult metadata = await OpenStreetCamMetadataMain.Get(this.downloader, randomPoint.Y, randomPoint.X, radius).ConfigureAwait(false);

                if (metadata.Status.HttpCode == 200)
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
                var imageUrl = $"http://openstreetcam.org/{selectedItem.Item.Name}";

                var taskImage = new TaskImage(imageUrl);
                taskImage.CachedImageAttribution = string.Join(" | ", "OpenStreetCam", selectedItem.Item.Username);
                images.Add(taskImage);

                string description = string.Empty;

                var envelope = geometry.EnvelopeInternal;

                return new GameTask(
                     new TaskQuestion(images, envelope.MinY, envelope.MaxY, envelope.MinX, envelope.MaxX),
                     new TaskAnswer(double.Parse(selectedItem.Item.Lat, CultureInfo.InvariantCulture), double.Parse(selectedItem.Item.Lng, CultureInfo.InvariantCulture), $"{description} ({selectedItem.Item.DateAdded})"),
                     new TaskCreationDetails(selectedItem.QueriedLat, selectedItem.QueriedLng));
            }
        }

        public Task<List<GameTask>> CreateTasks(string gameTypeKey, string areaKey, Geometry geometry, int maxCount)
        {
            throw new InvalidOperationException();
        }

        public Task<byte[]> GetImageData(string gameTypeKey, string imageKey)
        {
            return this.downloader.HttpGet(new Uri(imageKey));
        }

        public Task<string> GetImageAttribution(string gameTypeKey, string imageKey)
        {
            throw new InvalidOperationException("Data should be already cached");
        }

        private class FoundItem
        {
            public FoundItem(OpenStreetCamMetadataApiItem item, double queriedLat, double queriedLng)
            {
                this.Item = item;
                this.QueriedLat = queriedLat;
                this.QueriedLng = queriedLng;
            }

            public OpenStreetCamMetadataApiItem Item { get; private set; }

            public double QueriedLat { get; private set; }

            public double QueriedLng { get; private set; }
        }
    }
}
