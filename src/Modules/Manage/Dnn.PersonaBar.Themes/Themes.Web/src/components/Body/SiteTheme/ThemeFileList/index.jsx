import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import { Scrollbars } from "react-custom-scrollbars";

import ThemeFile from "./ThemeFile";

import "./style.less";

class ThemeFileList extends Component {
    constructor() {
        super();
        this.state = {
            themeName: '',
            level: 0
        };
    }

    componentWillMount(){
        const {props, state} = this;

        this.parseProps(props);
    }

    componentWillReceiveProps(newProps){
        const {props, state} = this;

        this.parseProps(newProps);
    }

    parseProps(props){
        const {state} = this;

        if(!props.theme || !props.theme.SiteLayout.themeName){
            return;
        }

        let themeName = props.type == 0 ? props.theme.SiteLayout.themeName : props.theme.SiteContainer.themeName;
        let path = props.type == 0 ? props.theme.SiteLayout.path : props.theme.SiteContainer.path;
        let level = path.indexOf('[G]') > -1 ? 2 : 1;

        this.setState({themeName: themeName, level: level}, function(){
            this.loadThemeFiles();
        });  
    }

    selectedAsSite(themeFile){
        const {props, state} = this;
        let currentTheme = props.theme;

        if(themeFile.type === 0){
            return currentTheme.SiteLayout.path.toLowerCase() === themeFile.path.toLowerCase();
        } else {
            return currentTheme.SiteContainer.path.toLowerCase() === themeFile.path.toLowerCase();
        }
    }

    selectedAsEdit(themeFile){
        const {props, state} = this;
        let currentTheme = props.theme;

        if(themeFile.type === 0){
            return currentTheme.EditLayout.path.toLowerCase() === themeFile.path.toLowerCase();
        } else {
            return currentTheme.EditContainer.path.toLowerCase() === themeFile.path.toLowerCase();
        }
    }

    getListWidth(){
        const {props, state} = this;

        let width = 0;
        let self = this;
        props.themeFiles[props.type].forEach((themeFile, index) => {
            width += (self.selectedAsSite(themeFile) || self.selectedAsEdit(themeFile)) ? 108 : 90;
        });

        return width - 10;
    }

    loadThemeFiles(){
        const {props, state} = this;

        if(!state.themeName ||  props.themeFiles[props.type].length !== 0){
            return;
        }

        props.dispatch(ThemeActions.getCurrentThemeFiles(state.themeName, props.type, state.level));
    }
    
    render() {
        const {props, state} = this;

        return (
            <div  className="theme-files-list">
                <Scrollbars
                    className="theme-files-scroller"
                    autoHeight
                    autoHeightMin={0}
                    autoHeightMax={180}>
                    <ul style={{width: this.getListWidth()}}>
                        {props.themeFiles[props.type].map((themeFile, index) => {
                            return <ThemeFile themeFile={themeFile} />;
                        }) }
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