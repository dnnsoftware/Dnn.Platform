import React from "react";
import { action } from "@storybook/addon-actions";
import SingleLineInputWithError from "./index";

export const WithContent = () => (
  <SingleLineInputWithError
    inputId={"create-term-name"}
    withLabel={true}
    label="Required Term *"
    value="Term Value"
    onChange={action("changed")}
    errorMessage="Error"
  />
);
