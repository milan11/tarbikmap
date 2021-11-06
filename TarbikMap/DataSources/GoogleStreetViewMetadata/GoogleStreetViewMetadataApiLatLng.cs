namespace TarbikMap.DataSources.GoogleStreetViewMetadata
{
    using System.Text.Json.Serialization;

    internal class GoogleStreetViewMetadataApiLatLng
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }
}