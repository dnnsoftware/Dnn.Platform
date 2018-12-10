import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Button from "dnn-button";
import Localization from "localization";
import { connect } from "react-redux";
import styles from "./style.less";

const inputStyle = { width: "100%" };
class Container extends Component {
    render() {
        const {props} = this;
        let { extensionBeingEdited } = props;
        return (
            <GridCell className={styles.editContainer + (props.className ? " " + props.className : "")}>
                <GridCell>
                    <SingleLineInputWithError
                        label={Localization.get("EditContainer_ThemePackageName.Label")}
                        value={extensionBeingEdited.themePackageName.value}
                        onChange={props.onChange.bind(this, "themePackageName")}
                        enabled={!props.disabled}
                        tooltipMessage={Localization.get("EditContainer_ThemePackageName.HelpText")}
                        style={inputStyle} />
                </GridCell>
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

Container.propTypes = {
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
export default connect(mapStateToProps)(Container);
