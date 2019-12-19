import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import InputGroup from "./index";
import SingleLineInputWithError from "../SingleLineInputWithError";

storiesOf("InputGroup", module).add("with content", () => (
  <InputGroup>
    <SingleLineInputWithError
      inputId={"create-term-name"}
      withLabel={true}
      label="Required Term *"
      value="Term Value"
      onChange={action("changed")}
      errorMessage="Error"
    />
  </InputGroup>
));
