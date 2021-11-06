namespace TarbikMap.DTO
{
    internal class GameTypeDTO
    {
        public GameTypeDTO(string key, string label, string categoryKey)
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
