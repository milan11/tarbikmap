import React, { Component } from "react";

type Props = {
  timeRemainingMs: number;
};
type State = {
  timeRemainingMs: number;
  onTime: number;
  counter: number;
};

export class TimerMessage extends Component<Props, State> {
  _isMounted: boolean = false;

  constructor(props: Props) {
    super(props);

    this.state = { timeRemainingMs: props.timeRemainingMs, onTime: Date.now(), counter: 0 };
  }

  componentDidMount() {
    this._isMounted = true;
    this.count();
  }

  componentWillUnmount() {
    this._isMounted = false;
  }

  componentDidUpdate(prevProps: Props) {
    if (prevProps && this.props.timeRemainingMs !== prevProps.timeRemainingMs) {
      this.setState({ timeRemainingMs: this.props.timeRemainingMs, onTime: Date.now() });
    }
  }

  count() {
    setTimeout(() => {
      if (this._isMounted) {
        this.setState({ counter: this.state.counter + 1 });
        this.count();
      }
    }, 100);
  }

  render() {
    const remaining = this.state.timeRemainingMs + this.state.onTime - Date.now();

    if (remaining >= 11000 || remaining < 0) {
      return null;
    }
    return <div className="message">{Math.floor(remaining / 1000)}</div>;
  }
}
