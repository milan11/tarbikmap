import { GameConfigurationDTO } from "./GameConfigurationDTO";
import { PlayerDTO } from "./PlayerDTO";
import { TaskAnswerDTO } from "./TaskAnswerDTO";
import { TaskQuestionDTO } from "./TaskQuestionDTO";

export type GameStateDTO = {
  configuration: GameConfigurationDTO;
  currentConfigurationError: string;
  loadedTypeLabel: string | null;
  loadedAreaLabel: string | null;
  starting: boolean;
  started: boolean;
  tasksCompleted: number;
  totalTasks?: number;
  questions: TaskQuestionDTO[];
  correctAnswers: TaskAnswerDTO[];
  players: PlayerDTO[];
  currentTaskCompleted?: boolean;
  currentPlayerIndex?: number;
  nextGameId?: string;
  currentTaskAnsweringRemainingMs?: number;
  currentTaskCompletingRemainingMs?: number;
  stateCounter: number;
};
