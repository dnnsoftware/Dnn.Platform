import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";

import { ExtensionActions } from "actions";

import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Label from "dnn-label";
import styles from "./style.less";

class GridHeader extends Component {
    constructor(props) {
        super(props);

        this.state = {
        };
    }

    componentWillMount() {
        const {props, state} = this;
    }
    
    renderGrid(){
        const {props, state} = this;

        return  <GridCell className="grid-header">
                    hello world
                </GridCell>;
    }

    render() {
        const {props, state} = this;

        return (
            this.renderGrid()
        );
    }
}

GridHeader.propTypes = {
    localization: PropTypes.object,
    definitions: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"])
};

GridHeader.DefaultProps = {
};

export default GridHeader;