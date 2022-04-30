import "regenerator-runtime";
import "core-js/modules/es.promise";
import "core-js/modules/es.array.iterator";
import "pepjs";

import React from "react";
import { createRoot } from "react-dom/client";

import "./index.css";

import "./i18n";
import "./palette";
import "./service-worker";

import App from "./App";

const container = document.getElementById("app");
// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
const root = createRoot(container!);
root.render(<App />);
