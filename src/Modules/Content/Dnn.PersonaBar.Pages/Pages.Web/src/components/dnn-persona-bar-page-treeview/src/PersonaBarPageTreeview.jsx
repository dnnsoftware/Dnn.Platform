import React, {Component} from "react";
import GridCell from "dnn-grid-cell";
import { PropTypes } from "prop-types";
import "./styles.less";

import PersonaBarPageIcon from "./_PersonaBarPageIcon";
import PersonaBarSelectionArrow from "./_PersonaBarSelectionArrow";

export class PersonaBarPageTreeview extends Component {


    render_tree(childListItems){
        const {getChildListItems, onSelection} = this.props;
        return (
             <PersonaBarPageTreeview
             getChildListItems={getChildListItems}
             listItems={childListItems}
             onSelection={onSelection}/>
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
             { item.childCount > 0  ? this.render_parentExpandIcon(item) : <div className="parent-expand-icon"></div>}
            </div>
        );
    }


    render_li() {
        const {listItems, getChildListItems, onSelection} = this.props;
        return listItems.map((item)=>{
            return (
                <li draggable="true">
                    <span>
                        {this.render_parentExpandButton(item)}
                        <PersonaBarPageIcon iconType={1}/>
                        <span className="item-name"  onClick={()=>{ onSelection(item.id); }} >{item.name}</span>
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
    listItems: PropTypes.array.isRequired,
    getChildListItems: PropTypes.func.isRequired,
    onSelection: PropTypes.func.isRequired,
    icons: PropTypes.object.isRequired,
    onSelect: PropTypes.func.isRequired
};