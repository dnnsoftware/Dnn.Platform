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
import InputGroup from "dnn-input-group";
import Input from "dnn-single-line-input";
import Dropdown from "dnn-dropdown";
import {
    siteSettings as SiteSettingsActions
} from "../../../actions";
import util from "../../../utils";
import resx from "../../../resources";

const re = /^[1-9][0-9]?[0-9]?[0-9]?[0-9]?[0-9]?$/;
let retainHistoryNumOptions = [];

class ProfilePropertyEditor extends Component {
    constructor() {
        super();

        this.state = {
            profileProperty: undefined,
            propertyLocalization: undefined,
            error: {
                name: true,
                category: true,
                datatype: true,
                localename: true
            },
            triedToSubmit: false,
            showFirstPage: true
        };
    }

    componentWillMount() {
        const {props} = this;

        if (props.propertyId) {
            props.dispatch(SiteSettingsActions.getProfileProperty(props.propertyId));
        }
        else {
            this.setState({
                profileProperty: {}
            });
        }
    }

    componentWillReceiveProps(props) {
        let {state} = this;
        if (props.profileProperty["PropertyName"] === "" || props.profileProperty["PropertyName"] === undefined) {
            state.error["name"] = true;
        }
        else if (props.profileProperty["PropertyName"] !== "" && props.profileProperty["PropertyName"] !== undefined) {
            state.error["name"] = false;
        }
        if (props.profileProperty["PropertyCategory"] === "" || props.profileProperty["PropertyCategory"] === undefined) {
            state.error["category"] = true;
        }
        else if (props.profileProperty["PropertyCategory"] !== "" && props.profileProperty["PropertyCategory"] !== undefined) {
            state.error["category"] = false;
        }
        if (props.profileProperty["DataType"] === "" || props.profileProperty["DataType"] === undefined) {
            state.error["datatype"] = true;
        }
        else if (props.profileProperty["DataType"] !== "" && props.profileProperty["DataType"] !== undefined) {
            state.error["datatype"] = false;
        }

        if (props.propertyLocalization) {
            if (props.propertyLocalization["PropertyName"] === "" || props.propertyLocalization["PropertyName"] === undefined) {
                state.error["localename"] = true;
            }
            else if (props.propertyLocalization["PropertyName"] !== "" && props.propertyLocalization["PropertyName"] !== undefined) {
                state.error["localename"] = false;
            }
        }

        this.setState({
            profileProperty: Object.assign({}, props.profileProperty),
            propertyLocalization: Object.assign({}, props.propertyLocalization),
            triedToSubmit: false,
            error: state.error
        });
    }

    onSettingChange(key, event) {
        let {state, props} = this;
        let profileProperty = Object.assign({}, state.profileProperty);

        if (profileProperty[key] === "" && key === "PropertyName") {
            state.error["name"] = true;
        }
        else if (profileProperty[key] !== "" && key === "PropertyName") {
            state.error["name"] = false;
        }
        if (profileProperty[key] === "" && key === "PropertyCategory") {
            state.error["category"] = true;
        }
        else if (profileProperty[key] !== "" && key === "PropertyCategory") {
            state.error["category"] = false;
        }
        if (profileProperty[key] === "" && key === "DataType") {
            state.error["datatype"] = true;
        }
        else if (profileProperty[key] !== "" && key === "DataType") {
            state.error["datatype"] = false;
        }

        if (key === "DefaultVisibility" || key === "DataType") {
            profileProperty[key] = event.value;
        }
        else {
            profileProperty[key] = typeof (event) === "object" ? event.target.value : event;
        }

        this.setState({
            profileProperty: profileProperty,
            triedToSubmit: false,
            error: state.error
        });

        props.dispatch(SiteSettingsActions.profilePropertyClientModified(profileProperty));
    }

    onLocaleSettingChange(key, event) {
        let {state, props} = this;
        let propertyLocalization = Object.assign({}, state.propertyLocalization);

        if (propertyLocalization[key] === "" && key === "PropertyName") {
            state.error["localename"] = true;
        }
        else if (propertyLocalization[key] !== "" && key === "PropertyName") {
            state.error["localename"] = false;
        }

        propertyLocalization[key] = typeof (event) === "object" ? event.target.value : event;

        this.setState({
            propertyLocalization: propertyLocalization,
            triedToSubmit: false,
            error: state.error
        });

        props.dispatch(SiteSettingsActions.propertyLocalizationClientModified(propertyLocalization));
    }

