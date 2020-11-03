const path = require("path");
const webpack = require("webpack");
const ReactRefreshWebpackPlugin = require("@pmmmwh/react-refresh-webpack-plugin");

const config = require("./webpack.common");

config.mode("development");

config.target('web'); // Remove this after https://github.com/webpack/webpack-dev-server/issues/2758 is fixed.

config.module
  .rule("ts")
  .use("babel")
  .options({
    plugins: ["react-refresh/babel"],
  });

config.module
  .rule("js")
  .use("babel")
  .options({
    plugins: ["react-refresh/babel"],
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

config.resolve.set("fallback", {
  querystring: require.resolve("querystring-es3"),
});

config.devServer
  .contentBase(path.resolve(__dirname, "public/"))
  .port(3000)
  .historyApiFallback(true)
  .hot(true);

config.plugin("react-refresh").use(new ReactRefreshWebpackPlugin());

module.exports = config.toConfig();
