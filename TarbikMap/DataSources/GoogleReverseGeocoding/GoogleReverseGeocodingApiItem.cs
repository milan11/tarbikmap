namespace TarbikMap.DataSources.GoogleReverseGeocoding
{
    using System.Text.Json.Serialization;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Deserialized (input from external API)")]
    internal class GoogleReverseGeocodingApiItem
    {
        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; } = null!;
    }
}