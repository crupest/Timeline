import "regenerator-runtime";
import "core-js/modules/es.promise";
import "core-js/modules/es.array.iterator";
import "pepjs";

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
