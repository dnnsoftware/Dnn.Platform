import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import RadioButtons from "dnn-radio-buttons";
import DropdownWithError from "dnn-dropdown-with-error";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Collapsible from "react-collapse";


import "./style.less";

class EditThemeAttributes extends Component {
    constructor() {
        super();
        this.state = {
            themeType: "layout",
            themeName: '',
            path: '',
            token: '',
            key: '',
            value: '',
            openEditPopup: true,
            level: 3
        };

    }

    componentWillMount()
    {
        const {props, state} = this;

        if(props.themes.layouts.length === 0){
            props.dispatch(ThemeActions.getThemes(state.level));
        }
    }

    getThemeType(){
        const {props, state} = this;

        return state.themeType == "container" ? 1 : 0;
    }

    getThemeOptions(){
        const {props, state} = this;

        let type = this.getThemeType();
        let source = type == 1 ? props.themes.layouts : props.themes.containers;
        return source.map(function(t){
            return {value: t.packageName, label: t.packageName};
        });
    }

    
    getThemeFileOptions(){
        const {props, state} = this;

        return props.themeFiles.map(function(f){
            return {value: f.path, label: f.name};
        });
    }

    onThemeChanged(themeName){
        const {props, state} = this;

        this.setState({themeName: themeName.value}, function(){
            let themeName = this.state.themeName;
            let type = this.getThemeType();
            let level = state.level;

            props.dispatch(ThemeActions.getEditThemeFiles(themeName, type, level));
        });
    }

    onThemeFileChanged(themeFile){
        const {props, state} = this;

        this.setState({path: themeFile.value}, function(){
            // let themeName = this.state.themeName;
            // let type = this.getThemeType();
            // let level = state.level;

            // props.dispatch(ThemeActions.getEditThemeFiles(themeName, type, level));
        });
    }

    onThemeTypeChanged(type){
        const {props, state} = this;

        this.setState({themeType: type});
    }

    render() {
        const {props, state} = this;

        return (
            <div className="edit-theme-attributes">
                <Button size="small">{Localization.get("EditThemeAttributes")}</Button>
                <Collapsible isOpened={state.openEditPopup} className="edit-popup" fixedHeight={420} style={{ float: "left" }}>
                    <h3>{Localization.get("EditThemeAttributes")}</h3>
                    <GridCell>
                        <GridCell columnSize="50">
                            <DropdownWithError 
                                options={this.getThemeOptions()}
                                value={state.themeName}
                                onSelect={this.onThemeChanged.bind(this)}
                                label={Localization.get("Theme")}/>
                        </GridCell>
                        <GridCell columnSize="50" className="right-column">
                            <RadioButtons 
                                options={[{value: "layout", label: Localization.get("Layout")}, {value: "container", label: Localization.get("Container")}]} 
                                onChange={this.onThemeTypeChanged.bind(this)}
                                value={this.state.themeType}
                                float="none"/>
                        </GridCell>
                        <div className="clear split" />
                        <GridCell columnSize="50">
                            <DropdownWithError 
                                options={this.getThemeFileOptions()}
                                value={state.path}
                                onSelect={this.onThemeFileChanged.bind(this)}
                                label={Localization.get("File")}/>
                        </GridCell>
                        <GridCell columnSize="50" className="right-column">
                            <DropdownWithError 
                                label={Localization.get("Setting")}/>
                        </GridCell>
                        <GridCell columnSize="50">
                            <DropdownWithError 
                                label={Localization.get("Token")}/>
                        </GridCell>
                        <GridCell columnSize="50" className="right-column">
                            <SingleLineInputWithError 
                                label={Localization.get("Value")}/>
                        </GridCell>
                        <GridCell columnSize="100" className="actions-cell">
                            <Button>{Localization.get("Cancel")}</Button>
                            <Button type="primary">{Localization.get("Apply")}</Button>
                        </GridCell>
                    </GridCell>
                </Collapsible>
            </div>
        );
    }
}

EditThemeAttributes.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        themes: state.theme.themes,
        themeFiles: state.theme.editThemeFiles
    };
}

export default connect(mapStateToProps)(EditThemeAttributes);