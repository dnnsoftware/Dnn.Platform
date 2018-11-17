import React from "react";
import { storiesOf } from "@storybook/react";
import Tooltip from "./index";

storiesOf("Tooltip", module).add("with error", () => (
  <Tooltip
    type="error"
    messages={["Tooltip message"]}
    rendered={true}
    tooltipPlace="bottom"
  />
));

storiesOf("Tooltip", module).add("with warning", () => (
  <Tooltip
    type="warning"
    messages={["Tooltip message"]}
    rendered={true}
    tooltipPlace="bottom"
  />
));

storiesOf("Tooltip", module).add("with info", () => (
  <Tooltip
    type="info"
    messages={["Tooltip message"]}
    rendered={true}
    tooltipPlace="bottom"
  />
));
