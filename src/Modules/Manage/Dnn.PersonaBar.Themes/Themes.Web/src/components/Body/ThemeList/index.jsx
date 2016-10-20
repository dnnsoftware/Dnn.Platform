import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import { Scrollbars } from "react-custom-scrollbars";

import Theme from "./Theme";

import "./style.less";

class ThemeList extends Component {
    constructor() {
        super();
        this.state = {};
    }
    
    render() {
        const {props, state} = this;

        return (
            <GridCell  className="theme-list">
                {props.dataSource.map((theme, index) => {
                    return <GridCell columnSize="25"><Theme theme={theme} /></GridCell>;
                }) }
            </GridCell>
        );
    }
}

ThemeList.propTypes = {
    dispatch: PropTypes.func.isRequired,
    dataSource: PropTypes.array
};

function mapStateToProps(state) {
    return {
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(ThemeList);