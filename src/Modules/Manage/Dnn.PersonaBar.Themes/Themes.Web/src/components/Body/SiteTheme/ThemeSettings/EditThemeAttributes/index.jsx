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
            setting: '',
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

        props.dispatch(ThemeActions.getEditableTokens());
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

    getTokenOptions(){
        const {props, state} = this;

        if(!state.path){
            return [];
        }

        return props.tokens.map(function(t){
            return {value: t.value, label: t.text};
        });
    }

    getSettingOptions(){
        const {props, state} = this;

        return props.settings.map(function(t){
            return {value: t.value, label: t.text};
        });
    }

    onThemeChanged(themeName){
        const {props, state} = this;

        this.setState({themeName: themeName.value}, function(){
            let themeName = this.state.themeName;
            let type = this.getThemeType();
            let level = state.level;

            props.dispatch(ThemeActions.getEditableThemeFiles(themeName, type, level));
        });
    }

    onThemeFileChanged(themeFile){
        const {props, state} = this;

        this.setState({path: themeFile.value});
    }

    onTokenChanged(token){
        const {props, state} = this;

        this.setState({token: token.value}, function(){
            let token = this.state.token;

            props.dispatch(ThemeActions.getEditableSettings(token));
        });
    }

    onSettingChanged(setting){
        const {props, state} = this;

        this.setState({setting: setting.value}, function(){
            let token = this.state.token;
            let setting = this.state.setting;

            props.dispatch(ThemeActions.getEditableValues(token, setting));
        });
    }

    onThemeTypeChanged(type){
        const {props, state} = this;

        this.setState({themeType: type});
    }

    renderValueField(){
        const {props, state} = this;

        let onFieldChange = function(value){
            if(value.value){
                value = value.value;
            }

            this.setState({value: value});
        };

        if(!props.value){
            return <SingleLineInputWithError value={state.value} onChange={onFieldChange.bind(this)} label={Localization.get("Value")}/>;
        }

        if(props.value.toLowerCase() === "pages"){
            //TODO: use combo box.
        }

        let options = props.value.split(',').map(function(value){
            if(value){
                return {value: value, label: value};
            }
        });

        return <DropdownWithError
                    options={options}
                    value={state.value}
                    onSelect={onFieldChange.bind(this)}
                    label={Localization.get("Value")}/>;
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
                                options={this.getSettingOptions()}
                                value={state.setting}
                                onSelect={this.onSettingChanged.bind(this)}
                                label={Localization.get("Setting")}/>
                        </GridCell>
                        <GridCell columnSize="50">
                            <DropdownWithError 
                                options={this.getTokenOptions()}
                                value={state.token}
                                onSelect={this.onTokenChanged.bind(this)}
                                label={Localization.get("Token")}/>
                        </GridCell>
                        <GridCell columnSize="50" className="right-column">
                            {this.renderValueField()}
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
        themeFiles: state.theme.editableThemeFiles,
        tokens: state.theme.editableTokens,
        settings: state.theme.editableSettings,
        value: state.theme.editableValue
    };
}

export default connect(mapStateToProps)(EditThemeAttributes);