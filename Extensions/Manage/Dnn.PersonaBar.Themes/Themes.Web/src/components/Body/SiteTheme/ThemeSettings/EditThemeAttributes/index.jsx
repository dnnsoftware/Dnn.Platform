import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    theme as ThemeActions
} from "actions";
import Localization from "localization";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import RadioButtons from "dnn-radio-buttons";
import DropdownWithError from "dnn-dropdown-with-error";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Collapsible from "react-collapse";
import utils from "utils";
import "./style.less";

/*eslint-disable eqeqeq*/
class EditThemeAttributes extends Component {
    constructor() {
        super();
        this.state = {
            themeType: "layout",
            themeName: "",
            path: "",
            token: "",
            setting: "",
            value: "",
            openEditPopup: false,
            level: 7,
            startSave: false
        };

    }

    UNSAFE_componentWillMount() {
        const {props} = this;

        if (props.tokens.length === 0) {
            props.dispatch(ThemeActions.getEditableTokens());
        }
    }

    getThemeType() {
        const {state} = this;

        return state.themeType == "container" ? 1 : 0;
    }

    getThemeOptions() {
        const {props} = this;

        let type = this.getThemeType();
        let source = type == 1 ? props.themes.containers : props.themes.layouts;
        let isHost = utils.params.settings.isHost;
        return source.filter(l => {
            return isHost || l.level === 1 || l.level === 2;
        }).map(function (t) {
            return { value: t.packageName, label: t.packageName, level: t.level };
        });
    }


    getThemeFileOptions() {
        const {props, state} = this;

        if (!state.themeName) {
            return [];
        }

        return props.themeFiles.map(function (f) {
            return { value: f.path, label: f.name };
        });
    }

    getTokenOptions() {
        const {props, state} = this;

        if (!state.path) {
            return [];
        }

        return props.tokens.map(function (t) {
            return { value: t.value, label: t.text };
        });
    }

    getSettingOptions() {
        const {props, state} = this;

        if (!state.path || !state.token) {
            return [];
        }

        return props.settings.map(function (t) {
            return { value: t.value, label: t.text };
        });
    }

    onThemeTypeChanged(type) {
        this.setState({ themeType: type, themeName: "", path: "", token: "", setting: "", value: "" });
    }


    onThemeChanged(themeName) {
        const {props} = this;
        this.setState({ themeName: themeName.value, level: themeName.level, path: "", token: "", setting: "", value: "" }, function () {
            let themeName = this.state.themeName;
            let type = this.getThemeType();
            let level = this.state.level;

            props.dispatch(ThemeActions.getEditableThemeFiles(themeName, type, level));
        });
    }

    onThemeFileChanged(themeFile) {
        this.setState({ path: themeFile.value, token: "", setting: "", value: "" });
    }

    onTokenChanged(token) {
        const {props} = this;

        this.setState({ token: token.value, setting: "", value: "" }, function () {
            let token = this.state.token;

            props.dispatch(ThemeActions.getEditableSettings(token));
        });
    }

    onSettingChanged(setting) {
        const {props} = this;

        this.setState({ setting: setting.value, value: "" }, function () {
            let token = this.state.token;
            let setting = this.state.setting;

            props.dispatch(ThemeActions.getEditableValues(token, setting));
        });
    }

    startEdit() {
        this.setState({ openEditPopup: true, themeName: "", path: "", token: "", setting: "", value: "" });
    }

    cancelEdit() {
        this.setState({ openEditPopup: false, themeName: "", path: "", token: "", setting: "", value: "" });
    }

    Save() {
        const {props, state} = this;

        this.setState({ startSave: true }, function () {

            if (!state.path || !state.token || !state.setting || !state.value) {
                return;
            }

            let self = this;
            props.dispatch(ThemeActions.updateTheme(state.path, state.token, state.setting, state.value, function () {
                self.setState({ openEditPopup: false });
                utils.utilities.notify(Localization.get("Successful"));
            }));
        });
    }

