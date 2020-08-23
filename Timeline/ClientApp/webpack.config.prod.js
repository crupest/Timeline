const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const CopyPlugin = require('copy-webpack-plugin');
const WorkboxPlugin = require('workbox-webpack-plugin');

const config = require('./webpack.common');

config.mode('production');

config
  .entry('index')
  .add(path.resolve(__dirname, 'src/app/service-worker.tsx'));

config.devtool('source-map');

config.plugin('clean').use(CleanWebpackPlugin);

config.plugin('copy').use(CopyPlugin, [
  {
    patterns: [
      {
        from: path.resolve(__dirname, 'public/'),
        to: path.resolve(__dirname, 'dist/'),
      },
    ],
  },
]);

config.plugin('workbox').use(WorkboxPlugin.InjectManifest, [
  {
    swSrc: path.resolve(__dirname, 'src/sw/sw.ts'),
    maximumFileSizeToCacheInBytes: 15000000,
  },
]);

module.exports = config.toConfig();
