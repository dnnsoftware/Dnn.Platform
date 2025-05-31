import React from "react";
import { action } from "@storybook/addon-actions";
import Tags from "./index";

export const WithContent = () => (
  <Tags
    tags={["Tag 1", "Tag 2", "Tag 3"]}
    onUpdateTags={action("Tags Update")}
  />
);
