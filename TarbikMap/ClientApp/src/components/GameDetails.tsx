import React, { Component } from "react";
import { QrCode } from "../QrCode";
import Clipboard from "react-clipboard.js";
import { WindowWithButtons } from "./WindowWithButtons";

type Props = {
  gameId: string;
  onBack: () => void;
};

type State = { copied: boolean };

export class GameDetails extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { copied: false };
  }

  render() {
    const url = "https://" + window.location.host + "/game/" + this.props.gameId;

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
          </div>
        }
      >
        <div>
          <div style={{ textAlign: "center" }}>
            <QrCode text={url} />
          </div>
          <div style={{ display: "flex", marginTop: "30px" }}>
            <input value={url} readOnly={true} />
            <Clipboard data-clipboard-text={url} button-className="btn" onSuccess={() => this.setState({ copied: true })}>
              Copy
            </Clipboard>
          </div>
          {this.state.copied ? <div>copied</div> : null}
        </div>
      </WindowWithButtons>
    );
  }
}
