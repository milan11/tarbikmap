import React, { Component } from "react";
import { WindowWithButtons } from "./WindowWithButtons";

type Props = {};
type State = { items: Item[]; selectedItem: number };

type Item = {
  name: string;
  author: string | null;
  repository: string | null;
  license: string | null;
  licenseText: string | null;
};

export class HomeAbout extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { items: [], selectedItem: -1 };
  }

  componentDidMount() {
    this.fetchLicenses();
  }

  async fetchLicenses() {
    const response = await fetch("/licenses.json");
    const json = await response.json();

    let items: Item[] = [];
    items = items.concat(json);

    items.sort((a, b) => {
      if (a.name < b.name) {
        return -1;
      }
      if (a.name > b.name) {
        return 1;
      }
      return 0;
    });

    this.setState({
      items: items,
    });
  }

  render() {
    return (
      <WindowWithButtons windowStyle="center">
        <h4>Tarbik Map</h4>
        <span>Game for 1 to 20 players about guessing locations on a map. The player with the nearest guess gets the most points.</span>

        <h4>We use:</h4>

        <h5>Map</h5>
        <div className="collection">
          <a className="collection-item" href="https://www.openstreetmap.org/" target="_blank" rel="noopener noreferrer">
            OpenStreetMap
          </a>
          <a className="collection-item" href="https://openmaptiles.org/" target="_blank" rel="noopener noreferrer">
            OpenMapTiles
          </a>
        </div>

        <h5>Areas data and areas search</h5>
        <div className="collection">
          <a className="collection-item" href="https://www.nominatim.org/" target="_blank" rel="noopener noreferrer">
            Nominatim
          </a>
          <a className="collection-item" href="https://wiki.openstreetmap.org/wiki/Overpass_API" target="_blank" rel="noopener noreferrer">
            Overpass API
          </a>
          <a className="collection-item" href="https://www.naturalearthdata.com/" target="_blank" rel="noopener noreferrer">
            Natural Earth
          </a>
        </div>

        <h5>Photos and photos search</h5>
        <div className="collection">
          <a className="collection-item" href="https://developers.google.com/maps/documentation/streetview/overview" target="_blank" rel="noopener noreferrer">
            Google StreetView Static API
          </a>
          <a className="collection-item" href="https://kartaview.org/" target="_blank" rel="noopener noreferrer">
            OpenStreetCam
          </a>
          <a className="collection-item" href="https://www.flickr.com/" target="_blank" rel="noopener noreferrer">
            Flickr
          </a>
          <a className="collection-item" href="https://www.wikidata.org/" target="_blank" rel="noopener noreferrer">
            Wikidata
          </a>
          <a className="collection-item" href="https://www.wikimedia.org/" target="_blank" rel="noopener noreferrer">
            Wikimedia
          </a>
        </div>

        <h5>Home page photos</h5>
        <div className="collection">
          <a className="collection-item" href="https://www.wikimedia.org/" target="_blank" rel="noopener noreferrer">
            Wikimedia
          </a>
        </div>

        <h5>Geocoding</h5>
        <div className="collection">
          <a className="collection-item" href="https://developers.google.com/maps/documentation/geocoding/overview" target="_blank" rel="noopener noreferrer">
            Google Reverse Geocoding API
          </a>
        </div>

        <h5>Client-side libraries</h5>
        <div className="collection">
          {this.state.items.map((item, i) => {
            if (i === this.state.selectedItem) {
              return this.renderDetails(item, i);
            } else {
              return this.renderNameOnly(item, i);
            }
          })}
        </div>

        <div>Additionally, please see NOTICE file in the source code.</div>
      </WindowWithButtons>
    );
  }

  renderNameOnly(item: Item, key: number) {
    return (
      // eslint-disable-next-line
      <a key={key} className="collection-item" onClick={() => this.setState({ selectedItem: this.state.selectedItem !== key ? key : -1 })} style={{ cursor: "pointer" }}>
        {item.name}
      </a>
    );
  }

  renderDetails(item: Item, key: number) {
    return (
      <div key={key} className="collection-item" onClick={() => this.setState({ selectedItem: this.state.selectedItem !== key ? key : -1 })} style={{ overflowY: "scroll" }}>
        {item.name ? (
          <div>
            <strong>Library:</strong> {item.name}
          </div>
        ) : null}
        {item.author ? (
          <div>
            <strong>Author:</strong> {item.author}
          </div>
        ) : null}
        {item.repository ? (
          <div>
            <strong>Repository:</strong> {item.repository}
          </div>
        ) : null}
        {item.license ? (
          <div>
            <strong>License:</strong> {item.license}
          </div>
        ) : null}
        {item.licenseText ? (
          <div>
            <strong>License text:</strong> <pre>{item.licenseText}</pre>
          </div>
        ) : null}
      </div>
    );
  }
}
