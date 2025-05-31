import React from "react";
import { action } from "@storybook/addon-actions";
import EditableField from "./index";

export const WithSingleLineInput =  () => (
  <EditableField
    label="Test"
    value="Content"
    onFocus={action("focus")}
    onEnter={action("enter")}
  />
);

export const WithMultiLineInput =  () => (
  <EditableField
    label="Test"
    value="Content"
    inputType="textArea"
    onFocus={action("focus")}
    onEnter={action("enter")}
  />
);
