import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell, PersonaBarPageHeader, PersonaBarPageBody, Button, Checkbox } from "@dnnsoftware/dnn-react-common";
import { ExtensionActions, VisiblePanelActions } from "actions";
import PackageInformation from "../common/BasicPackageInformation";
import Localization from "localization";
import utilities from "utils";
import styles from "./style.less";
class DeleteExtension extends Component {
    constructor() {
        super();
    }
    onToggleDeleteFiles() {
        this.props.dispatch(ExtensionActions.toggleDeleteFiles());
    }
    onCancel() {
        this.props.dispatch(VisiblePanelActions.selectPanel(0));
        this.props.dispatch(ExtensionActions.setPackageBeingDeleted({}, -1)); //clear
        if (this.props.deleteExtensionFiles) {
            this.onToggleDeleteFiles();
        }
    }
    onDelete() {
        const {props} = this;
        utilities.utilities.confirm(
            Localization.get("DeleteExtension.Warning").replace("{0}", props.extensionBeingDeleted.friendlyName),
            Localization.get("DeleteExtension.Confirm"),
            Localization.get("DeleteExtension.Cancel"), () => {
                props.dispatch(ExtensionActions.deletePackage({
                    id: props.extensionBeingDeleted.packageId,
                    deleteFiles: props.deleteExtensionFiles
                }, props.extensionBeingDeletedIndex, this.onCancel.bind(this)));
            });
    }
    render() {
        const {props} = this;
        const { extensionBeingDeleted } = props;
        const version = extensionBeingDeleted.version.value ? extensionBeingDeleted.version.value.split(".") : [0, 0, 0];
        return (
            <GridCell className={styles.DeleteExtension}>
                <PersonaBarPageHeader title={Localization.get("DeleteExtension.Action").replace("{0}", extensionBeingDeleted.friendlyName)} />
                <PersonaBarPageBody backToLinkProps={{
                    text: Localization.get("BackToExtensions"),
                    onClick: this.onCancel.bind(this)
                }}>
                    <GridCell className="delete-extension-box extension-form">
                        <PackageInformation
                            extensionData={extensionBeingDeleted}
                            installedPackageTypes={props.installedPackageTypes}
                            validationMapped={false}
                            installationMode={true}
                            primaryButtonText={Localization.get("Next")}
                            version={version}
                            disabled={true}
                            isAddMode={false}
                            buttonsAreHidden={true} />
                        <GridCell className="delete-files-box">
                            <Checkbox
                                label={Localization.get("DeleteFiles.Label")}
                                value={props.deleteExtensionFiles}
                                labelPlace="left"
                                onChange={this.onToggleDeleteFiles.bind(this)}
                                tooltipMessage={Localization.get("DeleteFiles.HelpText")} />
                        </GridCell>
                        <GridCell className="modal-footer">
                            <Button type="secondary" onClick={this.onCancel.bind(this)}>{Localization.get("Cancel.Button")}</Button>
                            <Button type="primary" onClick={this.onDelete.bind(this)}>{Localization.get("Delete.Button")}</Button>
                        </GridCell>
                    </GridCell>
                </PersonaBarPageBody>
            </GridCell>
        );
    }
}

DeleteExtension.propTypes = {
    dispatch: PropTypes.func.isRequired,
    extensionBeingDeleted: PropTypes.object,
    deleteExtensionFiles: PropTypes.bool,
    selectedInstalledPackageType: PropTypes.string
};

DeleteExtension.defaultProps = { 
    deleteExtensionFiles: false
};

function mapStateToProps(state) {
    return {
        selectedInstalledPackageType: state.extension.selectedInstalledPackageType,
        extensionBeingDeleted: state.extension.extensionBeingDeleted,
        extensionBeingDeletedIndex: state.extension.extensionBeingDeletedIndex,
        deleteExtensionFiles: state.extension.deleteExtensionFiles,
        installedPackageTypes: state.extension.installedPackageTypes
    };
}

export default connect(mapStateToProps)(DeleteExtension);
