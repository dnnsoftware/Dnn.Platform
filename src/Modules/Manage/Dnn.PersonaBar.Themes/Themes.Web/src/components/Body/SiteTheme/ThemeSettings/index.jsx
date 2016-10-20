import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import RadioButtons from "dnn-radio-buttons";
import utils from "utils";

import EditThemeAttributes from "./EditThemeAttributes";
import ParseThemePackage from "./ParseThemePackage";

import "./style.less";

class ThemeSettings extends Component {
    constructor() {
        super();
        this.state = {
            parseType: "0"
        };
    }

    renderLeftColumn(){
        return <div className="left-column">
            <EditThemeAttributes />
        </div>;
    }

    renderRightColumn(){
        let isHost = utils.params.settings.isHost;
        return isHost && <div className="right-column">
            <ParseThemePackage />
        </div>;
    }
    
    render() {
        const {props, state} = this;

        return (
            <GridSystem className="theme-settings" children={[this.renderLeftColumn(), this.renderRightColumn()]}>
            </GridSystem>
        );
    }
}

ThemeSettings.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
    };
}

export default connect(mapStateToProps)(ThemeSettings);