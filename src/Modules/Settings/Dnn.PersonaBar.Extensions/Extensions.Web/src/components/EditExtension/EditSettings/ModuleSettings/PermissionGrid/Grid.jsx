import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";

import { ExtensionActions } from "actions";

import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Label from "dnn-label";
import GridCaption from "./GridCaption";
import GridHeader from "./GridHeader";
import GridRow from "./GridRow";
import styles from "./style.less";

class Grid extends Component {
    constructor(props) {
        super(props);

        this.state = {
            definitions: props.permissions.definitions,
            permissions: props.permissions.permissions
        };
    }

    componentWillMount() {
        const {props, state} = this;
    }
    
    renderGrid(){
        const {props, state} = this;

        return  <GridCell className={props.type + "-permissions-grid"}>
                    <GridCaption service={props.service} localization={props.localization} type={props.type} />
                    <GridHeader type={props.type} definitions={state.definitions} />
                </GridCell>;
    }

    render() {
        const {props, state} = this;

        return (
            this.renderGrid()
        );
    }
}

Grid.propTypes = {
    dispatch: PropTypes.func.isRequired,
    permissions: PropTypes.object.isRequired,
    localization: PropTypes.object,
    service: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"])
};

Grid.DefaultProps = {
};

export default Grid;