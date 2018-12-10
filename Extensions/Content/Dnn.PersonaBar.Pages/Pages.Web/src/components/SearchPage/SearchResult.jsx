import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import Localization from "../../localization";
import SearchAdvanced from "./SearchAdvanced";
import SearchResultCard from "./SearchResultCard";
import LazyLoad from "./LazyLoad/LazyLoad";

class SearchResult extends Component {
    constructor(props) {
        super(props);
        this.state = {
            pageList:[],
            page:0
        };

    }

    loadMore(page) {
        if (this.props.filtersUpdated) {
            this.props.onSearchScroll(0);
            this.setState({
                page:0
            });
        } else {
            this.props.onSearchScroll(page);
            this.setState({
                page:page
            });
        }
    }

    hasMoreItems() {
        if (this.props.searchResult.TotalResults === this.props.searchList.length) {
            return false;
        }
        else {
            return true;
        }
    }
    
    /* eslint-disable react/no-danger */
    render() {
        const { searchResult, searchList } = this.props;
        const loader = <div key={-1} className={"lazy-loading"} ></div>;

        return ((this.props.searchResult !== null)?
            <GridCell columnSize={100} className="fade-in">
                <GridCell columnSize={100} style={{ padding: "20px" }}>
                    <SearchAdvanced {...this.props} />
                    <GridCell columnSize={100} style={{ textAlign: "center", padding: "10px", fontWeight: "bold", animation: "fadeIn .15s ease-in forwards" }}>
                        <p>{searchResult.TotalResults === 0 ? Localization.get("NoPageFound").toUpperCase() : (`${searchResult.TotalResults} ` + Localization.get(searchResult.TotalResults > 1 ? "lblPagesFound" : "lblPageFound").toUpperCase())}</p>
                    </GridCell>
                    <GridCell columnSize={100}>
                    <div>
                        <LazyLoad 
                            pageIndex={0} 
                            loadMore={this.loadMore.bind(this)}
                            hasMore={searchList.length !== searchResult.TotalResults}
                            loadingComponent={loader}
                            filtersUpdated={this.props.filtersUpdated}
                        >
                        {searchList.map((item,index) => {
                            return (
                                <SearchResultCard key={index}
                                    item={item}
                                    clearSearch={this.props.clearSearch}
                                    onLoadPage={this.props.onLoadPage}
                                    buildBreadCrumbPath={this.props.buildBreadCrumbPath}
                                    setEmptyStateMessage={this.props.setEmptyStateMessage}
                                    tags={this.props.tags}
                                    onSearch={this.props.onSearch}
                                    onViewPage={this.props.onViewPage}
                                    onViewEditPage={this.props.onViewEditPage}
                                    CallCustomAction={this.props.CallCustomAction}
                                    getPageTypeLabel={this.props.getPageTypeLabel}
                                    getPublishStatusLabel={this.props.getPublishStatusLabel}
                                    filterByPageType={this.props.filterByPageType}
                                    filterByPublishStatus={this.props.filterByPublishStatus}
                                    updateFilterByPageStatusOptions={this.props.updateFilterByPageStatusOptions}
                                    updateFilterByPageTypeOptions={this.props.updateFilterByPageTypeOptions}
                                    updateFilterByWorkflowOptions={this.props.updateFilterByWorkflowOptions}
                                    updateFilterStartEndDate={this.props.updateFilterStartEndDate}
                                    startDate={this.props.startDate}
                                    endDate={this.props.endDate} 
                                    pageInContextComponents={this.props.pageInContextComponents} 
                                    pageTypeSelectorComponents={this.props.pageTypeSelectorComponents}
                                    updateSearchAdvancedTags={this.props.updateSearchAdvancedTags}
                                    filterByWorkflow={this.props.filterByWorkflow}  />
                            );})}
                        </LazyLoad>                            
                        </div>
                    </GridCell>
                </GridCell>
            </GridCell>
        :null);
    }
}


SearchResult.propTypes = {
    pageInContextComponents : PropTypes.array,
    searchList : PropTypes.array,
    searchResult : PropTypes.object,
    pageTypeSelectorComponents : PropTypes.array,
    filters : PropTypes.array.isRequired,
    tags : PropTypes.string.isRequired,
    filterByPageType : PropTypes.string.isRequired,
    filterByPublishStatus : PropTypes.bool.isRequired,
    filtersUpdated : PropTypes.bool.isRequired,
    startDate : PropTypes.instanceOf(Date).isRequired,
    endDate : PropTypes.instanceOf(Date).isRequired,
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
    onLoadPage : PropTypes.func.isRequired,
    updateSearchAdvancedTags : PropTypes.func.isRequired,
    updateFilterByPageStatusOptions : PropTypes.func.isRequired,
    updateFilterByPageTypeOptions : PropTypes.func.isRequired,
    updateFilterByWorkflowOptions : PropTypes.func.isRequired,
    updateFilterStartEndDate : PropTypes.func.isRequired,
    filterByWorkflow: PropTypes.string.isRequired,
    onSearchScroll: PropTypes.func,
    buildTree : PropTypes.func.isRequired,
    pageIndex: PropTypes.number

};

const mapStateToProps = (state) => {
    return {
        pageInContextComponents : state.extensions.pageInContextComponents,
        searchResult : state.searchList.searchResult,
        searchList : state.searchList.searchList,
        pageTypeSelectorComponents: state.extensions.pageTypeSelectorComponents
    };
};

export default connect(mapStateToProps)(SearchResult);