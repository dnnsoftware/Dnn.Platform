import React, { useEffect, useState } from "react";
import {
  Button,
  GridCell,
  GridSystem,
  Label,
  TextOverflowWrapper as OverflowText,
} from "@dnnsoftware/dnn-react-common";
import InfoBlock from "../../common/InfoBlock";
import Localization from "../../../localization";
import "../tabs.less";
import "./style.less";
import upgradeService from "../../../services/upgradeService";
import { LocalUpgradeInfo } from "../../../models/LocalUpgradeInfo";
import UpgradeList from "./UpgradeList";
import FileUpload from "./Upload";
import Upgrading from "./Upgrading";
import Done from "./Done";
import { UpgradeSettings } from "models/UpgradeSettings";

interface IProps {
  applicationInfo: any;
}

const UpgradesTab: React.FC<IProps> = (props) => {
  const [upgrades, setUpgrades] = useState<LocalUpgradeInfo[]>([]);
  const [upgradeSettings, setUpgradeSettings] = useState<UpgradeSettings>({
    AllowDnnUpgradeUpload: false,
  });
  const [panelToShow, setPanelToShow] = useState<
    "done" | "upload" | "upgrading" | "list"
  >("list");
  const [showUploadCancelButton, setShowUploadCancelButton] = useState(true);

  const getUpgrades = () => {
    upgradeService.listUpgrades().then((upgrades) => {
      setUpgrades(upgrades);
    });
  };

  const deletePackage = (packageName: string) => {
    upgradeService.deletePackage(packageName).then(() => {
      getUpgrades();
    });
  };

  const installPackage = (packageName: string) => {
    setPanelToShow("upgrading");
    upgradeService.startUpgrade(packageName).then(() => {
      setPanelToShow("done");
    });
  };

  useEffect(() => {
    upgradeService.getUpgradeSettings().then((settings) => {
      setUpgradeSettings(settings);
    });
  }, []);

  useEffect(() => {
    if (panelToShow === "list") {
      getUpgrades();
    }
  }, [panelToShow]);

  let panel = null;
  switch (panelToShow) {
    case "list":
      panel = (
        <>
          <Label
            className="header-title"
            label={Localization.get("plAvailableUpgrades")}
          />
          <UpgradeList
            upgrades={upgrades}
            onDelete={(packageName) => deletePackage(packageName)}
            onInstall={(packageName) => installPackage(packageName)}
          />
          {upgradeSettings.AllowDnnUpgradeUpload && (
            <div className="dnn-servers-grid-panel-actions">
              <Button
                type="secondary"
                size="large"
                onClick={() => {
                  setPanelToShow("upload");
                }}
              >
                {Localization.get("UploadPackage")}
              </Button>
            </div>
          )}
        </>
      );
      break;
    case "upload":
      panel = (
        <div className="dnn-servers-grid-panel-actions">
          <div className="dnn-servers-grid-panel-upload">
            <FileUpload
              cancelInstall={() => setPanelToShow("list")}
              clearUploadedPackage={() => {}}
              uploadComplete={() => {
                setShowUploadCancelButton(false);
                setTimeout(() => {
                  setPanelToShow("list");
                  setShowUploadCancelButton(true);
                }, 500);
              }}
            />
          </div>
          {showUploadCancelButton && (
            <Button
              type="secondary"
              size="large"
              onClick={() => {
                setPanelToShow("list");
              }}
            >
              {Localization.get("Cancel")}
            </Button>
          )}
        </div>
      );
      break;
    case "upgrading":
      panel = <Upgrading />;
      break;
    case "done":
      panel = <Done />;
      break;
  }

  return (
    <GridCell>
      <GridCell className="dnn-servers-info-panel">
        <GridSystem>
          <GridCell>
            <InfoBlock
              label={Localization.get("plProduct")}
              tooltip={Localization.get("plProduct.Help")}
              text={props.applicationInfo.product || "..."}
            />
          </GridCell>
          <GridCell>
            <InfoBlock
              label={Localization.get("plCurrentVersion")}
              tooltip={Localization.get("plCurrentVersion.Help")}
              text={props.applicationInfo.version || "..."}
            />
          </GridCell>
        </GridSystem>
      </GridCell>
      <GridCell className="dnn-servers-grid-panel">{panel}</GridCell>
    </GridCell>
  );
};

export default UpgradesTab;
