import React, { Component } from "react";
import { Route } from "react-router-dom";
import { HomeAbout } from "./HomeAbout";
import { HomeJoin } from "./HomeJoin";
import { HomeNew } from "./HomeNew";

type Props = {
  onPrepareClean: () => void;
  onHideGeometry: () => void;
};
type State = {};

export class Home extends Component<Props, State> {
  componentDidMount() {
    this.props.onPrepareClean();
    this.props.onHideGeometry();
  }

  render() {
    return (
      <div>
        <Route exact path="/" render={(props) => this.renderMenu(0)} />
        <Route exact path="/join" render={(props) => this.renderMenu(1)} />
        <Route exact path="/about" render={(props) => this.renderMenu(2)} />

        <div className="homeContent">
          <Route exact path="/" render={(props) => <HomeNew />} />
          <Route exact path="/join" render={(props) => <HomeJoin />} />
          <Route exact path="/about" render={(props) => <HomeAbout />} />
        </div>
      </div>
    );
  }

  renderMenu(selectedItem: number) {
    return (
      <div className="nav">
        <nav>
          <div className="nav-wrapper">
            <Route
              render={({ history }) => (
                // eslint-disable-next-line
                <a
                  onClick={() => {
                    history.push("/");
                  }}
                  className="brand-logo right hide-on-small-and-down"
                  style={{ transform: "translateY(-50%)" }}
                >
                  Tarbik Map
                </a>
              )}
            />

            <ul id="nav-mobile" className="left">
              <li className={selectedItem === 0 ? "active" : undefined}>
                <Route
                  render={({ history }) => (
                    // eslint-disable-next-line
                    <a
                      onClick={() => {
                        history.push("/");
                      }}
                    >
                      New Game
                    </a>
                  )}
                />
              </li>
              <li className={selectedItem === 1 ? "active" : undefined}>
                <Route
                  render={({ history }) => (
                    // eslint-disable-next-line
                    <a
                      onClick={() => {
                        history.push("/join");
                      }}
                    >
                      Join Game
                    </a>
                  )}
                />
              </li>
              <li className={selectedItem === 2 ? "active" : undefined}>
                <Route
                  render={({ history }) => (
                    // eslint-disable-next-line
                    <a
                      onClick={() => {
                        history.push("/about");
                      }}
                    >
                      About
                    </a>
                  )}
                />
              </li>
            </ul>
          </div>
        </nav>
      </div>
    );
  }
}
