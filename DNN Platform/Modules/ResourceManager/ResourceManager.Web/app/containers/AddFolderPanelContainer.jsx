import React from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import localizeService from "../services/localizeService";
import addFolderPanelActions from "../actions/addFolderPanelActions";
import { DropDown } from "@dnnsoftware/dnn-react-common";

class AddFolderPanelContainer extends React.Component {
    constructor(props) {
        super(props);
        this.props.loadFolderMappings();
        this.nameInput = React.createRef();
    }

    componentDidUpdate(previousProps) {
        if (!previousProps.expanded && this.props.expanded) {
            this.nameInput.focus();
        }
    }

    getParentFolderName() {
        const {currentFolderId, currentFolderName, homeFolderId} = this.props.folderPanelState;

        if (currentFolderId === homeFolderId) {
            return "HOME";
        }

        return currentFolderName;
    }

    render() {
        const { expanded, hasPermission, hidePanel, folderTypes, formData, validationErrors, folderPanelState, 
            changeName, changeFolderType, addFolder, setValidationErrors } = this.props;

        let localizedFolderTypes = folderTypes ? folderTypes.map(obj =>
        {
            let label = localizeService.getString(obj.MappingName);
            return { label, value: obj.FolderMappingID };
        }) : undefined;

        let nameValidationError = validationErrors ? validationErrors.name : "";
        let typeValidationError = validationErrors ? validationErrors.type : "";

        function onSelectChange(option) {
            changeFolderType.call(this, option.value);
        }

        function formSubmit() {
            if (!validateForm()) {
                return;
            }

            let data = {
                formData,
                folderPanelState
            };

            addFolder(data);
        }

        function validateForm() {
            let result = true;
            let validationErrors = {};

            if (!formData.name) {
                validationErrors.name = localizeService.getString("FolderNameRequiredMessage");
                result = false;
            }

            if (!formData.folderType) {
                validationErrors.type = localizeService.getString("FolderTypeRequiredMessage");
                result = false;
            }

            setValidationErrors(validationErrors); 

            return result;
        }

        return hasPermission ? (
            <div className={"top-panel add-folder" + (expanded ? " rm-expanded" : "")} >
                <div className="folder-adding">
                    <div>
                        <label className="folder-parent-label">{ localizeService.getString("FolderParent") + ":" }</label>
                        <span>{ "/" + this.getParentFolderName() }</span>
                    </div>
                    <div className="rm-field">
                        <label className="formRequired">{ localizeService.getString("Name") }</label>
                        <input id="folderName" className="required" placeholder={localizeService.getString("FolderNamePlaceholder")} type="text"
                            value={formData.name} onChange={changeName} ref={this.nameInput} />
                        <label className="rm-error">{nameValidationError}</label>

                    </div>
                    {
                    folderPanelState.folder.folderPath === "" && 
                    <div className="rm-field right">
                        <label className="formRequired">{ localizeService.getString("Type") }</label>
                        <DropDown className="rm-dropdown" 
                            options={localizedFolderTypes} 
                            value={formData.folderType} 
                            onSelect={onSelectChange} />
                        <label className="rm-error">{typeValidationError}</label>                        
                    </div>
                    }
                    
                </div>

                <div className="cancel">
                    <a className="rm-button secondary" onClick={hidePanel}>{localizeService.getString("Cancel")}</a>
                </div>

                <div className="save">
                    <a className="rm-button primary" onClick={formSubmit}>{localizeService.getString("Save")}</a>
                </div>

                <div className="rm-clear"></div>
            </div>
        ) : null;
    }
}

AddFolderPanelContainer.propTypes = {
    expanded: PropTypes.bool,
    hasPermission: PropTypes.bool,
    hidePanel: PropTypes.func,
    loadFolderMappings: PropTypes.func,
    folderTypes: PropTypes.array,
    validationErrors: PropTypes.object,
    formData: PropTypes.object,
    folderPanelState: PropTypes.object,
    changeName: PropTypes.func,
    changeFolderType: PropTypes.func,
    addFolder: PropTypes.func,
    setValidationErrors: PropTypes.func
};

function mapStateToProps(state) {
    const folderPanelState = state.folderPanel;
    const addFolderPanelState = state.addFolderPanel;

    return {
        expanded: addFolderPanelState.expanded,
        hasPermission: folderPanelState.hasAddFoldersPermission,
        folderTypes: addFolderPanelState.folderMappings,
        formData: addFolderPanelState.formData || {},
        validationErrors: addFolderPanelState.validationErrors,
        folderPanelState
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            hidePanel: addFolderPanelActions.hidePanel,
            loadFolderMappings: addFolderPanelActions.loadFolderMappings,
            changeName: addFolderPanelActions.changeName,
            changeFolderType: addFolderPanelActions.changeFolderType,
            addFolder: addFolderPanelActions.addFolder,
            setValidationErrors: addFolderPanelActions.setValidationErrors
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(AddFolderPanelContainer);