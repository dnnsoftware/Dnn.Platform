import React, { } from "react";
import PropTypes from "prop-types";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import styles from "./style.less";
import Localization from "localization";
import ColumnSizes from "../ExtensionColumnSizes";
import TextOverflowWrapper from "dnn-text-overflow-wrapper";
import util from "utils";

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
            {_package.fileName && <form action={util.siteRoot + "API/PersonaBar/Extensions/DownloadPackage"} method="GET" target="_blank">
                <input type="hidden" name="FileName" value={_package.fileName} />
                <input type="hidden" name="PackageType" value={type} />
                <button className="dnn-ui-common-button download-button" type="submit" role="secondary">
                    <TextOverflowWrapper text={Localization.get("Download.Button") } maxWidth={60}/>
                </button>
            </form>}
            {!_package.fileName && <form action={util.siteRoot + "API/PersonaBar/Extensions/DownloadLanguagePackage"} method="GET" target="_blank">
                <input type="hidden" name="CultureCode" value={_package.description} />
                <button className="dnn-ui-common-button download-button" type="submit" role="secondary">
                    <TextOverflowWrapper text={Localization.get("Download.Button") } maxWidth={60}/>
                </button>
            </form>}
            {!_package.fileName &&
                <Button className="install-download-button" disabled={doingOperation} onClick={onDeploy.bind(this, _package)}>
                    <TextOverflowWrapper text={Localization.get("Deploy.Button") } maxWidth={60}/>
                </Button>
            }
            {_package.fileName &&
                <Button className="install-download-button" disabled={doingOperation} onClick={onInstall.bind(this, _package.fileName)}>
                    <TextOverflowWrapper text={Localization.get("Install.Button") } maxWidth={60}/>
                </Button>
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