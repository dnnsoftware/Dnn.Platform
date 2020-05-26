import React from "react";
import { storiesOf } from "@storybook/react";
import DropdownWithError from "./index";

storiesOf("DropdownWithError", module).add("with content", () => (
  <DropdownWithError
    error={true}
    errorMessage="Please select an item"
    label="Test"
    options={[
      { label: "Opt 1", value: 1 },
      { label: "Opt 2", value: 2 },
      { label: "Opt 3", value: 3 }
    ]}
  />
));
