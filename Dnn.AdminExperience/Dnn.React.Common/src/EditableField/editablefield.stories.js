import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import EditableField from "./index";

storiesOf("EditableField", module).add("with single line input", () => (
  <EditableField
    label="Test"
    value="Content"
    onFocus={action("focus")}
    onEnter={action("enter")}
  />
));

storiesOf("EditableField", module).add("with multi-line input", () => (
  <EditableField
    label="Test"
    value="Content"
    inputType="textArea"
    onFocus={action("focus")}
    onEnter={action("enter")}
  />
));
