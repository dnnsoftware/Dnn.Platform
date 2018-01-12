import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import cloneDeep from "lodash/clonedeep";

import GridCell from "dnn-grid-cell";
import { EyeIcon, TreeEdit } from "dnn-svg-icons";
import OverflowText from "dnn-text-overflow-wrapper";
import Localization from "../../localization";
import utils from "../../utils";
import securityService from "../../services/securityService";

import SearchAdvanced from "./SearchAdvanced";

class SearchResult extends Component {
    constructor(props) {
        super(props);

    }
    
    /* eslint-disable react/no-danger */
    render() {
        const { pageInContextComponents, searchList } = this.props;
        const render_card = (item) => {
            const onNameClick = (item) => {
                this.props.clearSearch(() => {
                    if (item.canManagePage) {
                        this.props.onLoadPage(item.id, () => { this.buildTree(item.id); });
                    }
                    else {
                        this.noPermissionSelectionPageId = item.id;
                        this.props.buildBreadCrumbPath(item.id);
                        this.props.setEmptyStateMessage(Localization.get("NoPermissionEditPage"));
                    }
                });
            };

            const publishedDate = new Date(item.publishDate.split(" ")[0]);

            const addToTags = (newTag) => {
                const condition = this.props.tags.indexOf(newTag) === -1;
                const update = () => {
                    let tags = this.props.tags;
                    tags = tags.length > 0 ? `${tags},${newTag}` : `${newTag}`;
                    tags = this.distinct(tags.split(",")).join(",");
                    this.setState({ tags, filtersUpdated: true }, () => this.props.onSearch());
                };

                condition ? update() : null;
            };
            const getTabPath = (path) => {
                path = path.startsWith("/") ? path.substring(1) : path;
                return path.split("/").join(" / ");
            };

            let visibleMenus = [];
            item.canViewPage && visibleMenus.push(<li onClick={() => this.props.onViewPage(item)}><div title={Localization.get("View")} dangerouslySetInnerHTML={{ __html: EyeIcon }} /></li>);
            item.canAddContentToPage && visibleMenus.push(<li onClick={() => this.props.onViewEditPage(item)}><div title={Localization.get("Edit")} dangerouslySetInnerHTML={{ __html: TreeEdit }} /></li>);
            if (pageInContextComponents && securityService.isSuperUser() && !utils.isPlatform()) {
                let additionalMenus = cloneDeep(pageInContextComponents || []);
                additionalMenus && additionalMenus.map(additionalMenu => {
                    visibleMenus.push(<li onClick={() => (additionalMenu.OnClickAction && typeof additionalMenu.OnClickAction === "function")
                        && this.props.CallCustomAction(additionalMenu.OnClickAction)}><div title={additionalMenu.title} dangerouslySetInnerHTML={{ __html: additionalMenu.icon }} /></li>);
                });
            }
            return (
                <GridCell columnSize={100}>
                    <div className="search-item-card">
                        {this.props.pageTypeSelectorComponents && this.props.pageTypeSelectorComponents.length > 0 &&                            
                            this.props.pageTypeSelectorComponents.map(function (component) {
                                const Component = component.component;
                                return <div className="search-item-thumbnail"><Component page={item} /></div>;
                            })
                        }
                        <div className={`search-item-details${utils.isPlatform() ? " full" : ""}`}>
                            <div className="search-item-details-left">
                                <h1 onClick={() => onNameClick(item)}><OverflowText text={item.name} /></h1>
                                <h2><OverflowText text={getTabPath(item.tabpath)} /></h2>
                            </div>
                            <div className="search-item-details-right">
                                <ul>
                                    {visibleMenus}
                                </ul>
                            </div>
                            <div className="search-item-details-list">
                                <ul>
                                    <li>
                                        <p>{Localization.get("PageType")}:</p>
                                        <p title={this.props.getPageTypeLabel(item.pageType)} onClick={() => { this.props.filterByPageType !== item.pageType && this.setState({ filterByPageType: item.pageType, filtersUpdated: true }, () => this.props.onSearch()); }} >{this.props.getPageTypeLabel(item.pageType)}</p>
                                    </li>
                                    <li>
                                        <p>{Localization.get("lblPublishStatus")}:</p>
                                        <p title={this.props.getPublishStatusLabel(item.publishStatus)} onClick={() => { this.props.filterByPublishStatus !== item.publishStatus && this.setState({ filterByPublishStatus: item.publishStatus, filtersUpdated: true }, () => this.props.onSearch()); }} >{this.props.getPublishStatusLabel(item.publishStatus)}</p>
                                    </li>
                                    <li>
                                        <p >{Localization.get(utils.isPlatform() ? "lblModifiedDate" : "lblPublishDate")}:</p>
                                        <p title={item.publishDate} onClick={() => { (this.props.startDate.toString() !== new Date(item.publishDate.split(" ")[0]).toString() || this.props.startDate.toString() !== this.props.endDate.toString()) && this.setState({ startDate: publishedDate, endDate: publishedDate, startAndEndDateDirty: true, filtersUpdated: true }, () => this.props.onSearch()); }}>{item.publishDate.split(" ")[0]}</p>
                                    </li>
                                </ul>
                            </div>
                            <div className="search-item-details-list">
                                <ul>
                                    {!utils.isPlatform() && <li>
                                        {/* TODO WORKFLOW */}
                                        <p >{Localization.get("WorkflowTitle")}:</p>    
                                        <p title={item.workflowName} onClick={() => { this.state.filterByWorkflow !== item.workflowId && this.setState({ filterByWorkflow: item.workflowId, filterByWorkflowName: item.workflowName, filtersUpdated: true }, () => this.props.onSearch()); }}>{item.workflowName}</p>
                                    </li>
                                    }
                                    <li style={{ width: !utils.isPlatform() ? "64%" : "99%" }}>
                                        <p>{Localization.get("Tags")}:</p>
                                        <p title={item.tags.join(",").trim(",")}>{
                                            item.tags.map((tag, count) => {
                                                return (
                                                    <span>
                                                        <span style={{ marginLeft: "5px" }} onClick={() => addToTags(tag)}>
                                                            {tag}
                                                        </span>
                                                        {count < (item.tags.length - 1) && <span style={{ color: "#000" }}>
                                                            ,
                                                        </span>}
                                                    </span>
                                                );
                                            })}
                                        </p>
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </GridCell >
            );
        };

        return (
            <GridCell columnSize={100} className="fade-in">
                <GridCell columnSize={100} style={{ padding: "20px" }}>
                    <SearchAdvanced {...this.props} />
                    <GridCell columnSize={80} style={{ padding: "0px" }}>
                        <div className="tags-container">
                            {this.props.filters ? this.props.render_filters() : null}
                        </div>
                    </GridCell>
                    <GridCell columnSize={20} style={{ textAlign: "right", padding: "10px", fontWeight: "bold", animation: "fadeIn .15s ease-in forwards" }}>
                        <p>{searchList.length === 0 ? Localization.get("NoPageFound").toUpperCase() : (`${searchList.length} ` + Localization.get(searchList.length > 1 ? "lblPagesFound" : "lblPageFound").toUpperCase())}</p>
                    </GridCell>
                    <GridCell columnSize={100}>
                        {searchList.map((item) => {
                            return render_card(item);
                        })}
                    </GridCell>
                </GridCell>
            </GridCell>
        );
    }
}


