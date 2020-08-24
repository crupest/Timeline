const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const PnpWebpackPlugin = require('pnp-webpack-plugin');
const postcssPresetEnv = require('postcss-preset-env');
const Config = require('webpack-chain');

const config = new Config();

config.entry('index').add(path.resolve(__dirname, 'src/app/index.tsx'));

config.module
  .rule('ts')
  .test(/\.ts(x?)$/)
  .exclude.add(/node_modules/)
  .end()
  .use('babel')
  .loader('babel-loader')
  .end()
  .use('ts')
  .loader('ts-loader')
  .end();

config.module
  .rule('js')
  .test(/\.js(x?)$/)
  .exclude.add(/node_modules/)
  .end()
  .use('babel')
  .loader('babel-loader')
  .end();

config.module
  .rule('css')
  .test(/\.css$/)
  .use('css')
  .loader('css-loader')
  .end()
  .use('postcss')
  .loader('postcss-loader')
  .options({
    plugins: () => [postcssPresetEnv(/* pluginOptions */)],
  })
  .end();

config.module
  .rule('sass')
  .test(/\.(scss|sass)$/)
  .use('css')
  .loader('css-loader')
  .end()
  .use('postcss')
  .loader('postcss-loader')
  .options({
    plugins: () => [postcssPresetEnv(/* pluginOptions */)],
  })
  .end()
  .use('sass')
  .loader('sass-loader')
  .end();

config.module
  .rule('file')
  .test(/\.(png|jpe?g|gif|svg|woff|woff2|ttf|eot)$/i)
  .use('url')
  .loader('url-loader')
  .options({
    limit: 8192,
  });

config.resolve.extensions
  .add('*')
  .add('.js')
  .add('.jsx')
  .add('.ts')
  .add('.tsx')
  .end()
  .plugin('pnp')
  .use(PnpWebpackPlugin);

config.resolveLoader.plugin('pnp').use(PnpWebpackPlugin.moduleLoader(module));

config.output
  .path(path.resolve(__dirname, 'dist/'))
  .filename('[name].[hash].js')
  .chunkFilename('[name].[hash].js')
  .publicPath('/');

config.plugin('html').use(HtmlWebpackPlugin, [
  {
    template: 'src/app/index.ejs',
    title: 'Timeline',
  },
]);

module.exports = config;
