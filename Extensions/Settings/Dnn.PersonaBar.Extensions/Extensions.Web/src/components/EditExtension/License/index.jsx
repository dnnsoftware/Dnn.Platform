import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "dnn-grid-cell";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import { Scrollbars } from "react-custom-scrollbars";
import Button from "dnn-button";
import Localization from "localization";
import "./style.less";

const inputStyle = { width: "100%" };
const licenseBoxStyle = {
    border: "1px solid #c8c8c8",
    marginBottom: 25,
    height: 250,
    width: "100%"
};

class License extends Component {
    render() {
        const {props} = this;
        const {value} = props;
        /* eslint-disable react/no-danger */
        return (
            <GridCell className="extension-license extension-form">
                {props.installationMode && <h6>{Localization.get("InstallExtension_License.Header")}</h6>}
                {props.installationMode && <p dangerouslySetInnerHTML={{ __html: Localization.get("InstallExtension_License.HelpText").replace("\\n", "<br/>") }}></p>}
                {!props.readOnly &&
                    <MultiLineInputWithError
                        label={!props.installationMode && Localization.get("InstallExtension_License.Header")}
                        style={inputStyle}
                        value={value}
                        enabled={!props.disabled}
                        onChange={props.onChange && props.onChange.bind(this, "license")} />}
                {props.readOnly &&
                    <Scrollbars style={licenseBoxStyle}>
                        <div className="read-only-license" dangerouslySetInnerHTML={{ __html: value }}></div>
                    </Scrollbars>
                }
                {props.acceptLicenseCheckbox}
                {!props.buttonsAreHidden && <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                    {!props.disabled && <Button type="primary" onClick={props.onSave.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>}
                    {(!props.disabled || props.installationMode) && <Button type="primary" onClick={props.onSave.bind(this)} disabled={props.primaryButtonDisabled}>{props.primaryButtonText}</Button>}
                </GridCell>}
            </GridCell>
        );
    }
}

License.PropTypes = {
    onCancel: PropTypes.func,
    readOnly: PropTypes.bool,
    onSave: PropTypes.func,
    value: PropTypes.string,
    onChange: PropTypes.func,
    primaryButtonText: PropTypes.string,
    acceptLicenseCheckbox: PropTypes.node
};

export default License;