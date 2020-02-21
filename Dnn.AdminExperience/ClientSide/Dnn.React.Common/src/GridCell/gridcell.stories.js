import React from "react";
import { storiesOf } from "@storybook/react";
import { action } from "@storybook/addon-actions";
import GridCell from "./index";

storiesOf("GridCell", module).add("with content", () => <GridCell><div>Cell Content</div></GridCell>);