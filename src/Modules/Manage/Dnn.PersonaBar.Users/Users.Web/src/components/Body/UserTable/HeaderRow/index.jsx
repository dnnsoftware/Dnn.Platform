import React from "react";
import GridCell from "dnn-grid-cell";
import Localization from "localization";
import ColumnSizes from "../ExtensionColumnSizes";
import "./style.less";

const ExtensionHeader = () => (
    <GridCell columnSize={100} className="header-row">
        <GridCell columnSize={ColumnSizes[0]}>
            <h6> </h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[1]}>
            <h6>{Localization.get("Name") }</h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[2]}>
            <h6>{Localization.get("Email") }</h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[3]}>
            <h6>{Localization.get("Joined") }</h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[4]}>
            <h6>{Localization.get("Status") }</h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[5]}>

        </GridCell>
    </GridCell>
);

export default ExtensionHeader;