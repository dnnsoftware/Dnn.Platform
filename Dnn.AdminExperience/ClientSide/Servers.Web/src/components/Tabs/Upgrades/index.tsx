import React, { useEffect, useState } from "react";
import {
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

interface IProps {
  applicationInfo: any;
}

const UpgradesTab: React.FC<IProps> = (props) => {
  const [upgrades, setUpgrades] = useState<LocalUpgradeInfo[]>([]);

  useEffect(() => {
    upgradeService.listUpgrades().then((upgrades) => {
      setUpgrades(upgrades);
    });
  }, []);

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
      <GridCell className="dnn-servers-grid-panel">
        <Label
          className="header-title"
          label={Localization.get("plAvailableUpgrades")}
        />
        <UpgradeList upgrades={upgrades} />
      </GridCell>
    </GridCell>
  );
};

export default UpgradesTab;
