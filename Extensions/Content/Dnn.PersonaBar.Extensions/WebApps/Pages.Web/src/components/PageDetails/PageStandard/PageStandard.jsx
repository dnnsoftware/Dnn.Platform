import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridSystem, GridCell, InputGroup, SingleLineInputWithError, MultiLineInputWithError, Tags, Label, PagePicker } from "@dnnsoftware/dnn-react-common";
import Localization from "../../../localization";
import styles from "./style.less";
import Utils from "../../../utils";

class PageDetails extends Component {
    constructor() {
        super();
        this.state = {};
    }

    onChangeField(key, event) {
        const { onChangeField } = this.props;
        onChangeField(key, event.target.value);
    }

    onChangeTags(tags) {
        const { onChangeField } = this.props;
        onChangeField("tags", tags.join(","));
    }

    onChangeParentId(value) {
        const { onChangeField } = this.props;
        onChangeField("parentId", value);
    }

    onSelect(parentPageId, parentPageName) {
        const { page } = this.props;
        if (page.parentId !== parseInt(parentPageId)) {
            this.props.onSelectParentPageId(parentPageId, parentPageName);
            this.onChangeParentId(parentPageId);
        }
    }


    render() {
        const { page, errors } = this.props;

        const tags = page.tags ? page.tags.split(",") : [];
        const TabParameters = {
            portalId: -2,
            cultureCode: "",
            isMultiLanguage: false,
            excludeAdminTabs: false,
            roles: "",
            sortOrder: 0,
            includeDisabled: true
        };

        let TabParameters_1 = Object.assign(Object.assign({}, TabParameters), { 
            disabledNotSelectable: false,
            includeDeletedChildren: false
        });
        const sf = Utils.getServiceFramework();

        const defaultLabel = page.hierarchy || Localization.get("NoneSpecified");
        const selectedTabId = page.parentId || -1;
        return (
            <div className={styles.pageStandard}>
                <GridSystem>
                    <GridCell className="left-column">
                        <SingleLineInputWithError
                            style={{ marginBottom: "0px" }}
                            label={Localization.get("Name") + "*"}
                            labelStyle={{ paddingBottom: "10px" }}
                            inputStyle={{ marginBottom: "32px" }}
                            tooltipMessage={Localization.get("NameTooltip")}
                            error={!!errors.name}
                            errorMessage={errors.name}
                            value={page.name}
                            onChange={this.onChangeField.bind(this, "name")}
                            maxLength={200}
                            inputId="name" />
                    </GridCell>
                    <GridCell className="right-column">
                        <SingleLineInputWithError
                            label={Localization.get("Title")}
                            labelStyle={{ paddingBottom: "10px" }}
                            inputStyle={{ marginBottom: "32px" }}
                            tooltipMessage={Localization.get("TitleTooltip")}
                            value={page.title}
                            onChange={this.onChangeField.bind(this, "title")}
                            maxLength={200} />
                    </GridCell>
                </GridSystem>
                <InputGroup style={{ padding: "0px", marginBottom: "0px" }}>
                    <MultiLineInputWithError
                        label={Localization.get("Description")}
                        value={page.description}
                        onChange={this.onChangeField.bind(this, "description")}
                        inputStyle={{ height: "64px", minHeight: "64px", marginBottom: "28px", paddingBottom: "0px" }}
                        labelStyle={{ paddingTop: "0px", paddingBottom: "10px" }}
                        maxLength={500} />
                </InputGroup>
                <InputGroup style={{ padding: "0px", margin: "0px" }}>
                    <MultiLineInputWithError
                        label={Localization.get("Keywords")}
                        value={page.keywords}
                        onChange={this.onChangeField.bind(this, "keywords")}
                        inputStyle={{ height: "64px", minHeight: "32px" }}
                        labelStyle={{ paddingBottom: "10px" }}
                        style={{ padding: "0px" }}
                        maxLength={500} />
                </InputGroup>
                <GridSystem>
                    <GridCell className="left-column input-cell">
                        <Label label={Localization.get("Tags")} style={{ paddingBottom: "10px" }} />
                        <Tags
                            tags={tags}
                            onUpdateTags={this.onChangeTags.bind(this)} />
                    </GridCell>
                    <GridCell className="right-column input-cell">
                        <InputGroup>
                            <Label label={Localization.get("ParentPage")} style={{ paddingBottom: "10px" }} />
                            <PagePicker
                                noneSpecifiedText={Localization.get("NoneSpecified")}
                                IsMultiSelect={false}
                                defaultLabel={defaultLabel}
                                selectedTabId={selectedTabId}
                                PortalTabsParameters={TabParameters_1}
                                style={{ width: "333px", zIndex: 5 }}
                                OnSelect={this.onSelect.bind(this)}
                                serviceFramework={sf} 
                                currentTabId={page.tabId} />
                        </InputGroup>
                    </GridCell>
                </GridSystem>
                <div style={{ clear: "both" }}></div>
            </div>
        );
    }

}

PageDetails.propTypes = {
    onSelectParentPageId: PropTypes.func,
    selectedParentPageName: PropTypes.string,
    selectedParentPageId: PropTypes.number,
    page: PropTypes.object.isRequired,
    errors: PropTypes.object.isRequired,
    onChangeField: PropTypes.func.isRequired
};

const mapStateToProps = (state) => {
    return {
        page : state.pages.selectedPage
    };
};

export default connect(mapStateToProps)(PageDetails);