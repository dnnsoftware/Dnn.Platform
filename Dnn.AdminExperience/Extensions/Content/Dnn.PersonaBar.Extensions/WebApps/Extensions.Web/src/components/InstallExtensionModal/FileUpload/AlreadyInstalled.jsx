import React, { Component } from "react";
import PropTypes from "prop-types";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";

export default class AlreadyInstalled extends Component {
    constructor() {
        super();
    }

    render() {
        const { props } = this;
        /* eslint-disable react/no-danger */
        return <div className="already-installed">
            <div className="already-installed-container">
                <div className="upload-file-name">{this.props.fileName || "undefined"}</div>
                <div className="upload-icon" dangerouslySetInnerHTML={{ __html: SvgIcons.ErrorStateIcon }} />
                <h4 dangerouslySetInnerHTML={{ __html: props.repairWarning }}></h4>
                <p className="repair-or-install">
                    <span onClick={props.repairInstall.bind(this)}>[{props.repairInstallText}] </span>
                    {props.orText}
                    <span onClick={props.cancelRepair.bind(this)}> [{props.cancelInstallText}]</span>
                </p>
            </div>
        </div>;
    }
}

AlreadyInstalled.propTypes = {
    errorText: PropTypes.string,
    fileName: PropTypes.string.isRequired,
    repairInstall: PropTypes.func,
    cancelRepair: PropTypes.func,
    repairWarning: PropTypes.string,
    repairInstallText: PropTypes.string,
    cancelInstallText: PropTypes.string,
    orText: PropTypes.string
};

AlreadyInstalled.defaultProps = {
    repairInstallText: "Repair Install",
    cancelInstallText: "Cancel",
    orText: "or",
    repairWarning: "Warning: You have selected to repair the installation of this package." +
    "<br/> This will cause the files in the package to overwrite all files that were previously installed."
};