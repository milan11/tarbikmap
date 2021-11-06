namespace TarbikMap.DTO
{
    internal class GameAreaDTO
    {
        public GameAreaDTO(string key, string label)
        {
            this.Key = key;
            this.Label = label;
        }

        public string Key { get; private set; }

        public string Label { get; private set; }
    }
}
