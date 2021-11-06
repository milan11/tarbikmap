namespace TarbikMap.DTO
{
    using System.Collections.Generic;

    internal class GeometryDTO
    {
        public GeometryDTO(string id, List<LineDTO> lines)
        {
            this.Id = id;
            this.Lines = lines;
        }

        public string Id { get; private set; }

        public List<LineDTO> Lines { get; private set; }
    }
}
