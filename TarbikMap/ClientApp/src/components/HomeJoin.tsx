import React, { Component } from "react";
import { Route } from "react-router-dom";
import { History } from "history";
import { WindowWithButtons } from "./WindowWithButtons";

const gameIdLength = 4;

type Props = {};
type State = {
  gameId: string;
  gameIdValid: boolean;
};

export class HomeJoin extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = { gameId: "", gameIdValid: true };
  }

  render() {
    return (
      <WindowWithButtons windowStyle="center">
        <Route
          render={({ history }) => (
            <div style={{ marginTop: "50px" }}>
              <div className="row">
                <div className="input-field inline">
                  <input
                    id="game_id"
                    type="text"
                    placeholder="Enter Game Code"
                    maxLength={gameIdLength}
                    autoComplete="off"
                    value={this.state.gameId}
                    className={!this.state.gameIdValid ? "invalid" : undefined}
                    onChange={(event) =>
                      this.setState({ gameId: event.target.value.toUpperCase(), gameIdValid: true }, () => {
                        if (this.state.gameId.length === gameIdLength) {
                          this.verifyGameAndRedirect(this.state.gameId, history);
                        }
                      })
                    }
                  />
                  <span className="helper-text" data-error="Game does not exist."></span>
                </div>
              </div>
            </div>
          )}
        />
      </WindowWithButtons>
    );
  }

  async verifyGameAndRedirect(gameId: string, history: History) {
    try {
      const response = await fetch("games/" + encodeURIComponent(gameId) + "/verify", {
        method: "POST",
      });
      if (response.ok) {
        const result = await response.json();
        if (result === true) {
          history.push("/game/" + gameId);
        } else {
          if (this.state.gameId === gameId) {
            this.setState({ gameIdValid: false });
          }
        }
      }
    } catch {}
  }
}
