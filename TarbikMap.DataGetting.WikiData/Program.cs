namespace TarbikMap.DataGetting.WikiData
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using TarbikMap.Common;

    public static class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1303", Justification = "This tool is not localized")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1508", Justification = "lastId is modified in the callback passed to Read)")]
        internal static void Main(string[] args)
        {
            string inputPath = args[0];
            string outputPath = args[1];

            try
            {
                uint? lastId = null;
                if (File.Exists(outputPath))
                {
                    {
                        var reader = new BinaryFormatReader();
                        using var stream = new FileStream(outputPath, FileMode.Open);

                        reader.Read(
                        stream,
                        () =>
                        {
                            uint id = reader.ReadId();
                            lastId = id;
                        });
                    }
                }

                {
                    using FileStream sw = new FileStream(outputPath, File.Exists(outputPath) ? FileMode.Append : FileMode.Create);
                    using BinaryWriter bw_global = new BinaryWriter(sw);

                    using var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "bunzip2",
                            Arguments = "\"" + inputPath + "\" -c",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true,
                        },
                    };

                    proc.Start();

                    var sr = proc.StandardOutput;

                    bool adding = false;
                    if (lastId == null)
                    {
                        Console.WriteLine("Continuing from the beginning");
                        adding = true;
                    }

                    int lineNumber = 0;
                    string? line = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        ++lineNumber;

                        if ((lineNumber % 100000) == 0)
                        {
                            Console.WriteLine(lineNumber);
                        }

                        if (!adding && !line.Contains("Q" + lastId!.Value, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        try
                        {
                            if (line == "[")
                            {
                                continue;
                            }

                            dynamic a = JsonConvert.DeserializeObject(line.Substring(0, line.Length - 1))!;

                            byte[] itemBytes;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                using BinaryWriter bw = new BinaryWriter(ms);
                                {
                                    string idStr = (string)a.id;
                                    if (idStr.StartsWith("P", StringComparison.Ordinal))
                                    {
                                        continue;
                                    }

                                    if (!idStr.StartsWith("Q", StringComparison.Ordinal))
                                    {
                                        throw new InvalidOperationException("Invalid ID: " + idStr);
                                    }

                                    uint id = uint.Parse(idStr.Substring(1), CultureInfo.InvariantCulture);

                                    if (!adding)
                                    {
                                        if (id == lastId!.Value)
                                        {
                                            Console.WriteLine("Continuing from line" + lineNumber);
                                            adding = true;
                                        }

                                        continue;
                                    }

                                    bw.Write(id);
                                }

                                double x = double.MinValue;
                                double y = double.MinValue;
                                {
                                    var snaks = (JArray)a.claims["P625"];
                                    if (snaks != null)
                                    {
                                        var values = snaks.Select(snak =>
                                        {
                                            var value = ((dynamic)snak).mainsnak.datavalue?.value;
                                            if (value == null)
                                            {
                                                return null;
                                            }

                                            return Tuple.Create((double)value.latitude, (double)value.longitude);
                                        }).Where(x => x != null);

                                        var value = values.FirstOrDefault();
                                        if (value != null)
                                        {
                                            x = value.Item2;
                                            y = value.Item1;
                                        }
                                    }
                                }

                                bw.Write(y);
                                bw.Write(x);
                                {
                                    List<uint> values = new List<uint>();

                                    var snaks = (JArray)a.claims["P31"];
                                    if (snaks != null)
                                    {
                                        values = snaks.Select(snak =>
                                        {
                                            var value = ((dynamic)snak).mainsnak.datavalue?.value;
                                            if (value == null)
                                            {
                                                return null;
                                            }

                                            string valueStr = (string)value.id;
                                            if (!valueStr.StartsWith("Q", StringComparison.Ordinal))
                                            {
                                                throw new InvalidOperationException("Invalid type");
                                            }

                                            return (uint?)uint.Parse(valueStr.Substring(1), CultureInfo.InvariantCulture);
                                        }).Where(x => x != null).Select(x => x!.Value).ToList();
                                    }

                                    WriteLengthAsShort(bw, values.Count);
                                    foreach (uint value in values)
                                    {
                                        bw.Write(value);
                                    }
                                }

                                {
                                    List<uint> values = new List<uint>();

                                    var snaks = (JArray)a.claims["P279"];
                                    if (snaks != null)
                                    {
                                        values = snaks.Select(snak =>
                                        {
                                            var value = ((dynamic)snak).mainsnak.datavalue?.value;
                                            if (value == null)
                                            {
                                                return null;
                                            }

                                            string valueStr = (string)value.id;
                                            if (!valueStr.StartsWith("Q", StringComparison.Ordinal))
                                            {
                                                throw new InvalidOperationException("Invalid type");
                                            }

                                            return (uint?)uint.Parse(valueStr.Substring(1), CultureInfo.InvariantCulture);
                                        }).Where(x => x != null).Select(x => x!.Value).ToList();
                                    }

                                    WriteLengthAsShort(bw, values.Count);
                                    foreach (uint value in values)
                                    {
                                        bw.Write(value);
                                    }
                                }

                                {
                                    List<uint> values = new List<uint>();

                                    var snaks = (JArray)a.claims["P1376"];
                                    if (snaks != null)
                                    {
                                        values = snaks.Select(snak =>
                                        {
                                            var value = ((dynamic)snak).mainsnak.datavalue?.value;
                                            if (value == null)
                                            {
                                                return null;
                                            }

                                            string valueStr = (string)value.id;
                                            if (!valueStr.StartsWith("Q", StringComparison.Ordinal))
                                            {
                                                throw new InvalidOperationException("Invalid type");
                                            }

                                            return (uint?)uint.Parse(valueStr.Substring(1), CultureInfo.InvariantCulture);
                                        }).Where(x => x != null).Select(x => x!.Value).ToList();
                                    }

                                    WriteLengthAsShort(bw, values.Count);
                                    foreach (uint value in values)
                                    {
                                        bw.Write(value);
                                    }
                                }

                                {
                                    List<string> values = new List<string>();

                                    var snaks = (JArray)a.claims["P18"];
                                    if (snaks != null)
                                    {
                                        values = snaks.Select(snak =>
                                        {
                                            var value = ((dynamic)snak).mainsnak.datavalue?.value;
                                            if (value == null)
                                            {
                                                return null;
                                            }

                                            return (string)value;
                                        }).Where(x => x != null).Select(x => x!).ToList();
                                    }

                                    WriteLengthAsShort(bw, values.Count);
                                    foreach (string image in values)
                                    {
                                        WriteString(bw, image);
                                    }
                                }

                                {
                                    var arr = (JObject)a.labels;

                                    List<string> names = new List<string>();
                                    foreach (var prop in arr)
                                    {
                                        names.Add(prop.Key + ':' + ((dynamic)(prop.Value!)).value);
                                    }

                                    WriteLengthAsShort(bw, names.Count);
                                    foreach (string name in names)
                                    {
                                        WriteString(bw, name);
                                    }
                                }

                                bw.Flush();

                                itemBytes = ms.ToArray();
                            }

                            WriteLengthAsShort(bw_global, itemBytes.Length);
                            bw_global.Write(itemBytes);
                            bw_global.Flush();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception on line {0}: {1}", lineNumber, e);
                            throw;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Global exception: " + e);
                throw;
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
    }
}
