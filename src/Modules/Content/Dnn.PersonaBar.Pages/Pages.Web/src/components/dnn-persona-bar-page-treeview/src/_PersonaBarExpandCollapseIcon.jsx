import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import {CollapseTree, ExpandTree} from "dnn-svg-icons";

export default class PersonaBarExpandCollapseIcon extends Component {

    render(){
        const {isOpen, item} = this.props;
        /*eslint-disable react/no-danger*/
        return(
            <div className="parent-expand-icon">
                {isOpen ? <div id={`collapse-${item.name}`} className="treeview-parent-collapse-button"  dangerouslySetInnerHTML={{ __html: CollapseTree }} /> : <div id={`expand-${item.name}`} className="treeview-parent-expand-button" dangerouslySetInnerHTML={{ __html: ExpandTree }} /> }
            </div>
        );
    }
}

PersonaBarExpandCollapseIcon.propTypes = {
    isOpen: PropTypes.bool.isRequired,
    item: PropTypes.object.isRequired
};