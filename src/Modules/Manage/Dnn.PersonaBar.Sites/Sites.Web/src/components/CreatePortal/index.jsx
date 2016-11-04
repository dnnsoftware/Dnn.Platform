import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import { CommonPortalListActions } from "dnn-sites-common-actions";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import RadioButtons from "dnn-radio-buttons";
import Button from "dnn-button";
import DropdownWithError from "dnn-dropdown-with-error";
import Switch from "dnn-switch";
import Localization from "localization";
import Collapse from "react-collapse";
import "./style.less";

const emptyNewPortal = {
    SiteTemplate: "",
    SiteName: "",
    SiteAlias: "",
    SiteDescription: "",
    SiteKeywords: "",
    IsChildSite: false,
    HomeDirectory: "Portals/[PortalID]",
    UseCurrentUserAsAdmin: true,
    Firstname: "",
    Lastname: "",
    Username: "",
    Email: "",
    Password: "",
    PasswordConfirm: ""
};

class CreatePortal extends Component {
    constructor() {
        super();
        this.state = {
            defaultTemplate: "",
            newPortal: Object.assign({}, emptyNewPortal),
            error: {
                SiteName: false,
                SiteAlias: false,
                Firstname: false,
                Lastname: false,
                Username: false,
                Email: false,
                Password: false,
                PasswordConfirm: false
            },
            triedToSave: false
        };
    }

    resetNewPortal() {
        this.setState({
            newPortal: Object.assign({}, emptyNewPortal)
        });
    }

    componentWillMount() {
        const { props, state } = this;
        props.dispatch(CommonPortalListActions.getPortalTemplates((data) => {
            let {newPortal} = state;
            newPortal.SiteTemplate = data.Results.DefaultTemplate;
            this.setState({
                defaultTemplate: data.Results.DefaultTemplate,
                newPortal
            });
        }));
    }
    onChange(key, event) {
        const value = typeof event === "object" ? event.target.value : event;
        let {newPortal, error} = this.state;
        switch (key) {
            case "IsChildSite"://Convert radio button's return of string to boolean.
                newPortal[key] = (value === "true");
                break;
            case "Firstname":
            case "Lastname":
            case "Username":
            case "Email":
            case "Password":
            case "PasswordConfirm":
            case "SiteName":
            case "SiteAlias":
                newPortal[key] = value;
                error[key] = (value === "");
                break;
            case "UseCurrentUserAsAdmin":
                newPortal[key] = value;
                if (!(value === "true")) {
                    error.Firstname = false;
                    error.Lastname = false;
                    error.Username = false;
                    error.Email = false;
                    error.Password = false;
                    error.PasswordConfirm = false;
                }
                break;
            default:
                newPortal[key] = value;
                break;

        }
        this.setState({
            newPortal
        });
    }
    onSelect(option) {
        let {newPortal} = this.state;
        newPortal.SiteTemplate = option.value;
        this.setState({
            newPortal
        });
    }
    checkForError(newPortal, key) {
        if (newPortal[key] === "") {
            return true;
        }
    }
    createPortal() {
        const { props, state } = this;
        let {triedToSave, error} = state;
        triedToSave = true;
        error.SiteName = this.checkForError(state.newPortal, "SiteName");
        error.SiteAlias = this.checkForError(state.newPortal, "SiteAlias");
        if (!state.newPortal.UseCurrentUserAsAdmin) {
            error.Firstname = this.checkForError(state.newPortal, "Firstname");
            error.Lastname = this.checkForError(state.newPortal, "Lastname");
            error.Username = this.checkForError(state.newPortal, "Username");
            error.Email = this.checkForError(state.newPortal, "Email");
            error.Password = this.checkForError(state.newPortal, "Password");
            error.PasswordConfirm = this.checkForError(state.newPortal, "PasswordConfirm");
        }
        this.setState({
            triedToSave,
            error
        }, () => {
            let withError = false;
            Object.keys(state.error).forEach((errorKey) => {
                if (state.error[errorKey]) {
                    withError = true;
                }
            });
            if (withError) {
                return;
            }
            props.dispatch(CommonPortalListActions.createPortal(state.newPortal, () => {
                this.resetNewPortal();
                props.onCancel();
            }));
        });
    }

