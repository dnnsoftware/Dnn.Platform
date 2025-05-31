import React from "react";
import { action } from "@storybook/addon-actions";
import CollapsibleRow from "./index";

export const WithText = () => (
  <CollapsibleRow
    label={<div>Click Header To Expand</div>}
    closeOnBlur={false}
    secondaryButtonText="Close"
    buttonsAreHidden={false}
    collapsed={true}
    onChange={action("changed")}
  >
    <p>Test Content</p>
  </CollapsibleRow>
);
