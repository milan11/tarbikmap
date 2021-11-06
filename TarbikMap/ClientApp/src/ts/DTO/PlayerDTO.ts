import { PlayerAnswerDTO } from "./PlayerAnswerDTO";

export type PlayerDTO = {
  name: string;
  answers: PlayerAnswerDTO[];
  pointsTotal: number;
};
