import React, { Component, PropTypes } from "react";
import Localization from "../../localization";
import Label from "dnn-label";
import Switch from "dnn-switch";

class DisplayInMenu extends Component {

    render() {
        const {props} = this;
        return <div className="input-group">
            <Label
                labelType="inline"
                tooltipMessage={Localization.get("DisplayInMenuTooltip")}
                label={Localization.get("DisplayInMenu")} />
            <Switch
                labelHidden={false}
                onText={Localization.get("On")}
                offText={Localization.get("Off")}
                value={props.includeInMenu}
                onChange={(value) => props.onChangeValue("includeInMenu", value)} />
            <div style={{ clear: "both" }}></div>
        </div>;
    }
}

DisplayInMenu.propTypes = {
    includeInMenu: PropTypes.bool.isRequired,
    onChangeValue: PropTypes.func.isRequired
};

export default DisplayInMenu;