import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";
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
    onSelect(key, option) {
        this.props.onChange(key, option.value);
    }

    render() {
        const {props, state} = this;
        let { extensionBeingEdited } = props;
        return (
            <GridCell className={styles.editExtensionLanguagePack}>
                <GridCell>
                    <DropdownWithError
                        label={Localization.get("EditExtensionLanguagePack_Language.Label")}
                        options={extensionBeingEdited.locales.value.map((locale) => {
                            return { label: locale.name, value: locale.id };
                        })}
                        value={extensionBeingEdited.languageId.value}
                        onSelect={this.onSelect.bind(this, "languageId")}
                        tooltipMessage={Localization.get("EditExtensionLanguagePack_Language.HelpText")}
                        style={inputStyle} />
                    <DropdownWithError
                        label={Localization.get("EditExtensionLanguagePack_Package.Label")}
                        options={extensionBeingEdited.packages.value.map((_package) => {
                            return { label: _package.name, value: _package.id };
                        })}
                        value={extensionBeingEdited.languagePackageId.value}
                        onSelect={this.onSelect.bind(this, "languagePackageId")}
                        tooltipMessage={Localization.get("EditExtensionLanguagePack_Package.HelpText")}
                        style={inputStyle} />
                </GridCell>

                {!props.actionButtonsDisabled &&
                    <GridCell columnSize={100} className="modal-footer">
                        <Button type="secondary" onClick={props.onCancel.bind(this)}>Cancel</Button>
                        <Button type="primary" onClick={props.onSave.bind(this)}>{props.primaryButtonText}</Button>
                    </GridCell>
                }
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

ExtensionLanguagePack.propTypes = {
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
export default connect(mapStateToProps)(ExtensionLanguagePack);