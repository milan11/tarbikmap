namespace TarbikMap.DataSources.OsmOverpass
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Deserialized (input from external API)")]
    internal class OsmOverpassApiResult
    {
        [JsonPropertyName("elements")]
        public List<OsmOverpassApiElement> Elements { get; set; } = null!;
    }
}