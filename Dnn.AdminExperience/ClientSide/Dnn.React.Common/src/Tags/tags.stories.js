import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import Tags from "./index";

storiesOf("Tags", module).add("with content", () => (
  <Tags
    tags={["Tag 1", "Tag 2", "Tag 3"]}
    onUpdateTags={action("Tags Update")}
  />
));
