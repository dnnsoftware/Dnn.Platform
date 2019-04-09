import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell, DropdownWithError, Button } from "@dnnsoftware/dnn-react-common";
import Localization from "localization";
import styles from "./style.less";

const inputStyle = { width: "100%" };
class CoreLanguagePack extends Component {

    onSelect(option) {
        this.props.onChange("languageId", option.value);
    }

    render() {
        const {props} = this;
        let { extensionBeingEdited } = props;
        return (
            <GridCell className={styles.editCoreLanguagePack + (props.className ? " " + props.className : "")}>
                <GridCell>
                    <DropdownWithError
                        label={Localization.get("EditExtensionLanguagePack_Language.Label")}
                        options={extensionBeingEdited.locales.value.map((locale) => {
                            return { label: locale.name, value: locale.id };
                        })}
                        enabled={!props.disabled}
                        value={extensionBeingEdited.languageId.value}
                        onSelect={this.onSelect.bind(this)}
                        tooltipMessage={Localization.get("EditExtensionLanguagePack_Language.HelpText")}
                        style={inputStyle} />
                </GridCell>
                {!props.actionButtonsDisabled &&
                    <GridCell columnSize={100} className="modal-footer">
                        <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                        {!props.disabled && <Button type="primary" onClick={props.onSave.bind(this, true)}>{Localization.get("EditModule_SaveAndClose.Button")}</Button>}
                        {!props.disabled && <Button type="primary">{props.primaryButtonText}</Button>}
                    </GridCell>
                }
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

CoreLanguagePack.propTypes = {
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
export default connect(mapStateToProps)(CoreLanguagePack);
