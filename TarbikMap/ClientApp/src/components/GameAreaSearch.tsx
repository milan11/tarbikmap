import React, { Component } from "react";
import { ButtonWithLoading } from "../ButtonWithLoading";
import { GameAreaDTO } from "../ts/DTO/GameAreaDTO";
import { GameAvailableConfigurationsDTO } from "../ts/DTO/GameAvailableConfigurationsDTO";
import { WindowWithButtons } from "./WindowWithButtons";

type Props = {
  selected: GameAreaDTO;
  onSelect: (key: string) => void;
  onBack: () => void;
};
type State = {
  configurations: GameAvailableConfigurationsDTO | null;
  searchQuery: string;
  selected: GameAreaDTO;
};

export class GameAreaSearch extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = { configurations: null, searchQuery: "", selected: this.props.selected };
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
      body: JSON.stringify(this.state.searchQuery),
    });

    if (response.ok) {
      const data = await response.json();
      this.setState({ configurations: data });
    }
  }

  render() {
    return (
      <WindowWithButtons
        windowStyle="topLeft"
        header={
          <div>
            <input
              type="text"
              maxLength={32}
              placeholder="City, country, area..."
              value={this.state.searchQuery}
              onChange={(event) => this.setState({ searchQuery: event.target.value })}
              style={{ backgroundColor: "#fff", paddingLeft: "5px", paddingRight: "5px", boxSizing: "border-box" }}
            />
            <ButtonWithLoading
              text="Search"
              automaticallyResetLoading={true}
              className="btn"
              onClick={async () => {
                await this.fetchConfigurations();
              }}
            />
          </div>
        }
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
                this.props.onSelect(this.state.selected.key);
              }}
            >
              Select
            </button>
          </div>
        }
      >
        <ul className="collection">
          {this.getAreas().map((area) => (
            // eslint-disable-next-line
            <a
              key={area.key}
              style={{ cursor: "pointer" }}
              className={area.key === this.state.selected.key ? "collection-item active" : "collection-item"}
              onClick={() => {
                this.setState({ selected: this.getAreas().find((t) => t.key === area.key)! });
              }}
            >
              {area.label}
            </a>
          ))}
        </ul>
      </WindowWithButtons>
    );
  }

  getAreas() {
    let areas = this.state.configurations ? this.state.configurations.areas.slice() : [];

    if (areas.find((t) => t.key === this.state.selected.key) === undefined) {
      areas.push(this.state.selected);
    }

    return areas;
  }
}
