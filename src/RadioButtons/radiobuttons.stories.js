import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import RadioButtons from "./index";

storiesOf("RadioButtons", module).add("with content", () => (
  <RadioButtons
    options={[
      { value: "1", label: "Value 1" },
      { value: "2", label: "Value 2" }
    ]}
    onChange={action("Change")}
    value={"1"}
  />
));
