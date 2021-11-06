import React, { Component } from "react";
import { GameAvailableConfigurationsDTO } from "../ts/DTO/GameAvailableConfigurationsDTO";
import { WindowWithButtons } from "./WindowWithButtons";

type Props = {
  selected: string;
  onSelect: (key: string) => void;
  onBack: () => void;
};
type State = {
  configurations: GameAvailableConfigurationsDTO | null;
  selected: string;
};

export class GameTypeSelection extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = { configurations: null, selected: this.props.selected };
  }

  componentDidMount() {
    this.fetchConfigurations();
  }

  async fetchConfigurations() {
    const response = await fetch("games/configurations", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(""),
    });

    if (response.ok) {
      const data = await response.json();
      this.setState({ configurations: data });
    }
  }

  render() {
    return (
      <WindowWithButtons
        windowStyle="center"
        buttons={
          <div>
            <button
              className="btn"
              onClick={() => {
                this.props.onBack();
              }}
            >
              Back
            </button>
            <button
              className="btn"
              style={{ marginLeft: "10px" }}
              onClick={() => {
                this.props.onSelect(this.state.selected);
              }}
            >
              Select
            </button>
          </div>
        }
      >
        {this.getTypes()
          .map((t) => t.categoryKey)
          .filter((x, i, a) => a.indexOf(x) === i)
          .map((categoryKey) => {
            return (
              <div>
                <h4>{categoryKey}</h4>
                <ul className="collection">
                  {this.getTypes()
                    .filter((t) => t.categoryKey === categoryKey)
                    .map((type) => {
                      return (
                        /* eslint-disable-next-line */
                        <a
                          style={{ cursor: "pointer" }}
                          className={type.key === this.state.selected ? "collection-item active" : "collection-item"}
                          onClick={() => {
                            this.setState({ selected: type.key });
                          }}
                        >
                          {type.label}
                        </a>
                      );
                    })}
                </ul>
              </div>
            );
          })}
      </WindowWithButtons>
    );
  }

  getTypes() {
    return this.state.configurations ? this.state.configurations.types.slice() : [];
  }
}
