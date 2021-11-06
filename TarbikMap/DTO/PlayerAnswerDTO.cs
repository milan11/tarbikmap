namespace TarbikMap.DTO
{
    internal class PlayerAnswerDTO
    {
        public PlayerAnswerDTO(double? lat, double? lon, double? distance, int points)
        {
            this.Lat = lat;
            this.Lon = lon;
            this.Distance = distance;
            this.Points = points;
        }

        public double? Lat { get; private set; }

        public double? Lon { get; private set; }

        public double? Distance { get; private set; }

        public int Points { get; private set; }
    }
}
