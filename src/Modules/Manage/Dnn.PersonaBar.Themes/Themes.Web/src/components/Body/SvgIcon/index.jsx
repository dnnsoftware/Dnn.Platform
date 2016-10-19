import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import "./style.less";

const SvgIcons = {
    EmptyThumbnail: <svg version="1.1" x="0px" y="0px" viewBox="0 0 2048 2048">
                        <g>
                            <rect x="255.2" y="254.3" width="480.1" height="523.2"/>
                            <rect x="875.8" y="589.7" width="917" height="187.8"/>
                            <rect x="255.2" y="905.9" width="1537.6" height="883.1"/>
                            <rect x="875.8" y="254.3" width="917" height="187.8"/>
                        </g>
                    </svg>,
    Checkmark: <svg version="1.1" x="0px" y="0px" viewBox="0 0 2048 2048">
                    <g>
                        <polygon points="1524.4,714.3 1417,606.9 868,1155.8 657.8,945.6 550.4,1053 868,1370.7 975.5,1263.3 975.5,1263.3"/>
                    </g>
                </svg>
};

class SvgIcon extends Component {
    constructor() {
        super();
        this.state = {};
    }

    
    render() {
        const {props, state} = this;

        return (
            SvgIcons[props.name]
        );
    }
}

SvgIcon.propTypes = {
    name: PropTypes.string
};

function mapStateToProps(state) {
    return {
    };
}

export default connect(mapStateToProps)(SvgIcon);