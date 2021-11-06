namespace TarbikMap.DataSources.OpenStreetCamMetadata
{
    using System.Text.Json.Serialization;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Deserialized (input from external API)")]
    internal class OpenStreetCamMetadataApiStatus
    {
        [JsonPropertyName("httpCode")]
        public int HttpCode { get; set; }

        [JsonPropertyName("httpMessage")]
        public string HttpMessage { get; set; } = null!;
    }
}