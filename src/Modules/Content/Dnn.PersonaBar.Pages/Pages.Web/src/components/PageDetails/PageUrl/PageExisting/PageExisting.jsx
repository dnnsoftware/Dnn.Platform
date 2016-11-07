import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import InputGroup from "dnn-input-group";
import PagePicker from "dnn-page-picker";
import utils from "../../../../utils";
import localization from "../../../../localization";
import Label from "dnn-label";

const noneSpecifiedText = "<" + localization.get("NoneSpecified") + ">";

/* eslint-disable spellcheck/spell-checker */
const PageToTestParameters = {
    portalId: -2,
    cultureCode: "",
    isMultiLanguage: false,
    excludeAdminTabs: false,
    disabledNotSelectable: false,
    roles: "0",
    sortOrder: 0
};
/* eslint-enable spellcheck/spell-checker */

class PageExisting extends Component {

    onChangeField(key) {
        const {onChangeField} = this.props;
        onChangeField(key, event.target.value);
    }

    render() {
        const serviceFramework = utils.getServiceFramework(); 

        return (
            <div className={styles.pageExisting}>
                <InputGroup>
                    <Label
                        tooltipMessage={localization.get("existing_page_tooltip")}
                        label={localization.get("Existing Page")} />
                    <PagePicker 
                        serviceFramework={serviceFramework}
                        style={{ width: "100%", zIndex: 2 }}
                        OnSelect={this.onChangeField.bind(this, "url")}
                        defaultLabel={noneSpecifiedText}
                        noneSpecifiedText={noneSpecifiedText}
                        CountText={"{0} Results"}
                        PortalTabsParameters={PageToTestParameters} />
                </InputGroup>
            </div>
        );
    }
}

PageExisting.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageExisting;