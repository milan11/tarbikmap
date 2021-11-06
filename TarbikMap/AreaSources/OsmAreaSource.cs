namespace TarbikMap.AreaSources
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using TarbikMap.Common.Downloader;
    using TarbikMap.DataSources.OsmNominatim;
    using TarbikMap.DataSources.OsmOverpass;
    using TarbikMap.Storage;

    internal class OsmAreaSource : IAreaSource
    {
        private static List<Area> areas = new List<Area>
        {
            new Area("14296", "Slovakia"),
            new Area("3992676",  "Bratislava"),
            new Area("2208772", "Bratislava - Dúbravka"),
        };

        private IDownloader downloader;

        public OsmAreaSource(IDownloader downloader)
        {
            this.downloader = downloader;
        }

        public async Task<List<Area>> Search(string query)
        {
            if (query.Length == 0)
            {
                return areas;
            }

            var results = await NominatimMain.QueryNominatim(this.downloader, query).ConfigureAwait(false);

            return results
                .Where(r => r.OsmType == "relation")
                .Select(r => new Area(r.OsmId.ToString(CultureInfo.InvariantCulture), r.DisplayName))
                .ToList();
        }

        public Task<string> GetLabel(string areaKey)
        {
            throw new NotImplementedException("Not implemented");
        }

        public Task<Geometry> GetGeometry(string areaKey)
        {
            return OsmOverpassMain.LoadGeometryFromOverpass(this.downloader, long.Parse(areaKey, CultureInfo.InvariantCulture));
        }
    }
}
