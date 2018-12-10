import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "dnn-grid-cell";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import { Scrollbars } from "react-custom-scrollbars";
import Button from "dnn-button";
import Localization from "localization";
import "./style.less";

const inputStyle = { width: "100%" };
const releaseBoxStyle = {
    border: "1px solid #c8c8c8",
    marginBottom: 25,
    height: 250,
    width: "100%"
};
class ReleaseNotes extends Component {
    render() {
        const {props} = this;
        const {value} = props;
        /* eslint-disable react/no-danger */
        return (
            <GridCell className="release-notes extension-form">
                {props.installationMode && <h6>{Localization.get("InstallExtension_ReleaseNotes.Header")}</h6>}
                {props.installationMode && <p>{Localization.get("InstallExtension_ReleaseNotes.HelpText")}</p>}
                {!props.readOnly && <MultiLineInputWithError
                    label={!props.installationMode && Localization.get("InstallExtension_ReleaseNotes.Header")}
                    style={inputStyle}
                    enabled={!props.disabled}
                    value={value}
                    onChange={props.onChange && props.onChange.bind(this, "releaseNotes")} />}
                {props.readOnly &&
                    <Scrollbars style={releaseBoxStyle}>
                        <div className="read-only-release-notes" dangerouslySetInnerHTML={{ __html: value }}></div>
                    </Scrollbars>
                }
                {!props.buttonsAreHidden && <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                    {!props.disabled && <Button type="primary" onClick={props.onSave.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>}
                    {(!props.disabled || props.installationMode) && <Button type="primary" onClick={props.onSave.bind(this)}>{props.primaryButtonText}</Button>}
                </GridCell>}
            </GridCell>
        );
    }
}

ReleaseNotes.propTypes = {
    onCancel: PropTypes.func,
    readOnly: PropTypes.bool,
    onSave: PropTypes.func,
    value: PropTypes.string,
    onChange: PropTypes.func,
    primaryButtonText: PropTypes.string
};

export default ReleaseNotes;
