namespace TarbikMap.DataSources.OpenStreetCamMetadata
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Deserialized (input from external API)")]
    internal class OpenStreetCamMetadataApiResult
    {
        [JsonPropertyName("status")]
        public OpenStreetCamMetadataApiStatus Status { get; set; } = null!;

        [JsonPropertyName("currentPageItems")]
        public List<OpenStreetCamMetadataApiItem> CurrentPageItems { get; set; } = null!;
    }
}