import * as React from "react";
import { LocalUpgradeInfo } from "../../../models/LocalUpgradeInfo";
import {
  Button,
  GridCell,
  TextOverflowWrapper,
  IconButton,
  Collapsible,
  SingleLineInputWithError,
  SvgIcons,
} from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import "./style.less";
import util from "../../../utils";
import upgradeService from "services/upgradeService";

interface IUpgradeRowProps {
  upgrade: LocalUpgradeInfo;
}

const UpgradeRow: React.FC<IUpgradeRowProps> = (props) => {
  const version = `${props.upgrade.Version._Major}.${props.upgrade.Version._Minor}.${props.upgrade.Version._Build}`;

  return (
    <div className="dnn-upgrades-grid-row">
      <GridCell columnSize={40}>
        <TextOverflowWrapper text={props.upgrade.PackageName} />
      </GridCell>
      <GridCell columnSize={20}>
        <TextOverflowWrapper text={version} />
      </GridCell>
      <GridCell columnSize={10}>
        {props.upgrade.IsValid ? "Yes" : "No"}
      </GridCell>
      <GridCell columnSize={20}>
        {props.upgrade.IsOutdated ? "Yes" : "No"}
      </GridCell>
      <GridCell columnSize={10}>
        <IconButton
          type="trash"
          className={"edit-icon"}
          onClick={() => {
            util.confirm(
              Localization.get("DeleteUpgradePackage.Confirm"),
              Localization.get("Confirm"),
              Localization.get("Cancel"),
              function () {
                  upgradeService.deletePackage(props.upgrade.PackageName);
              }.bind(this)
          );
          }}
          title={Localization.get("Delete")}
        />
      </GridCell>
    </div>
  );
};

export default UpgradeRow;
