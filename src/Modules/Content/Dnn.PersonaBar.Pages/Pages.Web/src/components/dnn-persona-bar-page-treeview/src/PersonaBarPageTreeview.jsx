import React, {Component} from "react";
import GridCell from "dnn-grid-cell";
import { PropTypes } from "prop-types";
import {DragSource} from 'react-dnd';


import "./styles.less";

import PersonaBarPageIcon from "./_PersonaBarPageIcon";
import PersonaBarSelectionArrow from "./_PersonaBarSelectionArrow";
import PersonaBarExpandCollapseIcon from "./_PersonaBarExpandCollapseIcon";
import PersonaBarDraftPencilIcon from "./_PersonaBarDraftPencilIcon";

export class PersonaBarPageTreeview extends Component {

    constructor(){
        super();
        this.state = {};
    }

    trimName(item){
        let maxLength = 20;
        let {name, tabpath} = item;
        let newLength = tabpath.split(/\//).length*2+1;
        newLength--;
        let depth = ( newLength < 20) ?  newLength: 1;
        return (item.name.length > maxLength-depth) ? `${item.name.slice(0,maxLength-depth)}...` : item.name;
    }

    render_tree(childListItems){
        const {
                draggedItem,
                droppedItem,
                dragOverItem,
                getChildListItems,
                onSelection,
                onDrop,
                onDrag,
                onDragStart,
                onDragOver,
                onDragLeave,
                onDragEnd,
                onMovePage

        } = this.props;

        return (
             <PersonaBarPageTreeview
                draggedItem={draggedItem}
                droppedItem={droppedItem}
                dragOverItem={dragOverItem}
                getChildListItems={getChildListItems}
                listItems={childListItems}
                onSelection={onSelection}
                onDrop={onDrop}
                onDrag={onDrag}
                onDragStart={onDragStart}
                onDragOver={onDragOver}
                onDragLeave={onDragLeave}
                onDragEnd={onDragEnd}
                onMovePage={onMovePage}
             />
        );
    }


    render_parentExpandIcon(item){
        return (
            <PersonaBarExpandCollapseIcon isOpen={item.isOpen} item={item}/>
        );
    }

    render_parentExpandButton(item){
        const {getChildListItems} = this.props;
        return (
            <div className="parent-expand-button" onClick={()=>{getChildListItems(item.id);}}>
             { item.childCount > 0  ? this.render_parentExpandIcon(item) : <div className="parent-expand-icon"></div> }
            </div>
        );
    }

    render_dropZone(direction, item) {
        const {onMovePage, draggedItem, dragOverItem} = this.props;
        if(item.onDragOverState) {
            return (
                <div
                    className={(item.id !== draggedItem.id ) ? "dropZoneArea" : "" }
                    style={(item.id === draggedItem.id) ? {display:"none"} : {}}
                    draggable="false"
                    onDragOver={(e)=>{e.preventDefault();}}
                    onDrop={()=>onMovePage({Action:direction, PageId:draggedItem.id, ParentId:draggedItem.parentId, RelatedPageId: dragOverItem.id})} >

                    +

                </div>
            );
        }
        return;
    }

    render_li() {
        const {listItems, getChildListItems, onSelection, onDrop, onDrag, onDragStart, onDragOver, onDragLeave, onDragEnd, draggedItem} = this.props;

        return listItems.map((item)=>{
            return (
                <li id={`list-item-${item.name}-${item.id}`} >
                    <div className={item.onDragOverState && item.id !== draggedItem.id ? "dropZoneActive" : "dropZoneInactive"} >
                        {this.render_dropZone("before", item)}
                        <span
                            id={`list-item-title-${item.name}-${item.id}`}
                            className={(item.selected) ? "list-item-highlight" : null}
                            draggable="true"
                            onDrop={(e)=>{ onDrop(item); }}
                            onDrag={(e)=> {onDrag(e); }}
                            onDragOver={(e)=>{ onDragOver(e, item); }}
                            onDragStart={(e)=>{ onDragStart(e, item); }}
                            onDragEnd={()=>{onDragEnd(item); }}
                         >
                            {this.render_parentExpandButton(item)}
                            <PersonaBarPageIcon iconType={item.pageType} selected={item.selected}/>
                            <span
                                className={`item-name`}
                                onClick={()=>{ onSelection(item.id); }}
                                >
                                <p>{this.trimName(item)}</p>
                            </span>
                            <div className="draft-pencil">
                                <PersonaBarDraftPencilIcon display={item.hasUnpublishedChanges} />
                            </div>
                            <PersonaBarSelectionArrow item={item} />
                        </span>
                        {this.render_dropZone("after", item)}
                    </div>
                    { item.childListItems && item.isOpen ? this.render_tree(item.childListItems) : null }
                </li>
            );
        });
    }

    render() {

        return (
            <ul>
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
    onDragStart: PropTypes.func.isRequired,
    onDragEnd: PropTypes.func.isRequired,
    onMovePage: PropTypes.func.isRequired,
    listItems: PropTypes.array.isRequired,
    getChildListItems: PropTypes.func.isRequired,
    onSelection: PropTypes.func.isRequired,
    icons: PropTypes.object.isRequired,
    onSelect: PropTypes.func.isRequired
};