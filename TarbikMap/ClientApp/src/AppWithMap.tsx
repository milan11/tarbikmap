import React, { Component } from "react";
import { Route } from "react-router";
import { Home } from "./components/Home";
import { Game } from "./components/Game";
import ReactMapGL, { Source, Layer, MapRef } from "react-map-gl";
import { Position } from "geojson";

import { Point, ShowGeometry_Data, PrepareForShowingResults_Data, PrepareForSelecting_Data } from "./ts/types";
import { StylesJson } from "./StylesJson";

const maxZoom = 18;
const pitch = 45;

type Viewport = {
  latitude: number;
  longitude: number;
  zoom: number;
};

type Props = {};
type State = {
  viewport: Viewport;
  selectableLocation: boolean;
  selectedLocation: Point | null;
  locationsShown:
    | {
        label: string;
        lat: number;
        lon: number;
      }[]
    | null;
  geometryShown: Position[][] | null;
  stylesJson: StylesJson | null;
  currentStyleUrl: string | null;
  downloadedStyle: string | null;
  deserializedStylePostProcessed: any | null;
  mapLabelsShown: boolean | null;
};

export class AppWithMap extends Component<Props, State> {
  private mapRef: MapRef | null = null;

  constructor(props: Props) {
    super(props);

    this.state = {
      selectedLocation: null,
      selectableLocation: false,
      locationsShown: null,
      geometryShown: null,
      viewport: {
        latitude: 48.167,
        longitude: 17.112726,
        zoom: 8,
      },
      stylesJson: null,
      currentStyleUrl: null,
      downloadedStyle: null,
      deserializedStylePostProcessed: null,
      mapLabelsShown: null,
    };
  }

  async componentDidMount() {
    const response = await fetch("/config/styles.json?currentHost=" + window.location.host);
    const json = await response.text();

    const stylesJson = new StylesJson(json);
    this.setState({ stylesJson: stylesJson }, () => {
      this.updateStyle();
    });
  }

  componentDidUpdate(prevProps: Props, prevState: State) {
    if (prevState.viewport.latitude !== this.state.viewport.latitude || prevState.viewport.longitude !== this.state.viewport.longitude || prevState.viewport.zoom !== this.state.viewport.zoom) {
      if (this.state.stylesJson !== null) {
        this.updateStyle();
      }
    }
  }

  updateStyle() {
    const possibleStyleUrls = this.getPossibleStyleUrls();
    if (this.state.currentStyleUrl === null || !possibleStyleUrls.includes(this.state.currentStyleUrl)) {
      const styleUrl = possibleStyleUrls[0];
      this.setState({ currentStyleUrl: styleUrl }, async () => {
        const response = await fetch(styleUrl);
        const json = await response.text();
        if (this.state.currentStyleUrl === styleUrl) {
          this.setState({ downloadedStyle: json }, () => {
            if (this.state.mapLabelsShown !== null) {
              this.postProcessStyle();
            }
          });
        }
      });
    }
  }

  postProcessStyle() {
    const postProcessed = JSON.parse(this.state.downloadedStyle!);
    if (!this.state.mapLabelsShown) {
      this.removeLabels(postProcessed);
    }

    this.setState({ deserializedStylePostProcessed: postProcessed });
  }

  removeLabels(deserializedStyle: any) {
    deserializedStyle.layers.forEach((layer: any) => {
      if (layer.layout && layer.layout["text-field"]) {
        layer.layout["text-field"] = "";
      }
    });
  }

  render() {
    return (
      <div style={{ width: "100%", height: "100%", position: "absolute" }}>
        {this.renderMap()}
        {this.renderInner()}
      </div>
    );
  }

  renderMap() {
    return (
      <div style={{ width: "100%", height: "100%", position: "absolute" }} id="mapWrapper">
        {this.map()}
      </div>
    );
  }

  map() {
    return (
      <ReactMapGL
        {...this.state.viewport}
        width="100%"
        height="100%"
        onViewportChange={(nextViewport: Viewport) => this.setViewport(nextViewport)}
        mapStyle={this.state.deserializedStylePostProcessed}
        onClick={(event) => {
          if (this.state.selectableLocation) {
            this.setState({
              selectedLocation: [event.lngLat[1], event.lngLat[0]],
            });
          }
        }}
        maxZoom={maxZoom}
        pitch={pitch}
        ref={(ref) => {
          this.mapRef = ref;
          this.mapRef?.getMap().setRenderWorldCopies(false);
        }}
      >
        {this.getAdditionalSources()}
      </ReactMapGL>
    );
  }

  getPossibleStyleUrls() {
    const zoom = Math.floor(this.state.viewport.zoom);
    const mapBounds = this.mapRef?.getMap().getBounds();

    const map_min_lat = mapBounds.getSouth();
    const map_max_lat = mapBounds.getNorth();
    const map_min_lon = mapBounds.getWest();
    const map_max_lon = mapBounds.getEast();

    return this.state.stylesJson!.getPossibleStyleUrls(zoom, map_min_lat, map_max_lat, map_min_lon, map_max_lon);
  }

  getAdditionalSources() {
    let sources: JSX.Element[] = [];

    if (this.state.geometryShown) {
      sources.push(
        <Source key="s_geometry" id="s_geometry" type="geojson" data={{ type: "Feature", geometry: { type: "MultiLineString", coordinates: this.state.geometryShown }, properties: {} }}>
          <Layer
            type="line"
            paint={{
              "line-color": "#007cbf",
              "line-width": 5,
            }}
          />
        </Source>
      );
    }

    if (this.state.selectedLocation) {
      sources.push(
        <Source key="s_selected" id="s_selected" type="geojson" data={{ type: "Feature", geometry: { type: "Point", coordinates: this.pointToPosition(this.state.selectedLocation) }, properties: {} }}>
          <Layer
            type="circle"
            paint={{
              "circle-radius": 4,
              "circle-color": "#0000ff",
            }}
          />
        </Source>
      );
    }

    if (this.state.locationsShown) {
      const correctLocation = this.state.locationsShown![this.state.locationsShown!.length - 1];

      for (let i = 0; i < this.state.locationsShown!.length - 1; ++i) {
        const line = [
          [
            [correctLocation.lon, correctLocation.lat],
            [this.state.locationsShown![i].lon, this.state.locationsShown![i].lat],
          ],
        ];

        sources.push(
          <Source key={"s_location_line_" + i} id={"s_location_line_" + i} type="geojson" data={{ type: "Feature", geometry: { type: "MultiLineString", coordinates: line }, properties: {} }}>
            <Layer
              type="line"
              paint={{
                "line-color": "#0000ff",
                "line-width": 1,
              }}
            />
          </Source>
        );
      }

      this.state.locationsShown.forEach((location, i) => {
        sources.push(
          <Source
            key={"s_location_point_" + i}
            id={"s_location_point_" + i}
            type="geojson"
            data={{ type: "Feature", geometry: { type: "Point", coordinates: [location.lon, location.lat] }, properties: {} }}
          >
            <Layer
              type="circle"
              paint={{
                "circle-radius": 4,
                "circle-color": "#0000ff",
              }}
            />
          </Source>
        );
        sources.push(
          <Source
            key={"s_location_label_" + i}
            id={"s_location_label_" + i}
            type="geojson"
            data={{ type: "Feature", geometry: { type: "Point", coordinates: [location.lon, location.lat] }, properties: { description: location.label } }}
          >
            <Layer
              type="symbol"
              paint={{
                "text-color": "#0000aa",
                "text-halo-color": "rgba(255,255,255,0.7)",
                "text-halo-width": 1.5,
              }}
              layout={{
                "text-field": ["get", "description"],
                "text-font": [this.state.stylesJson!.getDefaultFont()],
                "text-variable-anchor": ["top", "bottom", "left", "right"],
                "text-radial-offset": 0.5,
                "text-justify": "auto",
                "icon-image": ["get", "icon"],
              }}
            />
          </Source>
        );
      });
    }

    return sources;
  }

  pointToPosition(p: Point): Position {
    return [p[1], p[0]];
  }

  setViewport(viewport: Viewport) {
    this.setState({ viewport: viewport });
  }

  prepareForSelecting(data: PrepareForSelecting_Data) {
    const question = data.question;

    const viewport = this.createViewContaining([
      [question.initialLat1, question.initialLon1],
      [question.initialLat2, question.initialLon2],
    ]);

    if (viewport.zoom > maxZoom) {
      viewport.zoom = maxZoom;
    }
    if (viewport.zoom < 0) {
      viewport.zoom = 0;
    }

    this.setState({
      selectableLocation: data.canSelect,
      selectedLocation: null,
      locationsShown: null,
      viewport: viewport,
    });
  }

  prepareForShowingResults(data: PrepareForShowingResults_Data) {
    const locationsShown = [];
    data.playerAnswers.forEach((playerAnswer) => {
      locationsShown.push({ label: playerAnswer.name, lat: playerAnswer.answer.lat, lon: playerAnswer.answer.lon });
    });
    locationsShown.push({ label: data.correctAnswer.description, lat: data.correctAnswer.lat, lon: data.correctAnswer.lon });

    const viewport =
      data.playerAnswers.length > 0
        ? this.createViewContaining(locationsShown.map((location) => [location.lat, location.lon]))
        : this.createViewContaining([
            [data.question.initialLat1, data.question.initialLon1],
            [data.question.initialLat2, data.question.initialLon2],
          ]);

    if (viewport.zoom > maxZoom) {
      viewport.zoom = maxZoom;
    }
    if (viewport.zoom < 0) {
      viewport.zoom = 0;
    }

    this.setState({
      selectableLocation: false,
      selectedLocation: null,
      locationsShown: locationsShown,
      viewport: viewport,
    });
  }

  prepareClean() {
    this.setState({
      selectableLocation: false,
      selectedLocation: null,
      locationsShown: null,
    });
  }

  showGeometry(data: ShowGeometry_Data) {
    this.setState({ geometryShown: data.geometry.lines.map((l) => l.points.map(this.pointToPosition)) });
  }

  showMapLabels(mapLabelsShown: boolean, allowChange: boolean) {
    if (allowChange || this.state.mapLabelsShown === null) {
      if (this.state.mapLabelsShown !== mapLabelsShown) {
        this.setState({ mapLabelsShown: mapLabelsShown }, () => {
          if (this.state.downloadedStyle) {
            this.postProcessStyle();
          }
        });
      }
    }
  }

  hideGeometry() {
    this.setState({ geometryShown: null });
  }

  renderInner() {
    return (
      <div>
        <Route
          exact
          path={["/", "/join", "/about"]}
          render={(props) => (
            <Home
              {...props}
              onPrepareClean={() => this.prepareClean()}
              onHideGeometry={() => this.hideGeometry()}
              onShowMapLabels={(mapLabelsShown, allowChange) => this.showMapLabels(mapLabelsShown, allowChange)}
            />
          )}
        />
        <Route
          path="/game/:gameId"
          render={(props) => (
            <Game
              {...props}
              onPrepareForSelecting={(data) => this.prepareForSelecting(data)}
              onPrepareForShowingResults={(data) => this.prepareForShowingResults(data)}
              onPrepareClean={() => this.prepareClean()}
              onShowGeometry={(data) => this.showGeometry(data)}
              onShowMapLabels={(mapLabelsShown, allowChange) => this.showMapLabels(mapLabelsShown, allowChange)}
              selectedLocation={this.state.selectedLocation}
            />
          )}
        />
      </div>
    );
  }

  createViewContaining(points: Point[]): Viewport {
    let viewport = null;
    if (points.length > 0) {
      let min_lat = null;
      let max_lat = null;
      let min_lon = null;
      let max_lon = null;

      for (let i = 0; i < points.length; ++i) {
        const point = points[i];

        if (min_lat === null || point[0] < min_lat) {
          min_lat = point[0];
        }
        if (max_lat === null || point[0] > max_lat) {
          max_lat = point[0];
        }
        if (min_lon === null || point[1] < min_lon) {
          min_lon = point[1];
        }
        if (max_lon === null || point[1] > max_lon) {
          max_lon = point[1];
        }
      }

      viewport = this.centerZoomFromLocations(min_lat!, max_lat!, min_lon!, max_lon!);
    } else {
      viewport = { latitude: 48, longitude: 0, zoom: 5 };
    }

    return viewport;
  }

  centerZoomFromLocations(min_lat: number, max_lat: number, min_lon: number, max_lon: number): Viewport {
    const camera = this.mapRef!.getMap().cameraForBounds(
      [
        [min_lon, min_lat],
        [max_lon, max_lat],
      ],
      {
        bearing: 0,
        pitch: pitch,
        padding: 100,
      }
    );

    return {
      latitude: camera.center.lat,
      longitude: camera.center.lng,
      zoom: camera.zoom,
    };
  }
}
