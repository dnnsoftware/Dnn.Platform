import React, { PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import styles from "./style.less";
import ColumnSizes from "../ExtensionColumnSizes";

/* eslint-disable react/no-danger */
const ExtensionDetailRow = ({_package, onDownload}) => (
    <GridCell className={styles.extensionDetailRow} columnSize={100} style={{ padding: "20px" }}>
        <GridCell columnSize={ColumnSizes[0]} style={{ padding: 0 }}>
            <img src={_package.packageIcon.replace("~", "") }/>
        </GridCell>
        <GridCell columnSize={ColumnSizes[1]}>
            <span className="package-name">{_package.name}</span>
        </GridCell>
        <GridCell columnSize={ColumnSizes[2]}>
            <p>{_package.description}</p>
        </GridCell>
        <GridCell columnSize={ColumnSizes[3]}>
            <p>{_package.version}</p>
        </GridCell>
        <GridCell columnSize={ColumnSizes[4]}>
            <Button className="install-download-button" onClick={onDownload.bind(this, _package.name) }>Download</Button>
            <Button className="install-download-button">Install</Button>
        </GridCell>
    </GridCell>
);

ExtensionDetailRow.propTypes = {
    _package: PropTypes.object,
    onDownload: PropTypes.func
};

export default ExtensionDetailRow;