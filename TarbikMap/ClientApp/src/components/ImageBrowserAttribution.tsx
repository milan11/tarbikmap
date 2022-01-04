import React, { Component } from "react";

type Props = {
  gameId: string;
  taskIndex: number;
  imageIndex: number;
};
type State = {
  loadedAttribution: string | null;
};

export class ImageBrowserAttribution extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = { loadedAttribution: null };
  }

  componentDidMount() {
    this.fetchAttribution();
  }

  render() {
    return <div className="imageAttribution">{this.state.loadedAttribution} </div>;
  }

  async fetchAttribution() {
    const response = await fetch("games/" + encodeURIComponent(this.props.gameId) + "/imageAttribution/" + encodeURIComponent(this.props.taskIndex) + "/" + encodeURIComponent(this.props.imageIndex));
    const text = await response.text();

    this.setState({
      loadedAttribution: text,
    });
  }
}
