namespace TarbikMap.Storage
{
    internal class GameType
    {
        public GameType(string key, string label, string categoryKey)
        {
            this.Key = key;
            this.Label = label;
            this.CategoryKey = categoryKey;
        }

        public string Key { get; private set; }

        public string Label { get; private set; }

        public string CategoryKey { get; private set; }
    }
}
