import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import { Tab, Tabs, TabList, TabPanel } from "react-tabs";
import {
    pagination as PaginationActions,
    siteSettings as SiteSettingsActions
} from "../../actions";
import SiteAliases from "../siteAliases";
import InputGroup from "dnn-input-group";
import SingleLineInputWithError from "dnn-single-line-input-with-error";
import PagePicker from "dnn-page-picker";
import Grid from "dnn-grid-system";
import Switch from "dnn-switch";
import RadioButtons from "dnn-radio-buttons";
import Dropdown from "dnn-dropdown";
import Label from "dnn-label";
import Button from "dnn-button";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";

class SiteAliasSettingsPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            urlMappingSettings: undefined
        };
    }

    componentWillMount() {
        const {state, props} = this;
        if (props.urlMappingSettings) {
            this.setState({
                urlMappingSettings: props.urlMappingSettings
            });
            return;
        }
        props.dispatch(SiteSettingsActions.getUrlMappingSettings(props.portalId, (data) => {
            this.setState({
                urlMappingSettings: Object.assign({}, data.Settings)
            });
        }));
    }

    componentWillReceiveProps(props) {
        let {state} = this;

        this.setState({
            urlMappingSettings: Object.assign({}, props.urlMappingSettings)
        });
    }

    getMappingModeOptions() {
        let options = [];
        if (this.props.portalAliasMappingModes !== undefined) {
            options = this.props.portalAliasMappingModes.map((item) => {
                return { label: item.Key, value: item.Value };
            });
        }
        return options;
    }

    onSettingChange(key, event) {
        let {state, props} = this;

        let urlMappingSettings = Object.assign({}, state.urlMappingSettings);

        if (key === "PortalAliasMapping") {
            urlMappingSettings[key] = event;
        }
        else {
            urlMappingSettings[key] = typeof (event) === "object" ? event.target.value : event;
        }
        this.setState({
            urlMappingSettings: urlMappingSettings
        });

        props.dispatch(SiteSettingsActions.urlMappingSettingsClientModified(urlMappingSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;

        props.dispatch(SiteSettingsActions.updateUrlMappingSettings(state.urlMappingSettings, (data) => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, (error) => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    onCancel(event) {
        const {props, state} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SiteSettingsActions.getUrlMappingSettings(props.portalId, (data) => {
                let urlMappingSettings = Object.assign({}, data.Settings);
                this.setState({
                    urlMappingSettings
                });
            }));
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        if (state.urlMappingSettings) {
            return (
                <div className={styles.siteAliasSettings}>
                    <SiteAliases />
                    <div className="sectionTitleNoBorder">{resx.get("UrlMappingSettings")}</div>
                    <div className="urlMappingSettings">
                        <InputGroup>
                            <Label
                                labelType="inline"
                                tooltipMessage={resx.get("portalAliasModeButtonListLabel.Help")}
                                label={resx.get("portalAliasModeButtonListLabel")}
                                />
                            <RadioButtons
                                onChange={this.onSettingChange.bind(this, "PortalAliasMapping")}
                                options={this.getMappingModeOptions()}
                                buttonGroup="aliasMode"
                                buttonWidth={100}
                                value={state.urlMappingSettings.PortalAliasMapping} />
                        </InputGroup>
                        <InputGroup>
                            <div className="urlMappingSettings-row_switch">
                                <Label
                                    labelType="inline"
                                    tooltipMessage={resx.get("plAutoAddPortalAlias.Help")}
                                    label={resx.get("plAutoAddPortalAlias")}
                                    />
                                <Switch
                                    labelHidden={true}
                                    readOnly={!state.urlMappingSettings.AutoAddPortalAliasEnabled}
                                    value={state.urlMappingSettings.AutoAddPortalAlias}
                                    onChange={this.onSettingChange.bind(this, "AutoAddPortalAlias")}
                                    />
                            </div>
                        </InputGroup>
                    </div>
                    <div className="buttons-box">
                        <Button
                            disabled={!this.props.urlMappingSettingsClientModified}
                            type="secondary"
                            onClick={this.onCancel.bind(this)}>
                            {resx.get("Cancel")}
                        </Button>
                        <Button
                            disabled={!this.props.urlMappingSettingsClientModified}
                            type="primary"
                            onClick={this.onUpdate.bind(this)}>
                            {resx.get("Save")}
                        </Button>
                    </div>
                </div>
            );
        }
        else return <div />;
    }
}

SiteAliasSettingsPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    urlMappingSettings: PropTypes.object,
    portalAliasMappingModes: PropTypes.array,
    urlMappingSettingsClientModified: PropTypes.bool,
    portalId: PropTypes.number
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        urlMappingSettings: state.siteSettings.urlMappingSettings,
        portalAliasMappingModes: state.siteSettings.portalAliasMappingModes,
        urlMappingSettingsClientModified: state.siteSettings.urlMappingSettingsClientModified
    };
}

export default connect(mapStateToProps)(SiteAliasSettingsPanelBody);