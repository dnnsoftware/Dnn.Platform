import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import utils from "utils";

import "./style.less";

class RestoreTheme extends Component {
    constructor() {
        super();
        this.state = {};
    }

    restoreTheme() {
        const {props} = this;

        utils.utilities.confirm(Localization.get("RestoreThemeConfirm"), Localization.get("Confirm"), Localization.get("Cancel"), function () {
            props.dispatch(ThemeActions.restoreTheme());
        });
    }

    render() {
        return (
            <GridCell className="restore-theme" columnSize="50">
                <Button onClick={this.restoreTheme.bind(this)}>{Localization.get("RestoreTheme")}</Button>
            </GridCell>
        );
    }
}

RestoreTheme.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps() {
    return {};
}

export default connect(mapStateToProps)(RestoreTheme);