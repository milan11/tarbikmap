namespace TarbikMap.DTO
{
    using System.Collections.Generic;

    internal class PlayerDTO
    {
        public PlayerDTO(string name, List<PlayerAnswerDTO> answers, int pointsTotal)
        {
            this.Name = name;
            this.Answers = answers;
            this.PointsTotal = pointsTotal;
        }

        public string Name { get; private set; }

        public List<PlayerAnswerDTO> Answers { get; private set; }

        public int PointsTotal { get; private set; }
    }
}
