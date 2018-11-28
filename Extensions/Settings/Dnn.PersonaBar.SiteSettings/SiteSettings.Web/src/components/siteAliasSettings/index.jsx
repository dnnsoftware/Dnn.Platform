import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    siteBehavior as SiteBehaviorActions
} from "../../actions";
import SiteAliases from "./siteAliases";
import InputGroup from "dnn-input-group";
import Switch from "dnn-switch";
import RadioButtons from "dnn-radio-buttons";
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

    loadData() {
        const {props} = this;
        if (props.urlMappingSettings) {
            if (props.portalId === undefined || props.urlMappingSettings.PortalId === props.portalId) {
                return false;
            }
            else {
                return true;
            }
        }
        else {
            return true;
        }
    }

    componentDidMount() {
        const {props} = this;
        if (!this.loadData()) {
            this.setState({
                urlMappingSettings: props.urlMappingSettings
            });
            return;
        }
        props.dispatch(SiteBehaviorActions.getUrlMappingSettings(props.portalId, (data) => {
            this.setState({
                urlMappingSettings: Object.assign({}, data.Settings)
            });
        }));
    }

    componentDidUpdate(props) {
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

        props.dispatch(SiteBehaviorActions.urlMappingSettingsClientModified(urlMappingSettings));
    }

    onUpdate(event) {
        event.preventDefault();
        const {props, state} = this;
        props.dispatch(SiteBehaviorActions.updateUrlMappingSettings(state.urlMappingSettings, () => {
            util.utilities.notify(resx.get("SettingsUpdateSuccess"));
        }, () => {
            util.utilities.notifyError(resx.get("SettingsError"));
        }));
    }

    onCancel() {
        const {props} = this;
        util.utilities.confirm(resx.get("SettingsRestoreWarning"), resx.get("Yes"), resx.get("No"), () => {
            props.dispatch(SiteBehaviorActions.getUrlMappingSettings(props.portalId, (data) => {
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
                    <SiteAliases portalId={props.portalId}/>
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
                                    onText={resx.get("SwitchOn")}
                                    offText={resx.get("SwitchOff")}
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
    portalId: PropTypes.number,
    cultureCode: PropTypes.string
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        urlMappingSettings: state.siteBehavior.urlMappingSettings,
        portalAliasMappingModes: state.siteBehavior.portalAliasMappingModes,
        urlMappingSettingsClientModified: state.siteBehavior.urlMappingSettingsClientModified
    };
}

export default connect(mapStateToProps)(SiteAliasSettingsPanelBody);