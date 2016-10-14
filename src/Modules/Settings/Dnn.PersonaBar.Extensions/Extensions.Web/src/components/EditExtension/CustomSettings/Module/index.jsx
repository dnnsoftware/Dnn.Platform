import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Localization from "localization";
import styles from "./style.less";

const inputStyle = { width: "100%" };

class Module extends Component {
    render() {
        const {props, state} = this;

        return (
            <GridCell className={styles.editModule}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditAuthSystem_Type.Label")}
                            tooltipMessage={Localization.get("EditAuthSystem_Type.Tooltip")}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditAuthSystem_LoginCtrlSource.Label")}
                            tooltipMessage={Localization.get("EditAuthSystem_LoginCtrlSource.Tooltip")}
                            style={inputStyle} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditAuthSystem_LogoffCtrlSource.Label")}
                            tooltipMessage={Localization.get("EditAuthSystem_LogoffCtrlSource.Tooltip")}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label={Localization.get("EditAuthSystem_SettingsCtrlSource.Label")}
                            tooltipMessage={Localization.get("EditAuthSystem_SettingsCtrlSource.Tooltip")}
                            style={inputStyle}
                            enabled={!props.disabled} />
                        <Switch value={true}
                            label={Localization.get("EditAuthSystem_Enabled.Label")} />
                    </div>
                </GridSystem>
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary">Cancel</Button>
                    <Button type="primary">{props.primaryButtonText}</Button>
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

Module.PropTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string
};

export default Module;