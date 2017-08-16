import React, {Component} from "react";
import GridCell from "dnn-grid-cell";
import { PropTypes } from "prop-types";
import { ArrowForward, MoreMenuIcon } from "dnn-svg-icons";
import {PersonaBarTreeInContextMenu} from "./_PersonaBarTreeInContextMenu";
import "./styles.less";

export default class PersonaBarSelectionArrow extends Component {

    constructor(){
        super();
        this.state={
            showInContext:false
        };
    }

    toggleInContext(li){
        const {_traverse} = this.props;
        let updateReduxStore, pageList = null;
        _traverse((item, list, updateStore)=>{

            item.showInContextMenu =  item.id == li.id ? !item.showInContextMenu : delete item.showInContextMenu;

             updateReduxStore=updateStore;
             pageList=list;
        });
        updateReduxStore(pageList);
    }

    /* eslint-disable react/no-danger */
    render(){
        const {item} = this.props;
         /*eslint-disable react/no-danger*/
        return(
            <div id={`menu-item-${item.name}`} className="selection-arrow">
                { item.selected ? <div dangerouslySetInnerHTML={{__html:ArrowForward}} /> : <div></div> }
                { item.selected ?  <div className="dots" dangerouslySetInnerHTML={{__html:MoreMenuIcon}} onClick={()=>this.toggleInContext(item)}/> : <div></div> }
                 <PersonaBarTreeInContextMenu item={item} />
            </div>

        );
    }
}

PersonaBarSelectionArrow.propTypes = {
    _traverse: PropTypes.func.isRequired,
    item: PropTypes.object.isRequired
};