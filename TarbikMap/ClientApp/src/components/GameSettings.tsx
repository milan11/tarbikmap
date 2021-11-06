import React, { Component } from "react";
import { GameConfigurationDTO } from "../ts/DTO/GameConfigurationDTO";
import { WindowWithButtons } from "./WindowWithButtons";

type Configuration = {
  tasksCount: number;
  answeringTimeLimitSeconds: number;
  completingTimeLimitSeconds: number;
};

type Props = {
  configuration: GameConfigurationDTO;
  onSelect: (configuration: Configuration) => void;
  onBack: () => void;
};
type State = {
  configuration: Configuration;
};

export class GameSettings extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = {
      configuration: {
        tasksCount: this.props.configuration.tasksCount,
        answeringTimeLimitSeconds: this.props.configuration.answeringTimeLimitSeconds,
        completingTimeLimitSeconds: this.props.configuration.completingTimeLimitSeconds,
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
          Rounds:
          <select
            style={{ display: "initial" }}
            className="form-control"
            value={this.state.configuration.tasksCount}
            onChange={(event) => {
              this.setState({
                configuration: Object.assign({}, this.state.configuration, {
                  tasksCount: parseInt(event.target.value, 10),
                }),
              });
            }}
          >
            {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20].map((count) => (
              <option key={count} value={count}>
                {count}
              </option>
            ))}
          </select>
        </div>
        <div style={{ marginTop: "20px" }}>
          Answer Time Limit (seconds):
          <select
            style={{ display: "initial" }}
            className="form-control"
            value={this.state.configuration.answeringTimeLimitSeconds}
            onChange={(event) => {
              this.setState({
                configuration: Object.assign({}, this.state.configuration, {
                  answeringTimeLimitSeconds: parseInt(event.target.value, 10),
                }),
              });
            }}
          >
            {[5, 10, 15, 20, 30, 40, 50, 60, 90, 120].map((count) => (
              <option key={count} value={count}>
                {count}
              </option>
            ))}
          </select>
        </div>
        <div style={{ marginTop: "20px" }}>
          Results Time Limit (seconds):
          <select
            style={{ display: "initial" }}
            value={this.state.configuration.completingTimeLimitSeconds}
            onChange={(event) => {
              this.setState({
                configuration: Object.assign({}, this.state.configuration, {
                  completingTimeLimitSeconds: parseInt(event.target.value, 10),
                }),
              });
            }}
          >
            {[5, 10, 15, 20, 30, 40, 50, 60, 90, 120].map((count) => (
              <option key={count} value={count}>
                {count}
              </option>
            ))}
          </select>
        </div>
      </WindowWithButtons>
    );
  }
}
