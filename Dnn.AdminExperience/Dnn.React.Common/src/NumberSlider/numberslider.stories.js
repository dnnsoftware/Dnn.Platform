import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import NumberSlider from "./index";

storiesOf("NumberSlider", module).add("with content", () => (
  <NumberSlider
    min={1}
    max={100}
    step={5}
    value={50}
    onChange={action("Change")}
  />
));
