import { sizeof_uint32, byteArrayToInt_be, concatArrays } from "./utils/DataUtils";

const pako = require("pako");

/// <reference path="./utils/DataUtils" />

const gzippedFileHeader = new Uint8Array([31, 139, 8, 0, 0, 0, 0, 0, 0, 3]);

const mergedUrlScheme = "mergedtiles://";

type Index = {
  z: number;
  x: number;
  files: File[];
};

type File = {
  y: number;
  pos: number;
  length: number | null;
};

class LoadTile {
  async loadUrl(request: Request | string, orig: (input: RequestInfo, init?: RequestInit) => Promise<Response>) {
    const providedUrl = (request as Request).url ? (request as Request).url : (request as string);

    if (providedUrl.indexOf(mergedUrlScheme) === 0) {
      let urlWithoutScheme = providedUrl.substring(mergedUrlScheme.length);

      let url = "https://" + urlWithoutScheme;

      const parts = url.split("/");

      const changedUrl = parts.slice(0, parts.length - 3).join("/");

      const x = parseInt(parts[parts.length - 2]);
      const y = parseInt(parts[parts.length - 1].split(".")[0]);
      const z = parseInt(parts[parts.length - 3]);

      const tile = await this.loadTile(changedUrl, z, x, y);
      if (tile !== null) {
        return new Response(tile);
      } else {
        const init = { status: 404, statusText: "Not found" };
        return new Response("", init);
      }
    } else {
      return orig(request);
    }
  }

  async loadTile(url: string, z: number, x: number, y: number) {
    const index = await this.fetchIndex(url, z, x);

    if (index === null) {
      return null;
    }

    const foundFile = index.files.find((file) => file.y === y);
    if (foundFile === undefined) {
      return null;
    }

    return this.fetchTile(url, z, x, foundFile);
  }

  async fetchIndex(url: string, z: number, x: number) {
    const packedUrl = this.getPackedUrl_index(url, z, x);

    const complete = await this.fetchComplete(packedUrl, undefined);
    if (complete === null) {
      return null;
    }

    const part_intervalsCount = complete.subarray(0, 0 + sizeof_uint32);

    const intervalsCount = byteArrayToInt_be(part_intervalsCount, 0, sizeof_uint32);

    const part_intervals = complete.subarray(sizeof_uint32, sizeof_uint32 + intervalsCount * (2 * sizeof_uint32));

    let entriesCount = 0;

    {
      for (let i = 0; i < intervalsCount; ++i) {
        const begin = byteArrayToInt_be(part_intervals, i * (2 * sizeof_uint32), sizeof_uint32);
        const end = byteArrayToInt_be(part_intervals, i * (2 * sizeof_uint32) + sizeof_uint32, sizeof_uint32);

        entriesCount += end - begin + 1;
      }
    }

    const offset = sizeof_uint32 + intervalsCount * (2 * sizeof_uint32);
    const length = entriesCount * sizeof_uint32;
    const part_index = complete.subarray(offset, offset + length);

    const result: Index = { z: z, x: x, files: [] };

    {
      let entriesCount = 0;

      for (let i = 0; i < intervalsCount; ++i) {
        const begin = byteArrayToInt_be(part_intervals, i * (2 * sizeof_uint32), sizeof_uint32);
        const end = byteArrayToInt_be(part_intervals, i * (2 * sizeof_uint32) + sizeof_uint32, sizeof_uint32);

        for (let y = begin; y <= end; ++y) {
          const indexEntryPos = (entriesCount + (y - begin)) * sizeof_uint32;

          const pos = byteArrayToInt_be(part_index, indexEntryPos, sizeof_uint32);
          result.files.push({ y: y, pos: pos, length: null });
        }

        entriesCount += end - begin + 1;
      }
    }

    let allFilesPos = result.files.map((file) => file.pos);
    allFilesPos = allFilesPos.filter((x, i, a) => a.indexOf(x) === i);
    allFilesPos.sort((a, b) => a - b);

    result.files.forEach((file) => {
      const filePosIndex = allFilesPos.indexOf(file.pos);
      const fileLength = filePosIndex === allFilesPos.length - 1 ? null : allFilesPos[filePosIndex + 1] - file.pos;
      file.length = fileLength;
    });

    return result;
  }

  async fetchTile(url: string, z: number, x: number, file: File) {
    const packedUrl = this.getPackedUrl_tiles(url, z, x);

    let range = "" + file.pos + "-";
    if (file.length !== null) {
      range += file.pos + file.length - 1;
    }

    let result = await this.fetchComplete(packedUrl, range);
    if (result === null) {
      return null;
    }
    result = pako.inflate(concatArrays(gzippedFileHeader, result));
    return result;
  }

  async fetchComplete(packedUrl: string, range?: string) {
    const response = await fetch(
      packedUrl,
      range
        ? {
            headers: {
              Range: "bytes=" + range,
            },
          }
        : undefined
    );
    if (!response.ok) {
      return null;
    }

    const arrayBuffer = await response.arrayBuffer();
    const array = new Uint8Array(arrayBuffer);

    return array;
  }

  getPackedUrl_tiles(url: string, z: number, x: number) {
    return `${url}/${z}/${z}_${x}.t`;
  }

  getPackedUrl_index(url: string, z: number, x: number) {
    return `${url}/${z}/${z}_${x}.i`;
  }
}

export async function loadUrl(request: Request | string, orig: (input: RequestInfo, init?: RequestInit) => Promise<Response>) {
  return new LoadTile().loadUrl(request, orig);
}
