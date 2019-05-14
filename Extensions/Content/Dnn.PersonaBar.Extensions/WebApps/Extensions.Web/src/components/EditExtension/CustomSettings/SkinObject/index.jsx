import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, SingleLineInputWithError, GridSystem, Switch, Button } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import { connect } from "react-redux";
import styles from "./style.less";

const inputStyle = { width: "100%" };

class SkinObject extends Component {
    render() {
        const {props} = this;
        let { extensionBeingEdited } = props;

        return (
            <GridCell className={(styles.editSkinObj + (props.className ? " " + props.className : ""))}>
                <GridSystem className="with-right-border top-half">
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditSkinObject_ControlKey.Label")}
                            value={extensionBeingEdited.controlKey.value}
                            onChange={props.onChange.bind(this, "controlKey")}
                            tooltipMessage={Localization.get("EditSkinObject_ControlKey.HelpText")}
                            style={inputStyle}
                            enabled={!props.disabled} />
                    </div>
                    <div>
                        <SingleLineInputWithError
                            label={Localization.get("EditSkinObject_ControlSrc.Label")}
                            value={extensionBeingEdited.controlSrc.value}
                            onChange={props.onChange.bind(this, "controlSrc")}
                            tooltipMessage={Localization.get("EditSkinObject_ControlSrc.HelpText")}
                            style={inputStyle}
                            enabled={!props.disabled} />
                        <Switch
                            label={Localization.get("EditSkinObject_SupportsPartialRender.Label")}
                            onText={Localization.get("SwitchOn")}
                            offText={Localization.get("SwitchOff")}
                            value={extensionBeingEdited.supportsPartialRendering.value}
                            onChange={props.onChange.bind(this, "supportsPartialRendering")}
                            tooltipMessage={Localization.get("EditSkinObject_SupportsPartialRender.HelpText")}
                            readOnly={props.disabled} />
                    </div>
                </GridSystem>
                {!props.actionButtonsDisabled &&
                    <GridCell columnSize={100} className="modal-footer">
                        <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                        {!props.disabled && <Button type="primary" onClick={props.onSave.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>}
                        {!props.disabled && <Button type="primary" onClick={props.onSave.bind(this)}>{props.primaryButtonText}</Button>}
                    </GridCell>
                }
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

SkinObject.propTypes = {
    onCancel: PropTypes.func,
    onUpdateExtension: PropTypes.func,
    onChange: PropTypes.func,
    disabled: PropTypes.func,
    primaryButtonText: PropTypes.string
};

function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex
    };
}
export default connect(mapStateToProps)(SkinObject);
