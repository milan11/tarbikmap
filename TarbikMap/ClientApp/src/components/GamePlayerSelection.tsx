import React, { Component } from "react";
import { getCookie, setCookie } from "../cookieUtils";
import { GameAreaSearch } from "./GameAreaSearch";
import { GameTypeSelection } from "./GameTypeSelection";
import { GameSettings } from "./GameSettings";
import { ButtonWithLoading } from "../ButtonWithLoading";

import { GameStateDTO } from "../ts/DTO/GameStateDTO";
import { match } from "react-router-dom";
import { GameDetails } from "./GameDetails";
import { WindowWithButtons } from "./WindowWithButtons";

type Props = {
  gameState: GameStateDTO;
  match: match<{ gameId: string }>;
};
type State = {
  playerName: string;
  gameTypeSelectionShown: boolean;
  gameAreaSearchShown: boolean;
  settingsShown: boolean;
  detailsShown: boolean;
  confirmingStart: boolean;
};

export class GamePlayerSelection extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = {
      playerName: decodeURIComponent(atob(getCookie("player_name"))),
      gameAreaSearchShown: false,
      gameTypeSelectionShown: false,
      settingsShown: false,
      detailsShown: false,
      confirmingStart: false,
    };
  }

  componentDidUpdate(prevProps: Props) {
    if (prevProps && prevProps.gameState && this.props.gameState) {
      if (
        this.state.confirmingStart &&
        (prevProps.gameState.currentConfigurationError !== this.props.gameState.currentConfigurationError || prevProps.gameState.starting !== this.props.gameState.starting)
      ) {
        this.setState({ confirmingStart: false });
      }

      if (prevProps.gameState.starting !== this.props.gameState.starting) {
        this.setState({ gameAreaSearchShown: false, gameTypeSelectionShown: false, settingsShown: false, confirmingStart: false });
      }
    }
  }
  render() {
    if (this.state.gameTypeSelectionShown) {
      return (
        <GameTypeSelection
          selected={this.props.gameState.configuration.type}
          onBack={() => {
            this.setState({ gameTypeSelectionShown: false }, () => {});
          }}
          onSelect={(type) => {
            this.setState({ gameTypeSelectionShown: false }, () => {
              fetch("games/" + encodeURIComponent(this.props.match.params.gameId) + "/configuration", {
                method: "POST",
                headers: {
                  "Content-Type": "application/json",
                },
                body: JSON.stringify({
                  type: type,
                }),
              });
            });
          }}
        />
      );
    }

    if (this.state.gameAreaSearchShown) {
      return (
        <GameAreaSearch
          selected={{ key: this.props.gameState.configuration.area, label: this.props.gameState.loadedAreaLabel ?? "" }}
          onBack={() => {
            this.setState({ gameAreaSearchShown: false }, () => {});
          }}
          onSelect={(key) => {
            this.setState({ gameAreaSearchShown: false }, () => {
              fetch("games/" + encodeURIComponent(this.props.match.params.gameId) + "/configuration", {
                method: "POST",
                headers: {
                  "Content-Type": "application/json",
                },
                body: JSON.stringify({
                  area: key,
                }),
              });
            });
          }}
        />
      );
    }

    if (this.state.settingsShown) {
      return (
        <GameSettings
          configuration={this.props.gameState.configuration}
          onBack={() => {
            this.setState({ settingsShown: false }, () => {});
          }}
          onSelect={(configuration) => {
            this.setState({ settingsShown: false }, () => {
              fetch("games/" + encodeURIComponent(this.props.match.params.gameId) + "/configuration", {
                method: "POST",
                headers: {
                  "Content-Type": "application/json",
                },
                body: JSON.stringify(configuration),
              });
            });
          }}
        />
      );
    }

    if (this.state.detailsShown) {
      return (
        <GameDetails
          gameId={this.props.match.params.gameId}
          onBack={() => {
            this.setState({ detailsShown: false });
          }}
        />
      );
    }

    return (
      <WindowWithButtons
        windowStyle="center"
        buttons={
          <div>
            {this.renderStarting()}
            <span className="badge red" style={{ color: "white", fontSize: "large", cursor: "pointer" }} onClick={() => this.setState({ detailsShown: true })}>
              <strong>Game Code:</strong> {this.props.match.params.gameId}
            </span>
          </div>
        }
      >
        <div className="collection">
          {/* eslint-disable-next-line */}
          <a
            style={{ cursor: "pointer" }}
            onClick={() => {
              if (this.props.gameState.starting) {
                return;
              }
              this.setState({
                gameTypeSelectionShown: true,
              });
            }}
            className="collection-item"
          >
            <span className="badge">{this.props.gameState.loadedTypeLabel}</span>Type
          </a>
          {/* eslint-disable-next-line */}
          <a
            style={{ cursor: "pointer" }}
            onClick={() => {
              if (this.props.gameState.starting) {
                return;
              }
              this.setState({
                gameAreaSearchShown: true,
              });
            }}
            className="collection-item"
          >
            <span className="badge">{this.props.gameState.loadedAreaLabel}</span>Area
          </a>
          {/* eslint-disable-next-line */}
          <a
            style={{ cursor: "pointer" }}
            onClick={() => {
              if (this.props.gameState.starting) {
                return;
              }
              this.setState({
                settingsShown: true,
              });
            }}
            className="collection-item"
          >
            <span className="badge">
              {this.props.gameState.configuration.tasksCount} rounds, {this.props.gameState.configuration.answeringTimeLimitSeconds}+{this.props.gameState.configuration.completingTimeLimitSeconds}{" "}
              sec.
            </span>
            Settings
          </a>
        </div>
        <div style={{ display: "flex", marginBottom: "10px" }}>
          <div style={{ flexGrow: 1 }}>
            <ul className="collection">
              {this.props.gameState.players.map((player, index) => (
                <li key={index} className="collection-item">
                  {player.name}
                </li>
              ))}

              {this.props.gameState.currentPlayerIndex === null && !this.props.gameState.starting ? (
                <li className="collection-item">
                  <div style={{ display: "flex", alignItems: "baseline" }}>
                    <input
                      type="text"
                      maxLength={16}
                      placeholder="Enter Your Name"
                      value={this.state.playerName}
                      onChange={(event) => this.setState({ playerName: event.target.value })}
                      style={{ marginRight: "10px" }}
                    />
                    <ButtonWithLoading
                      text="Join"
                      className="btn"
                      automaticallyResetLoading={false}
                      onClick={async () => {
                        setCookie("player_name", btoa(encodeURIComponent(this.state.playerName)), 365);
                        const response = await fetch("games/" + encodeURIComponent(this.props.match.params.gameId) + "/join", {
                          method: "POST",
                          headers: {
                            "Content-Type": "application/json",
                          },
                          body: JSON.stringify(this.state.playerName),
                        });
                      }}
                    />
                  </div>
                </li>
              ) : null}
            </ul>
          </div>
        </div>
      </WindowWithButtons>
    );
  }

  renderStarting() {
    if (this.props.gameState.players.length === 0) {
      return null;
    }

    if (this.props.gameState.starting) {
      return (
        <div className="progress">
          <div className="indeterminate" style={{ width: "70%" }}></div>
        </div>
      );
    }

    if (this.props.gameState.currentConfigurationError) {
      return this.props.gameState.currentConfigurationError;
    }

    if (!this.state.confirmingStart) {
      return (
        <button
          className="btn"
          onClick={() => {
            this.setState({
              confirmingStart: true,
            });
          }}
        >
          Start Game
        </button>
      );
    } else {
      return (
        <ButtonWithLoading
          text={"Confirm (" + this.getCountMessage() + ")"}
          className="btn btn-success"
          automaticallyResetLoading={false}
          onClick={async () => {
            fetch("games/" + encodeURIComponent(this.props.match.params.gameId) + "/start", {
              method: "POST",
            });
          }}
        />
      );
    }
  }

  getCountMessage() {
    if (this.props.gameState.players.length === 1) {
      return this.props.gameState.players.length + " player";
    } else {
      return this.props.gameState.players.length + " players";
    }
  }
}
