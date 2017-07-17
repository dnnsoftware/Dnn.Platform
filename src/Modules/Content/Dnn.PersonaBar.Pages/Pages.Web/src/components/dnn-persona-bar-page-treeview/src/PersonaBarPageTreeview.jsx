import React, {Component} from "react";
import GridCell from "dnn-grid-cell";
import { PropTypes } from "prop-types";
import {DragSource} from 'react-dnd';


import "./styles.less";

import PersonaBarPageIcon from "./_PersonaBarPageIcon";
import PersonaBarSelectionArrow from "./_PersonaBarSelectionArrow";

export class PersonaBarPageTreeview extends Component {

    constructor(){
        super();
        this.state = {};
    }

    render_tree(childListItems){
        const {getChildListItems, onSelection, onDrop, onDrag, onDragStart, onDragEnd} = this.props;
        return (
             <PersonaBarPageTreeview
                getChildListItems={getChildListItems}
                listItems={childListItems}
                onSelection={onSelection}
                onDrop={onDrop}
                onDrag={onDrag}
                onDragStart={onDragStart}
                onDragEnd={onDragEnd}
             />
        );
    }


    render_parentExpandIcon(item){
        return (
            <div className="parent-expand-icon">
                {item.isOpen ? "[-]" : "[+]" }
            </div>
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

    render_li() {
        const {listItems, getChildListItems, onSelection, onDrop, onDrag, onDragStart, onDragEnd} = this.props;
        return listItems.map((item)=>{
            return (
                <li id={`list-item-${item.name}`}>
                    <span
                        draggable="true"
                        onDragOver={(e)=>{ e.preventDefault(); }}
                        onDrop={(e)=>{ onDrop(item); }}
                        onDrag={(e)=> {onDrag(e); }}
                        onDragStart={(e)=>{ onDragStart(e, item); }}
                        onDragEnd={()=>{onDragEnd(); }}
                     >
                        {this.render_parentExpandButton(item)}
                        <PersonaBarPageIcon iconType={1}/>
                        <span className="item-name"  onClick={()=>{ onSelection(item.id); }}>{item.name}</span>
                        <PersonaBarSelectionArrow item={item} />
                    </span>
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
    onDrop: PropTypes.func.isRequired,
    onDrag: PropTypes.func.isRequired,
    onDragStart: PropTypes.func.isRequired,
    onDragEnd: PropTypes.func.isRequired,
    listItems: PropTypes.array.isRequired,
    getChildListItems: PropTypes.func.isRequired,
    onSelection: PropTypes.func.isRequired,
    icons: PropTypes.object.isRequired,
    onSelect: PropTypes.func.isRequired
};