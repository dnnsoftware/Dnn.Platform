import React from "react";
import GridCell from "dnn-grid-cell";
import Localization from "localization";
import ColumnSizes from "../ExtensionColumnSizes";
import styles from "./style.less";

const ExtensionHeader = () => (
    <GridCell className={styles.extensionHeader} columnSize={100} style={{ padding: "20px" }}>
        <GridCell columnSize={ColumnSizes[0]}>
            <h6> </h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[1]}>
            <h6>{Localization.get("Name.Header") }</h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[2]}>
            <h6>{Localization.get("Description.Header") }</h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[3]}>
            <h6>{Localization.get("Version.Header") }</h6>

        </GridCell>
        <GridCell columnSize={ColumnSizes[4]}>

        </GridCell>
    </GridCell>
);

export default ExtensionHeader;