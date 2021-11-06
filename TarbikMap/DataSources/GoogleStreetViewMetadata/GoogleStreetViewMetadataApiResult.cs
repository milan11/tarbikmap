namespace TarbikMap.DataSources.GoogleStreetViewMetadata
{
    using System.Text.Json.Serialization;

    internal class GoogleStreetViewMetadataApiResult
    {
        [JsonPropertyName("copyright")]
        public string Copyright { get; set; } = null!;

        [JsonPropertyName("date")]
        public string Date { get; set; } = null!;

        [JsonPropertyName("location")]
        public GoogleStreetViewMetadataApiLatLng Location { get; set; } = null!;

        [JsonPropertyName("pano_id")]
        public string PanoId { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }
}