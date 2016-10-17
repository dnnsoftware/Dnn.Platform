import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import "./style.less";

class CurrentTheme extends Component {
    constructor() {
        super();
        this.state = {};
    }

    
    render() {
        const {props, state} = this;

        return (
            <div className="current-theme">
                <img src={props.theme.SiteLayout.thumbnail} />
            </div>
        );
    }
}

CurrentTheme.propTypes = {
    dispatch: PropTypes.func.isRequired,
    theme: PropTypes.object
};

function mapStateToProps(state) {
    return {
    };
}

export default connect(mapStateToProps)(CurrentTheme);