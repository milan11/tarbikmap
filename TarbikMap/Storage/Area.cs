namespace TarbikMap.Storage
{
    internal class Area
    {
        public Area(string key, string label)
        {
            this.Key = key;
            this.Label = label;
        }

        public string Key { get; private set; }

        public string Label { get; private set; }
    }
}
