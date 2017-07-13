import React, {Component} from "react";
import GridCell from "dnn-grid-cell";
import { PropTypes } from "prop-types";
import { ArrowRightIcon } from "dnn-svg-icons";
import "./styles.less";

export default class PersonaBarSelectionArrow extends Component {

    render(){
        const {item} = this.props;
        return(
            <div className="selection-arrow">
              { item.selected ? <div dangerouslySetInnerHTML={{__html:ArrowRightIcon}} /> : <div></div> }
            </div>

        );
    }
}

PersonaBarSelectionArrow.propTypes = {
    item: PropTypes.object.isRequired
};