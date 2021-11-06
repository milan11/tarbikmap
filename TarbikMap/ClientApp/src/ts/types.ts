import { GeometryDTO } from "./DTO/GeometryDTO";
import { PlayerAnswerDTO } from "./DTO/PlayerAnswerDTO";
import { TaskAnswerDTO } from "./DTO/TaskAnswerDTO";
import { TaskQuestionDTO } from "./DTO/TaskQuestionDTO";

export type Point = [number, number];

export type PrepareForSelecting_Data = {
  question: TaskQuestionDTO;
  canSelect: boolean;
};

export type PrepareForShowingResults_Data = {
  correctAnswer: TaskAnswerDTO;
  playerAnswers: { name: string; answer: PlayerAnswerDTO }[];
  question: TaskQuestionDTO;
};

export type ShowGeometry_Data = {
  geometry: GeometryDTO;
};
