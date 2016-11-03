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
import {
    siteSettings as SiteSettingsActions
} from "../../../../actions";
import util from "../../../../utils";
import resx from "../../../../resources";

class LanguageEditor extends Component {
    constructor() {
        super();

        this.state = {
            languageDetail: {

            }
        };
    }

    componentWillMount() {
        const {props} = this;
        props.dispatch(SiteSettingsActions.getLanguage(props.portalId, props.languageId));

        if (!props.fullLanguageList) {
            props.dispatch(SiteSettingsActions.getAllLanguages(props.portalId));
        }

        this.setState({
            languageDetail: Object.assign({}, props.languageDetail)
        });
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
        return options.sort(function (a, b) {
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

        if (key === "Name" || key === "Fallback") {
            languageDetail[key] = event.value;
        }
        else {
            languageDetail[key] = typeof (event) === "object" ? event.target.value : event;
        }
        this.setState({
            languageDetail: languageDetail
        });

        props.dispatch(SiteSettingsActions.languageClientModified(languageDetail));
    }

    onSave(event) {
        const {props, state} = this;

        props.onUpdate(state.languageDetail);
    }

    onCancel(event) {
        const {props, state} = this;
        if (props.languageClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(SiteSettingsActions.cancelLanguageClientModified());
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
        if (code) {
            return code;
        }
        else {
            let languages = this.getLanguageOptions();
            return languages.length > 0 ? languages[0].value : "";
        }
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
                    onSelect={this.onSettingChange.bind(this, "Name")}
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
                    enabled={true}
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
                        readOnly={false}
                        />
                </div>
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
                        disabled={!this.props.languageClientModified}
                        type="primary"
                        onClick={this.onSave.bind(this)}>
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
            if (props.id === "add") {
                return this.renderNewForm();
            }
            else {
                return this.renderEditForm();
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
    id: PropTypes.string,
    languageClientModified: PropTypes.bool,
    languageDisplayModes: PropTypes.array,
    openMode: PropTypes.number,
    portalId: PropTypes.number,
    languageDisplayMode: PropTypes.string
};

function mapStateToProps(state) {
    return {
        languageDetail: state.siteSettings.language,
        fallbacks: state.siteSettings.fallbacks,
        fullLanguageList: state.siteSettings.fullLanguageList,
        languageDisplayModes: state.siteSettings.languageDisplayModes,
        languageClientModified: state.siteSettings.languageClientModified
    };
}

export default connect(mapStateToProps)(LanguageEditor);