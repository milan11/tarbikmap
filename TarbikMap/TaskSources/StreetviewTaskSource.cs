namespace TarbikMap.TaskSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Shape.Random;
    using TarbikMap.Common.Downloader;
    using TarbikMap.DataSources.GoogleReverseGeocoding;
    using TarbikMap.DataSources.GoogleStreetViewMetadata;
    using TarbikMap.Storage;

    internal class StreetviewTaskSource : ITaskSource
    {
        private const string Label = "Street View";

        private IDownloader downloader;
        private EnvironmentConfig environmentConfig;
        private bool skipGoogleApis;
        private bool skipGeocoding;

        public StreetviewTaskSource(IDownloader downloader, EnvironmentConfig environmentConfig, bool skipGoogleApis, bool skipGeocoding)
        {
            this.downloader = downloader;
            this.environmentConfig = environmentConfig;
            this.skipGoogleApis = skipGoogleApis;
            this.skipGeocoding = skipGeocoding;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA5394", Justification = "No cryptography, Random is OK")]
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

                GoogleStreetViewMetadataApiResult metadata;
                if (!this.skipGoogleApis)
                {
                    int radius = (int)(Math.Pow(2, i) * 50);
                    metadata = await GoogleStreetViewMetadataMain.Get(this.downloader, this.environmentConfig, randomPoint.Y, randomPoint.X, radius).ConfigureAwait(false);
                }
                else
                {
                    metadata = new GoogleStreetViewMetadataApiResult()
                    {
                        Copyright = string.Empty,
                        Date = string.Empty,
                        Location = new GoogleStreetViewMetadataApiLatLng()
                        {
                            Lat = randomPoint.Y + 0.001,
                            Lng = randomPoint.X + 0.001,
                        },
                        PanoId = string.Empty,
                        Status = "OK",
                    };
                }

                if (metadata.Status == "OK")
                {
                    if (geometry.Contains(new Point(new Coordinate(metadata.Location.Lng, metadata.Location.Lat))))
                    {
                        foundItems.Add(new FoundItem(metadata, randomPoint.Y, randomPoint.X));
                        break;
                    }
                }
                else if (metadata.Status == "ZERO_RESULTS")
                {
                }
                else
                {
                    throw new ExternalDataException("Invalid status");
                }
            }

            {
                var selectedItem = foundItems.OrderBy(item => CalculationUtils.DistanceBetween(item.Item.Location.Lat, item.Item.Location.Lng, item.QueriedLat, item.QueriedLng)).First();

                var images = new List<TaskImage>();
                Random random = new Random();
                int initialHeading = random.Next(360);

                for (int i = 0; i < 4; ++i)
                {
                    int heading = (initialHeading + (i * 90)) % 360;
                    Uri imageUrl = new Uri($"https://maps.googleapis.com/maps/api/streetview?size=400x600&pano={selectedItem.Item.PanoId}&heading={heading}&key={this.environmentConfig.GetPrivateConfigValue("GoogleStreetViewStaticApiKey")}");

                    images.Add(new TaskImage(TaskImage.AccessType.HTTP, imageUrl));
                }

                string description = string.Empty;
                if (!this.skipGoogleApis && !this.skipGeocoding)
                {
                    var reverseGeocoding = await GoogleReverseGeocodingMain.Get(this.downloader, this.environmentConfig, selectedItem.Item.Location.Lat, selectedItem.Item.Location.Lng).ConfigureAwait(false);

                    if (reverseGeocoding.Status == "OK")
                    {
                        description = reverseGeocoding.Results[0].FormattedAddress;
                    }
                }

                var envelope = geometry.EnvelopeInternal;

                return new GameTask(
                    new TaskQuestion(images, envelope.MinY, envelope.MaxY, envelope.MinX, envelope.MaxX),
                    new TaskAnswer(selectedItem.Item.Location.Lat, selectedItem.Item.Location.Lng, $"{description} ({selectedItem.Item.Date})"),
                    new TaskCreationDetails(selectedItem.QueriedLat, selectedItem.QueriedLng));
            }
        }

        public Task<List<GameTask>> CreateTasks(string gameTypeKey, string areaKey, Geometry geometry, int maxCount)
        {
            throw new InvalidOperationException();
        }

        private class FoundItem
        {
            public FoundItem(GoogleStreetViewMetadataApiResult item, double queriedLat, double queriedLng)
            {
                this.Item = item;
                this.QueriedLat = queriedLat;
                this.QueriedLng = queriedLng;
            }

            public GoogleStreetViewMetadataApiResult Item { get; private set; }

            public double QueriedLat { get; private set; }

            public double QueriedLng { get; private set; }
        }
    }
}
