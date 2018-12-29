import React, { Component } from "react";
import PropTypes from "prop-types";
import GridCell from "../GridCell";

class GridHeader extends Component {
    constructor(props) {
        super(props);

        this.state = {
        };
    }

    getLocaliztion(key) {
        let localized = this.props.localization[key.replace(" ", "")];
        return localized || key;
    }

    renderHeader() {
        const {props} = this;
        const {roleColumnWidth, columnWidth, actionsWidth} = props;

        return <GridCell className="grid-header">
            <GridCell columnSize={roleColumnWidth}><span title={props.type}>{this.getLocaliztion(props.type)}</span></GridCell>
            {props.definitions.map((def) => {
                return <GridCell key={def.permissionName} columnSize={columnWidth}><span title={def.permissionName}>{this.getLocaliztion(def.permissionName)}</span></GridCell>;
            }) }
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