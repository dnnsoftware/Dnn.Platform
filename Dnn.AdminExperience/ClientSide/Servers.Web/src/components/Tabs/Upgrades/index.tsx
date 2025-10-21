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

interface IProps {
  applicationInfo: any;
}

const UpgradesTab: React.FC<IProps> = (props) => {
  const [upgrades, setUpgrades] = useState<LocalUpgradeInfo[]>([]);
  const [showUploadPanel, setShowUploadPanel] = useState(false);

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

  useEffect(() => {
    if (!showUploadPanel) {
      getUpgrades();
    }
  }, [showUploadPanel]);

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
      {showUploadPanel && (
        <GridCell className="dnn-servers-grid-panel">
          <div className="dnn-servers-grid-panel-actions">
            <div className="dnn-servers-grid-panel-upload">
              <FileUpload
                cancelInstall={() => setShowUploadPanel(false)}
                clearUploadedPackage={() => {}}
                onSelectLegacyType={() => {}}
                selectedLegacyType="Skin"
                alreadyInstalled={false}
                uploadPackage={(
                  file: File,
                  onSuccess: (data: any) => void,
                  onError: (error: any) => void
                ) => upgradeService.uploadPackage(file, onSuccess, onError)}
              />
            </div>
            <Button
              type="secondary"
              size="large"
              onClick={() => {
                setShowUploadPanel(false);
              }}
            >
              {Localization.get("Cancel")}
            </Button>
          </div>
        </GridCell>
      )}
      {!showUploadPanel && (
        <GridCell className="dnn-servers-grid-panel">
          <Label
            className="header-title"
            label={Localization.get("plAvailableUpgrades")}
          />
          <UpgradeList
            upgrades={upgrades}
            onDelete={(packageName) => deletePackage(packageName)}
          />
          <div className="dnn-servers-grid-panel-actions">
            <Button
              type="secondary"
              size="large"
              onClick={() => {
                setShowUploadPanel(true);
              }}
            >
              {Localization.get("UploadPackage")}
            </Button>
          </div>
        </GridCell>
      )}
    </GridCell>
  );
};

export default UpgradesTab;
