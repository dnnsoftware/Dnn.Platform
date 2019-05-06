import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import BackToLink from "./index";

storiesOf("BackToLink", module).add("with text", () => (
  <BackToLink onClick={action("clicked")} text="Hello BackToLink" />
));