    onCancel(event) {
        if (event) {
            event.preventDefault();
        }
        this.props.onCancel();
        this.resetNewPortal();
    }
    render() {
        const {props, state} = this;

        const templateOptions = props.portalTemplates.map(template => {
            return {
                label: template.Name,
                value: template.Value
            };
        });

        return (
            <div className="create-portal">
                <SocialPanelHeader title={Localization.get("AddNewSite.Header")} />
                <SocialPanelBody>
                    <GridCell className="create-site-container">
                        <GridCell>
                            <SingleLineInputWithError
                                label={Localization.get("Title.Label") + "*"}
                                inputId="add-new-site-title"
                                value={state.newPortal.SiteName}
                                onChange={this.onChange.bind(this, "SiteName")}
                                error={state.error.SiteName && state.triedToSave}
                                />
                        </GridCell>
                        <GridCell>
                            <MultiLineInputWithError
                                label={Localization.get("Description")}
                                inputId="add-new-site-description"
                                value={state.newPortal.SiteDescription}
                                className="portal-description"
                                onChange={this.onChange.bind(this, "SiteDescription")}
                                error={false}
                                />
                            <hr />
                        </GridCell>
                        <GridCell className="site-thumbnails-container">
                            <DropdownWithError
                                options={templateOptions}
                                label={Localization.get("SiteTemplate.Label")}
                                value={state.newPortal.SiteTemplate}
                                defaultDropdownValue={state.defaultTemplate}
                                onSelect={this.onSelect.bind(this)}
                                />
                            <hr />
                        </GridCell>
                        <GridCell className="site-type-container">
                            <GridCell columnSize={55}>
                                <RadioButtons
                                    label={Localization.get("SiteType.Label")}
                                    onChange={this.onChange.bind(this, "IsChildSite")}
                                    buttonGroup="siteType"
                                    value={state.newPortal.IsChildSite}
                                    defaultValue={state.newPortal.IsChildSite}
                                    buttonWidth={130}
                                    options={[
                                        {
                                            label: Localization.get("Domain"),
                                            value: false
                                        },
                                        {
                                            label: Localization.get("Directory"),
                                            value: true
                                        }
                                    ]} />
                            </GridCell>
                            <GridCell columnSize={45}>
                                <SingleLineInputWithError
                                    label={Localization.get("Description.Label")}
                                    className="home-directory"
                                    inputId="home-directory"
                                    value={state.newPortal.HomeDirectory}
                                    onChange={this.onChange.bind(this, "HomeDirectory")}
                                    labelType="inline"
                                    error={false}
                                    />
                            </GridCell>
                            <GridCell>
                                <SingleLineInputWithError
                                    label={Localization.get("SiteUrl.Label")}
                                    inputId="site-url"
                                    value={state.newPortal.SiteAlias}
                                    onChange={this.onChange.bind(this, "SiteAlias")}
                                    error={state.error.SiteAlias && state.triedToSave}
                                    />
                            </GridCell>
                        </GridCell>
                        <GridCell className="user-as-admin">
                            <Switch
                                label={Localization.get("AssignCurrentUserAsAdmin.Label")}
                                value={state.newPortal.UseCurrentUserAsAdmin}
                                onChange={this.onChange.bind(this, "UseCurrentUserAsAdmin")}
                                />
                            <Collapse style={{ float: "left" }} isOpened={!state.newPortal.UseCurrentUserAsAdmin}>
                                <GridSystem className="with-right-border top-half">
                                    <GridCell>
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminUserName.Label")}
                                            inputId="admin-user-name"
                                            value={state.newPortal.Username}
                                            onChange={this.onChange.bind(this, "Username")}
                                            error={state.error.Username && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminFirstName.Label")}
                                            inputId="admin-first-name"
                                            value={state.newPortal.Firstname}
                                            onChange={this.onChange.bind(this, "Firstname")}
                                            error={state.error.Firstname && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminLastName.Label")}
                                            inputId="admin-last-name"
                                            value={state.newPortal.Lastname}
                                            onChange={this.onChange.bind(this, "Lastname")}
                                            error={state.error.Lastname && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            />
                                    </GridCell>
                                    <GridCell>
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminEmail.Label")}
                                            inputId="admin-email"
                                            value={state.newPortal.Email}
                                            onChange={this.onChange.bind(this, "Email")}
                                            error={state.error.Email && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminPassword.Label")}
                                            inputId="admin-password"
                                            value={state.newPortal.Password}
                                            onChange={this.onChange.bind(this, "Password")}
                                            error={state.error.Password && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminPasswordConfirm.Label")}
                                            inputId="admin-password-confirm"
                                            value={state.newPortal.PasswordConfirm}
                                            onChange={this.onChange.bind(this, "PasswordConfirm")}
                                            error={state.error.PasswordConfirm && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            />
                                    </GridCell>
                                </GridSystem>
                            </Collapse>
                        </GridCell>
                        <GridCell className="site-action-buttons">
                            <Button type="secondary" onClick={this.onCancel.bind(this)}>{Localization.get("cmdCancel")}</Button>
                            <Button type="primary" onClick={this.createPortal.bind(this)}>{Localization.get("cmdCreateSite")}</Button>
                        </GridCell>
                    </GridCell>
                </SocialPanelBody>
            </div>
        );
    }
}

CreatePortal.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func
};


function mapStateToProps(state) {
    return {
        portalTemplates: state.exportPortal.templates
    };
}


export default connect(mapStateToProps)(CreatePortal);