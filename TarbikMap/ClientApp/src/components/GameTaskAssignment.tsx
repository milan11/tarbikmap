import React, { Component } from "react";
import { TimerMessage } from "./TimerMessage";
import { ImageBrowser } from "./ImageBrowser";
import { Point, PrepareForSelecting_Data } from "../ts/types";
import { match } from "react-router-dom";
import { GameStateDTO } from "../ts/DTO/GameStateDTO";
import { WindowWithButtons } from "./WindowWithButtons";

type Props = {
  gameState: GameStateDTO;
  match: match<{ gameId: string }>;
  selectedLocation: Point | null;
  onPrepareForSelecting: (data: PrepareForSelecting_Data) => void;
};
type State = {
  hidden: boolean;
  moreInformationShown: boolean;
};

export class GameTaskAssignment extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = { hidden: false, moreInformationShown: false };
  }

  componentDidMount() {
    this.props.onPrepareForSelecting({ question: this.props.gameState.questions[this.props.gameState.tasksCompleted], canSelect: this.props.gameState.currentPlayerIndex !== null });
  }

  componentDidUpdate(prevProps: Props) {
    if (prevProps.selectedLocation !== this.props.selectedLocation && this.state.hidden === false) {
      this.setState({ hidden: true });
    }
  }

  render() {
    return [this.renderInner(), this.props.gameState.currentTaskAnsweringRemainingMs !== null ? <TimerMessage timeRemainingMs={this.props.gameState.currentTaskAnsweringRemainingMs!} /> : null];
  }

  renderInner() {
    if (this.state.moreInformationShown) {
      return (
        <WindowWithButtons windowStyle="center">
          <div>
            <strong>{this.props.gameState.loadedTypeLabel}</strong>
          </div>
          <div>
            <strong>{this.props.gameState.loadedAreaLabel}</strong>
          </div>
          <div>
            {this.props.gameState.tasksCompleted + 1} / {this.props.gameState.totalTasks}
          </div>
          <br />
          <button className="btn" onClick={() => this.setState({ moreInformationShown: false })}>
            Close
          </button>
        </WindowWithButtons>
      );
    }
    if (this.state.hidden) {
      return (
        <div className="topLeft">
          {this.props.gameState.currentPlayerIndex !== null && this.props.selectedLocation !== null ? (
            <button
              className="btn"
              onClick={async () => {
                fetch("games/" + encodeURIComponent(this.props.match.params.gameId) + "/answer/" + encodeURIComponent(this.props.gameState.tasksCompleted), {
                  method: "POST",
                  headers: {
                    "Content-Type": "application/json",
                  },
                  body: JSON.stringify({ lat: this.props.selectedLocation![0], lon: this.props.selectedLocation![1] }),
                });
              }}
              style={{ marginBottom: "10px" }}
            >
              Submit Answer
            </button>
          ) : null}
          <img
            alt=""
            onClick={() => {
              this.setState({ hidden: false });
            }}
            src={"games/" + encodeURIComponent(this.props.match.params.gameId) + "/image/" + encodeURIComponent(this.props.gameState.tasksCompleted) + "/" + encodeURIComponent(0)}
            style={{ maxWidth: "200px", maxHeight: "200px", cursor: "pointer" }}
          />
        </div>
      );
    } else {
      return [
        <ImageBrowser
          gameId={this.props.match.params.gameId}
          taskIndex={this.props.gameState.tasksCompleted}
          imagesCount={this.props.gameState.questions[this.props.gameState.tasksCompleted].imagesCount}
          onClick={
            this.props.gameState.currentPlayerIndex !== null
              ? () => {
                  this.setState({ hidden: true });
                }
              : undefined
          }
          attributionShown={false}
        />,
        <div className="topLeftButtons">
          {this.props.gameState.currentPlayerIndex !== null ? (
            <button
              className="btn"
              style={{ marginRight: "10px" }}
              onClick={
                this.props.gameState.currentPlayerIndex !== null
                  ? () => {
                      this.setState({ hidden: true });
                    }
                  : undefined
              }
            >
              Go to Map
            </button>
          ) : null}
          <button className="btn" onClick={() => this.setState({ moreInformationShown: true })}>
            ?
          </button>
        </div>,
      ];
    }
  }
}
