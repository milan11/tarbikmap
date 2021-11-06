import React, { Component } from "react";

type Props = {};
type State = {
  hidden: boolean;
};

export class ObservingMessage extends Component<Props, State> {
  _isMounted: boolean = false;

  constructor(props: Props) {
    super(props);

    this.state = { hidden: false };
  }

  componentDidMount() {
    this._isMounted = true;
    setTimeout(() => {
      if (this._isMounted) {
        this.setState({ hidden: true });
      }
    }, 5000);
  }

  componentWillUnmount() {
    this._isMounted = false;
  }

  render() {
    if (this.state.hidden) {
      return null;
    }
    return <div className="message">Not joined - observing only.</div>;
  }
}
