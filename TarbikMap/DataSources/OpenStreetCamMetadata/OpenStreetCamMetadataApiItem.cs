namespace TarbikMap.DataSources.OpenStreetCamMetadata
{
    using System.Text.Json.Serialization;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Deserialized (input from external API)")]
    internal class OpenStreetCamMetadataApiItem
    {
        [JsonPropertyName("lat")]
        public string Lat { get; set; } = null!;

        [JsonPropertyName("lng")]
        public string Lng { get; set; } = null!;

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("date_added")]
        public string DateAdded { get; set; } = null!;
    }
}