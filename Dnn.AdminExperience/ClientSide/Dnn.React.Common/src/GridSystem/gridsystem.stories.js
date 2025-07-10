import React from "react";
import GridSystem from "./index";
import GridCell from "../GridCell";

export default {
    component: GridSystem,
};

export const WithContent =  () => <GridSystem>
    <GridCell title="Title">
        Content 1
    </GridCell>
    <GridCell>
        Content 2
    </GridCell>
</GridSystem>;