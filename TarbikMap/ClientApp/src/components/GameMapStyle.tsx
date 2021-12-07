import React, { Component } from "react";
import { GameConfigurationDTO } from "../ts/DTO/GameConfigurationDTO";
import { WindowWithButtons } from "./WindowWithButtons";

type Configuration = {
  mapLabels: boolean;
};

type Props = {
  configuration: GameConfigurationDTO;
  onSelect: (configuration: Configuration) => void;
  onBack: () => void;
};
type State = {
  configuration: Configuration;
};

export class GameMapStyle extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = {
      configuration: {
        mapLabels: this.props.configuration.mapLabels,
      },
    };
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
                this.props.onSelect(this.state.configuration);
              }}
            >
              Save
            </button>
          </div>
        }
      >
        <div>
          <ul className="collection">
            {[
              { label: "With Labels", value: true },
              { label: "Without Labels", value: false },
            ].map((item) => {
              return (
                /* eslint-disable-next-line */
                <a
                  style={{ cursor: "pointer" }}
                  className={item.value === this.state.configuration.mapLabels ? "collection-item active" : "collection-item"}
                  onClick={() => {
                    this.setState({
                      configuration: Object.assign({}, this.state.configuration, {
                        mapLabels: item.value,
                      }),
                    });
                  }}
                >
                  {item.label}
                </a>
              );
            })}
          </ul>
        </div>
      </WindowWithButtons>
    );
  }
}
