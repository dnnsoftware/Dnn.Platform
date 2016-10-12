import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Localization from "localization";
import styles from "./style.less";

const inputStyle = { width: "100%" };
function formatVersionNumber(n) {
    return n > 9 ? "" + n : "0" + n;
}
class AuthenticationSystem extends Component {
    render() {
        const {props, state} = this;

        return (
            <GridCell className={styles.editAuthenticationSystem}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label="Name"
                            tooltipMessage={Localization.get("EditExtension_PackageName.HelpText")}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label="Friendly Name"
                            tooltipMessage={Localization.get("EditExtension_PackageFriendlyName.HelpText")}
                            style={inputStyle} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label="Name"
                            tooltipMessage={Localization.get("EditExtension_PackageName.HelpText")}
                            style={inputStyle} />
                        <SingleLineInputWithError
                            label="Friendly Name"
                            tooltipMessage={Localization.get("EditExtension_PackageFriendlyName.HelpText")}
                            style={inputStyle}
                            enabled={!props.disabled} />
                        <Switch value={true} label="Add a Test Page:" />
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

AuthenticationSystem.PropTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string
};

export default AuthenticationSystem;