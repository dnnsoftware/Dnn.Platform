import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { DnnTabs as Tabs, GridCell } from "@dnnsoftware/dnn-react-common";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import CurrentTheme from "./CurrentTheme";
import ThemeFileList from "./ThemeFileList";
import ThemeSettings from "./ThemeSettings";

import utils from "utils";

import "./style.less";

class SiteTheme extends Component {
    constructor() {
        super();
        this.state = {};
    }

    UNSAFE_componentWillMount() {
        const {props} = this;
        props.dispatch(ThemeActions.getCurrentTheme());
    }

    hasData() {
        const {props} = this;

        let source = props.themes.layouts;
        let isHost = utils.params.settings.isHost;
        let hasData = source.filter(l => {
            return isHost || l.level === 1 || l.level === 2;
        }).length > 0;

        return isHost || hasData;
    }

    getTabs() {
        return this.hasData() ? [Localization.get("Layouts"), Localization.get("Containers"), Localization.get("Settings")]
            : [Localization.get("Layouts"), Localization.get("Containers")];
    }

    render() {
        const {props} = this;

        return (
            <GridCell className="site-theme">
                <GridCell columnSize={25}>
                    <CurrentTheme />
                </GridCell>
                <GridCell className="site-theme-tabs" columnSize={75}>
                    <div className="site-theme-title">
                        <label>{Localization.get("SiteTheme")}</label>
                        <span>{props.currentTheme.SiteLayout.themeName}</span>
                    </div>
                    <Tabs tabHeaders={this.getTabs()}
                        type="secondary">
                        <ThemeFileList type={0} />
                        <ThemeFileList type={1} />
                        {this.hasData() && <ThemeSettings />}
                    </Tabs>
                </GridCell>

            </GridCell>
        );
    }
}

SiteTheme.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        themes: state.theme.themes,
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(SiteTheme);