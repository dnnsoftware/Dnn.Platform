import React, { Component } from "react";
import { PropTypes } from "prop-types";
import utils from "utils";
import "./styles.less";
import { SingleLineInput } from "@dnnsoftware/dnn-react-common";
import PersonaBarPageIcon from "./_PersonaBarPageIcon";
import PersonaBarDraftPencilIcon from "./_PersonaBarDraftPencilIcon";

export class PersonaBarPageTreeview extends Component {

    constructor() {
        super();
        this.state = {};
    }

    trimName(item) {
        if (item.name){
            let tabPath = "";
            if (item.tabpath) {
                tabPath = item.tabpath;
            }
            let maxLength = 18;
            let { name } = item;
            let newLength = tabPath.split(/\//).length * 2 + 1;
            newLength--;
            let depth = (newLength < maxLength) ? newLength : 1;
            return (name.length > maxLength - depth) ? `${item.name.slice(0, maxLength - depth)}...` : item.name;
        } else {
            return item.name;
        }
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

    renderDropZone(direction, item) {
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
        }
    }

    _checkItemDisabled(item) {
        if (item.includeInMenu) {
            return false;
        }
        if (item.status !== "Visible") {
            return true;
        }  
        return false;
    }

    getClassName(item) {
        const DRAG_HOVER = item.onDragOverState === true;
        const SELECTED = item.selected === true;
        const DISABLED = this._checkItemDisabled(item);
        
        if (DRAG_HOVER || SELECTED) {
            return "list-item-highlight list-item-dragover";
        } else if (DISABLED) {
            return "list-item-disabled";
        }

        return "";
    }

    renderLi() {
        const {
            listItems,
            onSelection,
            onNoPermissionSelection,
            onDrop,
            onDrag,
            onDragStart,
            onDragEnter,
            onDragOver,
            onDragEnd,
            draggedItem,
            Localization, 
            selectedPageDirty } = this.props;

        let index = 0;
        let total = listItems.length;
        return listItems.map((item) => {
            const name = this.trimName(item);
            const canManagePage = (e, item, fn) => {
                const whenCanManage = () => e ? fn(e, item) : fn(item);
                const whenCannotManage = () => this.props.setEmptyPageMessage(Localization.get("NoPermissionManagePage"));
                item.canManagePage ? whenCanManage() : whenCannotManage();
            };

            const onDragLeave = e => e.target.classList.remove("list-item-dragover");
            index++;

            const style = item.canManagePage ? { "whiteSpace": "nowrap", height: "28px", lineHeight: "35px", marginLeft: "15px" } : { height: "28px", marginLeft: "15px" };
            const itemNameHidden = item.status === "Hidden" ? "item-name-hidden" : "";
            return (
                <li key={"list-item-key-" + index} id={`list-item-${item.name}-${item.id}`}>
                    <div className={item.onDragOverState && item.id !== draggedItem.id ? "dropZoneActive" : "dropZoneInactive"} >
                        {this.renderDropZone("before", item)}
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
                            onClick={() => { item.canManagePage ? onSelection(item) : onNoPermissionSelection(item); }}
                        >
                        </div>

                        <div style={style} className={this.getClassName(item)}>
                            <PersonaBarPageIcon iconType={item.pageType} selected={item.selected || false} />
                            <span
                                className={`item-name ${itemNameHidden}`}
                                onClick={() => item.canManagePage ? onSelection(item) : onNoPermissionSelection(item)}>
                                { (item.tabId === 0) || (item.selected && selectedPageDirty) ? 
                                    (
                                        <SingleLineInput 
                                            style={{ marginBottom: "0px", width:"80%", height:"100%"}}
                                            defaultValue={name}/>
                                    ):
                                    name
                                }
                            </span>
                            <div className="draft-pencil">
                                <PersonaBarDraftPencilIcon display={item.hasUnpublishedChanges} />
                            </div>
                        </div>
                        {((item.childListItems && !item.isOpen) || !item.childListItems) && index === total && this.renderDropZone("after", item)}
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
                {this.renderLi()}
            </ul>
        );
    }

}

PersonaBarPageTreeview.propTypes = {
    draggedItem: PropTypes.object,
    droppedItem: PropTypes.object,
    dragOverItem: PropTypes.object,
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
    icons: PropTypes.object,
    onSelect: PropTypes.func,
    setEmptyPageMessage: PropTypes.func.isRequired,
    Localization: PropTypes.object.isRequired,
    parentItem: PropTypes.object,
    selectedPageDirty: PropTypes.bool
};