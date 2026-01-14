import * as React from "react";
import utils from "../../../utils";
import Localization from "../../../localization";
import "./style.less";

interface IUpgradingProps {}

const Upgrading: React.FC<IUpgradingProps> = (props) => {
  return (
    <div className="dnn-servers-grid-panel-upgrading">
      <p className="upgrading-title">{Localization.get("Upgrading")}</p>
      <p className="upgrading-help">{Localization.get("Upgrading.Help")}</p>
    </div>
  );
};

export default Upgrading;
