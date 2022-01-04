namespace TarbikMap.Tests.Tasks
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TarbikMap.AreaSources;
    using TarbikMap.Common.Downloader;
    using TarbikMap.TaskSources;
    using TarbikMap.Utils;

    internal static class Program
    {
        private static int width = 15000;
        private static int height = 10000;
        private static double latMin;
        private static double latMax;
        private static double lonMin;
        private static double lonMax;

        private enum DrawingType
        {
            HEATMAP,
            QUERIED_VS_REAL,
            NAMES,
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1508", Justification = "We want currently unused code for e.g. different drawing types")]
        public static async Task Main([System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801", Justification = "Not used")] string[] args)
        {
            string searchText = "Bratislava";
            int areaResultIndex = 0;
            int gameTypeResultIndex = 0;
            int immediatelyLowerColorTo = 250;
            DrawingType drawingType = DrawingType.NAMES;
            TimeSpan sleepBetweenTasks = TimeSpan.FromMilliseconds(0);
            bool saveAlways = false;

            using var httpClientDownloader = new HttpClientDownloader();
            IDownloader downloader = new CachingDownloader(httpClientDownloader, "_download_cache");

            // ITaskSource taskSource = new TaskSource_Streetview(downloader, new AreaSource_Osm(downloader), true, true);
            ITaskSource taskSource = new WikidataTaskSource(new NoDownloader());
            IAreaSource areaSource = new NaturalEarthAreaSource();

            string gameType = (await taskSource.Search(searchText).ConfigureAwait(false))[gameTypeResultIndex].Key;
            string area = (await areaSource.Search(searchText).ConfigureAwait(false))[areaResultIndex].Key;

            var geometry = await areaSource.GetGeometry(area).ConfigureAwait(false);
            var geometryDto = GeometryUtils.GeometryToDTO(geometry, area);

            latMin = geometryDto.Lines.SelectMany(line => line.Points).Min(p => p[0]);
            latMax = geometryDto.Lines.SelectMany(line => line.Points).Max(p => p[0]);
            lonMin = geometryDto.Lines.SelectMany(line => line.Points).Min(p => p[1]);
            lonMax = geometryDto.Lines.SelectMany(line => line.Points).Max(p => p[1]);

            using Bitmap image = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(image))
            {
                g.FillRectangle(Brushes.White, 0, 0, width, height);

                foreach (var line in geometryDto.Lines)
                {
                    g.DrawPolygon(Pens.Red, line.Points.Select(TransformPoint).ToArray());
                }
            }

            DateTime lastSaveTime = DateTime.MinValue;
            while (true)
            {
                if (saveAlways || lastSaveTime == DateTime.MinValue || lastSaveTime < DateTime.UtcNow - TimeSpan.FromSeconds(10))
                {
                    image.Save("/tmp/image.png", System.Drawing.Imaging.ImageFormat.Png);
                    lastSaveTime = DateTime.UtcNow;
                }

                var task = await taskSource.CreateTask(gameType, area, geometry, new System.Collections.Generic.List<Storage.GameTask>()).ConfigureAwait(false);

                if (drawingType == DrawingType.HEATMAP)
                {
                    var point = TransformPoint(new double[] { task.Answer.Lat, task.Answer.Lon });

                    int x = (int)point.X;
                    int y = (int)point.Y;
                    if (x < 0 || x >= width)
                    {
                        continue;
                    }

                    if (y < 0 || y >= height)
                    {
                        continue;
                    }

                    int color = image.GetPixel(x, y).R;
                    if (color == 0)
                    {
                    }
                    else if (color == 255)
                    {
                        color = immediatelyLowerColorTo;
                    }
                    else
                    {
                        --color;
                    }

                    image.SetPixel(x, y, Color.FromArgb(color, color, color));

                    if (sleepBetweenTasks.TotalMilliseconds > 0)
                    {
                        Thread.Sleep(sleepBetweenTasks);
                    }
                }

                if (drawingType == DrawingType.QUERIED_VS_REAL)
                {
                    var queriedPoint = TransformPoint(new double[] { task.CreationDetails!.QueriedLat, task.CreationDetails!.QueriedLon });
                    var point = TransformPoint(new double[] { task.Answer.Lat, task.Answer.Lon });

                    using (Graphics g = Graphics.FromImage(image))
                    {
                        g.DrawLine(Pens.Gray, queriedPoint, point);
                        g.DrawEllipse(Pens.Green, queriedPoint.X - 2, queriedPoint.Y - 2, 4, 4);

                        g.DrawEllipse(Pens.Blue, point.X - 4, point.Y - 4, 8, 8);
                    }
                }

                if (drawingType == DrawingType.NAMES)
                {
                    var point = TransformPoint(new double[] { task.Answer.Lat, task.Answer.Lon });

                    using (Graphics g = Graphics.FromImage(image))
                    {
                        using var font = new Font("Arial", 10);

                        g.DrawEllipse(Pens.Blue, point.X - 4, point.Y - 4, 8, 8);
                        g.DrawString(task.Answer.Description, font, Brushes.Black, point.X + 5, point.Y + 5);
                    }
                }
            }
        }

        private static PointF TransformPoint(double[] latLon)
        {
            double ratio = Math.Min(width / (lonMax - lonMin), height / (latMax - latMin));

            double x = -((lonMin - latLon[1]) * ratio);
            double y = -((latLon[0] - latMax) * ratio);

            return new PointF((float)x, (float)y);
        }
    }
}
