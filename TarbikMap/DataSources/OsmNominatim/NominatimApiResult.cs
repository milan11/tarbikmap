namespace TarbikMap.DataSources.OsmNominatim
{
    using System.Text.Json.Serialization;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Deserialized (input from external API)")]
    internal class NominatimApiResult
    {
        [JsonPropertyName("osm_id")]
        public long OsmId { get; set; }

        [JsonPropertyName("osm_type")]
        public string OsmType { get; set; } = null!;

        [JsonPropertyName("class")]
        public string Class { get; set; } = null!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; } = null!;
    }
}