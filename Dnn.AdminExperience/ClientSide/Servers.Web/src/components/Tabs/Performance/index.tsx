import React, { useEffect, useRef } from "react";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import {
  GridSystem,
  GridCell,
  InputGroup,
  Button,
  Label,
} from "@dnnsoftware/dnn-react-common";
import RadioButtonBlock from "../../common/RadioButtonBlock";
import DropdownBlock from "../../common/DropdownBlock";
import InfoBlock from "../../common/InfoBlock";
import SwitchBlock from "../../common/SwitchBlock";
import WarningBlock from "../../common/WarningBlock";
import localization from "../../../localization";
import PerformanceTabActions from "../../../actions/performanceTab";
import utils from "../../../utils";

interface ISelectOption {
  label: string;
  value: string;
}

interface IPerformanceSettings {
  portalName: string;
  cachingProvider: string;
  pageStatePersistence: string;
  moduleCacheProvider: string;
  pageCacheProvider: string;
  cacheSetting: string;
  authCacheability: string;
  unauthCacheability: string;
  sslForCacheSynchronization: boolean;
  crmOverrideDefaultSettings: boolean;
  currentHostVersion: number;
  currentPortalVersion: number;
  cachingProviderOptions: ISelectOption[];
  pageStatePersistenceOptions: ISelectOption[];
  moduleCacheProviders: ISelectOption[];
  pageCacheProviders: ISelectOption[];
  cacheSettingOptions: ISelectOption[];
  authCacheabilityOptions: ISelectOption[];
  unauthCacheabilityOptions: ISelectOption[];
}

interface IStateProps {
  performanceSettings: IPerformanceSettings;
  loading: boolean;
  isSaving: boolean;
  incrementingVersion: boolean;
  errorMessage: string;
  infoMessage: string;
  isLoading: boolean;
}

interface IDispatchProps {
  onRetrievePerformanceSettings: () => void;
  onChangePerformanceSettingsValue: (key: string, value: any) => void;
  onSave: (settings: IPerformanceSettings) => void;
  onIncrementVersion: (isGlobalSettings: boolean) => void;
}

type IProps = IStateProps & IDispatchProps;

