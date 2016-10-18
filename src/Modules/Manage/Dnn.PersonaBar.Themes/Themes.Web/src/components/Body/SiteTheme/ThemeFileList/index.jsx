import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";

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

        if(!props.theme.SiteLayout.themeName){
            return;
        }

        let themeName = props.type == 0 ? props.theme.SiteLayout.themeName : props.theme.SiteContainer.themeName;
        let path = props.type == 0 ? props.theme.SiteLayout.path : props.theme.SiteContainer.path;
        let level = path.indexOf('[G]') > -1 ? 2 : 1;

        if(themeName === state.themeName){
            return;
        }

        this.setState({themeName: themeName, level: level}, function(){
            this.loadThemeFiles();
        });  
    }

    loadThemeFiles(){
        const {props, state} = this;

        if(!state.themeName){
            return;
        }

        props.dispatch(ThemeActions.getCurrentThemeFiles(state.themeName, props.type, state.level));
    }
    
    render() {
        const {props, state} = this;

        return (
            <ul className="theme-files-list">
                {props.themeFiles.map((themeFile, index) => {
                    return <ThemeFile themeFile={themeFile} />;
                }) }
            </ul>
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
        themeFiles: state.theme.currentThemeFiles
    };
}

export default connect(mapStateToProps)(ThemeFileList);