SearchResult.propTypes = {
    pageInContextComponents : PropTypes.array,
    searchList : PropTypes.array,
    pageTypeSelectorComponents : PropTypes.array,
    filters : PropTypes.array.isRequired,
    tags : PropTypes.string.isRequired,
    filterByPageType : PropTypes.string.isRequired,
    filterByPublishStatus : PropTypes.bool.isRequired,
    startDate : PropTypes.instanceOf(Date).isRequired,
    endDate : PropTypes.instanceOf(Date).isRequired,
    render_filters : PropTypes.func.isRequired,
    getPageTypeLabel : PropTypes.func.isRequired,
    getPublishStatusLabel : PropTypes.func.isRequired,
    onSearch : PropTypes.func.isRequired,
    clearSearch : PropTypes.func.isRequired,
    clearAdvancedSearch : PropTypes.func.isRequired,

    buildBreadCrumbPath : PropTypes.func.isRequired,
    setEmptyStateMessage : PropTypes.func.isRequired,
    onViewPage : PropTypes.func.isRequired,
    onViewEditPage : PropTypes.func.isRequired,
    CallCustomAction : PropTypes.func.isRequired,
    onLoadPage : PropTypes.func.isRequired

};

const mapStateToProps = (state) => {
    return {
        pageInContextComponents : state.extensions.pageInContextComponents,
        searchList : state.searchList.searchList,
        pageTypeSelectorComponents: state.extensions.pageTypeSelectorComponents
    };
};

export default connect(mapStateToProps)(SearchResult);