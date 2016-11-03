import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Tabs from "dnn-tabs";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import Label from "dnn-label";

import CurrentTheme from "./CurrentTheme";
import ThemeFileList from "./ThemeFileList";
import ThemeSettings from "./ThemeSettings";

import "./style.less";

class SiteTheme extends Component {
    constructor() {
        super();
        this.state = {};
    }

    componentWillMount() {
        const {props} = this;
        props.dispatch(ThemeActions.getCurrentTheme());
    }


    render() {
        const {props, state} = this;

        return (
            <GridCell className="site-theme">
                <GridCell columnSize={168} type="px">
                    <CurrentTheme />
                </GridCell>
                <GridCell className="site-theme-tabs" columnSize={560} type="px">
                    <div className="site-theme-title">
                        <label>{Localization.get("SiteTheme")}</label>
                        <span>{props.currentTheme.SiteLayout.themeName}</span>
                    </div>
                    <Tabs tabHeaders={[Localization.get("Layouts"), Localization.get("Containers"), Localization.get("Settings")]}
                            type="secondary">
                        <ThemeFileList type={0} />
                        <ThemeFileList type={1} />
                        <ThemeSettings />
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
        currentTheme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(SiteTheme);