const path = require('path');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');
const CopyPlugin = require('copy-webpack-plugin');
const PnpWebpackPlugin = require('pnp-webpack-plugin');
const WorkboxPlugin = require('workbox-webpack-plugin');

const { commonRules, htmlCommonConfig } = require('./webpack.common');

const config = {
  entry: ['./src/app/index.tsx', './src/app/service-worker.tsx'],
  mode: 'production',
  devtool: 'source-map',
  module: {
    rules: [
      ...commonRules,
      {
        test: /\.(js|jsx|ts|tsx)$/,
        exclude: /node_modules/,
        loader: 'babel-loader',
      },
    ],
  },
  resolve: {
    extensions: ['*', '.js', '.jsx', '.ts', '.tsx'],
    plugins: [PnpWebpackPlugin],
  },
  resolveLoader: {
    plugins: [PnpWebpackPlugin.moduleLoader(module)],
  },
  optimization: {
    runtimeChunk: 'single',
    splitChunks: {
      chunks: 'all',
      cacheGroups: {
        vendor: {
          test: /[\\/]node_modules[\\/]/,
          name: 'vendors',
          chunks: 'all',
        },
      },
    },
  },
  output: {
    path: path.resolve(__dirname, 'dist/'),
    filename: '[name].[hash].js',
    chunkFilename: '[name].[hash].js',
    publicPath: '/',
  },
  plugins: [
    new CleanWebpackPlugin(),
    new HtmlWebpackPlugin(htmlCommonConfig),
    new ForkTsCheckerWebpackPlugin({
      tsconfig: './src/app/tsconfig.json',
    }),
    new ForkTsCheckerWebpackPlugin({
      tsconfig: './src/sw/tsconfig.json',
    }),
    new CopyPlugin({
      patterns: [
        {
          from: path.resolve(__dirname, 'public/'),
          to: path.resolve(__dirname, 'dist/'),
        },
      ],
    }),
    new WorkboxPlugin.InjectManifest({
      swSrc: './src/sw/sw.ts',
      maximumFileSizeToCacheInBytes: 15000000,
    }),
  ],
};

module.exports = config;
