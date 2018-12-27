import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import PersonaBarExpandCollapseIcon from "./_PersonaBarExpandCollapseIcon";

export class PersonaBarPageTreeParentExpand extends Component {

    constructor(){
        super();
        this.state = {};
    }


    render_tree(childListItems){
        const {getChildListItems} = this.props;
        return (
             <PersonaBarPageTreeParentExpand listItems={childListItems} getChildListItems={getChildListItems} />
        );
    }

    render_parentExpandIcon(item){
        return (
            <PersonaBarExpandCollapseIcon isOpen={item.isOpen} item={item}/>
        );
    }

    render_parentExpandButton(item){
        const {getChildListItems}  = this.props;
        return (
            <div className="parent-expand-button" onClick={()=>{getChildListItems(item.id);}} >
             { item.childCount > 0  ? this.render_parentExpandIcon(item) : <div className="parent-expand-icon"></div> }
            </div>
        );
    }


    render_li() {
        const {listItems} = this.props;
        if(listItems && listItems.length > 0)
        return listItems.map((item)=>{
            return (
                <li key={item.id} className="list-item-menu">
                    <div
                        className={(item.selected) ? "list-item-highlight" : null}
                        style={{height:"28px"}}>

                        <div className="draft-pencil">
                           {this.render_parentExpandButton(item)}
                        </div>
                    </div>
                    { item.childListItems && item.isOpen ? this.render_tree(item.childListItems) : null }
                </li>
            );
        });
    }

    render() {

        return (
            <ul className="dnn-persona-bar-treeview-menu dnn-persona-bar-treeview-ul parent-expand">
                {this.render_li()}
            </ul>
        );
    }

}

PersonaBarPageTreeParentExpand.propTypes = {
    getChildListItems: PropTypes.func.isRequired,
    listItems: PropTypes.array.isRequired
};