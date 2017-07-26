import React, {Component} from "react";
import GridCell from "dnn-grid-cell";
import { PropTypes } from "prop-types";
import { ArrowForward } from "dnn-svg-icons";
import "./styles.less";

export default class PersonaBarSelectionArrow extends Component {
    /* eslint-disable react/no-danger */
    render(){
        const {item} = this.props;
        return(
            <div className="selection-arrow">
              { item.selected ? <div dangerouslySetInnerHTML={{__html:ArrowForward}} /> : <div></div> }
            </div>

        );
    }
}

PersonaBarSelectionArrow.propTypes = {
    item: PropTypes.object.isRequired
};