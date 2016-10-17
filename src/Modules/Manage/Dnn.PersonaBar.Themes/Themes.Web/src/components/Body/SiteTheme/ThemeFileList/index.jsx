import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import "./style.less";

class ThemeFileList extends Component {
    constructor() {
        super();
        this.state = {
            themeFiles: [],
            themeName: '',
            level: 0
        };
    }

    componentWillMount(){
        //this.loadThemeFiles();
    }

    componentWillReceiveProps(newProps){
        const {props, state} = this;

        console.log(newProps);

        let themeName = props.type == 0 ? newProps.theme.SiteLayout.themeName : newProps.theme.SiteContainer.themeName;
        let path = props.type == 0 ? newProps.theme.SiteLayout.path : newProps.theme.SiteContainer.path;
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

        props.dispatch(ThemeActions.getThemeFiles(state.themeName, props.type, state.level));
    }
    
    render() {
        const {props, state} = this;

        return (
            <ul className="">
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