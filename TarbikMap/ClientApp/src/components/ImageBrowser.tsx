import React, { Component } from "react";
import { ImageBrowserAttribution } from "./ImageBrowserAttribution";

type Props = {
  onClick: (() => void) | undefined;
  gameId: string;
  taskIndex: number;
  imagesCount: number;
  attributionShown: boolean;
};
type State = {
  imageIndex: number;
};

export class ImageBrowser extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = { imageIndex: 0 };
  }

  render() {
    return [
      <img
        alt=""
        onClick={this.props.onClick}
        src={"games/" + encodeURIComponent(this.props.gameId) + "/image/" + encodeURIComponent(this.props.taskIndex) + "/" + encodeURIComponent(this.state.imageIndex)}
        className="fullWindow"
        style={this.props.onClick ? { cursor: "pointer" } : undefined}
      />,
      this.props.attributionShown ? <ImageBrowserAttribution key={this.state.imageIndex} gameId={this.props.gameId} taskIndex={this.props.taskIndex} imageIndex={this.state.imageIndex} /> : null,
      this.props.imagesCount > 1 ? (
        <div
          className="left"
          style={{ cursor: "pointer", fontSize: "xxx-large", padding: "10px", backgroundColor: "#ffffffaa" }}
          onClick={() => {
            this.setState({
              imageIndex: this.state.imageIndex > 0 ? this.state.imageIndex - 1 : this.props.imagesCount - 1,
            });
          }}
        >
          &lt;
        </div>
      ) : null,
      this.props.imagesCount > 1 ? (
        <div
          className="right"
          style={{ cursor: "pointer", fontSize: "xxx-large", padding: "10px", backgroundColor: "#ffffffaa" }}
          onClick={() => {
            this.setState({
              imageIndex: (this.state.imageIndex + 1) % this.props.imagesCount,
            });
          }}
        >
          &gt;
        </div>
      ) : null,
    ];
  }
}
