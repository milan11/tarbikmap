import React, { Component } from "react";
import { AppWithMap } from "./AppWithMap";

import "./custom.css";

export default class App extends Component {
  static displayName = App.name;

  render() {
    return <AppWithMap></AppWithMap>;
  }
}
