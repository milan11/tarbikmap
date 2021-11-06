import React, { Component } from "react";

type Props = {
  windowStyle: string;
  header?: React.ReactNode;
  buttons?: React.ReactNode;
};
type State = {};

export class WindowWithButtons extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
  }

  render() {
    return (
      <div className={"window_" + this.props.windowStyle} style={{ display: "flex", flexDirection: "column", overflow: "visible" }}>
        {this.props.header ? <div style={{ flex: "1 0 auto" }}>{this.props.header}</div> : null}
        <div className="card-panel" style={{ flex: "1 1 auto", overflowY: "auto" }}>
          {this.props.children}
        </div>
        {this.props.buttons ? <div style={{ flex: "1 0 auto" }}>{this.props.buttons}</div> : null}
      </div>
    );
  }
}
