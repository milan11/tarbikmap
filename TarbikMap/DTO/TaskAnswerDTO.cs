namespace TarbikMap.DTO
{
    internal class TaskAnswerDTO
    {
        public TaskAnswerDTO(double lat, double lon, string description)
        {
            this.Lat = lat;
            this.Lon = lon;
            this.Description = description;
        }

        public double Lat { get; private set; }

        public double Lon { get; private set; }

        public string Description { get; private set; }
    }
}
