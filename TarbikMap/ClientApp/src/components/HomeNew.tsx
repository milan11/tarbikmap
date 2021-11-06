import React, { Component } from "react";
import { Route } from "react-router-dom";
import { PresetDTO } from "../ts/DTO/PresetDTO";

type Props = {};
type State = {
  presets: PresetDTO[] | null;
};

export class HomeNew extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = { presets: null };
  }

  componentDidMount() {
    this.fetchPresets();
  }

  async fetchPresets() {
    const response = await fetch("games/presets", {
      method: "POST",
    });

    if (response.ok) {
      const data = await response.json();
      this.setState({ presets: data });
    }
  }

  render() {
    return (
      <div style={{ textAlign: "center", width: "100%", height: "100%", position: "absolute", overflowY: "auto", padding: "20px" }}>
        <Route
          render={({ history }) => {
            return this.getPresets().map((preset) => {
              return (
                <div
                  style={{ width: "150px", height: "280px", display: "inline-block", margin: "10px", textAlign: "start", verticalAlign: "bottom", cursor: "pointer" }}
                  className="card"
                  onClick={async () => {
                    const gameId = await this.createGame();
                    await fetch("games/" + encodeURIComponent(gameId) + "/configuration", {
                      method: "POST",
                      headers: {
                        "Content-Type": "application/json",
                      },
                      body: JSON.stringify({
                        type: preset.typeKey,
                        area: preset.areaKey,
                      }),
                    });

                    if (gameId) {
                      history.push("/game/" + gameId);
                    }
                  }}
                >
                  <div className="card-image">
                    <img style={{ width: "150px", height: "150px" }} src={"/images/" + preset.imageKey} alt={preset.title} />
                    {/* eslint-disable-next-line */}
                    <a className="btn-floating halfway-fab waves-effect waves-light red">
                      <i className="material-icons">arrow_forward</i>
                    </a>
                  </div>
                  <div className="card-content">
                    <span style={{ fontSize: "large" }}>{preset.title}</span>
                    <div style={{ color: "#888" }}>{preset.description}</div>
                  </div>
                </div>
              );
            });
          }}
        />
      </div>
    );
  }

  async createGame() {
    const response = await fetch("games/create", {
      method: "POST",
    });
    if (response.ok) {
      const data = await response.json();
      return data.gameId;
    } else {
      return null;
    }
  }

  getPresets() {
    return this.state.presets ? this.state.presets.slice() : [];
  }
}
