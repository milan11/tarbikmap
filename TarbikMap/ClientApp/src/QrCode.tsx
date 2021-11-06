import React, { Component } from "react";
import QRCode from "qrcode";

type Props = {
  text: string;
};
type State = {
  createdImageUrl: string | null;
};

export class QrCode extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = { createdImageUrl: null };
  }

  componentDidMount() {
    this.recreateImageUrl();
  }

  componentDidUpdate(prevProps: Props) {
    if (prevProps && prevProps.text !== this.props.text) {
      this.recreateImageUrl();
    }
  }

  recreateImageUrl() {
    QRCode.toDataURL(this.props.text, (err, url) => {
      this.setState({ createdImageUrl: url });
    });
  }

  render() {
    if (!this.state.createdImageUrl) {
      return null;
    }

    return <img alt={this.props.text} src={this.state.createdImageUrl} />;
  }
}
