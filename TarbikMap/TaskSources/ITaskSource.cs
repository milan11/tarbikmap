namespace TarbikMap.TaskSources
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NetTopologySuite.Geometries;
    using TarbikMap.Storage;

    internal interface ITaskSource
    {
        Task<List<GameType>> Search(string query);

        Task<string> GetLabel(string gameTypeKey);

        bool IsFinite(string gameTypeKey);

        Task<GameTask> CreateTask(string gameTypeKey, string areaKey, Geometry geometry, IList<GameTask> existingTasks);

        Task<List<GameTask>> CreateTasks(string gameTypeKey, string areaKey, Geometry geometry, int maxCount);
    }
}
