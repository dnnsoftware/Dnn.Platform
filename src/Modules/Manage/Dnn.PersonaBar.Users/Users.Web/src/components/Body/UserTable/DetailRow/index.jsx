import React, { PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import styles from "./style.less";
import ColumnSizes from "../ExtensionColumnSizes";

import { SettingsIcon, UserIcon, MoreMenuIcon, ActivityIcon, ShieldIcon } from "dnn-svg-icons";

/* eslint-disable react/no-danger */
const ExtensionDetailRow = ({user}) => (
    <GridCell className={styles.extensionDetailRow} columnSize={100}>
        <GridCell columnSize={ColumnSizes[0]} className="user-avatar">
            <img src={user.AvatarUrl}/>
        </GridCell>
        <GridCell columnSize={ColumnSizes[1]} className="user-names">
            <h6>{user.Displayname}</h6>
            <p>{user.Username}</p>
        </GridCell>
        <GridCell columnSize={ColumnSizes[2]}>
            <p>{user.Email}</p>
        </GridCell>
        <GridCell columnSize={ColumnSizes[3]}>
            <p>{user.Joined}</p>
        </GridCell>
        <GridCell columnSize={ColumnSizes[4]}>
            <p>{user.Status || "Authorized"}</p>
        </GridCell>
        <GridCell columnSize={ColumnSizes[5]}>
            <div className="extension-action" dangerouslySetInnerHTML={{ __html: ActivityIcon }}></div>
            <div className="extension-action" dangerouslySetInnerHTML={{ __html: ShieldIcon }}></div>
            <div className="extension-action" dangerouslySetInnerHTML={{ __html: UserIcon }}></div>
            <div className="extension-action" dangerouslySetInnerHTML={{ __html: SettingsIcon }}></div>
            <div className="extension-action" dangerouslySetInnerHTML={{ __html: MoreMenuIcon }}></div>
        </GridCell>
    </GridCell>
);

ExtensionDetailRow.propTypes = {
    user: PropTypes.object
};

export default ExtensionDetailRow;