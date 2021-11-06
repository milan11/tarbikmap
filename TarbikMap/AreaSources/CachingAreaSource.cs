namespace TarbikMap.AreaSources
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using TarbikMap.Common;
    using TarbikMap.Storage;

    internal class CachingAreaSource : IAreaSource
    {
        private IAreaSource orig;

        private AsyncCache<string> cacheAreaKeyDisplayName = new AsyncCache<string>();
        private AsyncCache<Geometry> cacheAreaKeyToGeometry = new AsyncCache<Geometry>();

        public CachingAreaSource(IAreaSource orig)
        {
            this.orig = orig;
        }

        public Task<string> GetLabel(string areaKey)
        {
            return this.cacheAreaKeyDisplayName.Get(
                areaKey,
                () =>
                {
                    return this.orig.GetLabel(areaKey);
                },
                false);
        }

        public Task<Geometry> GetGeometry(string areaKey)
        {
            return this.cacheAreaKeyToGeometry.Get(
                areaKey,
                () =>
                {
                    return this.orig.GetGeometry(areaKey);
                },
                false);
        }

        public async Task<List<Area>> Search(string query)
        {
            List<Area> results = await this.orig.Search(query).ConfigureAwait(false);

            foreach (var result in results)
            {
                await this.cacheAreaKeyDisplayName.Get(
                result.Key,
                () =>
                {
                    return Task.FromResult(result.Label);
                },
                true).ConfigureAwait(false);
            }

            return results;
        }
    }
}