    getProfileVisibilityOptions() {
        let options = [];
        if (this.props.userVisibilityOptions !== undefined) {
            options = this.props.userVisibilityOptions.map((item) => {
                return { label: resx.get(item.label), value: item.value };
            });
        }
        return options;
    }

    getProfileLanguageOptions() {
        let options = [];
        if (this.props.languageOptions !== undefined) {
            options = this.props.languageOptions.map((item) => {
                return { label: item.Text, value: item.Value };
            });
        }
        return options;
    }

    getProfileDataTypeOptions() {
        let options = [];
        if (this.props.dataTypeOptions !== undefined) {
            options = this.props.dataTypeOptions.map((item) => {
                return { label: resx.get(item.Value), value: item.EntryID };
            });
        }
        return options;
    }

    onNext(event) {
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.name) {
            return;
        }

        if (props.profilePropertyClientModified) {
            if (props.id === "add") {
                props.dispatch(SiteSettingsActions.addProfileProperty(state.profileProperty, (data) => {
                    util.utilities.notify(resx.get("SettingsUpdateSuccess"));
                    props.dispatch(SiteSettingsActions.getProfilePropertyLocalization(state.profileProperty.PropertyName, state.profileProperty.PropertyCategory, ""));
                    this.setState({
                        showFirstPage: false
                    });
                }, (error) => {
                    util.utilities.notifyError(resx.get("SettingsError"));
                }));
            }
            else {
                props.dispatch(SiteSettingsActions.updateProfileProperty(state.profileProperty, (data) => {
                    util.utilities.notify(resx.get("SettingsUpdateSuccess"));
                    props.dispatch(SiteSettingsActions.getProfilePropertyLocalization(state.profileProperty.PropertyName, state.profileProperty.PropertyCategory, ""));
                    this.setState({
                        showFirstPage: false
                    });
                }, (error) => {
                    util.utilities.notifyError(resx.get("SettingsError"));
                }));
            }
        }
        else {
            props.dispatch(SiteSettingsActions.getProfilePropertyLocalization(state.profileProperty.PropertyName, state.profileProperty.PropertyCategory, ""));
            this.setState({
                showFirstPage: false
            });
        }
    }

    onSave(event) {
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.name) {
            return;
        }

    }

    onCancel(event) {
        const {props, state} = this;
        if (props.profilePropertyClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(SiteSettingsActions.cancelProfilePropertyClientModified());
                props.Collapse();
            });
        }
        else {
            props.Collapse();
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        /* eslint-disable react/no-danger */
        if (this.state.profileProperty !== undefined || this.props.id === "add") {
            const columnOne = <div className="left-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("ProfilePropertyDefinition_PropertyName.Help")}
                        label={resx.get("ProfilePropertyDefinition_PropertyName") + "*"}
                        />
                    <SingleLineInputWithError
                        enabled={this.props.id === "add" ? true : false}
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={this.state.error.name && this.state.triedToSubmit}
                        errorMessage={resx.get("ProfilePropertyDefinition_PropertyName.Required")}
                        value={this.state.profileProperty.PropertyName}
                        onChange={this.onSettingChange.bind(this, "PropertyName")}
                        />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("ProfilePropertyDefinition_PropertyCategory.Help")}
                        label={resx.get("ProfilePropertyDefinition_PropertyCategory") + "*"}
                        />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={this.state.error.category && this.state.triedToSubmit}
                        errorMessage={resx.get("ProfilePropertyDefinition_PropertyCategory.Required")}
                        value={this.state.profileProperty.PropertyCategory}
                        onChange={this.onSettingChange.bind(this, "PropertyCategory")}
                        />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("ProfilePropertyDefinition_DefaultValue.Help")}
                        label={resx.get("ProfilePropertyDefinition_DefaultValue")}
                        />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={false}
                        value={this.state.profileProperty.DefaultValue}
                        onChange={this.onSettingChange.bind(this, "DefaultValue")}
                        />
                </InputGroup>
                <InputGroup>
                    <div className="profileProperty-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("ProfilePropertyDefinition_Required.Help")}
                            label={resx.get("ProfilePropertyDefinition_Required")}
                            />
                        <Switch
                            labelHidden={true}
                            value={this.state.profileProperty.Required}
                            onChange={this.onSettingChange.bind(this, "Required")}
                            />
                    </div>
                </InputGroup>
                <InputGroup>
                    <div className="profileProperty-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("ProfilePropertyDefinition_Visible.Help")}
                            label={resx.get("ProfilePropertyDefinition_Visible")}
                            />
                        <Switch
                            labelHidden={true}
                            value={this.state.profileProperty.Visible}
                            onChange={this.onSettingChange.bind(this, "Visible")}
                            />
                    </div>
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("ProfilePropertyDefinition_DefaultVisibility.Help")}
                        label={resx.get("ProfilePropertyDefinition_DefaultVisibility")}
                        />
                    <Dropdown
                        options={this.getProfileVisibilityOptions()}
                        value={this.props.id === "add" ? 2 : this.state.profileProperty.DefaultVisibility}
                        onSelect={this.onSettingChange.bind(this, "DefaultVisibility")}
                        />
                </InputGroup>
            </div>;
            const columnTwo = <div className="right-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("ProfilePropertyDefinition_DataType.Help")}
                        label={resx.get("ProfilePropertyDefinition_DataType") + "*"}
                        />
                    <DropdownWithError
                        options={this.getProfileDataTypeOptions()}
                        value={this.state.profileProperty.DataType}
                        onSelect={this.onSettingChange.bind(this, "DataType")}
                        error={this.state.error.datatype && this.state.triedToSubmit}
                        errorMessage={resx.get("ProfilePropertyDefinition_DataType.Required")}
                        />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("ProfilePropertyDefinition_Length.Help")}
                        label={resx.get("ProfilePropertyDefinition_Length")}
                        />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={false}
                        value={this.state.profileProperty.Length}
                        onChange={this.onSettingChange.bind(this, "Length")}
                        />
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("ProfilePropertyDefinition_ValidationExpression.Help")}
                        label={resx.get("ProfilePropertyDefinition_ValidationExpression")}
                        />
                    <SingleLineInputWithError
                        inputStyle={{ margin: "0" }}
                        withLabel={false}
                        error={false}
                        value={this.state.profileProperty.ValidationExpression}
                        onChange={this.onSettingChange.bind(this, "ValidationExpression")}
                        />
                </InputGroup>
                <InputGroup>
                    <div className="profileProperty-row_switch">
                        <Label
                            labelType="inline"
                            tooltipMessage={resx.get("ProfilePropertyDefinition_ReadOnly.Help")}
                            label={resx.get("ProfilePropertyDefinition_ReadOnly")}
                            />
                        <Switch
                            labelHidden={true}
                            value={this.state.profileProperty.ReadOnly}
                            onChange={this.onSettingChange.bind(this, "ReadOnly")}
                            />
                    </div>
                </InputGroup>
                <InputGroup>
                    <Label
                        style={{ width: "90%" }}
                        tooltipMessage={resx.get("ProfilePropertyDefinition_ViewOrder.Help")}
                        label={resx.get("ProfilePropertyDefinition_ViewOrder")}
                        />
                    <div style={{ float: "right", marginTop: "2px" }}>{this.props.id === "add" ? 0 : this.state.profileProperty.ViewOrder}</div>
                </InputGroup>
            </div>;

            const columnThree = <div className="left-column">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plPropertyName.Help")}
                        label={resx.get("plPropertyName") + "*"}
                        />
                    {this.state.propertyLocalization &&
                        <SingleLineInputWithError
                            inputStyle={{ margin: "0" }}
                            withLabel={false}
                            error={this.state.error.localename && this.state.triedToSubmit}
                            errorMessage={resx.get("valPropertyName.ErrorMessage")}
                            value={this.state.propertyLocalization.PropertyName}
                            onChange={this.onLocaleSettingChange.bind(this, "PropertyName")}
                            />
                    }
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plCategoryName.Help")}
                        label={resx.get("plCategoryName")}
                        />
                    {this.state.propertyLocalization &&
                        <SingleLineInputWithError
                            inputStyle={{ margin: "0" }}
                            withLabel={false}
                            error={false}
                            value={this.state.propertyLocalization.CategoryName}
                            onChange={this.onLocaleSettingChange.bind(this, "CategoryName")}
                            />
                    }
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plPropertyValidation.Help")}
                        label={resx.get("plPropertyValidation")}
                        />
                    {this.state.propertyLocalization &&
                        <SingleLineInputWithError
                            inputStyle={{ margin: "0" }}
                            withLabel={false}
                            error={false}
                            value={this.state.propertyLocalization.PropertyValidation}
                            onChange={this.onLocaleSettingChange.bind(this, "PropertyValidation")}
                            />
                    }
                </InputGroup>
            </div>;
            const columnFour = <div className="right-column">
                <InputGroup style={{ paddingTop: "10px" }}>
                    <Label
                        tooltipMessage={resx.get("plPropertyHelp.Help")}
                        label={resx.get("plPropertyHelp")}
                        />
                    {this.state.propertyLocalization &&
                        <MultiLineInput
                            value={this.state.propertyLocalization.PropertyHelp}
                            onChange={this.onLocaleSettingChange.bind(this, "PropertyHelp")}
                            />
                    }
                </InputGroup>
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plPropertyRequired.Help")}
                        label={resx.get("plPropertyRequired")}
                        />
                    {this.state.propertyLocalization &&
                        <SingleLineInputWithError
                            inputStyle={{ margin: "0" }}
                            withLabel={false}
                            error={false}
                            value={this.state.propertyLocalization.PropertyRequired}
                            onChange={this.onLocaleSettingChange.bind(this, "PropertyRequired")}
                            />
                    }
                </InputGroup>
            </div>;

            return (
                <div className="property-editor">
                    <div className={this.state.showFirstPage ? "property-editor-page" : "property-editor-page-hidden"}>
                        <Grid children={[columnOne, columnTwo]} numberOfColumns={2} />
                        <div className="editor-buttons-box">
                            <Button
                                type="secondary"
                                onClick={this.onCancel.bind(this)}>
                                {resx.get("Cancel")}
                            </Button>
                            <Button
                                type="primary"
                                onClick={this.onNext.bind(this)}>
                                {resx.get("Next")}
                            </Button>
                        </div>
                    </div>
                    {this.state.propertyLocalization &&
                        <div className={this.state.showFirstPage ? "property-editor-page-hidden" : "property-editor-page"}>
                            <div className="topMessage">{resx.get("Localization.Help")}</div>
                            <InputGroup>
                                <Label
                                    tooltipMessage={resx.get("plLocales.Help")}
                                    label={resx.get("plLocales")}
                                    />
                                <Dropdown
                                    options={this.getProfileLanguageOptions()}
                                    value={this.props.id === "add" ? "en-US" : this.state.profileProperty.DefaultVisibility}
                                    onSelect={this.onSettingChange.bind(this, "DefaultVisibility")}
                                    />
                            </InputGroup>
                            <Grid children={[columnThree, columnFour]} numberOfColumns={2} />
                            <div className="editor-buttons-box">
                                <Button
                                    type="secondary"
                                    onClick={this.onCancel.bind(this)}>
                                    {resx.get("Cancel")}
                                </Button>
                                <Button
                                    disabled={!this.props.profilePropertyClientModified}
                                    type="primary"
                                    onClick={this.onSave.bind(this)}>
                                    {resx.get("Save")}
                                </Button>
                            </div>
                        </div>
                    }
                </div>
            );
        }
        else return <div />;
    }
}

ProfilePropertyEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    profileProperty: PropTypes.object,
    propertyLocalization: PropTypes.object,
    userVisibilityOptions: PropTypes.array,
    dataTypeOptions: PropTypes.array,
    languageOptions: PropTypes.array,
    propertyId: PropTypes.number,
    Collapse: PropTypes.func,
    onUpdate: PropTypes.func,
    id: PropTypes.string,
    profilePropertyClientModified: PropTypes.bool,
    propertyLocalizationClientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        profileProperty: state.siteSettings.profileProperty,
        propertyLocalization: state.siteSettings.propertyLocalization,
        userVisibilityOptions: state.siteSettings.userVisibilityOptions,
        dataTypeOptions: state.siteSettings.dataTypeOptions,
        languageOptions: state.siteSettings.languageOptions,
        profilePropertyClientModified: state.siteSettings.profilePropertyClientModified,
        propertyLocalizationClientModified: state.siteSettings.propertyLocalizationClientModified
    };
}

export default connect(mapStateToProps)(ProfilePropertyEditor);