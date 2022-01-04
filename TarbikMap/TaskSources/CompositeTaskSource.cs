namespace TarbikMap.TaskSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using TarbikMap.Storage;

    internal class CompositeTaskSource : ITaskSource
    {
        private List<Tuple<string, ITaskSource>> origSources = new List<Tuple<string, ITaskSource>>();

        public CompositeTaskSource Add(string prefix, ITaskSource origSource)
        {
            if (this.origSources.Any(s => s.Item1 == prefix))
            {
                throw new InvalidOperationException($"Prefix already registered: {prefix}");
            }

            this.origSources.Add(Tuple.Create(prefix, origSource));
            return this;
        }

        public Task<GameTask> CreateTask(string gameTypeKey, string areaKey, Geometry geometry, IList<GameTask> existingTasks)
        {
            var sourceAndGameTypeKey = this.SplitGameTypeKey(gameTypeKey);
            return sourceAndGameTypeKey.Item1.CreateTask(sourceAndGameTypeKey.Item2, areaKey, geometry, existingTasks);
        }

        public Task<List<GameTask>> CreateTasks(string gameTypeKey, string areaKey, Geometry geometry, int maxCount)
        {
            var sourceAndGameTypeKey = this.SplitGameTypeKey(gameTypeKey);
            return sourceAndGameTypeKey.Item1.CreateTasks(sourceAndGameTypeKey.Item2, areaKey, geometry, maxCount);
        }

        public Task<string> GetLabel(string gameTypeKey)
        {
            var sourceAndGameTypeKey = this.SplitGameTypeKey(gameTypeKey);
            return sourceAndGameTypeKey.Item1.GetLabel(sourceAndGameTypeKey.Item2);
        }

        public bool IsFinite(string gameTypeKey)
        {
            var sourceAndGameTypeKey = this.SplitGameTypeKey(gameTypeKey);
            return sourceAndGameTypeKey.Item1.IsFinite(sourceAndGameTypeKey.Item2);
        }

        public async Task<List<GameType>> Search(string query)
        {
            List<GameType> results = new List<GameType>();

            foreach (var origSource in this.origSources)
            {
                var origResults = await origSource.Item2.Search(query).ConfigureAwait(false);
                foreach (var origResult in origResults)
                {
                    results.Add(new GameType(origSource.Item1 + '_' + origResult.Key, origResult.Label, origResult.CategoryKey));
                }
            }

            return results;
        }

        public Task<byte[]> GetImageData(string gameTypeKey, string imageKey)
        {
            var sourceAndGameTypeKey = this.SplitGameTypeKey(gameTypeKey);
            return sourceAndGameTypeKey.Item1.GetImageData(sourceAndGameTypeKey.Item2, imageKey);
        }

        public Task<string> GetImageAttribution(string gameTypeKey, string imageKey)
        {
            var sourceAndGameTypeKey = this.SplitGameTypeKey(gameTypeKey);
            return sourceAndGameTypeKey.Item1.GetImageAttribution(sourceAndGameTypeKey.Item2, imageKey);
        }

        private Tuple<ITaskSource, string> SplitGameTypeKey(string gameTypeKey)
        {
            string[] parts = gameTypeKey.Split('_', 2);

            return Tuple.Create(
                this.origSources.Single(s => s.Item1 == parts[0]).Item2,
                parts[1]);
        }
    }
}
