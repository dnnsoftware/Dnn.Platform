import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import { GridSystem,Label,Button,Switch,InputGroup,Dropdown,Flag } from "@dnnsoftware/dnn-react-common";
import Roles from "./roles";
import {
    languages as LanguagesActions
} from "../../../../actions";
import util from "../../../../utils";
import resx from "../../../../resources";

let isHost = false;

class LanguageEditor extends Component {
    constructor() {
        super();

        this.state = {
            languageDetail: {
            }
        };
        isHost = util.settings.isHost;
    }

    componentDidMount() {
        const {props} = this;
        if (!props.languageDetail || (props.code !== props.languageDetail.Code)) {
            props.dispatch(LanguagesActions.getLanguage(props.portalId, props.languageId));

            if (props.id === "add" && !props.fullLanguageList) {
                props.dispatch(LanguagesActions.getAllLanguages());
            }
        }
        else {
            this.setState({
                languageDetail: Object.assign({}, props.languageDetail)
            });
        }
    }

    componentDidUpdate(newProps) {
        const { props } = this;
        if(props.languageDetail !== newProps.languageDetail) {
            this.setState({
                languageDetail: Object.assign({}, props.languageDetail)
            });
        }
    }

    getFlagItem(name, code) {
        return (
            <div title={name}>
                <Flag culture={code} title={name}/>
                <div style={{display:"inline-block"}}>{name}</div>
            </div>);
    }

    getLanguageOptions() {
        let {props, state} = this;
        let options = [];
        if (props.id === "add") {
            if (props.fullLanguageList !== undefined) {
                options = props.fullLanguageList.map((item) => {
                    if (props.languageDisplayMode === "NATIVE") {
                        return {
                            label: this.getFlagItem(item.NativeName, item.Name),
                            searchableValue: item.NativeName,
                            value: item.Name,
                            text: item.NativeName
                        };
                    }
                    else {
                        return {
                            label: this.getFlagItem(item.EnglishName, item.Name),
                            searchableValue: item.EnglishName,
                            value: item.Name, text: item.EnglishName
                        };
                    }
                });
            }
        }
        else {
            if (props.languageDisplayMode === "NATIVE") {
                options.unshift({ label: state.languageDetail.NativeName, value: state.languageDetail.Code });
            }
            else {
                options.unshift({ label: state.languageDetail.EnglishName, value: state.languageDetail.Code });
            }
        }
        return options.sort(function (a, b) {
            let nameA = a.text.toUpperCase();
            let nameB = b.text.toUpperCase();
            if (nameA < nameB) {
                return -1;
            }
            if (nameA > nameB) {
                return 1;
            }
            return 0;
        });
    }

    getFallbackOptions() {
        const {props} = this;
        let options = [];
        if (props.fallbacks !== undefined) {
            options = props.fallbacks.map((item) => {
                if (props.languageDisplayMode === "NATIVE") {
                    return {
                        label: this.getFlagItem(item.NativeName, item.Name),
                        searchableValue: item.NativeName,
                        value: item.Name
                    };
                }
                else {
                    return {
                        label: this.getFlagItem(item.EnglishName, item.Name),
                        searchableValue: item.EnglishName,
                        value: item.Name
                    };
                }
            });
        }
        return options;
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let languageDetail = Object.assign({}, state.languageDetail);

        if (key === "Code" || key === "Fallback") {
            languageDetail[key] = event.value;
        }
        else {
            languageDetail[key] = typeof (event) === "object" ? event.target.value : event;
        }
        this.setState({
            languageDetail: languageDetail
        });

        props.dispatch(LanguagesActions.languageClientModified(languageDetail));
    }

    onSave() {
        const {props, state} = this;
        props.onUpdate(state.languageDetail);
    }

    onSaveRoles() {
        const {props, state} = this;
        props.onUpdateRoles(state.languageDetail);
    }

