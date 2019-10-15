import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import { GridSystem, GridCell, InputGroup, Button, Label } from "@dnnsoftware/dnn-react-common";
import RadioButtonBlock from "../common/RadioButtonBlock";
import DropdownBlock from "../common/DropdownBlock";
import InfoBlock from "../common/InfoBlock";
import SwitchBlock from "../common/SwitchBlock";
import WarningBlock from "../common/WarningBlock";
import localization from "../../localization";
import PerformanceTabActions from "../../actions/performanceTab";
import utils from "../../utils";


class Performance extends Component {
    UNSAFE_componentWillMount() {
        this.props.onRetrievePerformanceSettings();
    }

    UNSAFE_componentWillReceiveProps(newProps) {
        if (this.props.infoMessage !== newProps.infoMessage && newProps.infoMessage) {
            utils.notify(newProps.infoMessage);
        }

        if (this.props.errorMessage !== newProps.errorMessage && newProps.errorMessage) {
            utils.notifyError(newProps.errorMessage);
        }
    }

    getClientResourcesManagementModeOptions() {
        return [
            {
                label: localization.get("PerformanceTab_GlobalClientResourcesManagementMode"),
                value: "h"
            },
            {
                label: this.props.performanceSettings.portalName,
                value: "p"
            }
        ];
    }

    onSave() {
        const {props} = this;

        props.onSave(props.performanceSettings);
    }

    confirmHandler() {
        const {props} = this;
        const isGlobalSettings = props.performanceSettings.clientResourcesManagementMode === "h";
        if (isGlobalSettings) {
            props.onIncrementVersion(props.performanceSettings.currentHostVersion, isGlobalSettings);
        } else {
            props.onIncrementVersion(props.performanceSettings.currentPortalVersion, isGlobalSettings);
        }
    }

    cancelHandler() {

    }

    onIncrementVersion() {
        utils.confirm(localization.get("PerformanceTab_PortalVersionConfirmMessage"),
            localization.get("PerformanceTab_PortalVersionConfirmYes"),
            localization.get("PerformanceTab_PortalVersionConfirmNo"),
            this.confirmHandler.bind(this), this.cancelHandler.bind(this));
    }

    onChangeField(key, event) {
        let value = event;
        if (event && event.value) {
            value = event.value;
        } else if (event && event.target && event.target.value) {
            value = event.target.value;
        }

        this.props.onChangePerformanceSettingsValue(key, value);
    }

