import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import "./style.less";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Grid from "dnn-grid-system";
import Label from "dnn-label";
import Button from "dnn-button";
import Switch from "dnn-switch";
import Select from "dnn-select";
import DropdownWithError from "dnn-dropdown-with-error";
import MultiLineInput from "dnn-multi-line-input";
import RadioButtons from "dnn-radio-buttons";
import InputGroup from "dnn-input-group";
import Input from "dnn-single-line-input";
import Dropdown from "dnn-dropdown";
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

    componentWillMount() {
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

    componentWillReceiveProps(props) {
        let {state} = this;

        if (!props.languageDetail) {
            return;
        }

        this.setState({
            languageDetail: Object.assign({}, props.languageDetail)
        });
    }

    getLanguageOptions() {
        let {props, state} = this;
        let options = [];
        if (props.id === "add") {
            if (props.fullLanguageList !== undefined) {
                options = props.fullLanguageList.map((item) => {
                    if (props.languageDisplayMode === "NATIVE") {
                        return { label: item.NativeName, value: item.Name };
                    }
                    else {
                        return { label: item.EnglishName, value: item.Name };
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
        return options.sort(function(a, b) {
            let nameA = a.label.toUpperCase();
            let nameB = b.label.toUpperCase();
            if (nameA < nameB) {
                return -1;
            }
            if (nameA > nameB) {
                return 1;
            }
            return 0;
        });
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

    onSave(event) {
        const {props, state} = this;
        props.onUpdate(state.languageDetail);
    }

    onSaveRoles(event) {
        const {props, state} = this;
        props.onUpdateRoles(state.languageDetail);
    }

    onCancel(event) {
        const {props, state} = this;
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

    getFallbackOptions() {
        let {props, state} = this;
        let options = [];
        if (props.fallbacks !== undefined) {
            options = props.fallbacks.map((item) => {
                if (props.languageDisplayMode === "NATIVE") {
                    return { label: item.NativeName, value: item.Name };
                }
                else {
                    return { label: item.EnglishName, value: item.Name };
                }
            });
        }
        return options;
    }

    getLanguageValue(code) {
        let {props, state} = this;
        if (code) {
            return code;
        }
        else {
            let languages = this.getLanguageOptions();

            if (languages.length > 0) {
                let languageDetail = Object.assign({}, state.languageDetail);
                languageDetail["Code"] = languages[0].value;
                this.setState({
                    languageDetail: languageDetail
                });
            }

            return languages.length > 0 ? languages[0].value : "";
        }
    }

    onRoleSelectChange(roleName, selected) {
        let {state, props} = this;
        let languageDetail = Object.assign({}, state.languageDetail);

        let roles = languageDetail.Roles.split(';');
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

        languageDetail.Roles = roles.join(';');

        this.setState({
            languageDetail: languageDetail
        });
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
                    value={state.languageDetail.Fallback}
                    onSelect={this.onSettingChange.bind(this, "Fallback")}
                    enabled={true}
                    />
            </InputGroup>
        </div>;
        return (
            <div className="language-editor">
                <Grid children={[columnOne, columnTwo]} numberOfColumns={2} />
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
        let {state, props} = this;
        const columnOne = <div className="left-column">
            <InputGroup>
                <Label
                    tooltipMessage={resx.get("fallBackLabel.Help")}
                    label={resx.get("fallBackLabel")}
                    />
                <Dropdown
                    options={this.getFallbackOptions()}
                    value={state.languageDetail.Fallback}
                    onSelect={this.onSettingChange.bind(this, "Fallback")}
                    enabled={isHost}
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
                        labelHidden={true}
                        value={state.languageDetail.Enabled}
                        onChange={this.onSettingChange.bind(this, "Enabled")}
                        readOnly={state.languageDetail.IsDefault}
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
                <Grid children={[columnOne, columnTwo]} numberOfColumns={2} />
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
        let {state, props} = this;
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
        languageClientModified: state.languages.languageClientModified
    };
}

export default connect(mapStateToProps)(LanguageEditor);