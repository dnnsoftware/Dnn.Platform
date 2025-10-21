import * as React from "react";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import { LocalUpgradeInfo } from "../../../models/LocalUpgradeInfo";
import Localization from "../../../localization";
import UpgradeRow from "./UpgradeRow";

interface IUpgradeListProps {
  upgrades: LocalUpgradeInfo[];
  onDelete: (packageName: string) => void;
}

const UpgradeList: React.FC<IUpgradeListProps> = (props) => {
  const rows = props.upgrades.map((upgrade) => (
    <UpgradeRow key={upgrade.PackageName} upgrade={upgrade} onDelete={(packageName) => props.onDelete(packageName)} />
  ));
  return (
    <div className="grid">
      <div className="row header">
        <GridCell columnSize={40}>{Localization.get("FileName")}</GridCell>
        <GridCell columnSize={20}>{Localization.get("Version")}</GridCell>
        <GridCell columnSize={10}>
          {Localization.get("IsValid")}
        </GridCell>
        <GridCell columnSize={20}>
          {Localization.get("IsOutdated")}
        </GridCell>
        <GridCell columnSize={10} className="right">
          {Localization.get("Actions")}
        </GridCell>
      </div>
      {rows}
    </div>
  );
};

export default UpgradeList;
