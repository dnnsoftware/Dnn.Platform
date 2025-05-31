import React from "react";
import { action } from "@storybook/addon-actions";
import InputGroup from "./index";
import SingleLineInputWithError from "../SingleLineInputWithError";

export const WithContent =  () => (
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
);
