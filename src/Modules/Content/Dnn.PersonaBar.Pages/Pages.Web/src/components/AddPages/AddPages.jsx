import React, {Component, PropTypes} from "react";
import Button from "dnn-button";
import Localization from "../../localization";
import styles from "./style.less";
import MultiLineInput from "dnn-multi-line-input";
import Label from "dnn-label";
import PagePicker from "dnn-page-picker";
import utils from "../../utils";
import MultiLineInputWithError from "dnn-multi-line-input-with-error";
import Tags from "dnn-tags";
import Switch from "dnn-switch";
import Scheduler from "../common/Scheduler/Scheduler";

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

class AddPages extends Component {
    onChangeValue(key, value) {
        const {onChangeField} = this.props;
        onChangeField(key, value);
    }

    onChangeEvent(key, event) {
        this.onChangeValue(key, event.target.value);
    }

    onChangeTags(tags) {
        this.onChangeValue("tags", tags.join(","));
    }
 
    render() {
        const {bulkPage, onCancel, onSave} = this.props;
        const noneSpecifiedText = Localization.get("MyWebsite");
        const tags = bulkPage.tags ? bulkPage.tags.split(",") : [];

        return (
            <div className={styles.addPages}>
                <div className="grid-columns">
                    <div className="left-column">
                        <div className="column-heading">
                            {Localization.get("BulkPageSettings")}
                        </div>
                        <div className="input-group">
                            <Label
                                label={Localization.get("BranchParent")} />
                            <PagePicker 
                                serviceFramework={utils.getServiceFramework()}
                                style={{ width: "100%", zIndex: 2 }}
                                OnSelect={(value) => this.onChangeValue("parentId", value)}
                                defaultLabel={noneSpecifiedText}
                                noneSpecifiedText={noneSpecifiedText}
                                CountText={"{0} Results"}
                                PortalTabsParameters={PageToTestParameters}
                                selectedTabId={bulkPage.parentId || -1} />
                        </div>
                        <div className="input-group">
                            <MultiLineInputWithError
                                className="keywords-field"
                                label={Localization.get("Keywords")}
                                value={bulkPage.keywords} 
                                onChange={(value) => this.onChangeEvent("keywords", value)} />
                            <div style={{clear: "both"}}></div>
                        </div>
                        <div className="input-group">
                            <Label label={Localization.get("Tags")}/>
                            <Tags 
                                tags={tags} 
                                onUpdateTags={(tags) => this.onChangeTags(tags)}/>
                            <div style={{clear: "both"}}></div>
                        </div>
                        <div className="input-group">
                            <Label
                                labelType="inline"
                                tooltipMessage={Localization.get("DisplayInMenuTooltip")}
                                label={Localization.get("DisplayInMenu")}
                                />
                            <Switch
                                labelHidden={true}
                                value={bulkPage.includeInMenu}
                                onChange={(value) => this.onChangeValue("includeInMenu", value)} />
                            <div style={{clear: "both"}}></div>
                        </div>
                        <div className="input-group">
                            <Scheduler 
                                startDate={bulkPage.startDate} 
                                endDate={bulkPage.endDate}
                                onChange={(key, value) => this.onChangeValue(key, value)} />
                            <div style={{clear: "both"}}></div>
                        </div>
                    </div>
                    <div className="right-column">
                        <div className="column-heading">
                            {Localization.get("BulkPagesToAdd")}
                        </div>
                        <Label
                            label={Localization.get("BulkPagesLabel")} />
                        <MultiLineInput
                            onChange={(event) => this.onChangeEvent("bulkPages", event)}
                            value={bulkPage.bulkPages}
                            className="bulk-page-input" />
                    </div>
                </div>
                <div style={{clear: "both"}}></div>
                <div className="buttons-box">
                    <Button
                        type="secondary"
                        onClick={onCancel}>
                        {Localization.get("Cancel")}
                    </Button>
                    <Button
                        type="primary"
                        onClick={onSave}
                        disabled={!bulkPage.bulkPages}>
                        {Localization.get("AddPages")}
                    </Button>
                </div>
            </div>
        );
    }
}

AddPages.propTypes = {
    bulkPage: PropTypes.object.isRequired,
    onCancel: PropTypes.func.isRequired,
    onSave: PropTypes.func.isRequired,
    onChangeField: PropTypes.func.isRequired
};

export default AddPages;