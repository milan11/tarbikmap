namespace TarbikMap.DataSources.GoogleReverseGeocoding
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Deserialized (input from external API)")]
    internal class GoogleReverseGeocodingApiResult
    {
        [JsonPropertyName("results")]
        public List<GoogleReverseGeocodingApiItem> Results { get; set; } = null!;

        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }
}