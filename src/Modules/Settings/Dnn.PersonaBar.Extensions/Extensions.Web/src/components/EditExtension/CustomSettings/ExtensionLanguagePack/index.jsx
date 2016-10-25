import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import DropdownWithError from "dnn-dropdown-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Localization from "localization";
import styles from "./style.less";

const inputStyle = { width: "100%" };
function formatVersionNumber(n) {
    return n > 9 ? "" + n : "0" + n;
}
class ExtensionLanguagePack extends Component {
    render() {
        const {props, state} = this;

        return (
            <GridCell className={styles.editExtensionLanguagePack}>
                <GridCell>
                    <DropdownWithError
                        label={Localization.get("EditExtensionLanguagePack_Language.Label")}
                        options={
                            [
                                { label: "", value: "" }
                            ]
                        }
                        tooltipMessage={Localization.get("EditExtensionLanguagePack_Language.HelpText")}
                        style={inputStyle} />
                    <DropdownWithError
                        label={Localization.get("EditExtensionLanguagePack_Package.Label")}
                        options={
                            [
                                { label: "", value: "" }
                            ]
                        }
                        tooltipMessage={Localization.get("EditExtensionLanguagePack_Package.HelpText")}
                        style={inputStyle} />
                </GridCell>
                
                {!props.actionButtonsDisabled &&
                <GridCell columnSize={100} className="modal-footer">
                    <Button type="secondary" onClick={props.onCancel.bind(this)}>Cancel</Button>
                    <Button type="primary">{props.primaryButtonText}</Button>
                </GridCell>
                }
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

ExtensionLanguagePack.PropTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string
};

export default ExtensionLanguagePack;