import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import "./style.less";

class EmptyThumbnail extends Component {
    constructor() {
        super();
        this.state = {};
    }

    
    render() {
        const {props, state} = this;

        return (
            <svg version="1.1" x="0px" y="0px" viewBox="0 0 2048 2048">
            <g>
                <rect x="255.2" y="254.3" width="480.1" height="523.2"/>
                <rect x="875.8" y="589.7" width="917" height="187.8"/>
                <rect x="255.2" y="905.9" width="1537.6" height="883.1"/>
                <rect x="875.8" y="254.3" width="917" height="187.8"/>
            </g>
        </svg>
        );
    }
}

EmptyThumbnail.propTypes = {
};

function mapStateToProps(state) {
    return {
    };
}

export default connect(mapStateToProps)(EmptyThumbnail);