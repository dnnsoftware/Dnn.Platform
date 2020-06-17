import React from "react";
import PropTypes from "prop-types";
import itemService from "../services/itemsService";
import localizeService from "../services/localizeService";

const FileDetails = ({file, handlers, validationErrors}) => (
    <div className="item-details" >
        <div className="details-info">
            <div className="details-icon" style={file.iconStyle}></div>
            <div className="details-field">
                <span className="details-label">{localizeService.getString("Created")}:</span>
                <span>{file.createdOnDate}</span>
            </div>
            <div className="vertical-separator"></div>
            <div className="details-field">
                <span className="details-label">{localizeService.getString("Size")}:</span>
                <span>{file.size}</span>
            </div>
            <div className="line-break"></div>
            <div className="details-field">
                <span className="details-label">{localizeService.getString("LastModified")}:</span>
                <span>{file.lastModifiedOnDate}</span>
            </div>
            <div className="line-break"></div>
            <div className="details-field rm-url">
                <span className="details-label">{localizeService.getString("URL")}:</span>
                <a target="_blank" rel="noopener noreferrer" href={itemService.getItemFullUrl(file.url)}>{itemService.getItemFullUrl(file.url)}</a>
            </div>
            
        </div>
        <div className="separator"></div>
        <div>
            <div className="rm-field">
                <label htmlFor="fileName" className="formRequired">{localizeService.getString("Name")}</label>
                <input id="fileName" onChange={handlers.changeName} type="text" className="required" maxLength="246" value={file.fileName} />
                <label className="rm-error">{validationErrors.changeName}</label>
            </div>
            <div className="rm-field right">
                <label htmlFor="description">{localizeService.getString("Description")}</label>
                <textarea id="description" onChange={handlers.changeDescription} maxLength="500" value={file.description} ></textarea>
            </div>
            <div className="rm-field">
                <label htmlFor="title">{localizeService.getString("Title")}</label>
                <input id="title" onChange={handlers.changeTitle} type="text" maxLength="256" value={file.title} />
            </div>
        </div>
        <div className="rm-clear"></div>
        <div className="cancel">
            <a className="rm-button secondary normal" onClick={handlers.cancelEditItem}>{localizeService.getString("Cancel")}</a>
        </div>
        <div className="save">
            <a className="rm-button primary normal" onClick={handlers.onSave}>{localizeService.getString("Save")}</a>
        </div>
        <div className="rm-clear"></div>
    </div>
);

FileDetails.propTypes = {
    file: PropTypes.object.isRequired,
    handlers: PropTypes.object.isRequired,
    validationErrors: PropTypes.object.isRequired
};

export default FileDetails;
