import React from "react";
import { storiesOf } from "@storybook/react";
import GridSystem from "./index";
import GridCell from "../GridCell";

storiesOf("GridSystem", module).add("with content", () => <GridSystem>
    <GridCell title="Title">
        Content 1
    </GridCell>
    <GridCell>
        Content 2
    </GridCell>
</GridSystem>);