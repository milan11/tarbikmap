import { GameAreaDTO } from "./GameAreaDTO";
import { GameTypeDTO } from "./GameTypeDTO";

export type GameAvailableConfigurationsDTO = {
  areas: GameAreaDTO[];
  types: GameTypeDTO[];
};
