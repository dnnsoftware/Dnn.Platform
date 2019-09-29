import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Flag from "./index";

storiesOf("Flag", module).add("with content", () => (
  <Flag title="Test" culture="en-US" onClick={action("Clicked")} />
));
