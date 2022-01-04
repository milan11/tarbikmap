import React, { Component } from "react";
import { HomeAboutLicenses } from "./HomeAboutLicenses";
import { HomeAboutAttributions } from "./HomeAboutAttributions";
import { WindowWithButtons } from "./WindowWithButtons";

type Props = {};
type State = {};

export class HomeAbout extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
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
        <HomeAboutLicenses />

        <h5>Home page images</h5>
        <HomeAboutAttributions />

        <div>Additionally, please see NOTICE file in the source code.</div>
      </WindowWithButtons>
    );
  }
}
