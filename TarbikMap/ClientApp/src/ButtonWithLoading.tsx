import React, { Component } from "react";

type Props = {
  text: string;
  className: string;
  disabled: boolean;
  automaticallyResetLoading: boolean;
  onClick: () => void;
};
type State = {
  loading: boolean;
};

export class ButtonWithLoading extends Component<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = {
      loading: false,
    };
  }

  render() {
    return (
      <button
        className={this.props.className}
        disabled={this.props.disabled || this.state.loading}
        onClick={() => {
          this.setState({ loading: true }, async () => {
            await this.props.onClick();
            if (this.props.automaticallyResetLoading) {
              this.setState({ loading: false });
            }
          });
        }}
      >
        {this.props.text}
      </button>
    );
  }
}
