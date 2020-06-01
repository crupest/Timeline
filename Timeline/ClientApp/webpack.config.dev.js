const path = require('path');
const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');
const PnpWebpackPlugin = require('pnp-webpack-plugin');

const { commonRules, htmlCommonConfig } = require('./webpack.common');

const config = {
  entry: ['react-hot-loader/patch', './src/index.tsx'],
  mode: 'development',
  devtool: 'eval-source-map',
  module: {
    rules: [
      ...commonRules,
      {
        test: /\.(js|jsx|ts|tsx)$/,
        exclude: /node_modules/,
        loader: 'babel-loader',
        options: {
          plugins: ['react-hot-loader/babel'],
        },
      },
    ],
  },
  resolve: {
    alias: {
      'react-dom': '@hot-loader/react-dom',
    },
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
  devServer: {
    contentBase: path.join(__dirname, 'public/'),
    host: '0.0.0.0',
    port: 3000,
    publicPath: 'http://localhost:3000/',
    historyApiFallback: true,
    hotOnly: true,
  },
  plugins: [
    new HtmlWebpackPlugin({
      ...htmlCommonConfig,
      devServer: 'http://localhost:3000',
    }),
    new ForkTsCheckerWebpackPlugin(),
    new webpack.HotModuleReplacementPlugin(),
  ],
};

module.exports = config;
