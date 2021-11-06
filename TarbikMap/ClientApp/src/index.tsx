import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter } from "react-router-dom";
import App from "./App";
import "materialize-css/dist/css/materialize.min.css";
import "material-icons/iconfont/material-icons.css";
import "mapbox-gl/dist/mapbox-gl.css";

const baseUrl = document.getElementsByTagName("base")[0].getAttribute("href")!;
const rootElement = document.getElementById("root");

document.addEventListener("contextmenu", (event) => event.preventDefault());

ReactDOM.render(
  <BrowserRouter basename={baseUrl}>
    <App />
  </BrowserRouter>,
  rootElement
);
