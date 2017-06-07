import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import PersonaBarPageBody from "dnn-persona-bar-page-body";
import Localization from "localization";
import PersonaBarPageHeader from "dnn-persona-bar-page-header";
import GridCell from "dnn-grid-cell";
import SiteTheme from "./SiteTheme";
import MiddleActions from "./MiddleActions";
import ThemeList from "./ThemeList";

import "./style.less";

class Body extends Component {
    constructor() {
        super();
        this.state = {
            searchText: "",
            level: 7
        };
    }

    getThemesData() {
        const {props, state} = this;

        if (props.themes.layouts.length === 0) {
            props.dispatch(ThemeActions.getThemes(state.level));
        }

        let searchText = state.searchText;
        return props.themes.layouts.filter(l => {
            return !searchText || l.packageName.toLowerCase().indexOf(searchText) > -1;
        });
    }

    onSearch(value) {
        this.setState({ searchText: value });
    }

    render() {
        return (
            <GridCell className="themes-body">
                <PersonaBarPageHeader title={Localization.get("Themes")}>
                </PersonaBarPageHeader>
                <PersonaBarPageBody>
                    <SiteTheme />
                    <MiddleActions onSearch={this.onSearch.bind(this)} />
                    <ThemeList dataSource={this.getThemesData()} />
                </PersonaBarPageBody >

            </GridCell>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        themes: state.theme.themes
    };
}

export default connect(mapStateToProps)(Body);