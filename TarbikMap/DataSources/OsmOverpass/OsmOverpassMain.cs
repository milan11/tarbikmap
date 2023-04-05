namespace TarbikMap.DataSources.OsmOverpass
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Operation.Polygonize;
    using TarbikMap.Common.Downloader;

    internal static class OsmOverpassMain
    {
        public static async Task<Geometry> LoadGeometryFromOverpass(IDownloader downloader, long relationId)
        {
            var overpassResponse = await QueryOverpass(downloader, relationId).ConfigureAwait(false);

            return OverpassResponseToGeometry(overpassResponse);
        }

        private static async Task<OsmOverpassApiResult> QueryOverpass(IDownloader downloader, long relationId)
        {
            byte[] content = await downloader.HttpPost(new Uri("https://overpass-api.de/api/interpreter"), Encoding.UTF8.GetBytes("[out:json];\nrel(" + relationId + ");\nway(r);\nout geom;\n")).ConfigureAwait(false);
            return JsonSerializer.Deserialize<OsmOverpassApiResult>(content)!;
        }

        private static Geometry OverpassResponseToGeometry(OsmOverpassApiResult response)
        {
            var geometries = response.Elements.Select(element => (Geometry)new LineString(element.Geometry.Select(latlon => new Coordinate(latlon.Lon, latlon.Lat)).ToArray()));

            Polygonizer polygonizer = new Polygonizer();
            polygonizer.Add(geometries.ToList());
            var polygons = polygonizer.GetPolygons();
            return new GeometryCollection(polygons.ToArray()).Union();
        }
    }
}