namespace TarbikMap.AreaSources
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using TarbikMap.Storage;

    internal interface IAreaSource
    {
        Task<List<Area>> Search(string query);

        Task<string> GetLabel(string areaKey);

        Task<Geometry> GetGeometry(string areaKey);
    }
}
