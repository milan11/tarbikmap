import React, { Component } from "react";
import { formatNumber, formatDistance } from "../textUtils";
import { TimerMessage } from "./TimerMessage";
import { ImageBrowser } from "./ImageBrowser";
import { ButtonWithLoading } from "../ButtonWithLoading";
import { GameStateDTO } from "../ts/DTO/GameStateDTO";
import { match } from "react-router-dom";
import { PrepareForShowingResults_Data } from "../ts/types";
import { PlayerDTO } from "../ts/DTO/PlayerDTO";
import { WindowWithButtons } from "./WindowWithButtons";

type Props = {
  gameState: GameStateDTO;
  match: match<{ gameId: string }>;
  onPrepareForShowingResults: (data: PrepareForShowingResults_Data) => void;
};
type State = {
  imageShown: boolean;
  hidden: boolean;
};

export class GameTaskResults extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = { hidden: this.props.gameState.currentPlayerIndex === null, imageShown: false };
  }

  componentDidMount() {
    this.refreshResultsOnMap();
  }

  componentDidUpdate(prevProps: Props) {
    if (prevProps.gameState !== null) {
      if (this.getAnswersCount(prevProps.gameState) !== this.getAnswersCount(this.props.gameState)) {
        this.refreshResultsOnMap();
      }
    }
  }

  getAnswersCount(gameState: GameStateDTO) {
    return gameState.players.filter((player) => !this.playerWaiting(player)).length;
  }

  refreshResultsOnMap() {
    const correctAnswer = this.props.gameState.correctAnswers[this.props.gameState.tasksCompleted];
    const playerAnswers = this.props.gameState.players
      .filter((player) => !this.playerWaiting(player) && !this.playerTimedOut(player))
      .map((player) => {
        return { name: player.name, answer: player.answers[this.props.gameState.tasksCompleted] };
      });

    this.props.onPrepareForShowingResults({ correctAnswer: correctAnswer, playerAnswers: playerAnswers, question: this.props.gameState.questions[this.props.gameState.tasksCompleted] });
  }

  render() {
    return [this.renderInner(), this.props.gameState.currentTaskCompletingRemainingMs !== null ? <TimerMessage timeRemainingMs={this.props.gameState.currentTaskCompletingRemainingMs!} /> : null];
  }

  renderInner() {
    if (this.state.hidden) {
      return (
        <div className="topLeft">
          <button
            className="btn"
            onClick={async () => {
              this.setState({ hidden: false });
            }}
          >
            Show Table
          </button>
        </div>
      );
    } else if (this.state.imageShown) {
      return (
        <ImageBrowser
          gameId={this.props.match.params.gameId}
          taskIndex={this.props.gameState.tasksCompleted}
          imagesCount={this.props.gameState.questions[this.props.gameState.tasksCompleted].imagesCount}
          onClick={() => this.setState({ imageShown: false })}
        />
      );
    } else {
      return (
        <WindowWithButtons
          windowStyle="center"
          buttons={
            this.props.gameState.currentTaskCompleted === false ? (
              <ButtonWithLoading
                text="Continue"
                className="btn"
                automaticallyResetLoading={false}
                onClick={async () => {
                  fetch("games/" + encodeURIComponent(this.props.match.params.gameId) + "/complete/" + encodeURIComponent(this.props.gameState.tasksCompleted), {
                    method: "POST",
                  });
                }}
              >
                Continue
              </ButtonWithLoading>
            ) : this.props.gameState.currentTaskCompleted === true ? (
              "Waiting for other players to continue..."
            ) : null
          }
        >
          <div className="progress">
            <div className="determinate" style={{ width: ((this.props.gameState.tasksCompleted + 1) * 100) / this.props.gameState.totalTasks! + "%" }}></div>
          </div>

          <div style={{ display: "flex", marginBottom: "10px" }}>
            <div>
              <img
                alt=""
                onClick={() => {
                  this.setState({ imageShown: true });
                }}
                src={"games/" + encodeURIComponent(this.props.match.params.gameId) + "/image/" + encodeURIComponent(this.props.gameState.tasksCompleted) + "/" + encodeURIComponent(0)}
                style={{ maxWidth: "150px", maxHeight: "150px", cursor: "pointer", float: "right" }}
              />
            </div>
            <div style={{ marginLeft: "10px", textAlign: "right", flexGrow: 1 }}>
              <div style={{ textAlign: "end" }}>
                {this.props.gameState.tasksCompleted + 1} / {this.props.gameState.totalTasks}
              </div>

              <div>
                <strong>{this.props.gameState.correctAnswers[this.props.gameState.tasksCompleted].description}</strong>
              </div>

              <button
                style={{ marginTop: "10px" }}
                className="btn"
                onClick={async () => {
                  this.setState({ hidden: true });
                }}
              >
                Show Map
              </button>
            </div>
          </div>

          <ul className="collection">
            {this.props.gameState.players
              .sort((a, b) => {
                if (this.playerWaiting(a) !== this.playerWaiting(b)) {
                  return this.boolToInt(this.playerWaiting(a)) - this.boolToInt(this.playerWaiting(b));
                }
                if (this.playerTimedOut(a) !== this.playerTimedOut(b)) {
                  return this.boolToInt(this.playerTimedOut(a)) - this.boolToInt(this.playerTimedOut(b));
                }

                if (this.distanceOrZero(a) !== this.distanceOrZero(b)) {
                  return this.distanceOrZero(a) - this.distanceOrZero(b);
                }

                return a.name.localeCompare(b.name);
              })
              .map((player) => (
                <li className="collection-item">
                  {!this.playerWaiting(player) ? (
                    <span className="new badge" data-badge-caption="points">
                      {formatNumber(player.answers[this.props.gameState.tasksCompleted].points)}
                    </span>
                  ) : null}

                  {!this.playerWaiting(player) ? (
                    !this.playerTimedOut(player) ? (
                      <span className="badge">{formatDistance(player.answers[this.props.gameState.tasksCompleted].distance!)}</span>
                    ) : (
                      <span className="badge">-</span>
                    )
                  ) : (
                    <span className="badge">...</span>
                  )}

                  {player.name}
                </li>
              ))}
          </ul>
        </WindowWithButtons>
      );
    }
  }

  playerWaiting(player: PlayerDTO) {
    return player.answers.length !== this.props.gameState.tasksCompleted + 1;
  }

  playerTimedOut(player: PlayerDTO) {
    return player.answers.length === this.props.gameState.tasksCompleted + 1 && player.answers[this.props.gameState.tasksCompleted].distance === null;
  }

  distanceOrZero(player: PlayerDTO) {
    if (!this.playerWaiting(player)) {
      return player.answers[this.props.gameState.tasksCompleted].distance!;
    } else {
      return 0;
    }
  }

  boolToInt(b: boolean) {
    return b ? 1 : 0;
  }
}
