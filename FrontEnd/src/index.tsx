import "regenerator-runtime";
import "core-js/modules/es.promise";
import "core-js/modules/es.array.iterator";
import "pepjs";

import "bootstrap/dist/css/bootstrap-reboot.css";
import "bootstrap/dist/css/bootstrap-grid.css";
import "bootstrap-icons/font/bootstrap-icons.css";

import React from "react";
import ReactDOM from "react-dom";

import "./index.css";

import "./i18n";
import "./palette";
import "./service-worker";

import App from "./App";

import { userService } from "./services/user";

void userService.checkLoginState();

ReactDOM.render(<App />, document.getElementById("app"));
