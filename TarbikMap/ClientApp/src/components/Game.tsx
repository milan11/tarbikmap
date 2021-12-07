import React, { Component } from "react";
import { GamePlayerSelection } from "./GamePlayerSelection";
import { GameTaskAssignment } from "./GameTaskAssignment";
import { GameTaskResults } from "./GameTaskResults";
import { GameFinalResults } from "./GameFinalResults";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { getCookie } from "../cookieUtils";
import { ObservingMessage } from "./ObservingMessage";
import { match } from "react-router";
import { GameStateDTO } from "../ts/DTO/GameStateDTO";
import { Point, PrepareForSelecting_Data, PrepareForShowingResults_Data, ShowGeometry_Data } from "../ts/types";
import { History } from "history";

type Props = {
  history: History;
  match: match<{ gameId: string }>;
  onPrepareForSelecting: (data: PrepareForSelecting_Data) => void;
  onPrepareForShowingResults: (data: PrepareForShowingResults_Data) => void;
  onShowGeometry: (data: ShowGeometry_Data) => void;
  onShowMapLabels: (mapLabelsShown: boolean, allowChange: boolean) => void;
  onPrepareClean: () => void;
  selectedLocation: Point | null;
};
type State = {
  connection: HubConnection | null;
  geometryShownForArea: string | null;
  mapLabelsShown: boolean | null;
  gameState?: GameStateDTO;
};

export class Game extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = { connection: null, geometryShownForArea: null, mapLabelsShown: null };
  }

  componentDidMount() {
    this.createSocket();
  }

  componentDidUpdate(prevProps: Props, prevState: State) {
    if (prevProps && prevProps.match.params.gameId !== this.props.match.params.gameId) {
      this.createSocket();
    }
    if (this.state.gameState && this.state.gameState.configuration.area !== this.state.geometryShownForArea) {
      this.setState(
        {
          geometryShownForArea: this.state.gameState.configuration.area,
        },
        async () => {
          const response = await fetch("games/" + encodeURIComponent(this.props.match.params.gameId) + "/geometry", {
            method: "POST",
          });
          if (response.ok) {
            const geometry = await response.json();
            this.props.onShowGeometry({ geometry: geometry });
          }
        }
      );
    }
    if (this.state.gameState && this.state.gameState.configuration.mapLabels !== this.state.mapLabelsShown) {
      this.setState(
        {
          mapLabelsShown: this.state.gameState.configuration.mapLabels,
        },
        () => {
          this.props.onShowMapLabels(this.state.gameState!.configuration.mapLabels, !this.gameEnded(this.state.gameState!));
        }
      );
    }
    if (this.state.gameState && prevState.gameState && this.state.gameState.players.length !== prevState.gameState.players.length) {
      this.createSocket();
    }
  }

  createSocket() {
    if (this.state.connection !== null) {
      this.state.connection.stop();
    }

    this.setState({ gameState: undefined }, async () => {
      const connection = new HubConnectionBuilder().withUrl("/gamestate").withAutomaticReconnect().build();

      connection.on("game_state", (data) => {
        this.setState((state) => {
          if (!state.gameState || data.stateCounter > state.gameState.stateCounter) {
            return {
              gameState: data,
            };
          } else {
            return {};
          }
        });
      });

      const sessionKey = getCookie("session_key");

      connection.onreconnected(() => {
        connection.send("Connect", this.props.match.params.gameId, sessionKey);
      });

      await connection.start();

      await connection.send("Connect", this.props.match.params.gameId, sessionKey);

      this.setState({ connection: connection });
    });
  }

  render() {
    if (this.state.gameState === undefined) {
      return null;
    }

    return [this.renderMain(), this.renderObservingMessage()];
  }

  renderMain() {
    const gameState = this.state.gameState!;

    if (gameState.started === false) {
      return <GamePlayerSelection match={this.props.match} gameState={gameState} />;
    }

    if (!this.gameEnded(gameState)) {
      if (gameState.correctAnswers!.length === gameState.tasksCompleted + 1) {
        return <GameTaskResults match={this.props.match} gameState={gameState} onPrepareForShowingResults={this.props.onPrepareForShowingResults} />;
      } else {
        return <GameTaskAssignment match={this.props.match} gameState={gameState} onPrepareForSelecting={this.props.onPrepareForSelecting} selectedLocation={this.props.selectedLocation} />;
      }
    }

    return <GameFinalResults match={this.props.match} history={this.props.history} gameState={gameState} onPrepareClean={this.props.onPrepareClean} />;
  }

  renderObservingMessage() {
    const gameState = this.state.gameState!;

    if (gameState.started === true && gameState.currentPlayerIndex === null) {
      return <ObservingMessage key={this.props.match.params.gameId} />;
    }

    return null;
  }

  gameEnded(gameState: GameStateDTO) {
    return gameState.tasksCompleted === gameState.totalTasks!;
  }
}
