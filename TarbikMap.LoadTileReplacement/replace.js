const file_dest = "../TarbikMap/ClientApp/node_modules/mapbox-gl/dist/mapbox-gl.js";
const file_backup = "../TarbikMap/ClientApp/node_modules/mapbox-gl/dist/mapbox-gl.js.bk";
const file_search = "search.txt";
const file_replacement = "dist/bundle.js";

const fs = require('fs');

if (fs.existsSync(file_backup)) {
  fs.renameSync(file_backup, file_dest);
}

fs.renameSync(file_dest, file_backup);

const string_search = fs.readFileSync(file_search, { encoding: 'utf8' });
let string_replacement = fs.readFileSync(file_replacement, { encoding: 'utf8' });

string_replacement += "LoadTileEntryPoint.loadUrl(i,o.fetch)";

let str = fs.readFileSync(file_backup, { encoding: 'utf8' });
str = str.replace(string_search, string_replacement);

fs.writeFileSync(file_dest, str);
