import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import SingleLineInputWithError from "./index";

storiesOf("SingleLineInputWithError", module).add("with content", () => (
  <SingleLineInputWithError
    inputId={"create-term-name"}
    withLabel={true}
    label="Required Term *"
    value="Term Value"
    onChange={action("changed")}
    errorMessage="Error"
  />
));
