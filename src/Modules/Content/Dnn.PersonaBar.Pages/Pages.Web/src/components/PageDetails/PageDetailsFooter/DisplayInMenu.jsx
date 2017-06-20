import React, {Component, PropTypes} from "react";
import Switch from "dnn-switch";
import Label from "dnn-label";
import Localization from "../../../localization";

class DisplayInMenu extends Component {

    render() {
        const {props} = this;
        return <div>
            <Label
                labelType="inline"
                tooltipMessage={Localization.get("DisplayInMenuTooltip") }
                label={Localization.get("DisplayInMenu") }
                />
            <Switch
                labelHidden={false}
                onText={Localization.get("On") }
                offText={Localization.get("Off") }
                value={props.includeInMenu}
                onChange={props.onChangeIncludeInMenu} />
            <div style={{ clear: "both" }}></div>
        </div>;
    }
}

DisplayInMenu.propTypes = {
    includeInMenu: PropTypes.bool.isRequired,
    onChangeIncludeInMenu: PropTypes.func.isRequired
};

export default DisplayInMenu;