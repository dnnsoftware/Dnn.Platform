import React from "react";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import ColumnSizes from "../ExtensionColumnSizes";
import styles from "./style.less";
const ExtensionHeader = () => (
    <GridCell className={styles.extensionHeader} columnSize={100} style={{ padding: "20px 20px 5px" }}>
        <GridCell columnSize={ColumnSizes[0]}>
        </GridCell>
        <GridCell columnSize={ColumnSizes[1]} style={{padding: "0 35px"}}>
            <h6>{Localization.get("Extension.Header") }</h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[2]}>
            <h6>{Localization.get("Version.Header") }</h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[3]}>
            <h6>{Localization.get("InUse.Header") }</h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[4]}>
            <h6>{Localization.get("Upgrade.Header") }</h6>
        </GridCell>
        <GridCell columnSize={ColumnSizes[5]}>

        </GridCell>
    </GridCell>
);

export default ExtensionHeader;