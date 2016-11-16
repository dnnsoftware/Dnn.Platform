import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
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
        const {props} = this;
        const {roleColumnWidth, columnWidth, actionsWidth} = props;

        return  <GridCell className="grid-header">
                    <GridCell columnSize={roleColumnWidth}><span>{props.type}</span></GridCell>
                    {props.definitions.map(function(def){
                        return <GridCell columnSize={columnWidth}><span>{def.permissionName}</span></GridCell>;
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
    type: PropTypes.oneOf(["role", "user"]),
    roleColumnWidth: PropTypes.number.isRequired,
    columnWidth: PropTypes.number.isRequired,
    actionsWidth: PropTypes.number.isRequired
};

GridHeader.DefaultProps = {
};

export default GridHeader;