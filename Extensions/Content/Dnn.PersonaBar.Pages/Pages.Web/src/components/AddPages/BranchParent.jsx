import React, {Component} from "react";
import PropTypes from "prop-types";
import { PagePicker, Label } from "@dnnsoftware/dnn-react-common";
import Localization from "../../localization";
import utils from "../../utils";

const PageToTestParameters = {
    portalId: -2,
    cultureCode: "",
    isMultiLanguage: false,
    excludeAdminTabs: false,
    disabledNotSelectable: false,
    roles: "",
    sortOrder: 0
};

class BranchParent extends Component {

    render() {
        const {props} = this;
        const noneSpecifiedText = utils.getPortalName() || Localization.get("Site");
        return <div className="input-group">
            <Label
                label={Localization.get("BranchParent")} />
            <PagePicker
                serviceFramework={utils.getServiceFramework()}
                style={{ width: "100%", zIndex: 2 }}
                OnSelect={(value) => props.onChangeValue("parentId", value)}
                defaultLabel={noneSpecifiedText}
                noneSpecifiedText={noneSpecifiedText}
                CountText={"{0} Results"}
                PortalTabsParameters={PageToTestParameters}
                selectedTabId={props.parentId || -1} />
        </div>;
    }
}

BranchParent.propTypes = {
    parentId: PropTypes.number.isRequired,
    onChangeValue: PropTypes.func.isRequired
};

export default BranchParent;