import React, {Component, PropTypes} from "react";
import styles from "./style.less";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import InputGroup from "dnn-input-group";
import PagePicker from "dnn-page-picker";
import Switch from "dnn-switch";
import utils from "../../../utils";
import localization from "../../../localization";
import Label from "dnn-label";

const noneSpecifiedText = "<" + localization.get("NoneSpecified") + ">";
const PageToTestParameters = {
    portalId: -2,
    cultureCode: "",
    isMultiLanguage: false,
    excludeAdminTabs: false,
    disabledNotSelectable: false,
    roles: "0",
    sortOrder: 0
};

class PageExisting extends Component {

    onChangeField(key, value){

    }

    render() {
        const {page} = this.props;
        
        return (
            <div className={styles.pageExisting}>
                <div className="existing-page-box">
                    <InputGroup>
                        <Label
                            tooltipMessage={localization.get("existing_page_tooltip")}
                            label={localization.get("Existing Page")} />
                        <PagePicker 
                            serviceFramework={utils.utilities.sf}
                            style={{ width: "100%", zIndex: 2 }}
                            OnSelect={this.onChangeField.bind(this, "url")}
                            defaultLabel={noneSpecifiedText}
                            noneSpecifiedText={noneSpecifiedText}
                            CountText={"{0} Results"}
                            PortalTabsParameters={PageToTestParameters} />
                    </InputGroup>
                    <GridSystem className="existing-page-attributes">
                        <GridCell className="left-column">
                            <Label
                                labelType="inline"
                                tooltipMessage={localization.get("permanent_redirect_tooltip")}
                                label={localization.get("Permanent Redirect")}
                                />
                            <Switch
                                labelHidden={true}
                                value={page.permanentRedirect}
                                onChange={this.onChangeField.bind(this, "permanentRedirect")} />
                        </GridCell>
                        <GridCell className="right-column">
                            <Label
                                labelType="inline"
                                tooltipMessage={localization.get("open_new_window_tooltip")}
                                label={localization.get("Open Link in New Window")}
                                />
                            <Switch
                                labelHidden={true}
                                value={page.openNewWindow}
                                onChange={this.onChangeField.bind(this, "openNewWindow")} />
                        </GridCell>
                    </GridSystem>
                </div>
            </div>
        );
    }
}

PageExisting.propTypes = {
    page: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default PageExisting;