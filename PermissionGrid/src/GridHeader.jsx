import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";

import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import GridSystem from "dnn-grid-system";
import Switch from "dnn-switch";
import Button from "dnn-button";
import Label from "dnn-label";

class GridHeader extends Component {
    constructor(props) {
        super(props);

        this.state = {
        };
    }

    componentWillMount() {
        const {props, state} = this;
    }
    
    renderHeader(){
        const {props, state} = this;
        let actionsWidth = 100 - 20 - props.definitions.length * 10;

        return  <GridCell className="grid-header">
                    <GridCell columnSize="20"><span>{props.type}</span></GridCell>
                    {props.definitions.map(function(def){
                        return <GridCell columnSize="10"><span>{def.permissionName}</span></GridCell>;
                    })}
                    <GridCell columnSize={actionsWidth} />
                </GridCell>;
    }

    render() {
        const {props, state} = this;

        return (
            this.renderHeader()
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