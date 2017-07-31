import React, {Component} from "react";
import { PropTypes } from "prop-types";
import "./styles.less";
import {TreeDraftIcon} from "dnn-svg-icons";

export default class PersonaBarPageIcon extends Component {

    render_icon(hasUnpublishedChanges){
        /*eslint-disable react/no-danger*/

        switch (true) {
            case hasUnpublishedChanges === true:
                return ( <div dangerouslySetInnerHTML={{ __html: TreeDraftIcon }} /> );
            case hasUnpublishedChanges === false:
                 return ( <div /> );
            default:
                return ( <div /> );
        }
    }

    render() {

        return (
            <div className="dnn-persona-bar-draft-icon">
                {this.render_icon(this.props.display)}
            </div>
        );
    }

}

PersonaBarPageIcon.propTypes = {
    display: PropTypes.number.isRequired
};