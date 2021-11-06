namespace TarbikMap.Storage
{
    using System.Collections.Generic;

    internal class TaskQuestion
    {
        public TaskQuestion(List<TaskImage> images, double initialLat1, double initialLat2, double initialLon1, double initialLon2)
        {
            this.Images = images;
            this.InitialLat1 = initialLat1;
            this.InitialLat2 = initialLat2;
            this.InitialLon1 = initialLon1;
            this.InitialLon2 = initialLon2;
        }

        public List<TaskImage> Images { get; private set; }

        public double InitialLat1 { get; private set; }

        public double InitialLat2 { get; private set; }

        public double InitialLon1 { get; private set; }

        public double InitialLon2 { get; private set; }
    }
}
