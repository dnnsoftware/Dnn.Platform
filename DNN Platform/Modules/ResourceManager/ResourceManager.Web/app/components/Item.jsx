import React from "react";
import PropTypes from "prop-types";

function truncateString(content, length) {
    if (typeof length === "undefined") {
        length = 13;
    }

    return content.length <= length ? content : content.substr(0, length) + "...";
} 

const Item = ({item, iconUrl, handlers, isHighlighted, isDetailed}) => (
    <div className="rm-card-container">
        <div className={"item rm-card" + (isHighlighted ? " highlight" : "") + (isDetailed ? " selected" : "") + (item.isFolder ? " rm-folder" : "")} 
            onClick={ handlers.onClick || null }>
            <div className="text-card text-card--noellipsis">
                <div>
                    <p title={item.itemName}>{ truncateString(item.itemName) }</p>
                </div>
            </div>
            <div className="image-center">
                <div className={"rm-circular" + (item.isFolder ? " rm-folder" : "")} id={"thumbnail-" + item.itemId} style={{backgroundImage: "url(" + iconUrl + ")"}}></div>
            </div>
            <div className="overlay-disabled"></div>
            
            <div className="rm-actions">
                { handlers.onEdit ? <div className="rm-edit" onClick={handlers.onEdit}></div> : "" }
                { handlers.onCopyToClipboard ? <div className="rm-link" onClick={handlers.onCopyToClipboard}></div> : "" }
                { handlers.onDownload ? <div className="rm-download" onClick={handlers.onDownload}></div> : "" }
                { handlers.onDelete ? <div className="rm-delete" onClick={handlers.onDelete}></div> : "" }
            </div>
        </div>
        {isDetailed && <div className="details-selector"><div></div></div>}
    </div>
);

Item.propTypes = {
    item: PropTypes.object.isRequired,
    iconUrl: PropTypes.string.isRequired,
    handlers: PropTypes.object.isRequired,
    isHighlighted: PropTypes.bool,
    isDetailed: PropTypes.bool
};

export default Item;
