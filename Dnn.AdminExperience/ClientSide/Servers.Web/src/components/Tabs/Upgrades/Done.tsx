import React, { useState } from "react";
import { Button } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import utils from "../../../utils";
import "./style.less";

interface IDoneProps {}

const Done: React.FC<IDoneProps> = (props) => {
  const [clicked, setClicked] = useState(false);
  const siteRoot = utils.getServiceFramework().getSiteRoot();

  return (
    <div className="dnn-servers-grid-panel-done">
      <p className="done-title">{Localization.get("UpgradeDone")}</p>
      <p className="done-link">
        <Button
          type="primary"
          size="large"
          disabled={clicked}
          onClick={() => {
            setClicked(true);
            window.location.href = siteRoot;
          }}
        >
          {Localization.get("UpgradeDoneLink")}
        </Button>
      </p>
    </div>
  );
};

export default Done;
