import React from "react";
import { action } from "@storybook/addon-actions";
import Button from "../Button";

export const WithText = () => (
  <Button onClick={action("clicked")}>Hello Button</Button>
);
