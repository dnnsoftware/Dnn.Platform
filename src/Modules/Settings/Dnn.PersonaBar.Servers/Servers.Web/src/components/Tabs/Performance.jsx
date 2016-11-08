import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import GridSystem from "dnn-grid-system";
import InputGroup from "dnn-input-group";
import Button from "dnn-button";
import Label from "dnn-label";
import RadioButtonBlock from "../common/RadioButtonBlock";
import DropdownBlock from "../common/DropdownBlock";
import InfoBlock from "../common/InfoBlock";
import SwitchBlock from "../common/SwitchBlock";
import localization from "../../localization";
import PerformanceTabActions from "../../actions/performanceTab";
import utils from "../../utils";

class Performance extends Component {
    componentWillMount() {
        this.props.onRetrievePerformanceSettings();
    }

    componentWillReceiveProps(newProps) {
        if (this.props.errorMessage !== newProps.errorMessage && newProps.errorMessage) {
            utils.notifyError(newProps.errorMessage);
        }
    }
    
    getClientResourcesManagementModeOptions(){
        return [
            {
                label: localization.get("PerformanceTab_GlobalClientResourcesManagementMode"),
                value: "h"
            },
            {
                label: localization.get("PerformanceTab_SiteClientResourcesManagementMode").replace("{0}", "Test site"),
                value: "p"
            }
        ];
    }
    
