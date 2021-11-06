namespace TarbikMap.DTO
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Deserialized (input from ASP.NET)")]
    internal class ProvidedAnswerDTO
    {
        public ProvidedAnswerDTO(double lat, double lon)
        {
            this.Lat = lat;
            this.Lon = lon;
        }

        public double Lat { get; private set; }

        public double Lon { get; private set; }
    }
}
