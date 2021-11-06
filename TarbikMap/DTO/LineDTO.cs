namespace TarbikMap.DTO
{
    using System.Collections.Generic;

    internal class LineDTO
    {
        public LineDTO(List<double[]> points)
        {
            this.Points = points;
        }

        public List<double[]> Points { get; private set; }
    }
}
