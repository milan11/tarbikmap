namespace TarbikMap.AreaSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Operation.Polygonize;
    using TarbikMap.Storage;

    internal class CommonAreaSource : IAreaSource
    {
        private static List<Area> areas = new List<Area>
        {
            new Area("world", "World"),
        };

        public Task<List<Area>> Search(string query)
        {
            if (query.Length == 0)
            {
                return Task.FromResult(areas);
            }

            return Task.FromResult(areas.Where(a => a.Label.Contains(query, StringComparison.InvariantCultureIgnoreCase)).ToList());
        }

        public Task<string> GetLabel(string areaKey)
        {
            {
                var found = areas.SingleOrDefault(t => t.Key == areaKey);
                if (found != null)
                {
                    return Task.FromResult(found.Label);
                }
            }

            throw new ArgumentException("Area not found");
        }

        public Task<Geometry> GetGeometry(string areaKey)
        {
            List<Coordinate> coordinates = new List<Coordinate>();

            if (areaKey == "world")
            {
                coordinates.Add(new Coordinate(-180, -90));
                coordinates.Add(new Coordinate(180, -90));
                coordinates.Add(new Coordinate(180, 90));
                coordinates.Add(new Coordinate(-180, 90));
                coordinates.Add(new Coordinate(-180, -90));
            }

            LineString lineString = new LineString(coordinates.ToArray());
            Polygonizer polygonizer = new Polygonizer();
            polygonizer.Add(lineString);
            var polygons = polygonizer.GetPolygons();
            return Task.FromResult(new GeometryCollection(polygons.ToArray()).Union());
        }
    }
}
