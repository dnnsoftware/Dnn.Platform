import PropTypes from 'prop-types';
import React, { Component } from "react";
import { connect } from "react-redux";
import { Collapsible as Collapse, Switch, DropdownWithError, RadioButtons, Button, PersonaBarPageHeader, PersonaBarPageBody, GridCell, GridSystem, SingleLineInputWithError, MultiLineInputWithError } from "@dnnsoftware/dnn-react-common";
import { CommonPortalListActions } from "dnn-sites-common-actions";
import Localization from "localization";
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
function extractDomain(url) {
    let domain, port;
    //find & remove protocol (http, ftp, etc.) and get domain
    if (url.indexOf("://") > -1) {
        domain = url.split("/")[2];
    }
    else {
        domain = url.split("/")[0];
    }
    [domain, port] = domain.split(":");
    return port !== undefined && port !== 80 && port !== 443 ? `${domain}:${port}` : domain;
}

function validateEmail(email) {
    let re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}

function getUniqueId() {
    return Math.random() * Date.now();
}

class CreatePortal extends Component {
    constructor() {
        super();
        this.state = {
            defaultTemplate: "",
            newPortal: Object.assign({}, emptyNewPortal),
            error: {
                SiteName: true,
                SiteAlias: true,
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
    resolveSiteUrl(isChildSite) {
        let rootDomain = extractDomain(window.location.href);
        if (isChildSite) {
            let { newPortal, error } = this.state;
            if (newPortal.SiteAlias !== "" && newPortal.SiteAlias.indexOf(rootDomain) === -1) {
                newPortal.SiteAlias = rootDomain + "/" + newPortal.SiteAlias;
            } else {
                newPortal.SiteAlias = rootDomain + "/" + newPortal.SiteName;
            }
            error.SiteAlias = this.resolveSiteAliasError(newPortal.SiteAlias, isChildSite);
            this.setState({ newPortal, error });
        } else {
            let { newPortal } = this.state;
            newPortal.SiteAlias = newPortal.SiteAlias.replace(rootDomain + "/", "");
            this.setState({ newPortal });
        }
    }

    resolveSiteAliasError(value, isChildSite) {
        let rootDomain = extractDomain(window.location.href);
        let subUrl = value.replace(rootDomain + "/", "");
        if (!isChildSite) {
            let regex = /[^\/a-z0-9-.:]/i;
            return value === "" || regex.test(value) || value.indexOf(" ") > 0;
        } else {
            let regex = /[^\/a-z0-9_-]/i;
            if (((regex.test(subUrl) && subUrl !== "") || (value.indexOf(rootDomain + "/")) === -1) || subUrl === "" || value.indexOf(" ") > 0) {
                return true;
            }
            return false;
        }
    }

    resolvePasswordError(value) {
        if (value !== this.state.newPortal.Password || value === "" || value.length < 7) {
            return true;
        }
        return false;
    }
    resolveEmailError(value) {
        return !validateEmail(value);
    }
    onChange(key, event) {
        const value = typeof event === "object" ? event.target.value : event;
        let {newPortal, error} = this.state;
        switch (key) {
            case "IsChildSite"://Convert radio button's return of string to boolean.
                newPortal[key] = (value === "true");
                this.resolveSiteUrl((value === "true"));
                break;
            case "PasswordConfirm":
                newPortal[key] = value;
                error[key] = this.resolvePasswordError(value);
                break;
            case "Email":
                newPortal[key] = value;
                error[key] = this.resolveEmailError(value);
                break;
            case "Password":
            case "Firstname":
            case "Lastname":
            case "Username":
            case "SiteName":
                newPortal[key] = value;
                error[key] = (value === "");
                break;
            case "SiteAlias":
                newPortal[key] = value;
                error[key] = this.resolveSiteAliasError(value, newPortal.IsChildSite);
                break;
            case "UseCurrentUserAsAdmin":
                newPortal[key] = value;
                if ((value === true)) {
                    error.Firstname = false;
                    error.Lastname = false;
                    error.Username = false;
                    error.Email = false;
                    error.Password = false;
                    error.PasswordConfirm = false;
                } else {
                    error.Firstname = (newPortal.Firstname === "");
                    error.Lastname = (newPortal.Lastname === "");
                    error.Username = (newPortal.Username === "");
                    error.Email = this.resolveEmailError(newPortal.Email);
                    error.Password = (newPortal.Password === "");
                    error.PasswordConfirm = this.resolvePasswordError(newPortal.PasswordConfirm);
                }
                break;
            default:
                newPortal[key] = value;
                break;
        }
        this.setState({
            newPortal,
            error
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
    setCreatingPortal(creatingPortal) {
        this.setState({
            creatingPortal
        });
    }
    createPortal() {
        const { props, state } = this;
        let {triedToSave} = state;
        triedToSave = true;
        this.setState({
            triedToSave
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
            this.setCreatingPortal(true);
            props.dispatch(CommonPortalListActions.createPortal(state.newPortal, () => {
                this.setCreatingPortal(false);
                this.resetNewPortal();
                props.onCancel();
            }, () => {
                //active the button after error message disappear
                setTimeout(() => {
                    this.setCreatingPortal(false);
                }, 5200);
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
                <PersonaBarPageHeader title={Localization.get("AddNewSite.Header")} />
                <PersonaBarPageBody backToLinkProps={{
                    text: Localization.get("BackToSites"),
                    onClick: props.onCancel.bind(this)
                }}>
                    <GridCell className="create-site-container">
                        <GridCell>
                            <SingleLineInputWithError
                                label={Localization.get("Title.Label") + "*"}
                                inputId="add-new-site-title"
                                value={state.newPortal.SiteName}
                                errorMessage={Localization.get("SiteTitleRequired.Error")}
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
                                    label={Localization.get("Directory")}
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
                                    errorMessage={Localization.get("SiteAliasRequired.Error")}
                                    error={state.error.SiteAlias && state.triedToSave}
                                    />
                            </GridCell>
                        </GridCell>
                        <GridCell className="user-as-admin">
                            <Switch
                                label={Localization.get("AssignCurrentUserAsAdmin.Label")}
                                onText={Localization.get("SwitchOn")}
                                offText={Localization.get("SwitchOff")}
                                value={state.newPortal.UseCurrentUserAsAdmin}
                                onChange={this.onChange.bind(this, "UseCurrentUserAsAdmin")}
                                />
                            <Collapse style={{clear:"both"}} isOpened={!this.state.newPortal.UseCurrentUserAsAdmin}>
                                <GridSystem className="with-right-border top-half">
                                    <GridCell>
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminUserName.Label")}
                                            inputId="admin-user-name"
                                            value={state.newPortal.Username}
                                            onChange={this.onChange.bind(this, "Username")}
                                            errorMessage={Localization.get("UsernameRequired.Error")}
                                            error={state.error.Username && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                            />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminFirstName.Label")}
                                            inputId="admin-first-name"
                                            value={state.newPortal.Firstname}
                                            onChange={this.onChange.bind(this, "Firstname")}
                                            errorMessage={Localization.get("FirstNameRequired.Error")}
                                            error={state.error.Firstname && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                            />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminLastName.Label")}
                                            inputId="admin-last-name"
                                            value={state.newPortal.Lastname}
                                            onChange={this.onChange.bind(this, "Lastname")}
                                            errorMessage={Localization.get("LastNameRequired.Error")}
                                            error={state.error.Lastname && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                            />
                                    </GridCell>
                                    <GridCell>
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminEmail.Label")}
                                            inputId="admin-email"
                                            value={state.newPortal.Email}
                                            onChange={this.onChange.bind(this, "Email")}
                                            errorMessage={Localization.get("EmailRequired.Error")}
                                            error={state.error.Email && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                            />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminPassword.Label")}
                                            inputId="admin-password"
                                            value={state.newPortal.Password}
                                            type="password"
                                            onChange={this.onChange.bind(this, "Password")}
                                            errorMessage={Localization.get("PasswordRequired.Error")}
                                            error={state.error.Password && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                            />
                                        <SingleLineInputWithError
                                            label={Localization.get("CreateSite_AdminPasswordConfirm.Label")}
                                            inputId="admin-password-confirm"
                                            type="password"
                                            value={state.newPortal.PasswordConfirm}
                                            errorMessage={Localization.get("PasswordConfirmRequired.Error")}
                                            onChange={this.onChange.bind(this, "PasswordConfirm")}
                                            error={state.error.PasswordConfirm && state.triedToSave && !state.newPortal.UseCurrentUserAsAdmin}
                                            autoComplete={getUniqueId()}
                                            />
                                    </GridCell>
                                </GridSystem>
                                <div style={{clear:"both"}}></div>
                            </Collapse>
                        </GridCell>
                        <GridCell className="site-action-buttons">
                            <Button type="secondary" onClick={this.onCancel.bind(this)}>{Localization.get("cmdCancel")}</Button>
                            <Button type="primary" disabled={this.state.creatingPortal} onClick={this.createPortal.bind(this)}>{Localization.get("cmdCreateSite")}</Button>
                        </GridCell>
                    </GridCell>
                </PersonaBarPageBody>
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