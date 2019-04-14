import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridSystem } from "@dnnsoftware/dnn-react-common";
import utils from "utils";
import EditThemeAttributes from "./EditThemeAttributes";
import ParseThemePackage from "./ParseThemePackage";

import "./style.less";
let canEdit = false;

class ThemeSettings extends Component {
    constructor() {
        super();
        this.state = {
            parseType: "0"
        };
        canEdit = utils.params.settings.isHost || utils.params.settings.isAdmin || (utils.params.settings.permissions && utils.params.settings.permissions.EDIT === true);
    }

    renderLeftColumn() {
        return <div className="left-column">
            {canEdit && <EditThemeAttributes />}
        </div>;
    }

    renderRightColumn() {
        let isHost = utils.params.settings.isHost;
        return isHost && <div className="right-column">
            <ParseThemePackage />
        </div>;
    }

    render() {
        return (
            <GridSystem className="theme-settings">
                {[this.renderLeftColumn(), this.renderRightColumn()]}
            </GridSystem>
        );
    }
}

ThemeSettings.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps() {
    return {};
}

export default connect(mapStateToProps)(ThemeSettings);