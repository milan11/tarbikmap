import React, { Component } from "react";
import { match, Route } from "react-router-dom";
import { formatNumber } from "../textUtils";
import { GameStateDTO } from "../ts/DTO/GameStateDTO";
import { History } from "history";
import { WindowWithButtons } from "./WindowWithButtons";

type Props = {
  onPrepareClean: () => void;
  gameState: GameStateDTO;
  history: History;
  match: match<{ gameId: string }>;
};
type State = {};

export class GameFinalResults extends Component<Props, State> {
  componentDidMount() {
    this.props.onPrepareClean();
  }

  componentDidUpdate(prevProps: Props) {
    if (prevProps && prevProps.gameState.nextGameId === null && this.props.gameState.nextGameId !== null && this.props.gameState.currentPlayerIndex === null) {
      this.props.history.push("/game/" + this.props.gameState.nextGameId);
    }
  }

  render() {
    return (
      <WindowWithButtons
        windowStyle="center"
        buttons={
          this.props.gameState.nextGameId === null ? (
            <Route
              render={({ history }) => (
                <button
                  className="btn"
                  onClick={async () => {
                    const response = await fetch("games/" + encodeURIComponent(this.props.match.params.gameId) + "/next", {
                      method: "POST",
                    });
                    if (response.ok) {
                      const nextGameId = await response.text();
                      history.push("/game/" + nextGameId);
                    }
                  }}
                >
                  Create Next Game
                </button>
              )}
            />
          ) : (
            <Route
              render={({ history }) => (
                <button
                  className="btn"
                  onClick={async () => {
                    history.push("/game/" + this.props.gameState.nextGameId);
                  }}
                >
                  Go to Next Game
                </button>
              )}
            />
          )
        }
      >
        <ul className="collection">
          {this.props.gameState.players
            .sort((a, b) => {
              if (a.pointsTotal !== b.pointsTotal) {
                return b.pointsTotal - a.pointsTotal;
              }

              return a.name.localeCompare(b.name);
            })
            .map((player) => (
              <li className="collection-item">
                <span className="new badge" data-badge-caption="points total">
                  {formatNumber(player.pointsTotal)}
                </span>
                {player.name}
              </li>
            ))}
        </ul>
      </WindowWithButtons>
    );
  }
}
