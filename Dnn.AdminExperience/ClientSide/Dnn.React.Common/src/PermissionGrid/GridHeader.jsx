import React, { Component } from "react";
import PropTypes from "prop-types";

class GridHeader extends Component {
    constructor(props) {
        super(props);

        this.state = {
        };
    }

    getLocalization(key) {
        let localized = this.props.localization[key.replace(" ", "")];
        return localized || key;
    }

    getName(type) {
        const key = `Permission${type.replace(" ", "")}`;
        const localized = this.props.localization[key];
        return localized || type;
    }

    getDescription(type) {
        const key = `Permission${type.replace(" ", "")}Description`;
        const localized = this.props.localization[key];
        return localized || type;
    }

    renderHeader() {
        const {props} = this;
        const {roleColumnWidth, actionsWidth} = props;

        return <tr className="grid-header">
            <th columnSize={roleColumnWidth}><span title={props.type}>{this.getLocalization(props.type)}</span></th>
            {props.definitions.map((def) => {
                return <th key={def.permissionName}>
                    <span title={this.getDescription(def.permissionName)}>
                        {this.getName(def.permissionName)}
                    </span>
                </th>;
            }) }
            <th columnSize={actionsWidth} />
        </tr>;
    }

    render() {
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