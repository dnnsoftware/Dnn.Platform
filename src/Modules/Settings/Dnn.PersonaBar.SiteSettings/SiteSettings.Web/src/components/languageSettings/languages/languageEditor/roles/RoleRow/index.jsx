import React, { Component, PropTypes } from "react";
import ReactDOM from "react-dom";
import "./style.less";
import Collapse from "react-collapse";
import GridCell from "dnn-grid-cell";
import Checkbox from "dnn-checkbox";

class RoleRow extends Component {
    constructor() {
        super();

        this.state = {
            selected: false
        };
    }

    componentWillMount() {
        const {props} = this;

        this.setState({
            selected: Object.assign({}, props.selected)
        });
    }

    onSelectionChange(id, event) {
        let {state, props} = this;

        this.setState({
            selected: event
        });
    }

    render() {
        const {props, state} = this;

        return (
            <div className={"collapsible-component1"}>
                <div className={"collapsible-header1 "} >
                    <GridCell title={props.roleName} columnSize={60} >
                        {props.roleName}</GridCell>
                    <GridCell columnSize={40} >
                        <Checkbox
                            style={{ float: "left" }}
                            value={state.selected}
                            onChange={this.onSelectionChange.bind(this, props.roleId)} /></GridCell>
                </div>
            </div>
        );

    }
}

RoleRow.propTypes = {
    roleName: PropTypes.string,
    roleId: PropTypes.number,
    selected: PropTypes.bool,
    id: PropTypes.string
};

export default (RoleRow);