const Performance: React.FC<IProps> = ({
  performanceSettings,
  isSaving,
  incrementingVersion,
  errorMessage,
  infoMessage,
  isLoading,
  onRetrievePerformanceSettings,
  onChangePerformanceSettingsValue,
  onSave,
  onIncrementVersion,
}) => {
  const prevInfoMessage = useRef(infoMessage);
  const prevErrorMessage = useRef(errorMessage);

  useEffect(() => {
    onRetrievePerformanceSettings();
  }, []);

  useEffect(() => {
    if (prevInfoMessage.current !== infoMessage && infoMessage) {
      utils.notify(infoMessage);
    }
    prevInfoMessage.current = infoMessage;
  }, [infoMessage]);

  useEffect(() => {
    if (prevErrorMessage.current !== errorMessage && errorMessage) {
      utils.notifyError(errorMessage);
    }
    prevErrorMessage.current = errorMessage;
  }, [errorMessage]);

  const handleSave = () => {
    onSave(performanceSettings);
  };

  const handleIncrementVersion = (isGlobalSettings: boolean) => {
    utils.confirm(
      localization.get("PerformanceTab_PortalVersionConfirmMessage"),
      localization.get("PerformanceTab_PortalVersionConfirmYes"),
      localization.get("PerformanceTab_PortalVersionConfirmNo"),
      () => onIncrementVersion(isGlobalSettings),
      () => {},
    );
  };

  const onChangeField = (key: string, event: any) => {
    let value = event;
    if (event && event.value !== undefined) {
      value = event.value;
    } else if (event && event.target && event.target.value !== undefined) {
      value = event.target.value;
    }
    onChangePerformanceSettingsValue(key, value);
  };

  if (isLoading) {
    return null;
  }

  return (
    <div className="dnn-servers-info-panel-big performanceSettingTab">
      <WarningBlock label={localization.get("PerformanceTab_AjaxWarning")} />
      <GridSystem>
        <div className="leftPane">
          <div className="tooltipAdjustment">
            {performanceSettings.pageStatePersistenceOptions && (
              <RadioButtonBlock
                options={performanceSettings.pageStatePersistenceOptions}
                label={localization.get(
                  "PerformanceTab_PageStatePersistenceMode",
                )}
                tooltip={localization.get(
                  "PerformanceTab_PageStatePersistenceMode.Help",
                )}
                onChange={(e: any) => onChangeField("pageStatePersistence", e)}
                value={performanceSettings.pageStatePersistence}
              />
            )}
          </div>
        </div>
        <div className="rightPane">
          {performanceSettings.cacheSettingOptions && (
            <DropdownBlock
              tooltip={localization.get("PerformanceTab_CacheSetting.Help")}
              label={localization.get("PerformanceTab_CacheSetting")}
              options={performanceSettings.cacheSettingOptions}
              value={performanceSettings.cacheSetting}
              onSelect={(e: any) => onChangeField("cacheSetting", e)}
            />
          )}
        </div>
      </GridSystem>
      <GridSystem>
        <div className="leftPane">
          <div className="tooltipAdjustment">
            {performanceSettings.cacheSettingOptions && (
              <DropdownBlock
                tooltip={localization.get(
                  "PerformanceTab_CachingProvider.Help",
                )}
                label={localization.get("PerformanceTab_CachingProvider")}
                options={performanceSettings.cachingProviderOptions}
                value={performanceSettings.cachingProvider}
                onSelect={(e: any) => onChangeField("cachingProvider", e)}
              />
            )}
          </div>
        </div>
        <div className="rightPane">
          {performanceSettings.authCacheabilityOptions && (
            <DropdownBlock
              tooltip={localization.get("PerformanceTab_AuthCacheability.Help")}
              label={localization.get("PerformanceTab_AuthCacheability")}
              options={performanceSettings.authCacheabilityOptions}
              value={performanceSettings.authCacheability}
              onSelect={(e: any) => onChangeField("authCacheability", e)}
            />
          )}
        </div>
      </GridSystem>
      <GridSystem>
        <div className="leftPane">
          <div className="tooltipAdjustment">
            {performanceSettings.moduleCacheProviders && (
              <DropdownBlock
                tooltip={localization.get(
                  "PerformanceTab_ModuleCacheProviders.Help",
                )}
                label={localization.get("PerformanceTab_ModuleCacheProviders")}
                options={performanceSettings.moduleCacheProviders}
                value={performanceSettings.moduleCacheProvider}
                onSelect={(e: any) => onChangeField("moduleCacheProvider", e)}
              />
            )}
          </div>
        </div>
        <div className="rightPane">
          {performanceSettings.unauthCacheabilityOptions && (
            <DropdownBlock
              tooltip={localization.get(
                "PerformanceTab_UnauthCacheability.Help",
              )}
              label={localization.get("PerformanceTab_UnauthCacheability")}
              options={performanceSettings.unauthCacheabilityOptions}
              value={performanceSettings.unauthCacheability}
              onSelect={(e: any) => onChangeField("unauthCacheability", e)}
            />
          )}
        </div>
      </GridSystem>
      <GridSystem>
        <div className="leftPane">
          <div className="tooltipAdjustment">
            {performanceSettings.pageCacheProviders && (
              <DropdownBlock
                tooltip={localization.get(
                  "PerformanceTab_PageCacheProviders.Help",
                )}
                label={localization.get("PerformanceTab_PageCacheProviders")}
                options={performanceSettings.pageCacheProviders}
                value={performanceSettings.pageCacheProvider}
                onSelect={(e: any) => onChangeField("pageCacheProvider", e)}
              />
            )}
          </div>
        </div>
        <div className="rightPane">
          <SwitchBlock
            label={localization.get(
              "PerformanceTab_SslForCacheSyncrhonization",
            )}
            onText={localization.get("SwitchOn")}
            offText={localization.get("SwitchOff")}
            tooltip={localization.get(
              "PerformanceTab_SslForCacheSyncrhonization.Help",
            )}
            value={performanceSettings.sslForCacheSynchronization}
            onChange={(e: any) =>
              onChangeField("sslForCacheSynchronization", e)
            }
          />
        </div>
      </GridSystem>
      <GridCell
        className="dnn-servers-grid-panel newSection"
        style={{ paddingLeft: 0 }}
      >
        <Label
          className="header-title"
          label={localization.get(
            "PerformanceTab_ClientResourceManagementTitle",
          )}
        />
        <InputGroup>
          <Label
            className="title lowerCase"
            label={localization.get(
              "PerformanceTab_ClientResourceManagementInfo",
            )}
            style={{ width: "auto", marginBottom: "10px", marginTop: "10px" }}
          />
        </InputGroup>
      </GridCell>
      <GridSystem>
        <div className="leftPane">
          <div className="currentHostVersion">
            <InfoBlock
              label={localization.get("PerformanceTab_CurrentHostVersion")}
              text={performanceSettings.currentHostVersion}
            />
          </div>
          <Button
            type="secondary"
            style={{ marginBottom: "40px" }}
            disabled={incrementingVersion}
            onClick={() => handleIncrementVersion(true)}
          >
            {localization.get("PerformanceTab_IncrementVersion")}
          </Button>
        </div>
        <div className="rightPane">
          <SwitchBlock
            label={localization.get(
              "PerformanceTab_CrmOverrideDefaultSettings",
            )}
            onText={localization.get("SwitchOn")}
            offText={localization.get("SwitchOff")}
            tooltip={localization.get(
              "PerformanceTab_CrmOverrideDefaultSettings.Help",
            )}
            value={performanceSettings.crmOverrideDefaultSettings}
            onChange={(e: any) =>
              onChangeField("crmOverrideDefaultSettings", e)
            }
          />
          <div className="currentHostVersion">
            <InfoBlock
              label={localization.get("PerformanceTab_CurrentPortalVersion")}
              text={performanceSettings.currentPortalVersion}
            />
          </div>
          <Button
            type="secondary"
            style={{ marginBottom: "40px" }}
            disabled={!performanceSettings.crmOverrideDefaultSettings || incrementingVersion}
            onClick={() => handleIncrementVersion(false)}
          >
            {localization.get("PerformanceTab_IncrementVersion")}
          </Button>
        </div>
      </GridSystem>
      <div className="clear" />
      <div className="buttons-panel">
        <Button type="primary" disabled={isSaving} onClick={handleSave}>
          {localization.get("SaveButtonText")}
        </Button>
      </div>
    </div>
  );
};

const mapStateToProps = (state: any): IStateProps => ({
  performanceSettings: state.performanceTab.performanceSettings,
  loading: state.performanceTab.saving,
  isSaving: state.performanceTab.saving,
  incrementingVersion: state.performanceTab.incrementingVersion,
  errorMessage: state.logsTab.errorMessage,
  infoMessage: state.performanceTab.infoMessage,
  isLoading: state.performanceTab.loading,
});

const mapDispatchToProps = (dispatch: any): IDispatchProps => ({
  ...bindActionCreators(
    {
      onRetrievePerformanceSettings:
        PerformanceTabActions.loadPerformanceSettings,
      onChangePerformanceSettingsValue:
        PerformanceTabActions.changePerformanceSettingsValue,
      onSave: PerformanceTabActions.save,
      onIncrementVersion: PerformanceTabActions.incrementVersion,
    },
    dispatch,
  ),
});

export default connect(mapStateToProps, mapDispatchToProps)(Performance);
