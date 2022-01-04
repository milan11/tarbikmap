namespace TarbikMap.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TarbikMap.AreaSources;
    using TarbikMap.Common.Downloader;
    using TarbikMap.TaskSources;
    using Xunit;

    public class WikidataTaskSourceTest
    {
        [Fact]
        public async void SlovakiaCapitals()
        {
            await Test(
                "Slovakia",
                "Capitals",
                1,
                new[] { "Bratislava" },
                new string[] { "Prievidza", "Tichý Potok" }).ConfigureAwait(false);
        }

        [Fact]
        public async void SlovakiaTowns()
        {
            await Test(
                "Slovakia",
                "Towns",
                null,
                new[] { "Bratislava", "Prievidza" },
                new string[] { "Tichý Potok" }).ConfigureAwait(false);
        }

        [Fact]
        public async void SlovakiaTownsAndVillages()
        {
            await Test(
                "Slovakia",
                "Towns and villages",
                null,
                new[] { "Bratislava", "Prievidza", "Tichý Potok" },
                null).ConfigureAwait(false);
        }

        [Fact]
        public async void ItalyCapitals()
        {
            await Test(
                "Italy",
                "Capitals",
                1,
                new[] { "Rome" },
                null).ConfigureAwait(false);
        }

        [Fact]
        public async void BratislavaRailwayStations()
        {
            await Test(
                "Bratislava",
                "Railway stations",
                null,
                new[] { "Bratislava hlavná stanica" },
                new[] { "Budova Strojníckej fakulty STU v Bratislave", "Most SNP", }).ConfigureAwait(false);
        }

        [Fact]
        public async void BratislavaBridges()
        {
            await Test(
                "Bratislava",
                "Bridges",
                null,
                new[] { "Most SNP" },
                new[] { "Budova Strojníckej fakulty STU v Bratislave", "Bratislava hlavná stanica" }).ConfigureAwait(false);
        }

        [Fact]
        public async void BratislavaPlaces()
        {
            await Test(
                "Bratislava",
                "Places",
                null,
                new[] { "Budova Strojníckej fakulty STU v Bratislave", "Most SNP", "Bratislava hlavná stanica" },
                null).ConfigureAwait(false);
        }

        private static async Task Test(string areaQuery, string taskQuery, int? expectedTasksCount, string[]? tasksShouldContain, string[]? tasksShouldNotContain)
        {
            ITaskSource taskSource = new WikidataTaskSource(new NoDownloader());
            var gameTypeKey = (await taskSource.Search(taskQuery).ConfigureAwait(false))[0].Key;

            IAreaSource areaSource = new NaturalEarthAreaSource();
            var areaKey = (await areaSource.Search(areaQuery).ConfigureAwait(false))[0].Key;
            var geometry = await areaSource.GetGeometry(areaKey).ConfigureAwait(false);

            var tasks = await taskSource.CreateTasks(gameTypeKey, areaKey, geometry, int.MaxValue).ConfigureAwait(false);
            if (expectedTasksCount != null)
            {
                Assert.Equal(expectedTasksCount.Value, tasks.Count);
            }

            IEnumerable<string> descriptions = tasks.Select(t => t.Answer.Description);
            if (tasksShouldContain != null)
            {
                foreach (var expected in tasksShouldContain)
                {
                    Assert.Contains(expected, descriptions);
                }
            }

            if (tasksShouldNotContain != null)
            {
                foreach (var expected in tasksShouldNotContain)
                {
                    Assert.DoesNotContain(expected, descriptions);
                }
            }
        }
    }
}