    render() {
        const {props} = this;
        const areGlobalSettings = props.performanceSettings.clientResourcesManagementMode === "h";
        const enableCompositeFiles = areGlobalSettings ? props.performanceSettings.hostEnableCompositeFiles 
                                        : props.performanceSettings.portalEnableCompositeFiles;
        const minifyCcs = areGlobalSettings ? props.performanceSettings.hostMinifyCcs 
                                        : props.performanceSettings.portalMinifyCcs;
        const minifyJs = areGlobalSettings ? props.performanceSettings.hostMinifyJs 
                                        : props.performanceSettings.portalMinifyJs;
                                        
        return <div className="dnn-servers-info-panel-big performanceSettingTab">
            <div className="clear" />
            <GridSystem>
                <div className="leftPane">
                    <div className="tooltipAdjustment">
                        {props.performanceSettings.pageStatePersistenceOptions &&
                        <RadioButtonBlock options={props.performanceSettings.pageStatePersistenceOptions}
                            label={localization.get("PerformanceTab_PageStatePersistenceMode")}
                            tooltip={localization.get("PerformanceTab_PageStatePersistenceMode.Help")}
                            onChange={props.onChangePerformanceSettingsMode}
                            value={props.performanceSettings.pageStatePersistence} />
                        }
                        {props.performanceSettings.cacheSettingOptions &&
                        <DropdownBlock
                                tooltip={localization.get("PerformanceTab_CachingProvider.Help")}
                                label={localization.get("PerformanceTab_CachingProvider")}
                                options={props.performanceSettings.cachingProviderOptions}
                                value={props.performanceSettings.cachingProvider}
                                onSelect={props.onChangeCacheSettingMode}
                                />
                        }
                        {props.performanceSettings.moduleCacheProviders &&
                        <DropdownBlock
                                tooltip={localization.get("PerformanceTab_ModuleCacheProviders.Help")}
                                label={localization.get("PerformanceTab_ModuleCacheProviders")}
                                options={props.performanceSettings.moduleCacheProviders}
                                value={props.performanceSettings.moduleCacheProvider}
                                onSelect={props.onChangeCacheSettingMode}
                                />
                        }
                        {props.performanceSettings.pageCacheProviders &&
                        <DropdownBlock
                                tooltip={localization.get("PerformanceTab_PageCacheProviders.Help")}
                                label={localization.get("PerformanceTab_PageCacheProviders")}
                                options={props.performanceSettings.pageCacheProviders}
                                value={props.performanceSettings.pageCacheProvider}
                                onSelect={props.onChangeCacheSettingMode}
                                />
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
                            onSelect={props.onChangeCacheSettingMode}
                            />
                    }
                    {props.performanceSettings.authCacheabilityOptions &&
                    <DropdownBlock
                            tooltip={localization.get("PerformanceTab_AuthCacheability.Help")}
                            label={localization.get("PerformanceTab_AuthCacheability")}
                            options={props.performanceSettings.authCacheabilityOptions}
                            value={props.performanceSettings.authCacheability}
                            onSelect={props.onChangeCacheSettingMode}
                            />
                    }
                    {props.performanceSettings.unauthCacheabilityOptions &&
                    <DropdownBlock
                            tooltip={localization.get("PerformanceTab_UnauthCacheability.Help")}
                            label={localization.get("PerformanceTab_UnauthCacheability")}
                            options={props.performanceSettings.unauthCacheabilityOptions}
                            value={props.performanceSettings.unauthCacheability}
                            onSelect={props.onChangeCacheSettingMode}
                            />
                    }
                    <SwitchBlock label={localization.get("PerformanceTab_SslForCacheSyncrhonization")}
                            tooltip={localization.get("PerformanceTab_SslForCacheSyncrhonization.Help")}
                            value={props.performanceSettings.sslForCacheSynchronization}
                            onChange={props.onChangeCacheSettingMode} />
                </div>
            </GridSystem>
            <div className="dnn-servers-grid-panel newSection" style={{marginLeft: 0}}>
                <Label className="header-title" label={localization.get("PerformanceTab_ClientResourceManagementTitle")} />
            </div>
            <GridSystem>
                <div className="leftPane">
                    <InputGroup>
                        <Label className="title lowerCase"
                            label={localization.get("PerformanceTab_ClientResourceManagementInfo")} 
                            style={{width: "auto"}}/>
                    </InputGroup>
                    <div className="currentHostVersion">
                        <InfoBlock label={localization.get("PerformanceTab_CurrentHostVersion")}
                            text={props.performanceSettings.currentHostVersion}/>
                    </div>
                    <Button type="secondary" style={{marginBottom: "75px"}}
                        onClick={props.onIncrementVersion}>{localization.get("PerformanceTab_IncrementVersion")}</Button>
                </div>
                <div className="rightPane">
                    <RadioButtonBlock options={this.getClientResourcesManagementModeOptions()}
                            label={localization.get("PerformanceTab_ClientResourcesManagementMode")}
                            tooltip={localization.get("PerformanceTab_ClientResourcesManagementMode.Help")}
                            onChange={props.onChangePerformanceSettingsMode}
                            value={props.performanceSettings.clientResourcesManagementMode} />
                    <SwitchBlock label={localization.get("PerformanceTab_EnableCompositeFiles")}
                            tooltip={localization.get("PerformanceTab_EnableCompositeFiles.Help")}
                            value={enableCompositeFiles}
                            onChange={props.onChangeCacheSettingMode}
                            isGlobal={areGlobalSettings} />
                    <SwitchBlock label={localization.get("PerformanceTab_MinifyCss")}
                            tooltip={localization.get("PerformanceTab_MinifyCss.Help")}
                            value={enableCompositeFiles ? minifyCcs : false}
                            readOnly={!enableCompositeFiles}
                            onChange={props.onChangeCacheSettingMode}
                            isGlobal={areGlobalSettings} />
                    <SwitchBlock label={localization.get("PerformanceTab_MinifyJs")}
                            tooltip={localization.get("PerformanceTab_MinifyJs.Help")}
                            value={enableCompositeFiles ? minifyJs : false}
                            readOnly={!enableCompositeFiles}
                            onChange={props.onChangeCacheSettingMode}
                            isGlobal={areGlobalSettings} />
                </div>
            </GridSystem>
            <div className="clear" />
            <div className="buttons-panel">
                 <Button type="primary" 
                    onClick={props.onSave}>{localization.get("SaveButtonText")}</Button>
            </div>
        </div>;
    }
}

Performance.propTypes = {   
    performanceSettings: PropTypes.object.isRequired,
    errorMessage: PropTypes.string,
    onRetrievePerformanceSettings: PropTypes.func.isRequired,
    onChangePerformanceSettingsMode: PropTypes.func.isRequired,
    onChangeCacheSettingMode: PropTypes.func.isRequired,
    onIncrementVersion: PropTypes.func.isRequired
};

function mapStateToProps(state) {    
    return {
        performanceSettings: state.performanceTab.performanceSettings,
        pageStatePersistenceMode: state.pageStatePersistenceMode,
        errorMessage: state.logsTab.errorMessage
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators ({
            onRetrievePerformanceSettings: PerformanceTabActions.loadPerformanceSettings,
            onChangePerformanceSettingsMode: PerformanceTabActions.changePerformanceSettingsMode,
            onChangeCacheSettingMode: PerformanceTabActions.changeCacheSettingMode,
            onIncrementVersion: PerformanceTabActions.incrementVersion,
            onSave: PerformanceTabActions.save
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(Performance);