    onCancel() {
        const {props} = this;
        if (props.languageClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(LanguagesActions.cancelLanguageClientModified());
                props.Collapse();
            });
        }
        else {
            props.Collapse();
        }
    }

    getLanguageDisplayModes() {
        const {props} = this;
        let options = [];
        if (props.languageDisplayModes !== undefined) {
            options = props.languageDisplayModes.map((item) => {
                return { label: item.Key, value: item.Value };
            });
        }
        return options;
    }

    getLanguageValue(code) {
        let {state} = this;
        if (code) {
            return code;
        }
        else {
            let languages = this.getLanguageOptions();

            if (languages.length > 0) {
                let languageDetail = Object.assign({}, state.languageDetail);
                languageDetail["Code"] = languages[0].value;
            }

            return languages.length > 0 ? languages[0].value : "";
        }
    }

    onToggleEnable(enabled) {
        if (!enabled) {
            const {languageDetail} = this.state;
            const languageName = this.props.languageDisplayMode.toLowerCase() === "native" ? languageDetail.NativeName : languageDetail.EnglishName;
            util.utilities.confirm(resx.get("DisableLanguageWarning").replace("{0}", languageName), resx.get("Yes"), resx.get("No"), () => {
                this.onSettingChange("Enabled", enabled);
            });
        } else {
            this.onSettingChange("Enabled", enabled);
        }
    }

    onRoleSelectChange(roleName, selected) {
        let {state, props} = this;
        let languageDetail = Object.assign({}, state.languageDetail);

        let roles = languageDetail.Roles.split(";");
        if (!roles) {
            roles = [];
        }

        let index = roles.indexOf(roleName);
        if (selected) {
            if (index === -1) {
                roles.push(roleName);
            }
        }
        else {
            if (index > -1) {
                roles.splice(index, 1);
            }
        }

        languageDetail.Roles = roles.join(";");

        this.setState({
            languageDetail: languageDetail
        });

        props.dispatch(LanguagesActions.languageClientModified(languageDetail));
    }

    renderNewForm() {
        let {state, props} = this;
        const columnOne = <div className="left-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("languageLabel.Help")}
                    label={resx.get("languageLabel")}
                />
                <Dropdown
                    options={this.getLanguageOptions()}
                    value={this.getLanguageValue(state.languageDetail.Code)}
                    onSelect={this.onSettingChange.bind(this, "Code")}
                    enabled={props.id === "add"}
                    getLabelText={(label) => label.props.title}
                />
            </InputGroup>
        </div>;
        const columnTwo = <div className="right-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("fallBackLabel.Help")}
                    label={resx.get("fallBackLabel")}
                />
                <Dropdown
                    options={this.getFallbackOptions()}
                    fixedHeight={100}
                    value={state.languageDetail.Fallback}
                    onSelect={this.onSettingChange.bind(this, "Fallback")}
                    enabled={true}
                    getLabelText={(label) => label.props.title}
                />
            </InputGroup>
        </div>;
        return (
            <div className="language-editor">
                <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                <div className="editor-buttons-box">
                    <Button
                        type="secondary"
                        onClick={this.onCancel.bind(this)}>
                        {resx.get("Cancel")}
                    </Button>
                    <Button
                        type="primary"
                        onClick={this.onSave.bind(this)}>
                        {resx.get("Save")}
                    </Button>
                </div>
            </div>
        );
    }

    renderEditForm() {
        let {state} = this;
        const columnOne = <div className="left-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("fallBackLabel.Help")}
                    label={resx.get("fallBackLabel")}
                />
                <Dropdown
                    options={this.getFallbackOptions()}
                    fixedHeight={100}
                    value={state.languageDetail.Fallback}
                    onSelect={this.onSettingChange.bind(this, "Fallback")}
                    enabled={isHost}
                    getLabelText={(label) => label.props.title}
                />
            </InputGroup>
        </div>;
        const columnTwo = <div className="right-column">
            <InputGroup>
                <div className="languageDetailSettings-row_switch">
                    <Label
                        labelType="inline"
                        label={resx.get("enableLanguageLabel")}
                    />
                    <Switch
                        onText={resx.get("SwitchOn")}
                        offText={resx.get("SwitchOff")}
                        value={state.languageDetail.Enabled}
                        onChange={this.onToggleEnable.bind(this)}
                        readOnly={!state.languageDetail.CanEnableDisable}
                    />
                </div>
            </InputGroup>
            {state.languageDetail.IsDefault &&
                <InputGroup>
                    <Label
                        style={{ marginTop: "-15px" }}
                        labelType="inline"
                        label={resx.get("DefaultLanguage")}
                    />
                </InputGroup>
            }
        </div>;
        return (
            <div className="language-editor">
                <GridSystem numberOfColumns={2}>{[columnOne, columnTwo]}</GridSystem>
                <div className="editor-buttons-box">
                    <Button
                        type="secondary"
                        onClick={this.onCancel.bind(this)}>
                        {resx.get("Cancel")}
                    </Button>
                    <Button
                        disabled={!this.props.languageClientModified}
                        type="primary"
                        onClick={this.onSave.bind(this)}>
                        {resx.get("Save")}
                    </Button>
                </div>
            </div>
        );
    }

    renderRoleForm() {
        let {props} = this;
        return (
            <div className="language-editor">
                {props.code &&
                    <Roles cultureCode={props.code}
                        onSelectChange={this.onRoleSelectChange.bind(this)} />
                }
                <div className="editor-buttons-box-roles">
                    <Button
                        type="secondary"
                        onClick={this.onCancel.bind(this)}>
                        {resx.get("Cancel")}
                    </Button>
                    <Button
                        disabled={!this.props.languageClientModified}
                        type="primary"
                        onClick={this.onSaveRoles.bind(this)}>
                        {resx.get("Save")}
                    </Button>
                </div>
            </div>
        );
    }

    /* eslint-disable react/no-danger */
    render() {
        let {state, props} = this;
        /* eslint-disable react/no-danger */
        if (state.languageDetail !== undefined || props.id === "add") {
            if (props.openMode === 1) {
                if (props.id === "add") {
                    return this.renderNewForm();
                }
                else {
                    return this.renderEditForm();
                }
            }
            else if (props.openMode === 2) {
                return this.renderRoleForm();
            }
        }
        else return <div />;
    }
}

LanguageEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    languageSettings: PropTypes.object,
    languageDetail: PropTypes.object,
    languageId: PropTypes.number,
    fallbacks: PropTypes.array,
    fullLanguageList: PropTypes.array,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func,
    onUpdateRoles: PropTypes.func,
    id: PropTypes.string,
    languageClientModified: PropTypes.bool,
    languageDisplayModes: PropTypes.array,
    openMode: PropTypes.number,
    portalId: PropTypes.number,
    languageDisplayMode: PropTypes.string,
    code: PropTypes.string
};

function mapStateToProps(state) {
    return {
        languageDetail: state.languages.language,
        fallbacks: state.languages.fallbacks,
        fullLanguageList: state.languages.fullLanguageList,
        languageDisplayModes: state.languages.languageDisplayModes,
        languageClientModified: state.languages.languageClientModified,
        portalId: state.siteInfo.settings ? state.siteInfo.settings.PortalId : undefined,
    };
}

export default connect(mapStateToProps)(LanguageEditor);