import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell, SingleLineInputWithError, GridSystem, Switch, Button } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import styles from "./style.less";

const inputStyle = { width: "100%" };

class AuthenticationSystem extends Component {
    render() {
        const {props } = this;
        let { extensionBeingEdited } = props;
        return (
            <GridCell className={styles.editAuthenticationSystem + (props.className ? " " + props.className : "")}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditAuthSystem_Type.Label")}
                            value={extensionBeingEdited.authenticationType.value}
                            onChange={props.onChange.bind(this, "authenticationType")}
                            tooltipMessage={Localization.get("EditAuthSystem_Type.Tooltip")}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditAuthSystem_LoginCtrlSource.Label")}
                            value={extensionBeingEdited.loginControlSource.value}
                            onChange={props.onChange.bind(this, "loginControlSource")}
                            tooltipMessage={Localization.get("EditAuthSystem_LoginCtrlSource.Tooltip")}
                            style={inputStyle} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditAuthSystem_LogoffCtrlSource.Label")}
                            value={extensionBeingEdited.logoffControlSource.value}
                            onChange={props.onChange.bind(this, "logoffControlSource")}
                            tooltipMessage={Localization.get("EditAuthSystem_LogoffCtrlSource.Tooltip")}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditAuthSystem_SettingsCtrlSource.Label")}
                            value={extensionBeingEdited.settingsControlSource.value}
                            onChange={props.onChange.bind(this, "settingsControlSource")}
                            tooltipMessage={Localization.get("EditAuthSystem_SettingsCtrlSource.Tooltip")}
                            style={inputStyle}
                            enabled={!props.disabled} />
                        <Switch
                            value={extensionBeingEdited.enabled.value}
                            onChange={props.onChange.bind(this, "enabled")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            label={Localization.get("EditAuthSystem_Enabled.Label")} />
                    </div>
                </GridSystem>
                {!props.actionButtonsDisabled &&
                    <GridCell columnSize={100} className="modal-footer">
                        <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                        <Button type="primary" onClick={props.onSave.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>
                        <Button type="primary" onClick={props.onSave.bind(this)}>{props.primaryButtonText}</Button>
                    </GridCell>
                }
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

AuthenticationSystem.PropTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string,
    extensionBeingEdited: PropTypes.object,
    extensionBeingEditedIndex: PropTypes.number
};
function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex
    };
}
export default connect(mapStateToProps)(AuthenticationSystem);