    render() {
        const {props} = this;
        if (props.isLoading) {
            return null;
        }

        const areGlobalSettings = props.performanceSettings.clientResourcesManagementMode === "h";
        let enableCompositeFiles;
        let minifyCss;
        let minifyJs;
        let enableCompositeFilesKey;
        let minifyCssKey;
        let minifyJsKey;
        let version;
        let versionLocalizationKey;
        if (areGlobalSettings) {
            enableCompositeFiles = props.performanceSettings.hostEnableCompositeFiles;
            minifyCss = props.performanceSettings.hostMinifyCss;
            minifyJs = props.performanceSettings.hostMinifyJs;
            enableCompositeFilesKey = "hostEnableCompositeFiles";
            minifyCssKey = "hostMinifyCss";
            minifyJsKey = "hostMinifyJs";
            version = props.performanceSettings.currentHostVersion;
            versionLocalizationKey = "PerformanceTab_CurrentHostVersion";
        } else {
            enableCompositeFiles = props.performanceSettings.portalEnableCompositeFiles;
            minifyCss = props.performanceSettings.portalMinifyCss;
            minifyJs = props.performanceSettings.portalMinifyJs;
            enableCompositeFilesKey = "portalEnableCompositeFiles";
            minifyCssKey = "portalMinifyCss";
            minifyJsKey = "portalMinifyJs";
            version = props.performanceSettings.currentPortalVersion;
            versionLocalizationKey = "PerformanceTab_CurrentPortalVersion";
        }

        return <div className="dnn-servers-info-panel-big performanceSettingTab">
            <WarningBlock label={localization.get("PerformanceTab_AjaxWarning")} />
            <GridSystem>
                <div className="leftPane">
                    <div className="tooltipAdjustment">
                        {props.performanceSettings.pageStatePersistenceOptions &&
                            <RadioButtonBlock options={props.performanceSettings.pageStatePersistenceOptions}
                                label={localization.get("PerformanceTab_PageStatePersistenceMode")}
                                tooltip={localization.get("PerformanceTab_PageStatePersistenceMode.Help")}
                                onChange={this.onChangeField.bind(this, "pageStatePersistence")}
                                value={props.performanceSettings.pageStatePersistence} />
                        }
                        {props.performanceSettings.cacheSettingOptions &&
                            <DropdownBlock
                                tooltip={localization.get("PerformanceTab_CachingProvider.Help")}
                                label={localization.get("PerformanceTab_CachingProvider")}
                                options={props.performanceSettings.cachingProviderOptions}
                                value={props.performanceSettings.cachingProvider}
                                onSelect={this.onChangeField.bind(this, "cachingProvider")} />
                        }
                        {props.performanceSettings.moduleCacheProviders &&
                            <DropdownBlock
                                tooltip={localization.get("PerformanceTab_ModuleCacheProviders.Help")}
                                label={localization.get("PerformanceTab_ModuleCacheProviders")}
                                options={props.performanceSettings.moduleCacheProviders}
                                value={props.performanceSettings.moduleCacheProvider}
                                onSelect={this.onChangeField.bind(this, "moduleCacheProvider")} />
                        }
                        {props.performanceSettings.pageCacheProviders &&
                            <DropdownBlock
                                tooltip={localization.get("PerformanceTab_PageCacheProviders.Help")}
                                label={localization.get("PerformanceTab_PageCacheProviders")}
                                options={props.performanceSettings.pageCacheProviders}
                                value={props.performanceSettings.pageCacheProvider}
                                onSelect={this.onChangeField.bind(this, "pageCacheProvider")} />
                        }
                    </div>
                </div>
                <div className="rightPane">
                    {props.performanceSettings.cacheSettingOptions &&
                        <DropdownBlock
                            tooltip={localization.get("PerformanceTab_CacheSetting.Help")}
                            label={localization.get("PerformanceTab_CacheSetting")}
                            options={props.performanceSettings.cacheSettingOptions}
                            value={props.performanceSettings.cacheSetting}
                            onSelect={this.onChangeField.bind(this, "cacheSetting")} />
                    }
                    {props.performanceSettings.authCacheabilityOptions &&
                        <DropdownBlock
                            tooltip={localization.get("PerformanceTab_AuthCacheability.Help")}
                            label={localization.get("PerformanceTab_AuthCacheability")}
                            options={props.performanceSettings.authCacheabilityOptions}
                            value={props.performanceSettings.authCacheability}
                            onSelect={this.onChangeField.bind(this, "authCacheability")} />
                    }
                    {props.performanceSettings.unauthCacheabilityOptions &&
                        <DropdownBlock
                            tooltip={localization.get("PerformanceTab_UnauthCacheability.Help")}
                            label={localization.get("PerformanceTab_UnauthCacheability")}
                            options={props.performanceSettings.unauthCacheabilityOptions}
                            value={props.performanceSettings.unauthCacheability}
                            onSelect={this.onChangeField.bind(this, "unauthCacheability")} />
                    }
                    <SwitchBlock label={localization.get("PerformanceTab_SslForCacheSyncrhonization")}
                        onText={localization.get("SwitchOn")}
                        offText={localization.get("SwitchOff")}
                        tooltip={localization.get("PerformanceTab_SslForCacheSyncrhonization.Help")}
                        value={props.performanceSettings.sslForCacheSynchronization}
                        onChange={this.onChangeField.bind(this, "sslForCacheSynchronization")} />
                </div>
            </GridSystem>
            <GridCell className="dnn-servers-grid-panel newSection" style={{ paddingLeft: 0 }}>
                <Label className="header-title" label={localization.get("PerformanceTab_ClientResourceManagementTitle")} />
            </GridCell>
            <WarningBlock label={localization.get("PerformanceTab_MinifactionWarning")} />
            <GridSystem>
                <div className="leftPane">
                    <InputGroup>
                        <Label className="title lowerCase"
                            label={localization.get("PerformanceTab_ClientResourceManagementInfo")}
                            style={{ width: "auto" }} />
                    </InputGroup>
                    <div className="currentHostVersion">
                        <InfoBlock label={localization.get(versionLocalizationKey)}
                            text={version} />
                    </div>
                    <Button type="secondary" style={{ marginBottom: "40px" }} disable={props.incrementingVersion}
                        onClick={this.onIncrementVersion.bind(this)}>{localization.get("PerformanceTab_IncrementVersion")}</Button>
                </div>
                <div className="rightPane borderSeparation">
                    <RadioButtonBlock options={this.getClientResourcesManagementModeOptions()}
                        label={localization.get("PerformanceTab_ClientResourcesManagementMode")}
                        tooltip={localization.get("PerformanceTab_ClientResourcesManagementMode.Help")}
                        onChange={this.onChangeField.bind(this, "clientResourcesManagementMode")}
                        value={props.performanceSettings.clientResourcesManagementMode} />
                    <SwitchBlock label={localization.get("PerformanceTab_EnableCompositeFiles")}
                        onText={localization.get("SwitchOn")}
                        offText={localization.get("SwitchOff")}
                        tooltip={localization.get("PerformanceTab_EnableCompositeFiles.Help")}
                        value={enableCompositeFiles}
                        onChange={this.onChangeField.bind(this, enableCompositeFilesKey)}
                        isGlobal={areGlobalSettings} 
                        globalTooltipStyle={{margin: "8px 0px 0px 5px"}}/>
                    <SwitchBlock label={localization.get("PerformanceTab_MinifyCss")}
                        onText={localization.get("SwitchOn")}
                        offText={localization.get("SwitchOff")}
                        tooltip={localization.get("PerformanceTab_MinifyCss.Help")}
                        value={enableCompositeFiles ? minifyCss : false}
                        readOnly={!enableCompositeFiles}
                        onChange={this.onChangeField.bind(this, minifyCssKey)}
                        isGlobal={areGlobalSettings} 
                        globalTooltipStyle={{margin: "8px 0px 0px 5px"}}/>
                    <SwitchBlock label={localization.get("PerformanceTab_MinifyJs")}
                        onText={localization.get("SwitchOn")}
                        offText={localization.get("SwitchOff")}
                        tooltip={localization.get("PerformanceTab_MinifyJs.Help")}
                        value={enableCompositeFiles ? minifyJs : false}
                        readOnly={!enableCompositeFiles}
                        onChange={this.onChangeField.bind(this, minifyJsKey)}
                        isGlobal={areGlobalSettings} 
                        globalTooltipStyle={{margin: "8px 0px 0px 5px"}}/>
                </div>
            </GridSystem>
            <div className="clear" />
            <div className="buttons-panel">
                <Button type="primary" disabled={props.isSaving}
                    onClick={this.onSave.bind(this)}>{localization.get("SaveButtonText")}</Button>
            </div>
        </div>;
    }
}

Performance.propTypes = {
    performanceSettings: PropTypes.object.isRequired,
    loading: PropTypes.bool,
    isSaving: PropTypes.bool,
    incrementingVersion: PropTypes.bool,
    errorMessage: PropTypes.string,
    onRetrievePerformanceSettings: PropTypes.func.isRequired,
    onChangePerformanceSettingsValue: PropTypes.func.isRequired,
    onSave: PropTypes.func.isRequired,
    onIncrementVersion: PropTypes.func.isRequired,
    infoMessage: PropTypes.string
};

function mapStateToProps(state) {
    return {
        performanceSettings: state.performanceTab.performanceSettings,
        loading: state.performanceTab.saving,
        isSaving: state.performanceTab.saving,
        incrementingVersion: state.performanceTab.incrementingVersion,
        errorMessage: state.logsTab.errorMessage,
        infoMessage: state.performanceTab.infoMessage
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            onRetrievePerformanceSettings: PerformanceTabActions.loadPerformanceSettings,
            onChangePerformanceSettingsValue: PerformanceTabActions.changePerformanceSettingsValue,
            onSave: PerformanceTabActions.save,
            onIncrementVersion: PerformanceTabActions.incrementVersion
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(Performance);