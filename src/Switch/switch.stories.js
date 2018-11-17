import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Switch from "./index";

storiesOf("Switch", module).add("with off", () => (
  <Switch
    labelHidden={false}
    onText="On"
    offText="Off"
    label="Example Switch"
    onChange={action("changed")}
    labelPlacement="left"
  />
));

storiesOf("Switch", module).add("with on", () => (
  <Switch
    labelHidden={false}
    onText="On"
    offText="Off"
    label="Example Switch"
    onChange={action("changed")}
    labelPlacement="left"
    value={true}
  />
));
