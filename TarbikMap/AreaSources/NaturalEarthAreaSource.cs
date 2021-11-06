namespace TarbikMap.AreaSources
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NetTopologySuite.IO.Streams;
    using TarbikMap.Storage;
    using TarbikMap.Utils;

    internal class NaturalEarthAreaSource : IAreaSource
    {
        public Task<List<Area>> Search(string query)
        {
            if (query.Length == 0)
            {
                return Task.FromResult(new List<Area>());
            }

            List<Area> results = new List<Area>();

            BrowseMetadata(query, (area) =>
            {
                results.Add(area);
                return true;
            });

            return Task.FromResult(results);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1508", Justification = "foundLabel is modified in the callback passed to BrowseMetadata")]
        public Task<string> GetLabel(string areaKey)
        {
            string? foundLabel = null;

            BrowseMetadata(string.Empty, (area) =>
            {
                if (area.Key == areaKey)
                {
                    foundLabel = area.Label;
                    return false;
                }
                else
                {
                    return true;
                }
            });

            if (foundLabel != null)
            {
                return Task.FromResult(foundLabel!);
            }
            else
            {
                throw new InvalidOperationException("Label not found");
            }
        }

        public Task<Geometry> GetGeometry(string areaKey)
        {
            int separatorIndex = areaKey.LastIndexOf('_');
            string fileName = areaKey.Substring(0, separatorIndex);
            long indexInFile = long.Parse(areaKey.Substring(separatorIndex + 1), CultureInfo.InvariantCulture);

            var streamProviderRegistry = new NetTopologySuite.IO.Streams.ShapefileStreamProviderRegistry(
                    new NetTopologySuite.IO.Streams.ByteStreamProvider(StreamTypes.Shape, ResourcesProvider.GetStream("areas", "natural_earth", fileName + ".shp"), true),
                    new NetTopologySuite.IO.Streams.ByteStreamProvider(StreamTypes.Data, ResourcesProvider.GetStream("areas", "natural_earth", fileName + ".dbf"), true),
                    new NetTopologySuite.IO.Streams.ByteStreamProvider(StreamTypes.Index, ResourcesProvider.GetStream("areas", "natural_earth", fileName + ".shx"), true));

            GeometryFactory factory = new GeometryFactory();
            using (ShapefileDataReader shapeFileDataReader = new ShapefileDataReader(streamProviderRegistry, factory))
            {
                DbaseFileHeader header = shapeFileDataReader.DbaseHeader;

                long itemsCounter = 0;
                while (shapeFileDataReader.Read())
                {
                    ++itemsCounter;

                    if (itemsCounter == indexInFile)
                    {
                        return Task.FromResult(shapeFileDataReader.Geometry);
                    }
                }
            }

            throw new InvalidOperationException("Geometry not found");
        }

        private static string ToItemId(string fileName, long indexInFile)
        {
            return $"{fileName}_{indexInFile}";
        }

        private static void BrowseMetadata(string query, Func<Area, bool> foundAreaProvider)
        {
            IEnumerable<string> fileNames = ResourcesProvider.GetFileNames("areas", "natural_earth")
                            .Where(fileName => fileName.EndsWith(".shp", StringComparison.Ordinal))
                            .Select(fileName => fileName.Substring(0, fileName.Length - ".shp".Length));

            foreach (string fileName in fileNames)
            {
                var streamProviderRegistry = new NetTopologySuite.IO.Streams.ShapefileStreamProviderRegistry(
                    new NetTopologySuite.IO.Streams.ByteStreamProvider(StreamTypes.Shape, ResourcesProvider.GetStream("areas", "natural_earth", fileName + ".shp"), true),
                    new NetTopologySuite.IO.Streams.ByteStreamProvider(StreamTypes.Data, ResourcesProvider.GetStream("areas", "natural_earth", fileName + ".dbf"), true),
                    new NetTopologySuite.IO.Streams.ByteStreamProvider(StreamTypes.Index, ResourcesProvider.GetStream("areas", "natural_earth", fileName + ".shx"), true),
                    false,
                    false,
                    false,
                    new NetTopologySuite.IO.Streams.ByteStreamProvider(StreamTypes.DataEncoding, new MemoryStream(Encoding.UTF8.GetBytes("UTF-8")), true));

                DbaseFileReader db = new DbaseFileReader(streamProviderRegistry);
                {
                    DbaseFileHeader header = db.GetHeader();

                    List<int> nameIndexes = new List<int>();
                    int nameIndex = -1;
                    int nameEnIndex = -1;
                    for (int i = 0; i < header.NumFields; i++)
                    {
                        DbaseFieldDescriptor fldDescriptor = header.Fields[i];

                        string fieldName = fldDescriptor.Name.ToUpperInvariant();
                        if (fieldName == "NAME" || fieldName.StartsWith("NAME_", StringComparison.Ordinal))
                        {
                            nameIndexes.Add(i);
                        }

                        if (fieldName == "NAME")
                        {
                            nameIndex = i;
                        }

                        if (fieldName == "NAME_EN")
                        {
                            nameEnIndex = i;
                        }
                    }

                    long itemsCounter = 0;
                    foreach (ArrayList item in db)
                    {
                        ++itemsCounter;

                        bool matches = false;
                        foreach (var index in nameIndexes)
                        {
                            string? fieldValue = item[index] as string;
                            if (fieldValue != null && fieldValue.Contains(query, StringComparison.InvariantCultureIgnoreCase))
                            {
                                matches = true;
                                break;
                            }
                        }

                        if (matches)
                        {
                            string? selectedName = null;
                            {
                                selectedName = item[nameEnIndex] as string;
                            }

                            if (string.IsNullOrWhiteSpace(selectedName))
                            {
                                selectedName = item[nameIndex] as string;
                            }

                            if (string.IsNullOrWhiteSpace(selectedName))
                            {
                                selectedName = item[nameIndexes[0]] as string;
                            }

                            if (selectedName == null)
                            {
                                throw new ExternalDataException("Cannot find name");
                            }

                            {
                                string itemId = ToItemId(fileName, itemsCounter);
                                if (!foundAreaProvider(new Area(itemId, selectedName!)))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
