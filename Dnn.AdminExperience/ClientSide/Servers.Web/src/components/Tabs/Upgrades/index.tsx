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
import upgradeService from "services/upgradeService";
import { UpgradeFile } from "models/upgradeFile";

interface IProps {
  applicationInfo: any;
}

const UpgradesTab: React.FC<IProps> = (props) => {
  const [upgrades, setUpgrades] = useState<UpgradeFile[]>([]);
  console.log("here3");

  useEffect(() => {
    console.log("here2");
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
      </GridCell>
    </GridCell>
  );
};

export default UpgradesTab;
