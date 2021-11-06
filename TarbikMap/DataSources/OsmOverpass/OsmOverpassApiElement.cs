namespace TarbikMap.DataSources.OsmOverpass
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1812", Justification = "Deserialized (input from external API)")]
    internal class OsmOverpassApiElement
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("geometry")]
        public IList<OsmOverpassApiLatLon> Geometry { get; set; } = null!;
    }
}