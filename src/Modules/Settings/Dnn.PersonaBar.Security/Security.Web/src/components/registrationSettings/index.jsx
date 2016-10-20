import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import {
    pagination as PaginationActions,
    security as SecurityActions
} from "../../actions";
import InputGroup from "dnn-input-group";
import SingleLineInput from "dnn-single-line-input";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import SearchableTags from "dnn-searchable-tags";
import Dropdown from "dnn-dropdown";
import Switch from "dnn-switch";
import RadioButtons from "dnn-radio-buttons";
import Label from "dnn-label";
import Button from "dnn-button";
import PagePicker from "dnn-page-picker";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

const svgIcon = require(`!raw!./../svg/global.svg`);

class RegistrationSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            registrationSettings: undefined,
            triedToSubmit: false,
            error: {
                registrationFields: ""
            },
            resetPagePicker: false
        };
    }

    componentWillMount() {
        const {props} = this;
        if (props.registrationSettings) {
            this.setState({
                registrationSettings: props.registrationSettings
            });
            return;
        }
        props.dispatch(SecurityActions.getRegistrationSettings((data) => {
            let registrationSettings = Object.assign({}, data.Results.Settings);
            this.setState({
                registrationSettings
            });
        }));
    }

    componentWillReceiveProps(props) {
        let {state} = this;

        this.setState({
            registrationSettings: Object.assign({}, props.registrationSettings),
            triedToSubmit: false
        });
    }

    onSettingChange(key, event) {
        let {state, props} = this;

        if(state.resetPagePicker){
            return;
        }
        
        let registrationSettings = Object.assign({}, state.registrationSettings);

        if (key === "UserRegistration" || key === "RegistrationFormType") {
            registrationSettings[key] = parseInt(event);
        }
        else if (key === "RedirectAfterRegistrationTabId") {
            registrationSettings[key] = event;
        }
        else {
            registrationSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }
        this.setState({
            registrationSettings: registrationSettings,
            triedToSubmit: false
        });

        props.dispatch(SecurityActions.registrationSettingsClientModified(registrationSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;
        this.setState({
            triedToSubmit: true
        });

        props.dispatch(SecurityActions.updateRegistrationSettings(state.registrationSettings, (data) => {
            util.utilities.notify(resx.get("RegistrationSettingsUpdateSuccess"));
        }, (error) => {
            util.utilities.notifyError(resx.get("RegistrationSettingsError"));
            const errorMessage = JSON.parse(error.responseText);
            state.error["registrationFields"] = errorMessage.Message;
            this.setState({
                error: state.error
            });
        }));
    }

    onCancel(event) {
        const {props, state} = this;
        util.utilities.confirm(resx.get("RegistrationSettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SecurityActions.getRegistrationSettings((data) => {
                let registrationSettings = Object.assign({}, data.Results.Settings);
                this.setState({
                    registrationSettings
                }, () => {
                    this.setState({
                        resetPagePicker: true
                    }, () => {
                        this.setState({ resetPagePicker: false });
                    });
                });
            }));
        });
    }

    keyValuePairsToOptions(keyValuePairs) {
        let options = [];
        if (keyValuePairs !== undefined) {
            options = keyValuePairs.map((item) => {
                return { label: item.Key, value: item.Value };
            });
        }
        return options;
    }

    isCustomFormType() {
        const {props, state} = this;
        if (state.registrationSettings != undefined && state.registrationSettings.RegistrationFormType === 1) {
            return true;
        }
        return false;
    }

    getRegistrationFields(fields) {
        let fieldList = [];
        if (fields !== undefined) {
            fieldList = fields.split(',').map((item) => {
                return { id: item, name: item };
            });
        }
        return fieldList;
    }

    onUpdateTags(event) {
        let {state, props} = this;
        let registrationSettings = Object.assign({}, state.registrationSettings);
        let fields = event.map((field) => { return field.name; }).join(",");
        registrationSettings["RegistrationFields"] = fields;
        this.setState({
            registrationSettings: registrationSettings,
            triedToSubmit: false
        });
        props.dispatch(SecurityActions.registrationSettingsClientModified(registrationSettings));
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        const noneSpecifiedText = "<" + resx.get("NoneSpecified") + ">";
        const RedirectAfterRegistrationParameters = {
            portalId: -2,
            cultureCode: "",
            isMultiLanguage: false,
            excludeAdminTabs: false,
            disabledNotSelectable: false,
            roles: "1;-1",
            sortOrder: 0
        };
        if (state.registrationSettings) {
            return (
                <div className={styles.registrationSettings}>
                    <InputGroup>
                        <div className="registrationSettings-row-options">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plUserRegistration.Help") }
                                label={resx.get("plUserRegistration") }
                                />
                            <RadioButtons
                                onChange={this.onSettingChange.bind(this, "UserRegistration") }
                                options={this.keyValuePairsToOptions(props.userRegistrationOptions) }
                                buttonGroup="registrationType"
                                value={state.registrationSettings.UserRegistration}/>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-options">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("registrationFormTypeLabel.Help") }
                                label={resx.get("registrationFormTypeLabel") }
                                />
                            <RadioButtons
                                onChange={this.onSettingChange.bind(this, "RegistrationFormType") }
                                options={this.keyValuePairsToOptions(props.registrationFormTypeOptions) }
                                buttonGroup="formType"
                                value={state.registrationSettings.RegistrationFormType}/>

                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-input">
                            <Label
                                tooltipMessage={resx.get("Security_DisplayNameFormat.Help") }
                                label={resx.get("Security_DisplayNameFormat") }
                                />
                            <SingleLineInput
                                value={state.registrationSettings.DisplayNameFormat}
                                onChange={this.onSettingChange.bind(this, "DisplayNameFormat") }
                                />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-input">
                            <Label
                                tooltipMessage={resx.get("Security_UserNameValidation.Help") }
                                label={resx.get("Security_UserNameValidation") }
                                />
                            <SingleLineInput
                                value={state.registrationSettings.UserNameValidation}
                                onChange={this.onSettingChange.bind(this, "UserNameValidation") }
                                />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-input">
                            <Label
                                tooltipMessage={resx.get("Security_EmailValidation.Help") }
                                label={resx.get("Security_EmailValidation") }
                                />
                            <SingleLineInput
                                value={state.registrationSettings.EmailAddressValidation}
                                onChange={this.onSettingChange.bind(this, "EmailAddressValidation") }
                                />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-input">
                            <Label
                                tooltipMessage={resx.get("Registration_ExcludeTerms.Help") }
                                label={resx.get("Registration_ExcludeTerms") }
                                />
                            <SingleLineInput
                                value={state.registrationSettings.ExcludedTerms}
                                onChange={this.onSettingChange.bind(this, "ExcludedTerms") }
                                />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-input">
                            <Label
                                tooltipMessage={resx.get("Redirect_AfterRegistration.Help") }
                                label={resx.get("Redirect_AfterRegistration") }
                                />
                            <PagePicker
                                serviceFramework={util.utilities.sf}
                                style={{ width: "100%", zIndex: 1 }}
                                selectedTabId={state.registrationSettings.RedirectAfterRegistrationTabId}
                                OnSelect={this.onSettingChange.bind(this, "RedirectAfterRegistrationTabId") }
                                defaultLabel={state.registrationSettings.RedirectAfterRegistrationTabName !== "" ? state.registrationSettings.RedirectAfterRegistrationTabName : noneSpecifiedText}
                                noneSpecifiedText={noneSpecifiedText}
                                CountText={"{0} Results"}
                                PortalTabsParameters={RedirectAfterRegistrationParameters}
                                ResetSelected={state.resetPagePicker}
                                />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plEnableRegisterNotification.Help") }
                                label={resx.get("plEnableRegisterNotification") }
                                />
                            <Switch
                                labelHidden={true}
                                value={state.registrationSettings.EnableRegisterNotification }
                                onChange={this.onSettingChange.bind(this, "EnableRegisterNotification") }
                                />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Registration_UseAuthProviders.Help") }
                                label={resx.get("Registration_UseAuthProviders") }
                                />
                            <Switch
                                labelHidden={true}
                                value={state.registrationSettings.UseAuthenticationProviders }
                                onChange={this.onSettingChange.bind(this, "UseAuthenticationProviders") }
                                />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Registration_UseProfanityFilter.Help") }
                                label={resx.get("Registration_UseProfanityFilter") }
                                />
                            <Switch
                                labelHidden={true}
                                value={state.registrationSettings.UseProfanityFilter }
                                onChange={this.onSettingChange.bind(this, "UseProfanityFilter") }
                                />
                        </div>
                    </InputGroup>
                    {!this.isCustomFormType() &&
                        <InputGroup>
                            <div className="registrationSettings-row_switch">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("Registration_UseEmailAsUserName.Help") }
                                    label={resx.get("Registration_UseEmailAsUserName") }
                                    />
                                <Switch
                                    labelHidden={true}
                                    value={state.registrationSettings.UseEmailAsUsername }
                                    onChange={this.onSettingChange.bind(this, "UseEmailAsUsername") }
                                    />
                            </div>
                        </InputGroup>
                    }
                    {this.isCustomFormType() &&
                        <InputGroup>
                            <div className="registrationSettings-row-input">
                                <Label
                                    tooltipMessage={resx.get("registrationFieldsLabel.Help") }
                                    label={resx.get("registrationFieldsLabel") }
                                    />
                                <SearchableTags
                                    utils={util}
                                    tags={this.getRegistrationFields(state.registrationSettings.RegistrationFields) }
                                    onUpdateTags={this.onUpdateTags.bind(this) }
                                    error={this.state.error.registrationFields !== ""}
                                    errorMessage={this.state.error.registrationFields }
                                    />
                            </div>
                        </InputGroup>
                    }
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Registration_RequireUniqueDisplayName.Help") }
                                label={resx.get("Registration_RequireUniqueDisplayName") }
                                />
                            <Switch
                                labelHidden={true}
                                value={state.registrationSettings.RequireUniqueDisplayName }
                                onChange={this.onSettingChange.bind(this, "RequireUniqueDisplayName") }
                                />
                        </div>
                    </InputGroup>
                    {!this.isCustomFormType() &&
                        <InputGroup>
                            <div className="registrationSettings-row_switch">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("Registration_RandomPassword.Help") }
                                    label={resx.get("Registration_RandomPassword") }
                                    />
                                <Switch
                                    labelHidden={true}
                                    value={state.registrationSettings.UseRandomPassword }
                                    onChange={this.onSettingChange.bind(this, "UseRandomPassword") }
                                    />
                            </div>
                        </InputGroup>
                    }
                    {!this.isCustomFormType() &&
                        <InputGroup>
                            <div className="registrationSettings-row_switch">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("Registration_RequireConfirmPassword.Help") }
                                    label={resx.get("Registration_RequireConfirmPassword") }
                                    />
                                <Switch
                                    labelHidden={true}
                                    value={state.registrationSettings.RequirePasswordConfirmation }
                                    onChange={this.onSettingChange.bind(this, "RequirePasswordConfirmation") }
                                    />
                            </div>
                        </InputGroup>
                    }
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Security_RequireValidProfile.Help") }
                                label={resx.get("Security_RequireValidProfile") }
                                />
                            <Switch
                                labelHidden={true}
                                value={state.registrationSettings.RequireValidProfile }
                                onChange={this.onSettingChange.bind(this, "RequireValidProfile") }
                                />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Security_CaptchaRegister.Help") }
                                label={resx.get("Security_CaptchaRegister") }
                                />
                            <Switch
                                labelHidden={true}
                                value={state.registrationSettings.UseCaptchaRegister }
                                onChange={this.onSettingChange.bind(this, "UseCaptchaRegister") }
                                />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("RequiresUniqueEmail.Help") }
                                label={resx.get("RequiresUniqueEmail") }
                                />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.RequiresUniqueEmail }
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("PasswordFormat.Help") }
                                label={resx.get("PasswordFormat") }
                                />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.PasswordFormat }
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("PasswordRetrievalEnabled.Help") }
                                label={resx.get("PasswordRetrievalEnabled") }
                                />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.PasswordRetrievalEnabled }
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("PasswordResetEnabledTitle.Help") }
                                label={resx.get("PasswordResetEnabledTitle") }
                                />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.PasswordResetEnabled }
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("PasswordResetEnabledTitle.Help") }
                                label={resx.get("PasswordResetEnabledTitle") }
                                />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.MinPasswordLength }
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("MinNonAlphanumericCharactersTitle.Help") }
                                label={resx.get("MinNonAlphanumericCharactersTitle") }
                                />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.MinNonAlphanumericCharacters }
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("RequiresQuestionAndAnswerTitle.Help") }
                                label={resx.get("RequiresQuestionAndAnswerTitle") }
                                />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.RequiresQuestionAndAnswer }
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("PasswordStrengthRegularExpressionTitle.Help") }
                                label={resx.get("PasswordStrengthRegularExpressionTitle") }
                                />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.PasswordStrengthRegularExpression }
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("MaxInvalidPasswordAttemptsTitle.Help") }
                                label={resx.get("MaxInvalidPasswordAttemptsTitle") }
                                />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.MaxInvalidPasswordAttempts }
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("PasswordAttemptWindowTitle.Help") }
                                label={resx.get("PasswordAttemptWindowTitle") }
                                />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.PasswordAttemptWindow }
                            </div>
                        </div>
                    </InputGroup>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.registrationSettingsClientModified}
                            type="secondary"
                            onClick={this.onCancel.bind(this) }>
                            {resx.get("Cancel") }
                        </Button>
                        <Button
                            disabled={!this.props.registrationSettingsClientModified}
                            type="primary"
                            onClick={this.onUpdate.bind(this) }>
                            {resx.get("Save") }
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div/>;
    }
}

RegistrationSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    registrationSettings: PropTypes.object,
    userRegistrationOptions: PropTypes.array,
    registrationFormTypeOptions: PropTypes.array,
    registrationSettingsClientModified: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        registrationSettings: state.security.registrationSettings,
        userRegistrationOptions: state.security.userRegistrationOptions,
        registrationFormTypeOptions: state.security.registrationFormTypeOptions,
        registrationSettingsClientModified: state.security.registrationSettingsClientModified
    };
}

export default connect(mapStateToProps)(RegistrationSettingsPanelBody);