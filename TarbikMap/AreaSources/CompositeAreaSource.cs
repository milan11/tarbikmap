namespace TarbikMap.AreaSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using TarbikMap.Storage;

    internal class CompositeAreaSource : IAreaSource
    {
        private List<Tuple<string, IAreaSource>> origSources = new List<Tuple<string, IAreaSource>>();

        public CompositeAreaSource Add(string prefix, IAreaSource origSource)
        {
            if (this.origSources.Any(s => s.Item1 == prefix))
            {
                throw new InvalidOperationException($"Prefix already registered: {prefix}");
            }

            this.origSources.Add(Tuple.Create(prefix, origSource));
            return this;
        }

        public Task<string> GetLabel(string areaKey)
        {
            var sourceAndAreaKey = this.SplitAreaKey(areaKey);
            return sourceAndAreaKey.Item1.GetLabel(sourceAndAreaKey.Item2);
        }

        public Task<Geometry> GetGeometry(string areaKey)
        {
            var sourceAndAreaKey = this.SplitAreaKey(areaKey);
            return sourceAndAreaKey.Item1.GetGeometry(sourceAndAreaKey.Item2);
        }

        public async Task<List<Area>> Search(string query)
        {
            List<Area> results = new List<Area>();

            foreach (var origSource in this.origSources)
            {
                var origResults = await origSource.Item2.Search(query).ConfigureAwait(false);
                foreach (var origResult in origResults)
                {
                    results.Add(new Area(origSource.Item1 + '_' + origResult.Key, origResult.Label));
                }
            }

            return results;
        }

        private Tuple<IAreaSource, string> SplitAreaKey(string areaKey)
        {
            string[] parts = areaKey.Split('_', 2);

            return Tuple.Create(
                this.origSources.Single(s => s.Item1 == parts[0]).Item2,
                parts[1]);
        }
    }
}
