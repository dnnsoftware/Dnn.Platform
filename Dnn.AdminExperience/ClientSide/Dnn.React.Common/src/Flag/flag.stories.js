import React from "react";
import { action } from "@storybook/addon-actions";
import Flag from "./index";

export const WithContent = () => (
  <Flag title="Test" culture="en-US" onClick={action("Clicked")} />
);
