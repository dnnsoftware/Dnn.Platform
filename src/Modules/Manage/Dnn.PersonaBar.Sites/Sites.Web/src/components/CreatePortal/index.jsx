import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import { portal as PortalActions } from "actions";
import GridCell from "dnn-grid-cell";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import RadioButtons from "dnn-radio-buttons";
import Button from "dnn-button";
import DropdownWithError from "dnn-dropdown-with-error";
import Switch from "dnn-switch";
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
    UseCurrentUserAsAdmin: true
};

class CreatePortal extends Component {
    constructor() {
        super();
        this.state = {
            defaultTemplate: "",
            newPortal: Object.assign({}, emptyNewPortal),
            error: {
                SiteName: false,
                SiteAlias: false
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
        props.dispatch(PortalActions.getPortalTemplates((data) => {
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
            case "SiteName":
            case "SiteAlias":
                newPortal[key] = value;
                error[key] = false;
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
    createPortal() {
        const { props, state } = this;
        let {triedToSave, error} = state;
        triedToSave = true;
        if (state.newPortal.SiteName === "") {
            error.SiteName = true;
        }
        if (state.newPortal.SiteAlias === "") {
            error.SiteAlias = true;
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
            props.dispatch(PortalActions.createPortal(state.newPortal, () => {
                this.resetNewPortal();
                props.onCancel();
            }));
        });
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
                        </GridCell>
                        <GridCell className="site-action-buttons">
                            <Button type="secondary" onClick={props.onCancel.bind(this)}>{Localization.get("cmdCancel")}</Button>
                            <Button type="primary" onClick={this.createPortal.bind(this)}>{Localization.get("cmdCreateSite")}</Button>
                        </GridCell>
                    </GridCell>
                </SocialPanelBody>
            </div>
        );
    }
}

CreatePortal.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    onCancel: PropTypes.func
};


function mapStateToProps(state) {
    return {
        portalTemplates: state.portal.templates
    };
}


export default connect(mapStateToProps)(CreatePortal);