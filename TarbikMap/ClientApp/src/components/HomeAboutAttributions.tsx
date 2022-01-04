import React, { Component } from "react";
import { PresetDTO } from "../ts/DTO/PresetDTO";

type Props = {};
type State = {
  presets: PresetDTO[] | null;
};

export class HomeAboutAttributions extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { presets: null };
  }

  componentDidMount() {
    this.fetchAttributions();
  }

  async fetchAttributions() {
    const response = await fetch("games/presets", {
      method: "POST",
    });

    if (response.ok) {
      const data = await response.json();
      this.setState({ presets: data });
    }
  }

  render() {
    return (
      <div className="collection">
        {this.getPresets().map((preset, i) => {
          return this.renderItem(preset, i);
        })}
      </div>
    );
  }

  renderItem(preset: PresetDTO, key: number) {
    return (
      // eslint-disable-next-line
      <a key={key} className="collection-item">
        <img style={{ width: "50px", height: "50px", marginRight: "10px" }} src={"/images/" + preset.imageKey} alt={preset.title} />
        {preset.imageAttribution}
      </a>
    );
  }

  getPresets() {
    return this.state.presets ? this.state.presets.slice() : [];
  }
}
