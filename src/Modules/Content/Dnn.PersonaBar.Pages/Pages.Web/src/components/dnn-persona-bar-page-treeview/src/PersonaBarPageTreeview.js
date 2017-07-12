import React, {Component} from "react";
import GridCell from "dnn-grid-cell";
import { PropTypes } from "prop-types";

export class PersonaBarPageTreeview extends Component {


    render_tree(childListItems){
        const {getChildListItems} = this.props;
        return (
             <PersonaBarPageTreeview getChildListItems={getChildListItems} listItems={childListItems} />
        );
    }

    toggleParentState(item) {

    }

    render_li() {
        const {listItems, getChildListItems} = this.props;
        return listItems.map((item)=>{
            return (
                <li>
                    <span onClick={()=>getChildListItems(item.id)}>{item.name}</span>
                    {item.childListItems && item.isOpen ? this.render_tree(item.childListItems) : null}
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
    icons: PropTypes.object.isRequired,
    onSelect: PropTypes.func.isRequired
};