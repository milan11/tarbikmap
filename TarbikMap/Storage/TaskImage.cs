namespace TarbikMap.Storage
{
    internal class TaskImage
    {
        public TaskImage(string imageKey)
        {
            this.ImageKey = imageKey;
        }

        public string ImageKey { get; private set; }

        public byte[]? CachedImageData { get; set; }

        public string? CachedImageAttribution { get; set; }
    }
}
