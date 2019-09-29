import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import styles from "./style.less";
import { InputGroup, PagePicker, Label } from "@dnnsoftware/dnn-react-common";
import utils from "../../../../utils";
import Localization from "../../../../localization";
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

const mapStateToProps = (state) => {
    return ({page : state.pages.selectedPage});
};

export default connect(mapStateToProps)(PageExisting);