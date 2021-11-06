namespace TarbikMap.DataGetting.WikiDataClasses
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using TarbikMap.Common;

    internal static class Program
    {
        private const uint IdCapital = 5119;
        private const uint IdSovereignState = 3624078;
        private const uint IdCity = 515;
        private const uint IdTown = 3957;
        private const uint IdMunicipality = 15284;

        private const uint IdCastle = 23413;
        private const uint IdChateau = 751876;

        private const uint IdSkyscraper = 11303;
        private const uint IdBridge = 12280;
        private const uint IdRailwayStation = 55488;
        private const uint IdSquare = 174782;

        private const uint IdHistoricalCountry = 3024240;
        private const uint IdHistoricalPeriod = 11514315;
        private const uint IdAspectOfHistory = 17524420;

        private const uint IdQuarter = 2983893;
        private const uint IdNaturalRegion = 1970725;
        private const uint IdLowSpot = 22978151;

        private static HashSet<uint> interesting = new HashSet<uint>();
        private static Dictionary<uint, HierarchyItem> processed = new Dictionary<uint, HierarchyItem>();

        internal static void Main(string[] args)
        {
            string inputPath = args[0];
            string outputPath = args[1];
            {
                var reader = new BinaryFormatReader();
                using var stream = new FileStream(inputPath, FileMode.Open);

                reader.Read(
                stream,
                () =>
                {
                    uint id = reader.ReadId();
                    double y = reader.ReadCoordinate();
                    double x = reader.ReadCoordinate();

                    if (!(x == double.MinValue && y == double.MinValue))
                    {
                        HashSet<uint> thisInteresting = new HashSet<uint>();
                        {
                            // instance of
                            ushort count = reader.ReadLength();
                            for (int i = 0; i < count; ++i)
                            {
                                uint otherId = reader.ReadId();
                                thisInteresting.Add(otherId);
                            }
                        }

                        {
                            // subclass of
                            ushort count = reader.ReadLength();
                            for (int i = 0; i < count; ++i)
                            {
                                uint otherId = reader.ReadId();
                                thisInteresting.Add(otherId);
                            }
                        }

                        {
                            // capital of
                            ushort count = reader.ReadLength();
                            for (int i = 0; i < count; ++i)
                            {
                                uint otherId = reader.ReadId();
                                thisInteresting.Add(otherId);
                            }
                        }

                        {
                            // images
                            ushort count = reader.ReadLength();
                            if (count == 0)
                            {
                                return;
                            }

                            for (int i = 0; i < count; ++i)
                            {
                                reader.ReadString();
                            }
                        }

                        {
                            // names
                            ushort count = reader.ReadLength();
                            if (count == 0)
                            {
                                return;
                            }

                            for (int i = 0; i < count; ++i)
                            {
                                reader.ReadString();
                            }
                        }

                        foreach (uint otherId in thisInteresting)
                        {
                            interesting.Add(otherId);
                        }
                    }
                });
            }

            {
                while (interesting.Count > 0)
                {
                    bool somethingChanged = false;
                    {
                        var reader = new BinaryFormatReader();
                        using var stream = new FileStream(inputPath, FileMode.Open);

                        reader.Read(
                        stream,
                        () =>
                        {
                            uint id = reader.ReadId();
                            if (interesting.Contains(id))
                            {
                                interesting.Remove(id);
                                HierarchyItem hierarchyItem = new HierarchyItem();
                                processed.Add(id, hierarchyItem);
                                somethingChanged = true;

                                double y = reader.ReadCoordinate();
                                double x = reader.ReadCoordinate();
                                {
                                    // instance of
                                    ushort count = reader.ReadLength();
                                    for (int i = 0; i < count; ++i)
                                    {
                                        uint otherId = reader.ReadId();
                                        if (!processed.ContainsKey(otherId))
                                        {
                                            interesting.Add(otherId);
                                        }

                                        hierarchyItem.InstanceOf.Add(otherId);
                                    }
                                }

                                {
                                    // subclass of
                                    ushort count = reader.ReadLength();
                                    for (int i = 0; i < count; ++i)
                                    {
                                        uint otherId = reader.ReadId();
                                        if (!processed.ContainsKey(otherId))
                                        {
                                            interesting.Add(otherId);
                                        }

                                        hierarchyItem.SubclassOf.Add(otherId);
                                    }
                                }
                            }
                        });
                    }

                    if (!somethingChanged)
                    {
                        break;
                    }
                }
            }

            {
                var bw_capitals = CreateWriters(outputPath, "capitals");
                var bw_regional = CreateWriters(outputPath, "regional");
                var bw_towns = CreateWriters(outputPath, "towns");
                var bw_villages = CreateWriters(outputPath, "villages");
                var bw_castles = CreateWriters(outputPath, "castles");
                var bw_high_buildings = CreateWriters(outputPath, "high_buildings");
                var bw_bridges = CreateWriters(outputPath, "bridges");
                var bw_railway_stations = CreateWriters(outputPath, "railway_stations");
                var bw_squares = CreateWriters(outputPath, "squares");
                var bw_places = CreateWriters(outputPath, "places");
                {
                    var reader = new BinaryFormatReader();
                    using var stream = new FileStream(inputPath, FileMode.Open);

                    reader.Read(
                    stream,
                    () =>
                    {
                        uint id = reader.ReadId();

                        double y = reader.ReadCoordinate();
                        double x = reader.ReadCoordinate();

                        if (x == double.MinValue && y == double.MinValue)
                        {
                            return;
                        }

                        int slice = ((int)x + 180) / 10;
                        if (slice < 0 || slice > 35)
                        {
                            Console.WriteLine("warning: " + x);
                            return;
                        }

                        slice = Math.Clamp(slice, 0, 35);

                        byte[] itemBytes;
                        BinaryWriter selectedWriter;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            BinaryWriter bw = new BinaryWriter(ms);

                            bw.Write(y);
                            bw.Write(x);

                            List<uint> parents = new List<uint>();
                            {
                                // instance of
                                ushort count = reader.ReadLength();
                                for (int i = 0; i < count; ++i)
                                {
                                    uint otherId = reader.ReadId();
                                    parents.Add(otherId);
                                }
                            }

                            {
                                // subclass of
                                ushort count = reader.ReadLength();
                                for (int i = 0; i < count; ++i)
                                {
                                    uint otherId = reader.ReadId();
                                    parents.Add(otherId);
                                }
                            }

                            List<uint> capitalOf = new List<uint>();
                            {
                                // capital of
                                ushort count = reader.ReadLength();
                                for (int i = 0; i < count; ++i)
                                {
                                    uint otherId = reader.ReadId();
                                    capitalOf.Add(otherId);
                                }
                            }

                            {
                                if (parents.Any(x => !IsPresentlyValid(x) || Is(x, IdQuarter) || Is(x, IdNaturalRegion) || Is(x, IdLowSpot)))
                                {
                                    return;
                                }
                            }

                            {
                                if (capitalOf.Any(x => IsPresentlyValid(x) && Is(x, IdSovereignState)))
                                {
                                    selectedWriter = bw_capitals[slice];
                                }
                                else if (capitalOf.Any(x => IsPresentlyValid(x)) || parents.Any(x => Is(x, IdCapital)))
                                {
                                    selectedWriter = bw_regional[slice];
                                }
                                else if (parents.Any(x => Is(x, IdCity)) || parents.Any(x => Is(x, IdTown)))
                                {
                                    selectedWriter = bw_towns[slice];
                                }
                                else if (parents.Any(x => Is(x, IdMunicipality)))
                                {
                                    selectedWriter = bw_villages[slice];
                                }
                                else if (parents.Any(x => Is(x, IdCastle)) || parents.Any(x => Is(x, IdChateau)))
                                {
                                    selectedWriter = bw_castles[slice];
                                }
                                else if (parents.Any(x => Is(x, IdSkyscraper)))
                                {
                                    selectedWriter = bw_high_buildings[slice];
                                }
                                else if (parents.Any(x => Is(x, IdBridge)))
                                {
                                    selectedWriter = bw_bridges[slice];
                                }
                                else if (parents.Any(x => Is(x, IdRailwayStation)))
                                {
                                    selectedWriter = bw_railway_stations[slice];
                                }
                                else if (parents.Any(x => Is(x, IdSquare)))
                                {
                                    selectedWriter = bw_squares[slice];
                                }
                                else
                                {
                                    selectedWriter = bw_places[slice];
                                }
                            }

                            {
                                // images
                                ushort count = reader.ReadLength();
                                if (count == 0)
                                {
                                    return;
                                }

                                for (int i = 0; i < count; ++i)
                                {
                                    string str = reader.ReadString();
                                    if (i == 0)
                                    {
                                        WriteString(bw, str);
                                    }
                                }
                            }

                            {
                                // names
                                ushort count = reader.ReadLength();
                                if (count == 0)
                                {
                                    return;
                                }

                                List<string> names = new List<string>(count);
                                for (int i = 0; i < count; ++i)
                                {
                                    names.Add(reader.ReadString());
                                }

                                string? name = null;
                                if (name == null)
                                {
                                    name = names.FirstOrDefault(n => n.StartsWith("en:", StringComparison.Ordinal));
                                }

                                if (name == null)
                                {
                                    name = names.FirstOrDefault(n => n.StartsWith("sk:", StringComparison.Ordinal));
                                }

                                if (name == null)
                                {
                                    name = names[0];
                                }

                                if (name == null)
                                {
                                    throw new InvalidOperationException("Name is null");
                                }

                                name = name.Split(':', 2)[1];

                                WriteString(bw, name);
                            }

                            bw.Flush();

                            itemBytes = ms.ToArray();
                        }

                        WriteLengthAsShort(selectedWriter, itemBytes.Length);
                        selectedWriter.Write(itemBytes);
                        selectedWriter.Flush();
                    });
                }

                DisposeWriters(bw_capitals);
                DisposeWriters(bw_regional);
                DisposeWriters(bw_towns);
                DisposeWriters(bw_villages);
                DisposeWriters(bw_castles);
                DisposeWriters(bw_high_buildings);
                DisposeWriters(bw_bridges);
                DisposeWriters(bw_railway_stations);
                DisposeWriters(bw_squares);
                DisposeWriters(bw_places);
            }
        }

        private static List<BinaryWriter> CreateWriters(string outputPath, string name)
        {
            List<BinaryWriter> result = new List<BinaryWriter>();
            for (int i = 0; i <= 35; ++i)
            {
                result.Add(new BinaryWriter(new FileStream(Path.Combine(outputPath, $"{name}_{i}.bin"), FileMode.Create)));
            }

            return result;
        }

        private static void DisposeWriters(List<BinaryWriter> writers)
        {
            foreach (var writer in writers)
            {
                writer.Dispose();
            }
        }

        private static void WriteLengthAsShort(BinaryWriter bw, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            if (length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            bw.Write((ushort)length);
        }

        private static void WriteString(BinaryWriter bw, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            WriteLengthAsShort(bw, bytes.Length);

            bw.Write(bytes);
        }

        private static bool IsPresentlyValid(uint checkedId)
        {
            return !Is(checkedId, IdHistoricalCountry) && !Is(checkedId, IdHistoricalPeriod) && !Is(checkedId, IdAspectOfHistory);
        }

        private static bool Is(uint checkedId, uint expectedId)
        {
            return Is(checkedId, expectedId, new HashSet<uint>(), 0);
        }

        private static bool Is(uint checkedId, uint expectedId, HashSet<uint> alreadyChecked, uint depth)
        {
            if (depth == 3)
            {
                return false;
            }

            if (alreadyChecked.Contains(checkedId))
            {
                return false;
            }

            if (checkedId == expectedId)
            {
                return true;
            }

            HashSet<uint> checkedNew = new HashSet<uint>(alreadyChecked);
            checkedNew.Add(checkedId);

            if (processed.TryGetValue(checkedId, out var other))
            {
                foreach (var otherId in other.InstanceOf)
                {
                    if (Is(otherId, expectedId, checkedNew, depth + 1))
                    {
                        return true;
                    }
                }

                foreach (var otherId in other.SubclassOf)
                {
                    if (Is(otherId, expectedId, checkedNew, depth + 1))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private class HierarchyItem
        {
            public List<uint> InstanceOf { get; set; } = new List<uint>();

            public List<uint> SubclassOf { get; set; } = new List<uint>();
        }
    }
}
