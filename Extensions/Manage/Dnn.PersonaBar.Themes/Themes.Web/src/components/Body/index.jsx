import React, { Component } from "react";
import PropTypes from "prop-types";
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
import Button from "dnn-button";
import "./style.less";
import utils from "utils";

class Body extends Component {
    constructor() {
        super();
        this.state = {
            searchText: "",
            level: 7
        };
    }

    getThemesData(reload) {
        const { props, state } = this;

        if (reload || props.themes.layouts.length === 0) {
            props.dispatch(ThemeActions.getThemes(reload ? 7 : state.level));
        }

        let searchText = state.searchText;
        let level = state.level;
        return props.themes.layouts.filter(l => {
            return !searchText || l.packageName.toLowerCase().indexOf(searchText.toLowerCase()) > -1;
        }).filter(l => {
            return level === 7 || l.level === level;
        });
    }

    onSearch(value) {
        this.setState({ searchText: value });
    }

    onFilterChanged(value) {
        this.setState({ level: value });
    }

    backToThemes() {
        utils.utilities.loadPanel("Dnn.Themes", {});
        this.getThemesData(true);
    }

    installTheme() {
        let event = document.createEvent("Event");

        event.initEvent("installPortalTheme", true, true);

        let settings = {
            isHost: utils.params.settings.isHost,
            installPortalTheme: {},
            referrer: "Dnn.Themes",
            referrerText: Localization.get("BackToThemes"),
            backToReferrerFunc: this.backToThemes.bind(this)            
        };

        event = Object.assign(event, settings);

        utils.utilities.loadPanel("Dnn.Extensions", {
            settings
        });

        document.dispatchEvent(event);
    }

    render() {
        return (
            <GridCell className="themes-body">
                <PersonaBarPageHeader title={Localization.get("Themes")}>
                    {utils.params.settings.isHost &&
                    <Button type="primary" size="large" onClick={this.installTheme.bind(this)}>
                        {Localization.get("InstallTheme")}
                    </Button>
                    }
                </PersonaBarPageHeader>
                <PersonaBarPageBody>
                    <SiteTheme />
                    <MiddleActions onSearch={this.onSearch.bind(this)} onFilterChanged={this.onFilterChanged.bind(this)} />
                    <ThemeList dataSource={this.getThemesData()} />
                </PersonaBarPageBody>
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