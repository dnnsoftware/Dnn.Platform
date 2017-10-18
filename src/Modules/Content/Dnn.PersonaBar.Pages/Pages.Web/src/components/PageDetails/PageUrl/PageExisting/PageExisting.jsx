import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import InputGroup from "dnn-input-group";
import PagePicker from "dnn-page-picker";
import utils from "../../../../utils";
import Localization from "../../../../localization";
import Label from "dnn-label";
import PageUrlCommons from "../PageUrlCommons/PageUrlCommons";

/* eslint-disable spellcheck/spell-checker */
const PageToTestParameters = {
    portalId: -2,
    cultureCode: "",
    isMultiLanguage: false,
    excludeAdminTabs: false,
    disabledNotSelectable: false,
    roles: "",
    sortOrder: 0
};
/* eslint-enable spellcheck/spell-checker */

class PageExisting extends Component {

    onChangePage(value) {
        const {onChangeField} = this.props;
        onChangeField("existingTabRedirection", parseInt(value) === -1 ? "" : value);
    }

    render() {
        const {page} = this.props;
        const serviceFramework = utils.getServiceFramework();
        const noneSpecifiedText = "<" + Localization.get("NoneSpecified") + ">";

        return (
            <div className={styles.pageExisting}>
                <InputGroup>
                    <Label
                        tooltipMessage={Localization.get("ExistingPageTooltip") }
                        label={Localization.get("ExistingPage") } />
                    <PagePicker
                        serviceFramework={serviceFramework}
                        style={{ width: "100%", zIndex: 2 }}
                        OnSelect={this.onChangePage.bind(this) }
                        defaultLabel={noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={PageToTestParameters}
                        selectedTabId={page.existingTabRedirection || -1} />
                </InputGroup>
                <PageUrlCommons {...this.props} />
                <div style={{ clear: "both" }}></div>
            </div>
        );
    }
}

PageExisting.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageExisting;