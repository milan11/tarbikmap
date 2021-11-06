interface Styles {
  styles: Style[];
  default_font: string;
}

interface Style {
  url: string;
  has_tiles: Tiles[];
}

interface Tiles {
  zoom: "all" | number | MinMax;
  x: "all" | number | MinMax;
  y: "all" | number | MinMax;
}

interface MinMax {
  min: number;
  max: number;
}

export class StylesJson {
  json: Styles;

  constructor(jsonStr: string) {
    this.json = JSON.parse(jsonStr);
  }

  tileXtoLon(zoom: number, x: number) {
    const n = Math.pow(2, zoom);
    return (x / n) * 360.0 - 180.0;
  }

  tileYtoLat(zoom: number, y: number) {
    const n = Math.pow(2, zoom);
    const lat_rad = Math.atan(Math.sinh(Math.PI * (1 - (2 * y) / n)));
    return (lat_rad * 180.0) / Math.PI;
  }

  getPossibleStyles(zoom: number, map_min_lat: number, map_max_lat: number, map_min_lon: number, map_max_lon: number) {
    const found: { styleUrl: string; forZoom: number }[] = [];

    this.json.styles.forEach((style) => {
      let styleMaxZoom = -1;
      style.has_tiles.forEach((tiles) => {
        const zoom = this.getMax(tiles.zoom);
        if (zoom > styleMaxZoom) {
          styleMaxZoom = zoom;
        }
      });

      if (styleMaxZoom === -1) {
        throw new Error("Cannot find any zoom for style: " + style.url);
      }

      if (zoom <= styleMaxZoom) {
        if (this.hasTilesForZoom(zoom, style, map_min_lat, map_max_lat, map_min_lon, map_max_lon)) {
          found.push({ styleUrl: style.url, forZoom: zoom });
        }
      } else {
        if (this.hasTilesForZoom(styleMaxZoom, style, map_min_lat, map_max_lat, map_min_lon, map_max_lon)) {
          found.push({ styleUrl: style.url, forZoom: styleMaxZoom });
        }
      }
    });

    let bestFoundZoom = 0;
    found.forEach((item) => {
      if (item.forZoom > bestFoundZoom) {
        bestFoundZoom = item.forZoom;
      }
    });

    return found.filter((item) => item.forZoom === bestFoundZoom).map((item) => item.styleUrl);
  }

  hasTilesForZoom(zoom: number, style: Style, map_min_lat: number, map_max_lat: number, map_min_lon: number, map_max_lon: number) {
    return style.has_tiles.some((tiles) => {
      return (
        this.isIn(zoom, tiles.zoom) &&
        this.isIn(map_min_lat, this.convertY(zoom, tiles.y)) &&
        this.isIn(map_max_lat, this.convertY(zoom, tiles.y)) &&
        this.isIn(map_min_lon, this.convertX(zoom, tiles.x)) &&
        this.isIn(map_max_lon, this.convertX(zoom, tiles.x))
      );
    });
  }

  isIn(val: number, def: "all" | number | MinMax) {
    if (def === "all") {
      return true;
    }

    if (def === val) {
      return true;
    }

    return val >= (def as MinMax).min && val <= (def as MinMax).max;
  }

  getMax(def: "all" | number | MinMax) {
    if (def === "all") {
      return 999;
    }

    if (typeof def === "number") {
      return def;
    }

    return (def as MinMax).max;
  }

  convertX(zoom: number, def: "all" | number | MinMax) {
    if (def === "all") {
      return "all";
    }

    if (typeof def === "number") {
      return this.tileXtoLon(zoom, def);
    }

    return {
      min: this.tileXtoLon(zoom, def.min),
      max: this.tileXtoLon(zoom, def.max),
    };
  }

  convertY(zoom: number, def: "all" | number | MinMax) {
    if (def === "all") {
      return "all";
    }

    if (typeof def === "number") {
      return this.tileYtoLat(zoom, def);
    }

    return {
      min: this.tileYtoLat(zoom, def.max),
      max: this.tileYtoLat(zoom, def.min),
    };
  }

  getDefaultFont() {
    return this.json.default_font;
  }
}
