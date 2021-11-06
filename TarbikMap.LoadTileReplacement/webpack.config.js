const path = require('path');

module.exports = {
  entry: './tiles/loadTile.ts',
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader'
      },
    ],
  },
  resolve: {
    extensions: ['.tsx', '.ts', '.js'],
  },
  output: {
    filename: 'bundle.js',
    path: path.resolve(__dirname, 'dist'),
    libraryTarget: 'var',
    library: 'LoadTileEntryPoint'
  }
};
