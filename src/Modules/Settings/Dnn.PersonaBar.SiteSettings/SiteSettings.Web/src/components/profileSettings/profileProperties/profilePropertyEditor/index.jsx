import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import "./style.less";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import Grid from "dnn-grid-system";
import Label from "dnn-label";
import Button from "dnn-button";
import Switch from "dnn-switch";
import DropdownWithError from "dnn-dropdown-with-error";
import MultiLineInput from "dnn-multi-line-input";
import InputGroup from "dnn-input-group";
import Dropdown from "dnn-dropdown";
import {
    siteBehavior as SiteBehaviorActions
} from "../../../../actions";
import util from "../../../../utils";
import resx from "../../../../resources";

const re = /^([0-9]+|[1-9])$/;

class ProfilePropertyEditor extends Component {
    constructor() {
        super();

        this.state = {
            profileProperty: {
                PropertyName: "",
                PropertyCategory: "",
                DataType: "",
                DefaultVisibility: 2,
                Length: 0,
                DefaultValue: "",
                PropertyValidation: ""
            },
            propertyLocalization: undefined,
            error: {
                name: true,
                category: true,
                datatype: true,
                localeName: false,
                length: false
            },
            triedToSubmit: false,
            showFirstPage: true
        };
    }

    componentWillMount() {
        const {props} = this;
        props.dispatch(SiteBehaviorActions.getProfileProperty(props.propertyId, props.portalId));
    }

