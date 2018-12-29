import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Checkbox from "./index";

storiesOf("Checkbox", module).add("with text", () => (
  <Checkbox value={false} onChange={action("changed")} label="Hello Checkbox" />
));

storiesOf("Checkbox", module).add("already checked", () => (
  <Checkbox value={true} onChange={action("changed")} label="I should be pre-checked" />
));
