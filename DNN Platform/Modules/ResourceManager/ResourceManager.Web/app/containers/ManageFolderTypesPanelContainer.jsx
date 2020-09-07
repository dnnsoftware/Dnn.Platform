import React from "react";
import { PropTypes } from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import localizeService from "../services/localizeService";
import manageFolderTypesPanelActions from "../actions/manageFolderTypesPanelActions";
import { SvgIcons }  from "@dnnsoftware/dnn-react-common";
import addFolderPanelActions from "../actions/addFolderPanelActions";
import dialogModalActions from "../actions/dialogModalActions";

class ManageFolderTypesPanelContainer extends React.Component {
    constructor(props) {
        super(props);
        this.props.loadFolderMappings();
    }

    handleConfirmRemoveFolderType(e, folderTypeMappingId) {
        e.preventDefault();
        let dialogHeader = localizeService.getString("RemoveFolderTypeDialogHeader");
        let dialogBody = localizeService.getString("RemoveFolderTypeDialogBody");
        this.props.openDialog(dialogHeader, dialogBody, () => this.handleRemoveFolderType(folderTypeMappingId), this.props.closeDialog);
    }
    
    handleRemoveFolderType(folderTypeMappingId) {
        this.props.removeFolderType(folderTypeMappingId);
        this.props.loadFolderMappings();
    }

    render() {
        const {
            expanded,
            hidePanel,
            folderTypes,
            isAdmin,
        } = this.props;
        
        return isAdmin && expanded ? (
                <div className={"top-panel manage-folder-types" + (expanded ? " rm-expanded" : "")} >
                    <h3>{localizeService.getString("FolderTypeDefinitions")}</h3>
                    <table className="folder-types">
                        <thead>
                            <tr>
                                <th>&nbsp;</th>
                                <th>{localizeService.getString("Name")}</th>
                                <th>{localizeService.getString("FolderProvider")}</th>
                                <th>&nbsp;</th>
                            </tr>
                        </thead>
                        <tbody>
                            {folderTypes && folderTypes.map(folderType => {
                                return (
                                    <tr key={folderType.FolderMappingID}>
                                        <td>
                                            {!folderType.IsDefault &&
                                                <button 
                                                    title={localizeService.getString("EditFolderType")}
                                                    dangerouslySetInnerHTML={{ __html: SvgIcons.EditIcon }}
                                                />
                                            }
                                        </td>
                                        <td>{folderType.MappingName}</td>
                                        <td>{folderType.FolderProviderType}</td>
                                        <td>
                                            {!folderType.IsDefault &&
                                                <button 
                                                    title={localizeService.getString("RemoveFolderType")}
                                                    dangerouslySetInnerHTML={{ __html: SvgIcons.TrashIcon }}
                                                    onClick={e => this.handleConfirmRemoveFolderType(e, folderType.FolderMappingID)}
                                                />
                                            }
                                        </td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>

                    <div className="cancel">
                    <a className="rm-button secondary" onClick={hidePanel}>{localizeService.getString("Cancel")}</a>
                    </div>

                    <div className="save">
                        <a className="rm-button primary">{localizeService.getString("AddFolderType")}</a>
                    </div>

                    <div className="rm-clear"></div>
                </div>
        ) : null;
    }
}

ManageFolderTypesPanelContainer.propTypes = {
    expanded: PropTypes.bool,
    hidePanel: PropTypes.func,
    folderTypes: PropTypes.array,
    isAdmin: PropTypes.bool,
    loadFolderMappings: PropTypes.func,
    removeFolderType: PropTypes.func,
    openDialog: PropTypes.func,
    closeDialog: PropTypes.func,
};

function mapStateToProps(state) {
    const folderPanelState = state.folderPanel;
    const manageFolderTypesPanelState = state.manageFolderTypesPanel;
    const addFolderPanelState = state.addFolderPanel;
    const module = state.module;

    return {
        expanded: manageFolderTypesPanelState.expanded,
        isAdmin: module.isAdmin,
        folderTypes: addFolderPanelState.folderMappings,
        folderPanelState
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            hidePanel: manageFolderTypesPanelActions.hidePanel,
            loadFolderMappings: addFolderPanelActions.loadFolderMappings,
            removeFolderType: manageFolderTypesPanelActions.removeFolderType,
            openDialog: dialogModalActions.open,
            closeDialog: dialogModalActions.close
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(ManageFolderTypesPanelContainer);