    renderValueField() {
        const {props, state} = this;

        let onFieldChange = function (value) {
            let editValue = value.value;
            if (value.target) {
                editValue = value.target.value;
            }

            this.setState({ value: editValue });
        };

        if (!props.value) {
            return <SingleLineInputWithError
                value={state.value}
                onChange={onFieldChange.bind(this)}
                error={state.startSave && !state.value}
                label={Localization.get("Value")} />;
        }

        if (props.value.toLowerCase() === "pages") {
            //TODO: use combo box.
        }

        let options = props.value.split(",").map(function (value) {
            if (value) {
                return { value: value, label: value };
            }
        });

        return <DropdownWithError
            options={options}
            value={state.value}
            onSelect={onFieldChange.bind(this)}
            fixedHeight={100}
            error={state.startSave && !state.value}
            label={Localization.get("Value")} />;
    }

    render() {
        const {state} = this;

        return (
            <div className="edit-theme-attributes">
                <Button size="small" onClick={this.startEdit.bind(this)}>{Localization.get("EditThemeAttributes")}</Button>
                <Collapsible isOpened={state.openEditPopup} className="edit-popup" fixedHeight={420} style={{ float: "left" }}>
                    <div>
                        <h3>{Localization.get("EditThemeAttributes")}</h3>
                        <GridCell>
                            <GridCell columnSize="50">
                                <DropdownWithError
                                    defaultDropdownValue={Localization.get("NoneSpecified")}
                                    options={this.getThemeOptions()}
                                    value={state.themeName}
                                    onSelect={this.onThemeChanged.bind(this)}
                                    fixedHeight={100}
                                    error={state.startSave && !state.themeName}
                                    label={Localization.get("Theme")} />
                            </GridCell>
                            <GridCell columnSize="50" className="right-column">
                                <RadioButtons
                                    options={[{ value: "layout", label: Localization.get("Layout") }, { value: "container", label: Localization.get("Container") }]}
                                    onChange={this.onThemeTypeChanged.bind(this)}
                                    value={this.state.themeType}
                                    float="none" />
                            </GridCell>
                            <div className="clear split" />
                            <GridCell columnSize="50">
                                <DropdownWithError
                                    defaultDropdownValue={Localization.get("NoneSpecified")}
                                    options={this.getThemeFileOptions()}
                                    value={state.path}
                                    onSelect={this.onThemeFileChanged.bind(this)}
                                    enabled={state.themeName}
                                    fixedHeight={100}
                                    error={state.startSave && !state.path}
                                    label={Localization.get("File")} />
                            </GridCell>
                            <GridCell columnSize="50" className="right-column">
                                <DropdownWithError
                                    defaultDropdownValue={Localization.get("NoneSpecified")}
                                    options={this.getSettingOptions()}
                                    value={state.setting}
                                    onSelect={this.onSettingChanged.bind(this)}
                                    enabled={state.path && state.token}
                                    fixedHeight={100}
                                    error={state.startSave && !state.setting}
                                    label={Localization.get("Setting")} />
                            </GridCell>
                            <GridCell columnSize="50">
                                <DropdownWithError
                                    defaultDropdownValue={Localization.get("NoneSpecified")}
                                    options={this.getTokenOptions()}
                                    value={state.token}
                                    onSelect={this.onTokenChanged.bind(this)}
                                    enabled={state.path}
                                    fixedHeight={100}
                                    error={state.startSave && !state.token}
                                    label={Localization.get("Token")} />
                            </GridCell>
                            <GridCell columnSize="50" className="right-column">
                                {this.renderValueField()}
                            </GridCell>
                            <GridCell columnSize="100" className="actions-cell">
                                <Button onClick={this.cancelEdit.bind(this)}>{Localization.get("Cancel")}</Button>
                                <Button type="primary" onClick={this.Save.bind(this)}>{Localization.get("Apply")}</Button>
                            </GridCell>
                        </GridCell>
                    </div>
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