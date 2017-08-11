import React, {Component} from "react";
import GridCell from "dnn-grid-cell";
import TextOverflowWrapperNew from "dnn-text-overflow-wrapper-new"
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
        let maxLength = 15;
        let {name, tabpath} = item;
        let newLength = tabpath.split(/\//).length*2+1;
        newLength--;
        let depth = ( newLength < maxLength) ?  newLength: 1;
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
        const onDragOver = (item, direction) => {
            const elm = document.getElementById(`dropzone-${item.name}-${item.id}-${direction}`);
            (direction==="before") ? elm.style.borderBottom="2px solid blue" : elm.style.borderTop="2px solid blue";
        };

        if(item.onDragOverState) {
            return (
                <div
                    id={`dropzone-${item.name}-${item.id}-${direction}`}
                    className={(item.id !== draggedItem.id ) ? "dropZoneArea" : "" }
                    style={(item.id === draggedItem.id) ? {display:"none"} : {}}
                    draggable="false"
                    onDragOver={(e)=>onDragOver(item)}
                    onDrop={()=>onMovePage({Action:direction, PageId:draggedItem.id, ParentId:draggedItem.parentId, RelatedPageId: dragOverItem.id})} >

                    +

                </div>
            );
        }
        return;
    }

    render_li() {
        const {listItems, getChildListItems, onSelection, onDrop, onDrag, onDragStart, onDragOver, onDragLeave, onDragEnd, draggedItem} = this.props;
        const hotspotStyles = {
            position:"relative",
            zIndex: 10000,
            wordWrap: "break-word",
            textOverflow: "wrap",
            width:"90%",
            height: "20px",
            marginTop:"-20px",
            backgroundColor:"transparent"

        };

        return listItems.map((item)=>{
            const name = this.trimName(item);
            const showTooltip = /\.\.\./.test(name);

            return (
                <li id={`list-item-${item.name}-${item.id}`} >
                    <div className={item.onDragOverState && item.id !== draggedItem.id ? "dropZoneActive" : "dropZoneInactive"} >
                        {this.render_dropZone("before", item)}
                        <div
                            id={`list-item-title-${item.name}-${item.id}`}
                            className={(item.selected) ? "list-item-highlight" : null}
                            style={{height:"28px"}}
                            draggable="true"
                            onDrop={(e)=>{ onDrop(item, e); }}
                            onDrag={(e)=> {onDrag(e); }}
                            onDragOver={(e)=>{ onDragOver(e, item); }}
                            onDragStart={(e)=>{ onDragStart(e, item); }}
                            onDragEnd={()=>{onDragEnd(item); }}
                         >

                            <PersonaBarPageIcon iconType={item.pageType} selected={item.selected}/>
                            <span
                                className={`item-name`}
                                onClick={()=>{ onSelection(item.id); }}
                                >
                                <p>{name}</p>
                            </span>
                            <div className="draft-pencil">
                                <PersonaBarDraftPencilIcon display={item.hasUnpublishedChanges} />
                            </div>
                            {showTooltip ? <TextOverflowWrapperNew text={item.name} hotspotStyles={hotspotStyles} />: null }
                        </div>
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