const path = require('path');
const webpack = require('webpack');

const config = require('./webpack.common');

config.mode('development');

config.entry('index').add('react-hot-loader/patch');

config.module
  .rule('ts')
  .use('babel')
  .options({
    plugins: ['react-hot-loader/babel'],
  });

config.module
  .rule('js')
  .use('babel')
  .options({
    plugins: ['react-hot-loader/babel'],
  });

config.devtool('eval-cheap-module-source-map');

config.resolve.alias.set('react-dom', '@hot-loader/react-dom');

config.devServer
  .contentBase(path.resolve(__dirname, 'public/'))
  .host('0.0.0.0')
  .port(3000)
  .historyApiFallback(true)
  .hotOnly(true)
  .allowedHosts.add('.myide.io');

config.plugin('hot').use(webpack.HotModuleReplacementPlugin);

module.exports = (env) => {
  if (env && env.TIMELINE_USE_MOCK_BACKEND) {
    config
      .entry('index')
      .add(path.join(__dirname, 'src/app/http/mock/install.ts'));
  }

  return config.toConfig();
};