    componentWillReceiveProps(props) {
        let {state} = this;

        if (!props.profileProperty) {
            return;
        }

        if (props.profileProperty["PropertyName"] === undefined || props.profileProperty["PropertyName"] === "") {
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
        let length = props.profileProperty["Length"];
        if (this.isValidLength(length)) {
            state.error["length"] = false;
        }
        else {
            state.error["length"] = true;
        }

        if (props.propertyLocalization) {
            if (props.propertyLocalization["PropertyName"] === "" || props.propertyLocalization["PropertyName"] === undefined) {
                state.error["localeName"] = true;
            }
            else if (props.propertyLocalization["PropertyName"] !== "" && props.propertyLocalization["PropertyName"] !== undefined) {
                state.error["localeName"] = false;
            }
        }

        this.setState({
            profileProperty: Object.assign({}, props.profileProperty),
            propertyLocalization: Object.assign({}, props.propertyLocalization),
            triedToSubmit: false,
            error: state.error
        });
    }

    isValidLength(val) {
        let {props} = this;
        if (props.profileProperty) {
            if (props.profileProperty["DataType"] !== "" && props.profileProperty["DataType"] !== undefined) {
                if (props.profileProperty["DataType"] === 349) {
                    return re.test(val) && val > 0;
                }
                else {
                    return re.test(val);
                }
            }
            else {
                return re.test(val);
            }
        }
        else {
            return true;
        }
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

        if (this.isValidLength(profileProperty[key]) && key === "Length") {
            state.error["length"] = false;
        }
        else {
            state.error["length"] = true;
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

        props.dispatch(SiteBehaviorActions.profilePropertyClientModified(profileProperty));
    }

    onLanguageChange(event) {
        let {state, props} = this;
        let propertyLocalization = Object.assign({}, state.propertyLocalization);

        propertyLocalization["Language"] = event.value;
        props.dispatch(SiteBehaviorActions.getProfilePropertyLocalization(props.portalId, state.profileProperty.PropertyName, state.profileProperty.PropertyCategory, event.value));

        this.setState({
            propertyLocalization: propertyLocalization,
            triedToSubmit: false,
            error: state.error
        });
    }

    onLocaleSettingChange(key, event) {
        let {state, props} = this;
        let propertyLocalization = Object.assign({}, state.propertyLocalization);

        if (propertyLocalization[key] === "" && key === "PropertyName") {
            state.error["localeName"] = true;
        }
        else if (propertyLocalization[key] !== "" && key === "PropertyName") {
            state.error["localeName"] = false;
        }

        propertyLocalization[key] = typeof (event) === "object" ? event.target.value : event;

        this.setState({
            propertyLocalization: propertyLocalization,
            triedToSubmit: false,
            error: state.error
        });

        props.dispatch(SiteBehaviorActions.propertyLocalizationClientModified(propertyLocalization));
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
                return {
                    label: <div style={{ float: "left", display: "flex" }}>
                        <div className="language-flag">
                            <img src={item.Icon} alt={item.Text} />
                        </div>
                        <div className="language-name">{item.Text}</div>
                    </div>, value: item.Value
                };
            });
        }
        return options;
    }

    getProfileDataTypeOptions() {
        let options = [];
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        if (this.props.dataTypeOptions !== undefined) {
            options = this.props.dataTypeOptions.map((item) => {
                return { label: resx.get(item.Value), value: item.EntryID };
            });
            options.unshift({ label: noneSpecifiedText, value: "" });
        }
        return options;
    }

    getDefaultVisibility() {
        const {props, state} = this;
        if (!state.profileProperty) {
            if (props.id === "add") {
                return 2;
            }
        }
        else {
            return state.profileProperty.DefaultVisibility;
        }
    }

    onNext() {
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });

        if (state.error.name || state.error.category || state.error.datatype || state.error.length) {
            return;
        }

        if (props.profilePropertyClientModified) {
            if (props.id === "add") {
                const property = Object.assign({}, state.profileProperty);
                property["PortalId"] = props.portalId;
                props.dispatch(SiteBehaviorActions.addProfileProperty(property, () => {
                    util.utilities.notify(resx.get("SettingsUpdateSuccess"));
                    props.dispatch(SiteBehaviorActions.getProfileProperties());
                    props.dispatch(SiteBehaviorActions.getProfilePropertyLocalization(props.portalId, state.profileProperty.PropertyName, state.profileProperty.PropertyCategory, props.cultureCode));
                    this.setState({
                        showFirstPage: false
                    });
                }, () => {
                    util.utilities.notifyError(resx.get("SettingsError"));
                }));
            }
            else {
                props.dispatch(SiteBehaviorActions.updateProfileProperty(state.profileProperty, () => {
                    util.utilities.notify(resx.get("SettingsUpdateSuccess"));
                    props.dispatch(SiteBehaviorActions.getProfileProperties());
                    props.dispatch(SiteBehaviorActions.getProfilePropertyLocalization(props.portalId, state.profileProperty.PropertyName, state.profileProperty.PropertyCategory, props.cultureCode));
                    this.setState({
                        showFirstPage: false
                    });
                }, () => {
                    util.utilities.notifyError(resx.get("SettingsError"));
                }));
            }
        }
        else {
            props.dispatch(SiteBehaviorActions.getProfilePropertyLocalization(props.portalId,
                state.profileProperty.PropertyName, state.profileProperty.PropertyCategory, props.cultureCode, (data) => {
                    this.setState({
                        propertyLocalization: data.PropertyLocalization,
                        showFirstPage: false
                    });
                }));
        }
    }

    onSave() {
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });
        if (state.error.localeName) {
            return;
        }

        let localization = {
            PortalId: props.portalId,
            PropertyName: state.profileProperty.PropertyName,
            PropertyCategory: state.profileProperty.PropertyCategory,
            Language: state.propertyLocalization.Language,
            PropertyNameString: state.propertyLocalization.PropertyName,
            PropertyHelpString: state.propertyLocalization.PropertyHelp,
            PropertyRequiredString: state.propertyLocalization.PropertyRequired,
            PropertyValidationString: state.propertyLocalization.PropertyValidation,
            CategoryNameString: state.propertyLocalization.CategoryName
        };

        props.dispatch(SiteBehaviorActions.updateProfilePropertyLocalization(localization, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
            props.Collapse();
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    onCancel() {
        const {props} = this;
        if (props.profilePropertyClientModified) {
            util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
                props.dispatch(SiteBehaviorActions.cancelProfilePropertyClientModified());
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
                        value={this.state.profileProperty ? this.state.profileProperty.PropertyName : ""}
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
                        value={this.state.profileProperty ? this.state.profileProperty.PropertyCategory : ""}
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
                        value={this.state.profileProperty ? this.state.profileProperty.DefaultValue : ""}
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
                            value={this.state.profileProperty ? this.state.profileProperty.Required : false}
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
                            value={this.state.profileProperty ? this.state.profileProperty.Visible : false}
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
                        value={this.getDefaultVisibility()}
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
                        value={this.state.profileProperty ? this.state.profileProperty.DataType : ""}
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
                        error={this.state.error.length && this.state.triedToSubmit}
                        errorMessage={resx.get("RequiredTextBox")}
                        value={this.state.profileProperty ? this.state.profileProperty.Length : 0}
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
                        value={this.state.profileProperty ? this.state.profileProperty.ValidationExpression : ""}
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
                            value={this.state.profileProperty ? this.state.profileProperty.ReadOnly : false}
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

            const columnThree = <div className="left-column2">
                <InputGroup>
                    <Label
                        tooltipMessage={resx.get("plPropertyName.Help")}
                        label={resx.get("plPropertyName") + "*"}
                    />
                    {this.state.propertyLocalization &&
                        <SingleLineInputWithError
                            inputStyle={{ margin: "0" }}
                            withLabel={false}
                            error={this.state.error.localeName && this.state.triedToSubmit}
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
            const columnFour = <div className="right-column2">
                <InputGroup style={{ paddingTop: "10px" }}>
                    <Label
                        tooltipMessage={resx.get("plPropertyHelp.Help")}
                        label={resx.get("plPropertyHelp")}
                    />
                    {this.state.propertyLocalization &&
                        <MultiLineInput
                            value={this.state.propertyLocalization.PropertyHelp}
                            onChange={this.onLocaleSettingChange.bind(this, "PropertyHelp")}
                            style={{ padding: "8px 16px 75px" }}
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
                                    value={this.state.propertyLocalization.Language}
                                    onSelect={this.onLanguageChange.bind(this)}
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
                                    disabled={!this.props.propertyLocalizationClientModified}
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
    id: PropTypes.string,
    profilePropertyClientModified: PropTypes.bool,
    propertyLocalizationClientModified: PropTypes.bool,
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
    return {
        profileProperty: state.siteBehavior.profileProperty,
        propertyLocalization: state.siteBehavior.propertyLocalization,
        userVisibilityOptions: state.siteBehavior.userVisibilityOptions,
        dataTypeOptions: state.siteBehavior.dataTypeOptions,
        languageOptions: state.siteBehavior.languageOptions,
        profilePropertyClientModified: state.siteBehavior.profilePropertyClientModified,
        propertyLocalizationClientModified: state.siteBehavior.propertyLocalizationClientModified
    };
}

export default connect(mapStateToProps)(ProfilePropertyEditor);