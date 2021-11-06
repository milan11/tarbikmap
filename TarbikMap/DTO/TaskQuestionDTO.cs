namespace TarbikMap.DTO
{
    internal class TaskQuestionDTO
    {
        public TaskQuestionDTO(double initialLat1, double initialLon1, double initialLat2, double initialLon2, int imagesCount)
        {
            this.InitialLat1 = initialLat1;
            this.InitialLon1 = initialLon1;
            this.InitialLat2 = initialLat2;
            this.InitialLon2 = initialLon2;
            this.ImagesCount = imagesCount;
        }

        public double InitialLat1 { get; private set; }

        public double InitialLon1 { get; private set; }

        public double InitialLat2 { get; private set; }

        public double InitialLon2 { get; private set; }

        public int ImagesCount { get; private set; }
    }
}
