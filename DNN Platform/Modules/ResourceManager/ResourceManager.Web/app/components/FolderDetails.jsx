import React from "react";
import PropTypes from "prop-types";
import localizeService from "../services/localizeService";
import itemsService from "../services/itemsService";
import Permissions from "./Permissions"; 

const FolderDetails = ({folder, handlers, validationErrors}) => (
    <div className="item-details">
        <div className="details-info">
            <div className="details-icon folder" style={folder.iconStyle}></div>
            <div className="details-field">
                <span className="details-label">{localizeService.getString("Created")}:</span>
                <span>{folder.createdOnDate}</span>
            </div>
            <div className="vertical-separator"></div>
            <div className="details-field">
                <span className="details-label">{localizeService.getString("LastModified")}:</span>
                <span>{folder.lastModifiedOnDate}</span>
            </div>
            <div className="line-break"></div>
            <div className="details-field">
                <span className="details-label">{localizeService.getString("FolderType")}:</span>
                <span>{folder.type}</span>
            </div>
            <div className="line-break"></div>
            <div className="details-field rm-url">
                <span className="details-label">{localizeService.getString("URL")}:</span>
                <a target="_blank" rel="noopener noreferrer" href={itemsService.getFolderUrl(folder.folderId)}>{itemsService.getFolderUrl(folder.folderId)}</a>
            </div>
        </div>
        <div className="separator"></div>
        <div>
            <Permissions
                folderBeingEdited={folder}
                updateFolderBeingEdited={(permissions) => handlers.changePermissions(permissions)}
            ></Permissions>
        </div>
        <div className="separator"></div>
        <div>
            <div className="rm-field">
                <label htmlFor="fileName" className="formRequired">{localizeService.getString("Name")}</label>
                <input id="fileName" onChange={handlers.changeName} type="text" className="required" maxLength="246" value={folder.folderName} />
                <label className="rm-error">{validationErrors.fileName}</label>
            </div>
        </div>
        <div className="cancel">
            <a className="rm-button secondary normal" onClick={handlers.cancelEditItem}>{localizeService.getString("Cancel")}</a>
        </div>
        <div className="save">
            <a className="rm-button primary normal" onClick={handlers.onSave}>{localizeService.getString("Save")}</a>
        </div>
        <div className="rm-clear"></div>
    </div>
);

FolderDetails.propTypes = {
    folder: PropTypes.object.isRequired,
    handlers: PropTypes.object.isRequired,
    validationErrors: PropTypes.object.isRequired
};

export default FolderDetails;
