import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import {CollapseTree, ExpandTree} from "dnn-svg-icons";

export default class PersonaBarExpandCollapseIcon extends Component {

    render(){
        const {isOpen} = this.props;

        return(
            <div className="parent-expand-icon">
                {isOpen ? <div dangerouslySetInnerHTML={{ __html: CollapseTree }} /> : <div dangerouslySetInnerHTML={{ __html: ExpandTree }} /> }
            </div>
        );
    }
}

PersonaBarExpandCollapseIcon.propTypes = {
    isOpen: PropTypes.bool.isRequired
};