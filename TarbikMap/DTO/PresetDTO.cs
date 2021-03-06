namespace TarbikMap.DTO
{
    internal class PresetDTO
    {
        public PresetDTO(string title, string description, string imageKey, string imageAttribution, string typeKey, string areaKey)
        {
            this.Title = title;
            this.Description = description;
            this.ImageKey = imageKey;
            this.ImageAttribution = imageAttribution;
            this.TypeKey = typeKey;
            this.AreaKey = areaKey;
        }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public string ImageKey { get; private set; }

        public string ImageAttribution { get; private set; }

        public string TypeKey { get; private set; }

        public string AreaKey { get; private set; }
    }
}
