import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import { Scrollbars } from "react-custom-scrollbars";

import ThemeFile from "./ThemeFile";

import "./style.less";

class ThemeFileList extends Component {
    constructor() {
        super();
        this.state = {
            themeName: "",
            level: 0
        };
    }

    UNSAFE_componentWillMount() {
        const {props} = this;

        this.parseProps(props);
    }

    UNSAFE_componentWillReceiveProps(newProps) {
        this.parseProps(newProps);
    }

    /*eslint-disable eqeqeq*/
    parseProps(props) {

        if (!props.theme || !props.theme.SiteLayout.themeName) {
            return;
        }

        let themeName = props.type == 0 ? props.theme.SiteLayout.themeName : props.theme.SiteContainer.themeName;
        let path = props.type == 0 ? props.theme.SiteLayout.path : props.theme.SiteContainer.path;
        let level = path.indexOf("[G]") > -1 ? 4 : (path.indexOf("[S]") > -1 ? 2 : 1);

        this.setState({ themeName: themeName, level: level }, function () {
            this.loadThemeFiles();
        });
    }

    selectedAsSite(themeFile) {
        const {props} = this;
        let currentTheme = props.theme;

        if (themeFile.type === 0) {
            return currentTheme.SiteLayout.path.toLowerCase() === themeFile.path.toLowerCase();
        } else {
            return currentTheme.SiteContainer.path.toLowerCase() === themeFile.path.toLowerCase();
        }
    }

    selectedAsEdit(themeFile) {
        const {props} = this;
        let currentTheme = props.theme;

        if (themeFile.type === 0) {
            return currentTheme.EditLayout.path.toLowerCase() === themeFile.path.toLowerCase();
        } else {
            return currentTheme.EditContainer.path.toLowerCase() === themeFile.path.toLowerCase();
        }
    }

    getListWidth() {
        const {props} = this;

        let width = 0;
        let self = this;
        props.themeFiles[props.type].forEach((themeFile) => {
            width += (self.selectedAsSite(themeFile) || self.selectedAsEdit(themeFile)) ? 108 : 90;
        });

        return width + 20;
    }

    loadThemeFiles() {
        const {props, state} = this;

        if (!state.themeName || props.themeFiles[props.type].length !== 0) {
            return;
        }

        props.dispatch(ThemeActions.getCurrentThemeFiles(state.themeName, props.type, state.level));
    }

    render() {
        const {props} = this;

        return (
            <div className="theme-files-list">
                <Scrollbars
                    className="theme-files-scroller"
                    autoHeight
                    autoHeightMin={0}
                    autoHeightMax={180}>
                    <ul style={{ width: this.getListWidth() }}>
                        {props.themeFiles[props.type].map((themeFile, i) => {
                            return <ThemeFile themeFile={themeFile} key={i} />;
                        })}
                    </ul>
                </Scrollbars>
            </div>
        );
    }
}

ThemeFileList.propTypes = {
    dispatch: PropTypes.func.isRequired,
    theme: PropTypes.object,
    type: PropTypes.number
};

function mapStateToProps(state) {
    return {
        themeFiles: state.theme.currentThemeFiles,
        theme: state.theme.currentTheme
    };
}

export default connect(mapStateToProps)(ThemeFileList);