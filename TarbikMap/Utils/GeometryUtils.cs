namespace TarbikMap.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NetTopologySuite.Geometries;
    using TarbikMap.DTO;

    internal static class GeometryUtils
    {
        public static GeometryDTO GeometryToDTO(Geometry geometry, string areaKey)
        {
            GeometryDTO result = new GeometryDTO(areaKey, new List<LineDTO>());

            if (geometry is LineString)
            {
                result.Lines.Add(ToLine((LineString)geometry));
            }
            else
            {
                Geometry[] geometries;
                if (geometry is MultiPolygon)
                {
                    geometries = ((MultiPolygon)geometry).Geometries;
                }
                else if (geometry is Polygon)
                {
                    geometries = new Geometry[] { geometry };
                }
                else
                {
                    throw new ArgumentException("Invalid geometry type: " + geometry.GetType());
                }

                foreach (var g in geometries)
                {
                    var polygon = (Polygon)g;

                    result.Lines.Add(ToLine(polygon.Shell));
                    foreach (var hole in polygon.Holes)
                    {
                        result.Lines.Add(ToLine(hole));
                    }
                }
            }

            return result;
        }

        private static LineDTO ToLine(LineString ls)
        {
            var coordinates = ls.Coordinates.Select(c => new double[] { c.Y, c.X }).ToList();

            return new LineDTO(coordinates);
        }
    }
}
