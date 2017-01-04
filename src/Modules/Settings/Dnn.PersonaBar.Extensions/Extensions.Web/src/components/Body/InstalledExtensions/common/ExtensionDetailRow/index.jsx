import React, { PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import styles from "./style.less";
import ColumnSizes from "../ExtensionColumnSizes";

import { EditIcon, TrashIcon } from "dnn-svg-icons";

/* eslint-disable react/no-danger */
const ExtensionDetailRow = ({_package, onEdit, onDelete, isHost}) => (
    <GridCell className={styles.extensionDetailRow} columnSize={100} style={{ padding: "20px 0 20px 20px" }}>
        <GridCell columnSize={ColumnSizes[0]} style={{ padding: 0 }}>
            <img src={_package.packageIcon && _package.packageIcon.replace("~", "")} />
        </GridCell>
        <GridCell columnSize={ColumnSizes[1]} style={{ padding: "0 35px" }}>
            <span className="package-name">{_package.friendlyName}</span>
            <p dangerouslySetInnerHTML={{ __html: _package.description }}></p>
        </GridCell>
        <GridCell columnSize={ColumnSizes[2]}>
            <p>{_package.version}</p>
        </GridCell>
        <GridCell columnSize={ColumnSizes[3]}>
            <p>{_package.inUse}</p>
        </GridCell>
        <GridCell columnSize={ColumnSizes[4]}>
            <a href={_package.upgradeUrl} target="_blank">
                <img src={_package.upgradeIndicator} />
            </a>
        </GridCell>
        <GridCell columnSize={ColumnSizes[5]} style={{ paddingRight: 0 }}>
            {(_package.canDelete && isHost) && <div className="extension-action" dangerouslySetInnerHTML={{ __html: TrashIcon }} onClick={onDelete}></div>}
            <div className="extension-action" onClick={onEdit.bind(this, _package.packageId)} dangerouslySetInnerHTML={{ __html: EditIcon }}></div>
        </GridCell>
    </GridCell >
);

ExtensionDetailRow.propTypes = {
    _package: PropTypes.object,
    onEdit: PropTypes.func,
    onDelete: PropTypes.func,
    isHost: PropTypes.bool
};

export default ExtensionDetailRow;