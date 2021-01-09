import React, { Component } from "react";
import DnnCommon from "@dnnsoftware/dnn-react-common";

export default class CustomIconButton extends DnnCommon.IconButton {
  constructor(props) {
    super(props);
  }

  getIcon() {
    const { props } = this;
    return require("!raw-loader!../../../img/common/" +
      props.type.toLowerCase() +
      ".svg").default;
  }
}
