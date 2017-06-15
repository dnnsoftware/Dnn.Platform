import React, { PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import styles from "./style.less";
import Localization from "localization";
import ColumnSizes from "../ExtensionColumnSizes";

/* eslint-disable react/no-danger */
const ExtensionDetailRow = ({_package, type, onInstall, onDeploy, doingOperation}) => (
    <GridCell className={styles.extensionDetailRow} columnSize={100} style={{ padding: "20px 0 20px 20px" }}>
        <GridCell columnSize={ColumnSizes[0]} style={{ padding: 0 }}>
            <img src={_package.packageIcon.replace("~", "")} alt={_package.friendlyName} />
        </GridCell>
        <GridCell columnSize={ColumnSizes[1]} style={{ padding: "0 35px" }}>
            <span className="package-name">{_package.friendlyName}</span>
            <p dangerouslySetInnerHTML={{ __html: _package.description }}></p>
        </GridCell>
        <GridCell columnSize={ColumnSizes[2]}>
            <p>{_package.version}</p>
        </GridCell>
        <GridCell columnSize={ColumnSizes[3]} style={{ paddingRight: 0 }}>
            {_package.fileName && <form action="/API/PersonaBar/Extensions/DownloadPackage" method="GET" target="_blank">
                <input type="hidden" name="FileName" value={_package.fileName} />
                <input type="hidden" name="PackageType" value={type} />
                <button className="dnn-ui-common-button download-button" type="submit" role="secondary">{Localization.get("Download.Button")}</button>
            </form>}
            {!_package.fileName && <form action="/API/PersonaBar/Extensions/DownloadLanguagePackage" method="GET" target="_blank">
                <input type="hidden" name="CultureCode" value={_package.description} />
                <button className="dnn-ui-common-button download-button" type="submit" role="secondary">{Localization.get("Download.Button")}</button>
            </form>}
            {!_package.fileName &&
                <Button className="install-download-button" disabled={doingOperation} onClick={onDeploy.bind(this, _package)}>{Localization.get("Deploy.Button")}</Button>
            }
            {_package.fileName &&
                <Button className="install-download-button" disabled={doingOperation} onClick={onInstall.bind(this, _package.fileName)}>{Localization.get("Install.Button")}</Button>
            }
        </GridCell>
    </GridCell>
);

ExtensionDetailRow.propTypes = {
    _package: PropTypes.object,
    type: PropTypes.string,
    onDownload: PropTypes.func,
    onInstall: PropTypes.func,
    onDeploy: PropTypes.func,
    doingOperation: PropTypes.bool
};

export default ExtensionDetailRow;