import React from "react";
import { action } from "@storybook/addon-actions";
import BackToLink from "./index";

export const WithText = () => (
  <BackToLink onClick={action("clicked")} text="Hello BackToLink" />
);
