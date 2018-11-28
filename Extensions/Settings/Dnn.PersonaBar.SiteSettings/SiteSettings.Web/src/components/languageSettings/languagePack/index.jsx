import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    languages as LanguagesActions
} from "../../../actions";
import PersonaBarPageBody from "dnn-persona-bar-page-body";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import InputGroup from "dnn-input-group";
import Checkbox from "dnn-checkbox";
import Dropdown from "dnn-dropdown";
import RadioButtons from "dnn-radio-buttons";
import Label from "dnn-label";
import Button from "dnn-button";
import "./style.less";
import util from "../../../utils";
import resx from "../../../resources";
import styles from "./style.less";
import MessageBox from "../../messageBox";

class LanguagePackPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            languagePack: {
                PackType: "Core",
                ModuleIds: [],
                CultureCode: "en-US",
                FileName: "Core"
            },
            error: {
                name: false,
                module: true
            },
            triedToSubmit: false,
            showMessageBox: false,
            message: ""
        };
    }

    getLanguageOptions() {
        const {props} = this;
        let options = [];
        if (props.languages !== undefined) {
            options = props.languages.map((item) => {
                if (props.languageSettings.LanguageDisplayMode === "NATIVE") {
                    return {
                        label: <div style={{ float: "left", display: "flex" }}>
                            <div className="language-flag">
                                <img src={item.Icon} alt={item.NativeName} />
                            </div>
                            <div className="language-name">{item.NativeName}</div>
                        </div>, value: item.Code
                    };
                }
                else {
                    return {
                        label: <div style={{ float: "left", display: "flex" }}>
                            <div className="language-flag">
                                <img src={item.Icon} alt={item.EnglishName} />
                            </div>
                            <div className="language-name">{item.EnglishName}</div>
                        </div>, value: item.Code
                    };
                }
            });
        }
        return options;
    }

    getTypeOptions() {
        let options = [{ label: resx.get("Core.LangPackType"), value: "Core" },
            { label: resx.get("Module.LangPackType"), value: "Module" },
            { label: resx.get("Provider.LangPackType"), value: "Provider" },
            { label: resx.get("AuthSystem.LangPackType"), value: "AuthSystem" },
            { label: resx.get("Full.LangPackType"), value: "Full" }];
        return options;
    }

    renderModuleSelection() {
        const {props} = this;
        if (props.modules) {
            return props.modules.map((item, i) => {
                return (
                    <div className="languagePack-module" key={i}>
                        <Checkbox
                            style={{ float: "left" }}
                            value={this.isModuleSelected(item.Value)}
                            onChange={this.onModuleSelectionChange.bind(this, item.Value)} />
                        <div>{item.Key}</div>
                    </div>
                );
            });
        }
    }

    isModuleSelected(id) {
        const { state} = this;
        return state.languagePack.ModuleIds.indexOf(id) > -1 ? true : false;
    }

    onModuleSelectionChange(id, event) {
        let {state} = this;
        let languagePack = Object.assign({}, state.languagePack);

        let index = languagePack.ModuleIds.indexOf(id);
        if (event) {
            if (index === -1) {
                languagePack.ModuleIds.push(id);
            }
        }
        else {
            if (index > -1) {
                languagePack.ModuleIds.splice(index, 1);
            }
        }

        if (languagePack.ModuleIds.length === 0) {
            state.error["module"] = true;
        }
        else {
            state.error["module"] = false;
        }

        this.setState({
            languagePack: languagePack,
            error: state.error,
            triedToSubmit: false
        });
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let languagePack = Object.assign({}, state.languagePack);

        if (key === "CultureCode") {
            languagePack[key] = event.value;
        }
        else if (key === "PackType") {
            languagePack[key] = event;
            if (event === "Module" || event === "Provider" || event === "AuthSystem") {
                props.dispatch(LanguagesActions.getModuleList(event));
            }
            else {
                languagePack.FileName = event;
            }
            languagePack.ModuleIds = [];
        }
        else {
            languagePack[key] = typeof (event) === "object" ? event.target.value : event;
        }

        if (languagePack[key] === "" && key === "FileName") {
            state.error["name"] = true;
        }
        else if (languagePack[key] !== "" && key === "FileName") {
            state.error["name"] = false;
        }

        this.setState({
            languagePack: languagePack,
            error: state.error,
            triedToSubmit: false
        });
    }

    onCreate(event) {
        event.preventDefault();
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });

        if (state.error.name && (state.languagePack.PackType === "Core" || state.languagePack.PackType === "Full")) {
            return;
        }

        if (state.error.module && (state.languagePack.PackType === "Module"
            || state.languagePack.PackType === "Provider"
            || state.languagePack.PackType === "AuthSystem")) {
            util.utilities.notifyError(resx.get("ModuleRequired.Error"));
            return;
        }

        props.dispatch(LanguagesActions.createLanguagePack(state.languagePack, (data) => {
            util.utilities.notify(data.Message);
        }, (error) => {
            const errorMessage = JSON.parse(error.responseText);
            util.utilities.notifyError(errorMessage.Message);
        }));
    }

    closeMessageBox() {
        this.setState({
            showMessageBox: false
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div className={styles.languagePack}>
                <PersonaBarPageBody
                    className="create-language-pack-panel"
                    backToLinkProps={{
                        text: resx.get("BackToLanguages"),
                        onClick: props.closeLanguagePack.bind(this)
                    }}>
                    <div className="languagePack-wrapper">
                        <InputGroup>
                            <Label
                                tooltipMessage={resx.get("lbLocale.Help")}
                                label={resx.get("lbLocale")}
                            />
                            <Dropdown
                                options={this.getLanguageOptions()}
                                value={state.languagePack.CultureCode}
                                onSelect={this.onSettingChange.bind(this, "CultureCode")}
                            />
                        </InputGroup>
                        <div className="seperator" />
                        <InputGroup>
                            <Label
                                tooltipMessage={resx.get("lblType.Help")}
                                label={resx.get("lblType")}
                            />
                            <RadioButtons
                                onChange={this.onSettingChange.bind(this, "PackType")}
                                options={this.getTypeOptions()}
                                buttonGroup="languagePackType"
                                value={state.languagePack.PackType}
                            />
                        </InputGroup>
                        {(state.languagePack.PackType === "Core" || state.languagePack.PackType === "Full") &&
                            <InputGroup>
                                <Label
                                    style={{ width: "100%" }}
                                    tooltipMessage={resx.get("lblName.Help")}
                                    label={resx.get("lblName")}
                                />
                                <div className="name-prefix">ResourcePack.</div>
                                <SingleLineInputWithError
                                    inputStyle={{ margin: "0" }}
                                    withLabel={false}
                                    error={state.error.name && state.triedToSubmit}
                                    errorMessage={resx.get("valName.ErrorMessage")}
                                    value={state.languagePack.FileName}
                                    onChange={this.onSettingChange.bind(this, "FileName")}
                                />
                                <div className="name-suffix">.&lt;version&gt;.&lt;locale&gt;.zip</div>
                            </InputGroup>
                        }
                        {(state.languagePack.PackType === "Module" || state.languagePack.PackType === "Provider" || state.languagePack.PackType === "AuthSystem") &&
                            <InputGroup>
                                <Label
                                    label={resx.get("SelectModules")}
                                />
                                <div className="moduleSelection">
                                    {this.renderModuleSelection()}
                                </div>
                            </InputGroup>
                        }
                        <div className="buttons-box">
                            <Button
                                type="secondary"
                                onClick={props.closeLanguagePack.bind(this)}>
                                {resx.get("Cancel")}
                            </Button>
                            <Button
                                type="primary"
                                onClick={this.onCreate.bind(this)}>
                                {resx.get("Create")}
                            </Button>
                        </div>
                    </div>
                    <MessageBox
                        message={state.message}
                        fixedHeight={500}
                        isOpened={state.showMessageBox}
                        onClose={this.closeMessageBox.bind(this)}
                        collapsibleWidth={485}
                    />
                </PersonaBarPageBody>
            </div>
        );
    }
}

LanguagePackPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    portalId: PropTypes.number,
    closeLanguagePack: PropTypes.func,
    languages: PropTypes.array,
    modules: PropTypes.array,
    languageSettings: PropTypes.object
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        languages: state.languages.languageList,
        modules: state.languages.modules,
        languageSettings: state.languages.languageSettings
    };
}

export default connect(mapStateToProps)(LanguagePackPanelBody);