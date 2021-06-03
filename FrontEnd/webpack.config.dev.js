const path = require("path");
const ReactRefreshWebpackPlugin = require("@pmmmwh/react-refresh-webpack-plugin");
const ReactRefreshTypeScript = require("react-refresh-typescript");

const config = require("./webpack.common");

config.mode("development");

config.module
  .rule("jsts")
  .use("ts")
  .options({
    getCustomTransformers: () => ({
      before: [ReactRefreshTypeScript()],
    }),
  });

config.module
  .rule("css")
  .use("style")
  .before("css")
  .loader("style-loader")
  .end();

config.module
  .rule("sass")
  .use("style")
  .before("css")
  .loader("style-loader")
  .end();

config.devtool("eval-cheap-module-source-map");

config.devServer.port(3000).historyApiFallback(true).hot(true);

config.plugin("react-refresh").use(new ReactRefreshWebpackPlugin());

module.exports = config.toConfig();
