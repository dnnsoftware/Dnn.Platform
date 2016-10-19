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
        this.state = {
            level: 3
        };
    }

    componentWillMount(){
        const {props, state} = this;
        
        if(props.themes.layouts.length === 0){
            props.dispatch(ThemeActions.getThemes(state.level));
        }
    }

    
    render() {
        const {props, state} = this;

        return (
            <GridCell  className="theme-list">
                {props.themes.layouts.map((theme, index) => {
                    return <GridCell columnSize="25"><Theme theme={theme} /></GridCell>;
                }) }
            </GridCell>
        );
    }
}

ThemeList.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        themes: state.theme.themes,
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(ThemeList);