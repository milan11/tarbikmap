namespace TarbikMap.DTO
{
    using System.Collections.Generic;

    internal class GameAvailableConfigurationsDTO
    {
        public GameAvailableConfigurationsDTO(List<GameAreaDTO> areas, List<GameTypeDTO> types)
        {
            this.Areas = areas;
            this.Types = types;
        }

        public List<GameAreaDTO> Areas { get; private set; }

        public List<GameTypeDTO> Types { get; private set; }
    }
}
