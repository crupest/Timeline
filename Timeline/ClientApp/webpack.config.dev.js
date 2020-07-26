const path = require('path');
const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const PnpWebpackPlugin = require('pnp-webpack-plugin');

const { commonRules, htmlCommonConfig } = require('./webpack.common');

module.exports = (env) => {
  const entry = ['react-hot-loader/patch', './src/app/index.tsx'];

  if (env && env.TIMELINE_USE_MOCK_BACKEND) {
    entry.push(path.join(__dirname, 'src/app/http/mock/install.ts'));
  }

  return {
    entry,
    mode: 'development',
    devtool: 'eval-source-map',
    module: {
      rules: [
        ...commonRules,
        {
          test: /\.ts(x?)$/,
          exclude: /node_modules/,
          use: [
            {
              loader: 'babel-loader',
              options: {
                plugins: ['react-hot-loader/babel'],
              },
            },
            {
              loader: 'ts-loader',
            },
          ],
        },
        {
          test: /\.js(x?)$/,
          exclude: /node_modules/,
          use: [
            {
              loader: 'babel-loader',
              options: {
                plugins: ['react-hot-loader/babel'],
              },
            },
          ],
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
      contentBase: path.resolve(__dirname, 'public/'),
      host: '0.0.0.0',
      port: 3000,
      historyApiFallback: true,
      hotOnly: true,
      allowedHosts: ['.myide.io'],
    },
    plugins: [
      new HtmlWebpackPlugin({
        ...htmlCommonConfig,
      }),
      new webpack.HotModuleReplacementPlugin(),
    ],
  };
};
