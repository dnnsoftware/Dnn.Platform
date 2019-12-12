import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, SingleLineInputWithError, Switch, Button } from "@dnnsoftware/dnn-react-common";
import { connect } from "react-redux";
import Localization from "localization";
import styles from "./style.less";

const inputStyle = { width: "100%" };
class AuthenticationSystemSettings extends Component {
    render() {
        const {props} = this;

        let { extensionBeingEdited } = props;
        return (
            <GridCell className={styles.editAuthenticationSystem + " extension-form"}>
                <GridCell className="auth-system-site-settings">
                    <SingleLineInputWithError
                        label={Localization.get("AuthSystemSiteSettings_AppId.Label")}
                        value={extensionBeingEdited.appId.value}
                        onChange={props.onChange.bind(this, "appId")}
                        tooltipMessage={Localization.get("AuthSystemSiteSettings_AppId.HelpText")}
                        style={inputStyle} />

                    <SingleLineInputWithError
                        label={Localization.get("AuthSystemSiteSettings_AppSecret.Label")}
                        value={extensionBeingEdited.appSecret.value}
                        onChange={props.onChange.bind(this, "appSecret")}
                        tooltipMessage={Localization.get("AuthSystemSiteSettings_AppSecret.HelpText")}
                        style={inputStyle} />
                    <GridCell columnSize={50} style={{ padding: 0 }}>
                        <Switch
                            label={Localization.get("AuthSystemSiteSettings_AppEnabled.Label")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            value={extensionBeingEdited.appEnabled.value}
                            onChange={props.onChange.bind(this, "appEnabled")}
                            tooltipMessage={Localization.get("AuthSystemSiteSettings_AppEnabled.HelpText")} />
                    </GridCell>
                </GridCell>
                {!props.actionButtonsDisabled &&
                    <GridCell columnSize={100} className="modal-footer">
                        <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                        <Button type="primary" onClick={props.onSave.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>
                        <Button type="primary" onClick={props.onSave.bind(this)}>{Localization.get("Save")}</Button>
                    </GridCell>
                }
            </GridCell>
        );
    }
}

AuthenticationSystemSettings.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onChange: PropTypes.func,
    extensionBeingEdited: PropTypes.object,
    extensionBeingEditedIndex: PropTypes.object
};

function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex
    };
}
export default connect(mapStateToProps)(AuthenticationSystemSettings);