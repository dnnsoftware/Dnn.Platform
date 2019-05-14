import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import { 
    InputGroup,
    SingleLineInputWithError,
    SearchableTags,
    Switch,
    RadioButtons,
    Label,
    Button,
    PagePicker
} from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

let canEdit = false;
/*eslint-disable eqeqeq*/
class RegistrationSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            registrationSettings: undefined,
            triedToSubmit: false,
            error: {
                registrationFields: ""
            }
        };
        canEdit = util.settings.isHost || util.settings.isAdmin || util.settings.permissions.REGISTRATION_SETTINGS_EDIT;
    }

    UNSAFE_componentWillMount() {
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

    UNSAFE_componentWillReceiveProps(props) {
        this.setState({
            registrationSettings: Object.assign({}, props.registrationSettings),
            triedToSubmit: false
        });
    }

    onSettingChange(key, event) {
        let {state, props} = this;

        let registrationSettings = Object.assign({}, state.registrationSettings);

        if (key === "UserRegistration" || key === "RegistrationFormType") {
            registrationSettings[key] = parseInt(event);
        }
        else if (key === "RedirectAfterRegistrationTabId") {
            if (registrationSettings[key] !== parseInt(event)) {
                registrationSettings[key] = event;
            }
            else {
                return;
            }
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

        props.dispatch(SecurityActions.updateRegistrationSettings(state.registrationSettings, () => {
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

    onCancel() {
        const {props} = this;
        util.utilities.confirm(resx.get("RegistrationSettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SecurityActions.getRegistrationSettings((data) => {
                let registrationSettings = Object.assign({}, data.Results.Settings);
                this.setState({
                    registrationSettings
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
        const {state} = this;
        if (state.registrationSettings != undefined && state.registrationSettings.RegistrationFormType === 1) {
            return true;
        }
        return false;
    }

    getRegistrationFields(fields) {
        let fieldList = [];
        if (fields) {
            fieldList = fields.split(",").map((item) => {
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
                                label={resx.get("plUserRegistration") } />
                            <RadioButtons
                                onChange={this.onSettingChange.bind(this, "UserRegistration") }
                                options={this.keyValuePairsToOptions(props.userRegistrationOptions) }
                                buttonGroup="registrationType"
                                value={state.registrationSettings.UserRegistration}
                                disabled={!canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup style={{ marginBottom: "5px" }}>
                        <div className="registrationSettings-row-options">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("registrationFormTypeLabel.Help") }
                                label={resx.get("registrationFormTypeLabel") } />
                            <RadioButtons
                                onChange={this.onSettingChange.bind(this, "RegistrationFormType") }
                                options={this.keyValuePairsToOptions(props.registrationFormTypeOptions) }
                                buttonGroup="formType"
                                value={state.registrationSettings.RegistrationFormType}
                                disabled={!canEdit} />

                        </div>
                    </InputGroup>
                    {this.isCustomFormType() &&
                        <InputGroup style={{ marginBottom: "30px" }}>
                            <div className="registrationSettings-row-input">
                                <Label
                                    tooltipMessage={resx.get("registrationFieldsLabel.Help") }
                                    label={resx.get("registrationFieldsLabel") } />
                                <SearchableTags
                                    utils={util}
                                    tags={this.getRegistrationFields(state.registrationSettings.RegistrationFields) }
                                    onUpdateTags={this.onUpdateTags.bind(this) }
                                    error={this.state.error.registrationFields !== ""}
                                    errorMessage={this.state.error.registrationFields}
                                    enabled={canEdit} />
                            </div>
                        </InputGroup>
                    }
                    <InputGroup>
                        <div className="registrationSettings-row-input">
                            <Label
                                tooltipMessage={resx.get("Security_DisplayNameFormat.Help") }
                                label={resx.get("Security_DisplayNameFormat") } />
                            <SingleLineInputWithError
                                error={false}
                                value={state.registrationSettings.DisplayNameFormat}
                                onChange={this.onSettingChange.bind(this, "DisplayNameFormat") }
                                enabled={canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-input">
                            <Label
                                tooltipMessage={resx.get("Security_UserNameValidation.Help") }
                                label={resx.get("Security_UserNameValidation") } />
                            <SingleLineInputWithError
                                error={false}
                                value={state.registrationSettings.UserNameValidation}
                                onChange={this.onSettingChange.bind(this, "UserNameValidation") }
                                enabled={canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-input">
                            <Label
                                tooltipMessage={resx.get("Security_EmailValidation.Help") }
                                label={resx.get("Security_EmailValidation") } />
                            <SingleLineInputWithError
                                error={false}
                                withLabel={false}
                                value={state.registrationSettings.EmailAddressValidation}
                                onChange={this.onSettingChange.bind(this, "EmailAddressValidation") }
                                enabled={canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-input">
                            <Label
                                tooltipMessage={resx.get("Registration_ExcludeTerms.Help") }
                                label={resx.get("Registration_ExcludeTerms") } />
                            <SingleLineInputWithError
                                error={false}
                                withLabel={false}
                                value={state.registrationSettings.ExcludedTerms}
                                onChange={this.onSettingChange.bind(this, "ExcludedTerms") }
                                enabled={canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-input">
                            <Label
                                tooltipMessage={resx.get("Redirect_AfterRegistration.Help") }
                                label={resx.get("Redirect_AfterRegistration") } />
                            <PagePicker
                                serviceFramework={util.utilities.sf}
                                style={{ width: "100%", zIndex: 1 }}
                                selectedTabId={state.registrationSettings.RedirectAfterRegistrationTabId}
                                OnSelect={this.onSettingChange.bind(this, "RedirectAfterRegistrationTabId") }
                                defaultLabel={state.registrationSettings.RedirectAfterRegistrationTabName !== "" ? state.registrationSettings.RedirectAfterRegistrationTabName : noneSpecifiedText}
                                noneSpecifiedText={noneSpecifiedText}
                                CountText={"{0} Results"}
                                PortalTabsParameters={RedirectAfterRegistrationParameters}
                                enabled={canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("plEnableRegisterNotification.Help") }
                                label={resx.get("plEnableRegisterNotification") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.registrationSettings.EnableRegisterNotification}
                                onChange={this.onSettingChange.bind(this, "EnableRegisterNotification") }
                                readOnly={!canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Registration_UseAuthProviders.Help") }
                                label={resx.get("Registration_UseAuthProviders") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.registrationSettings.UseAuthenticationProviders}
                                onChange={this.onSettingChange.bind(this, "UseAuthenticationProviders") }
                                readOnly={!canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Registration_UseProfanityFilter.Help") }
                                label={resx.get("Registration_UseProfanityFilter") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.registrationSettings.UseProfanityFilter}
                                onChange={this.onSettingChange.bind(this, "UseProfanityFilter") }
                                readOnly={!canEdit} />
                        </div>
                    </InputGroup>
                    {!this.isCustomFormType() &&
                        <InputGroup>
                            <div className="registrationSettings-row_switch">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("Registration_UseEmailAsUserName.Help") }
                                    label={resx.get("Registration_UseEmailAsUserName") } />
                                <Switch
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
                                    value={state.registrationSettings.UseEmailAsUsername}
                                    onChange={this.onSettingChange.bind(this, "UseEmailAsUsername") }
                                    readOnly={!canEdit} />
                            </div>
                        </InputGroup>
                    }
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Registration_RequireUniqueDisplayName.Help") }
                                label={resx.get("Registration_RequireUniqueDisplayName") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.registrationSettings.RequireUniqueDisplayName}
                                onChange={this.onSettingChange.bind(this, "RequireUniqueDisplayName") }
                                readOnly={!canEdit} />
                        </div>
                    </InputGroup>
                    {!this.isCustomFormType() &&
                        <InputGroup>
                            <div className="registrationSettings-row_switch">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("Registration_RandomPassword.Help") }
                                    label={resx.get("Registration_RandomPassword") } />
                                <Switch
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
                                    value={state.registrationSettings.UseRandomPassword}
                                    onChange={this.onSettingChange.bind(this, "UseRandomPassword") }
                                    readOnly={!canEdit} />
                            </div>
                        </InputGroup>
                    }
                    {!this.isCustomFormType() &&
                        <InputGroup>
                            <div className="registrationSettings-row_switch">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("Registration_RequireConfirmPassword.Help") }
                                    label={resx.get("Registration_RequireConfirmPassword") } />
                                <Switch
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
                                    value={state.registrationSettings.RequirePasswordConfirmation}
                                    onChange={this.onSettingChange.bind(this, "RequirePasswordConfirmation") }
                                    readOnly={!canEdit} />
                            </div>
                        </InputGroup>
                    }
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Security_RequireValidProfile.Help") }
                                label={resx.get("Security_RequireValidProfile") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.registrationSettings.RequireValidProfile}
                                onChange={this.onSettingChange.bind(this, "RequireValidProfile") }
                                readOnly={!canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row_switch">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("Security_CaptchaRegister.Help") }
                                label={resx.get("Security_CaptchaRegister") } />
                            <Switch
                                onText={resx.get("SwitchOn")}
                                offText={resx.get("SwitchOff")}
                                value={state.registrationSettings.UseCaptchaRegister}
                                onChange={this.onSettingChange.bind(this, "UseCaptchaRegister") }
                                readOnly={!canEdit} />
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("RequiresUniqueEmail.Help") }
                                label={resx.get("RequiresUniqueEmail") } />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.RequiresUniqueEmail}
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("PasswordFormat.Help") }
                                label={resx.get("PasswordFormat") } />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.PasswordFormat}
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("PasswordRetrievalEnabled.Help") }
                                label={resx.get("PasswordRetrievalEnabled") } />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.PasswordRetrievalEnabled}
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("PasswordResetEnabledTitle.Help") }
                                label={resx.get("PasswordResetEnabledTitle") } />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.PasswordResetEnabled}
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("MinPasswordLengthTitle.Help") }
                                label={resx.get("MinPasswordLengthTitle") } />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.MinPasswordLength}
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("MinNonAlphanumericCharactersTitle.Help") }
                                label={resx.get("MinNonAlphanumericCharactersTitle") } />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.MinNonAlphanumericCharacters}
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("RequiresQuestionAndAnswerTitle.Help") }
                                label={resx.get("RequiresQuestionAndAnswerTitle") } />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.RequiresQuestionAndAnswer}
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("PasswordStrengthRegularExpressionTitle.Help") }
                                label={resx.get("PasswordStrengthRegularExpressionTitle") } />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.PasswordStrengthRegularExpression}
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("MaxInvalidPasswordAttemptsTitle.Help") }
                                label={resx.get("MaxInvalidPasswordAttemptsTitle") } />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.MaxInvalidPasswordAttempts}
                            </div>
                        </div>
                    </InputGroup>
                    <InputGroup>
                        <div className="registrationSettings-row-static">
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("PasswordAttemptWindowTitle.Help") }
                                label={resx.get("PasswordAttemptWindowTitle") } />
                            <div className="registrationSettings-row-static-text">
                                {state.registrationSettings.PasswordAttemptWindow}
                            </div>
                        </div>
                    </InputGroup>
                    {canEdit &&
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
                    }
                </div>
            );
        }
        else return <div />;
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