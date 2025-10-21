import * as React from "react";
import { LocalUpgradeInfo } from "../../../models/LocalUpgradeInfo";
import {
  GridCell,
  TextOverflowWrapper,
  IconButton,
} from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import "./style.less";
import util from "../../../utils";

interface IUpgradeRowProps {
  upgrade: LocalUpgradeInfo;
  onDelete: (packageName: string) => void;
  onInstall: (packageName: string) => void;
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
              () => props.onDelete(props.upgrade.PackageName)
            );
          }}
          title={Localization.get("Delete")}
        />
        {props.upgrade.IsValid && !props.upgrade.IsOutdated && (
          <IconButton
            type="traffic"
            className={"edit-icon"}
            onClick={() => {
              util.confirm(
                Localization.get("Backup.Confirm"),
                Localization.get("Confirm"),
                Localization.get("Cancel"),
                () => props.onInstall(props.upgrade.PackageName)
              );
            }}
            title={Localization.get("Install")}
          />
        )}
      </GridCell>
    </div>
  );
};

export default UpgradeRow;
