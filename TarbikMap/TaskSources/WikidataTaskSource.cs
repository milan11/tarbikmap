namespace TarbikMap.TaskSources
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Simplify;
    using TarbikMap.Common;
    using TarbikMap.Common.Downloader;
    using TarbikMap.DataSources.Wikimedia;
    using TarbikMap.Storage;
    using TarbikMap.Utils;

    internal class WikidataTaskSource : ITaskSource
    {
        private const string FileNameCapitals = "capitals";
        private const string FileNameRegional = "regional";
        private const string FileNameTowns = "towns";
        private const string FileNameVillages = "villages";
        private const string FileNameCastles = "castles";
        private const string FileNameHighBuildings = "high_buildings";
        private const string FileNameBridges = "bridges";
        private const string FileNameRailwayStations = "railway_stations";
        private const string FileNameSquares = "squares";
        private const string FileNamePlaces = "places";

        private static List<WikidataGameType> wikidataGameTypes = new List<WikidataGameType>()
        {
            new WikidataGameType("capitals", "Capitals", new[] { FileNameCapitals }, "Towns"),
            new WikidataGameType("regional_capitals", "Regional Capitals", new[] { FileNameCapitals, FileNameRegional }, "Towns"),
            new WikidataGameType("towns", "Towns", new[] { FileNameCapitals, FileNameRegional, FileNameTowns }, "Towns"),
            new WikidataGameType("municipalities", "Towns and Villages", new[] { FileNameCapitals, FileNameRegional, FileNameTowns, FileNameVillages }, "Towns"),
            new WikidataGameType("places", "All Places", new[] { FileNameCastles, FileNameHighBuildings, FileNameBridges, FileNameRailwayStations, FileNameSquares, FileNamePlaces }, "Places"),
            new WikidataGameType("castles", "Castles", new[] { FileNameCastles }, "Places"),
            new WikidataGameType("high_buildings", "High Buildings", new[] { FileNameHighBuildings }, "Places"),
            new WikidataGameType("bridges", "Bridges", new[] { FileNameBridges }, "Places"),
            new WikidataGameType("railway_stations", "Railway Stations", new[] { FileNameRailwayStations }, "Places"),
            new WikidataGameType("squares", "Squares", new[] { FileNameSquares }, "Places"),
        };

        private IDownloader downloader;

        private AsyncCache<List<GameTask>> cacheItemsForArea = new AsyncCache<List<GameTask>>();

        public WikidataTaskSource(IDownloader downloader)
        {
            this.downloader = downloader;
        }

        public Task<List<GameType>> Search(string query)
        {
            return Task.FromResult(wikidataGameTypes
                .Where(x => string.IsNullOrWhiteSpace(query) || x.Label.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => new GameType(x.Key, x.Label, x.CategoryLabel))
                .ToList());
        }

        public Task<string> GetLabel(string gameTypeKey)
        {
            return Task.FromResult(wikidataGameTypes.Single(t => t.Key == gameTypeKey).Label);
        }

        public Task<GameTask> CreateTask(string gameTypeKey, string areaKey, Geometry geometry, IList<GameTask> existingTasks)
        {
            throw new InvalidOperationException();
        }

        public bool IsFinite(string gameTypeKey)
        {
            return true;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA5394", Justification = "No cryptography, Random is OK")]
        public async Task<List<GameTask>> CreateTasks(string gameTypeKey, string areaKey, Geometry geometry, int maxCount)
        {
            List<GameTask> items = await this.GetItems(gameTypeKey, areaKey, geometry).ConfigureAwait(false);

            if (items.Count <= maxCount)
            {
                Random random = new Random();

                return items.OrderBy(a => random.Next()).ToList();
            }
            else
            {
                Random random = new Random();

                return items.Select(item => new { item = item, random = random.Next() }).OrderBy(x => x.random).Take(maxCount).Select(x => x.item).ToList();
            }
        }

        public Task<byte[]> GetImageData(string gameTypeKey, string imageKey)
        {
            string imageNameForUrl = imageKey.Replace(' ', '_');

            byte[] hashBytes = Md5ForWikimedia(imageNameForUrl);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2", CultureInfo.InvariantCulture));
            }

            string imageNameMd5 = sb.ToString();

            string imageUrlStr;

            string extension = ExtractExtension(imageNameForUrl);
            if (extension == "PDF")
            {
                imageUrlStr = $"https://upload.wikimedia.org/wikipedia/commons/thumb/{imageNameMd5.Substring(0, 1)}/{imageNameMd5.Substring(0, 2)}/{imageNameForUrl}/page1-{Constants.ImageSize}px-{imageNameForUrl}.jpg";
            }
            else if (extension == "WEBP")
            {
                imageUrlStr = $"https://upload.wikimedia.org/wikipedia/commons/thumb/{imageNameMd5.Substring(0, 1)}/{imageNameMd5.Substring(0, 2)}/{imageNameForUrl}/{Constants.ImageSize}px-{imageNameForUrl}.jpg";
            }
            else if (extension == "SVG" || extension == "XCF")
            {
                imageUrlStr = $"https://upload.wikimedia.org/wikipedia/commons/thumb/{imageNameMd5.Substring(0, 1)}/{imageNameMd5.Substring(0, 2)}/{imageNameForUrl}/{Constants.ImageSize}px-{imageNameForUrl}.png";
            }
            else
            {
                imageUrlStr = $"https://upload.wikimedia.org/wikipedia/commons/{imageNameMd5.Substring(0, 1)}/{imageNameMd5.Substring(0, 2)}/{imageNameForUrl}";
            }

            return this.downloader.HttpGet(new Uri(imageUrlStr));
        }

        public async Task<string> GetImageAttribution(string gameTypeKey, string imageKey)
        {
            var attribution = await WikimediaApiMain.LoadAttribution(this.downloader, imageKey).ConfigureAwait(false);

            string?[] parts = new string?[]
            {
                "Wikimedia Commons",
                attribution.ArtistHtml != null ? XmlUtils.XmlToString(attribution.ArtistHtml) : null,
                attribution.LicenseShortName,
                attribution.LicenseUrl,
            };

            return string.Join(" | ", parts.Where(part => part != null));
        }

        private static List<GameTask> GetItemsInGeometry(Geometry geometry, WikidataGameType wikidataGameType)
        {
            List<GameTask> result = new List<GameTask>();

            geometry = TopologyPreservingSimplifier.Simplify(geometry, 0.001);

            var envelope = geometry.EnvelopeInternal;
            double minX = envelope.MinX;
            double maxX = envelope.MaxX;
            double minY = envelope.MinY;
            double maxY = envelope.MaxY;

            foreach (string fileName in wikidataGameType.FileNames)
            {
                int minSlice = Math.Clamp(((int)minX + 180) / 10, 0, 35);
                int maxSlice = Math.Clamp(((int)maxX + 180) / 10, 0, 35);

                for (int slice = minSlice; slice <= maxSlice; ++slice)
                {
                    BinaryFormatReader br = new BinaryFormatReader();

                    using var stream = ResourcesProvider.GetStream("tasks", "wikidata", $"wikidata_binary_{fileName}_{slice}.bin");

                    br.Read(
                        stream,
                        () =>
                        {
                            double y = br.ReadCoordinate();
                            double x = br.ReadCoordinate();

                            if (x >= minX && x <= maxX && y >= minY && y <= maxY)
                            {
                                if (geometry.Contains(new Point(new Coordinate(x, y))))
                                {
                                    string image = br.ReadString();
                                    string name = br.ReadString();

                                    var taskImages = new List<TaskImage>();

                                    taskImages.Add(new TaskImage(image));

                                    GameTask task = new GameTask(
                                        new TaskQuestion(taskImages, envelope.MinY, envelope.MaxY, envelope.MinX, envelope.MaxX),
                                        new TaskAnswer(y, x, name),
                                        null);

                                    result.Add(task);
                                }
                            }
                        });
                }
            }

            return result;
        }

        private static string ExtractExtension(string str)
        {
            int dotPos = str.LastIndexOf('.');
            string ext = str.Substring(dotPos + 1).ToUpperInvariant();
            return ext;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA5351", Justification = "External API wants this")]
        private static byte[] Md5ForWikimedia(string imageName)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(imageName);
                return md5.ComputeHash(inputBytes);
            }
        }

        private Task<List<GameTask>> GetItems(string gameTypeKey, string areaKey, Geometry geometry)
        {
            string cacheKey = JsonSerializer.Serialize(new { t = gameTypeKey, a = areaKey });

            return this.cacheItemsForArea.Get(
                cacheKey,
                () =>
                {
                    WikidataGameType wikidataGameType = wikidataGameTypes.Single(t => t.Key == gameTypeKey);
                    return Task.FromResult(GetItemsInGeometry(geometry, wikidataGameType));
                },
                false);
        }

        private class WikidataGameType
        {
            public WikidataGameType(string key, string label, string[] fileNames, string categoryLabel)
            {
                this.Key = key;
                this.Label = label;
                this.FileNames = fileNames;
                this.CategoryLabel = categoryLabel;
            }

            public string Key { get; private set; }

            public string Label { get; private set; }

            public string[] FileNames { get; private set; }

            public string CategoryLabel { get; private set; }
        }
    }
}
