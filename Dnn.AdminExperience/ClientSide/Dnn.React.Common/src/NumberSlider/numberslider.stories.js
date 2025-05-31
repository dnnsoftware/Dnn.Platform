import React from "react";
import { action } from "@storybook/addon-actions";
import NumberSlider from "./index";

export const WithContent =  () => (
  <NumberSlider
    min={1}
    max={100}
    step={5}
    value={50}
    onChange={action("Change")}
  />
);
