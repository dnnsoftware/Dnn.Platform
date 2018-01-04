import React, { Component } from "react";
import TextOverflowWrapperNew from "dnn-text-overflow-wrapper-new";
import { PropTypes } from "prop-types";
import utils from "utils";

import "./styles.less";

import PersonaBarPageIcon from "./_PersonaBarPageIcon";
import PersonaBarExpandCollapseIcon from "./_PersonaBarExpandCollapseIcon";
import PersonaBarDraftPencilIcon from "./_PersonaBarDraftPencilIcon";

// const SAFARI_PADDING = 3;
// const OTHER_PADDING = 1;

export class PersonaBarPageTreeview extends Component {

    

    constructor() {
        super();
        this.state = {};
        this.ua = navigator.userAgent.toLowerCase(); 
        console.log("this.ua",this.ua);
        console.log("isSafari",(this.ua.indexOf('safari') != -1));
        // this.BROWSER_PADDING = SAFARI_PADDING;
    }

    trimName(item) {
        let maxLength = 18;
        let { name, tabpath } = item;
        let newLength = tabpath.split(/\//).length * 2 + this.BROWSER_PADDING;
        newLength--;
        let depth = (newLength < maxLength) ? newLength : 1;
        return (item.name.length > maxLength - depth) ? `${item.name.slice(0, maxLength - depth)}...` : item.name;

    }

    render_tree(item, childListItems) {
        const {
            draggedItem,
            droppedItem,
            dragOverItem,
            getChildListItems,
            onSelection,
            onNoPermissionSelection,
            onDrop,
            onDrag,
            onDragStart,
            onDragEnter,
            onDragOver,
            onDragLeave,
            onDragEnd,
            onMovePage,
            setEmptyPageMessage,
            Localization
        } = this.props;

        return (
            <PersonaBarPageTreeview
                draggedItem={draggedItem}
                droppedItem={droppedItem}
                dragOverItem={dragOverItem}
                getChildListItems={getChildListItems}
                listItems={childListItems}
                onSelection={onSelection}
                onNoPermissionSelection={onNoPermissionSelection}
                onDrop={onDrop}
                onDrag={onDrag}
                onDragStart={onDragStart}
                onDragEnter={onDragEnter}
                onDragOver={onDragOver}
                onDragLeave={onDragLeave}
                onDragEnd={onDragEnd}
                onMovePage={onMovePage}
                setEmptyPageMessage={setEmptyPageMessage}
                Localization={Localization}
                parentItem={item}
            />
        );
    }


    render_parentExpandIcon(item) {
        return (
            <PersonaBarExpandCollapseIcon isOpen={item.isOpen} item={item} />
        );
    }

    render_parentExpandButton(item) {
        const { getChildListItems } = this.props;
        return (
            <div className="parent-expand-button" onClick={() => { getChildListItems(item.id); }}>
                {item.childCount > 0 ? this.render_parentExpandIcon(item) : <div className="parent-expand-icon"></div>}
            </div>
        );
    }

    render_dropZone(direction, item) {
        const { onMovePage, onDragEnd, draggedItem, dragOverItem, parentItem } = this.props;
        const onDragOver = (e, item, direction) => {
            e.preventDefault();
            const elm = document.getElementById(`dropzone-${item.name}-${item.id}-${direction}`);
            (direction === "before") ? elm.classList.add("list-item-border-bottom") : elm.classList.add("list-item-border-top");
        };

        const onDragLeave = (item, direction) => {
            const elm = document.getElementById(`dropzone-${item.name}-${item.id}-${direction}`);
            (direction === "before") ? elm.classList.remove("list-item-border-bottom") : elm.classList.remove("list-item-border-top");
        };
        if (!utils.getIsSuperUser() && (parentItem === undefined || parentItem && !parentItem.canManagePage)) {
            return;
        }

        if (item.onDragOverState) {
            return (
                <div
                    id={`dropzone-${item.name}-${item.id}-${direction}`}
                    className={(item.id !== draggedItem.id) ? "dropZoneArea" : ""}
                    style={(item.id === draggedItem.id) ? { display: "none" } : {}}
                    draggable="false"
                    onDragOver={(e) => onDragOver(e, item, direction)}
                    onDragLeave={() => onDragLeave(item, direction)}
                    onDragEnd={() => onDragEnd(item, direction)}
                    onDrop={(e) => onMovePage({ e: e, Action: direction, PageId: draggedItem.id, ParentId: draggedItem.parentId, RelatedPageId: dragOverItem.id, RelatedPageParentId: dragOverItem.parentId })} >
                </div>
            );
            // (draggedItem.parentId === dragOverItem.parentId ? draggedItem.parentId : dragOverItem.parentId)
        }
        return;
    }

    render_li() {
        const {
            listItems,
            getChildListItems,
            onSelection,
            onNoPermissionSelection,
            onDrop,
            onDrag,
            onDragStart,
            onDragEnter,
            onDragOver,
            onDragEnd,
            draggedItem,
            Localization } = this.props;

        const hotspotStyles = {
            position: "relative",
            zIndex: 9997,
            wordWrap: "break-word",
            textOverflow: "wrap",
            width: "100%",
            height: "20px",
            marginTop: "-20px"
            //backgroundColor: "rgb(0,1,2,.5)"
        };

        let index = 0;
        let total = listItems.length;
        return listItems.map((item) => {
            const name = this.trimName(item);
            const shouldShowTooltip = /\.\.\./.test(name);
            const canManagePage = (e, item, fn) => {
                const message = Localization.get("NoPermissionManagePage");
                const left = () => {
                    e ? fn(e, item) : fn(item);
                };
                const right = () => {
                    this.props.setEmptyPageMessage(message);
                };
                item.canManagePage ? left() : right();
            };

            let activate = false;
            const onDragLeave = (e, item) => {
                e.target.classList.remove("list-item-dragover");
            };
            index++;

            const style = item.canManagePage ? { height: "28px", lineHeight: "35px", marginLeft: "15px" } : { height: "28px", marginLeft: "15px" };
            return (
                <li id={`list-item-${item.name}-${item.id}`}>
                    <div className={item.onDragOverState && item.id !== draggedItem.id ? "dropZoneActive" : "dropZoneInactive"} >
                        {this.render_dropZone("before", item)}
                        <div
                            style={style}
                            id={`list-item-title-${item.name}-${item.id}`}
                            className="dragged-proxy"
                            draggable={item.canManagePage ? "true" : "false"}
                            onDrop={(e) => { canManagePage(e, item, onDrop); }}
                            onDrag={(e) => { canManagePage(e, item, onDrag); }}
                            onDragOver={(e) => { canManagePage(e, item, onDragOver); }}
                            onDragEnter={(e) => canManagePage(e, item, onDragEnter)}
                            onDragStart={(e) => { canManagePage(e, item, onDragStart); }}
                            onDragLeave={(e) => canManagePage(e, item, onDragLeave)}
                            onDragEnd={(e) => { canManagePage(e, item, onDragEnd); }}
                            onClick={(e) => { item.canManagePage ? onSelection(item) : onNoPermissionSelection(item); }}
                        >
                        </div>

                        <div style={style} className={(item.selected || item.onDragOverState) ? "list-item-highlight list-item-dragover" : null}>
                            <PersonaBarPageIcon iconType={item.pageType} selected={item.selected} />
                            <span
                                className={`item-name`}
                                onClick={(e) => { item.canManagePage ? onSelection(item) : onNoPermissionSelection(item); }}>
                                {name}
                            </span>
                            <div className="draft-pencil">
                                <PersonaBarDraftPencilIcon display={item.hasUnpublishedChanges} />
                            </div>
                            {false && <TextOverflowWrapperNew text={item.name} hotspotStyles={hotspotStyles} />}
                        </div>
                        {((item.childListItems && !item.isOpen) || !item.childListItems) && index === total && this.render_dropZone("after", item)}
                    </div>
                    {item.childListItems && item.isOpen ? this.render_tree(item, item.childListItems) : null}
                </li>
            );
        });
    }

    render() {
        const { listItems } = this.props;
        return (
            <ul style={!listItems.length ? { padding: "0px", height: "0px" } : null}>
                {this.render_li()}
            </ul>
        );
    }

}

PersonaBarPageTreeview.propTypes = {
    draggedItem: PropTypes.object.isRequired,
    droppedItem: PropTypes.object.isRequired,
    dragOverItem: PropTypes.object.isRequired,
    onDrop: PropTypes.func.isRequired,
    onDrag: PropTypes.func.isRequired,
    onDragOver: PropTypes.func.isRequired,
    onDragLeave: PropTypes.func.isRequired,
    onDragEnter: PropTypes.func.isRequired,
    onDragStart: PropTypes.func.isRequired,
    onDragEnd: PropTypes.func.isRequired,
    onMovePage: PropTypes.func.isRequired,
    listItems: PropTypes.array.isRequired,
    getChildListItems: PropTypes.func.isRequired,
    onSelection: PropTypes.func.isRequired,
    onNoPermissionSelection: PropTypes.func.isRequired,
    icons: PropTypes.object.isRequired,
    onSelect: PropTypes.func.isRequired,
    setEmptyPageMessage: PropTypes.func.isRequired,
    Localization: PropTypes.func.isRequired,
    parentItem: PropTypes.object
};