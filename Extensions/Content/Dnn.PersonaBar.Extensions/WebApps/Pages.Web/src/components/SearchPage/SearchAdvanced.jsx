import React, { Component } from "react";
import PropTypes from "prop-types";
import { Collapsible, SvgIcons } from "@dnnsoftware/dnn-react-common";
import Localization from "../../localization";
import SearchAdvancedDetails from "./SearchAdvancedDetails";
import "./styles.less";


class SearchAdvanced extends Component {
    constructor(props) {
        super(props);
        this.state = {
            collapsed:false
        };
    }

    toggle() {
        this.setState({
            collapsed:!this.state.collapsed
        });
    }

    /*eslint-disable react/no-danger*/
    render() {
        return (
            <div className={`advancedCollapsibleComponent ${this.state.collapsed?"open":""}`}>
                <div className={`collapsible-header false` } onClick={this.toggle.bind(this)}>
                    <div className="search-advanced-header">
                        <div className="search-advanced-icon" dangerouslySetInnerHTML={{ __html: SvgIcons.SearchIcon }}>
                        </div>
                        <div className="search-advanced-label">
                            {Localization.get("AdvancedFilters")}
                        </div>
                    </div>
                    <span
                        className={`collapse-icon ${this.state.collapsed?"collapsed":""}`}>
                    </span>
                </div>
                <Collapsible isOpened={this.state.collapsed} className="search-header-collapsible">
                        {this.state.collapsed && 
                            <SearchAdvancedDetails 
                                getFilterByPageTypeOptions={this.props.getFilterByPageTypeOptions}
                                getFilterByPageStatusOptions={this.props.getFilterByPageStatusOptions}
                                getFilterByWorkflowOptions={this.props.getFilterByWorkflowOptions}
                                filterByWorkflow={this.props.filterByWorkflow}
                                onApplyChangesDropdownDayPicker={this.props.onApplyChangesDropdownDayPicker}
                                updateFilterByPageTypeOptions={this.props.updateFilterByPageTypeOptions}
                                updateFilterByPageStatusOptions={this.props.updateFilterByPageStatusOptions}
                                updateFilterByWorkflowOptions={this.props.updateFilterByWorkflowOptions}
                                filterByPageType={this.props.filterByPageType}
                                filterByPublishStatus={this.props.filterByPublishStatus}
                                onDayClick={this.props.onDayClick}
                                startDate={this.props.startDate}
                                endDate={this.props.endDate}
                                startAndEndDateDirty={this.props.startAndEndDateDirty}
                                tags={this.props.tags}
                                onSearch={this.props.onSearch}
                                collapsed={this.state.collapsed}
                                clearAdvancedSearch={this.props.clearAdvancedSearch}
                                clearAdvancedSearchDateInterval={this.props.clearAdvancedSearchDateInterval}
                                updateSearchAdvancedTags={this.props.updateSearchAdvancedTags}
                            />
                        }
                </Collapsible>    
            </div>
        );
    }
}

SearchAdvanced.propTypes = {
    getFilterByPageTypeOptions : PropTypes.func.isRequired,
    getFilterByPageStatusOptions : PropTypes.func.isRequired,
    getFilterByWorkflowOptions : PropTypes.func.isRequired,
    filterByWorkflow : PropTypes.number,
    onApplyChangesDropdownDayPicker : PropTypes.func.isRequired,
    updateFilterByPageTypeOptions : PropTypes.func.isRequired,
    updateFilterByPageStatusOptions : PropTypes.func.isRequired,
    updateFilterByWorkflowOptions : PropTypes.func.isRequired,
    filterByPageType : PropTypes.string,
    filterByPublishStatus : PropTypes.string,
    onDayClick : PropTypes.func.isRequired,
    startDate : PropTypes.instanceOf(Date).isRequired,
    endDate : PropTypes.instanceOf(Date).isRequired,
    startAndEndDateDirty : PropTypes.bool.isRequired,
    tags : PropTypes.string.isRequired,
    onSearch : PropTypes.func.isRequired,
    clearAdvancedSearch : PropTypes.func.isRequired,
    clearAdvancedSearchDateInterval : PropTypes.func.isRequired,
    updateSearchAdvancedTags : PropTypes.func.isRequired
};

export default SearchAdvanced;