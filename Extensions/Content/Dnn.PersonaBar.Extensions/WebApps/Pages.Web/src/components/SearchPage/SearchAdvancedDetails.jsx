import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "../../localization";
import { GridCell, Tags, Dropdown, Button, SvgIcons } from "@dnnsoftware/dnn-react-common";
import DropdownDayPicker from "../DropdownDayPicker/DropdownDayPicker";
import utils from "../../utils";

class SearchAdvancedDetails extends Component {

    constructor(props) {
        super(props); 
        this.state = {
            DropdownCalendarIsActive : false,
            tags : props.tags?props.tags.split(","):[]
        };
    }

    componentDidUpdate(prevProps) {
        if (this.props.tags !== prevProps.tags) {
            this.setState({
                tags: this.props.tags.split(",")
            });
        }
    }

    getDateLabel() {
        let filterByDateText = utils.isPlatform() ? "FilterByModifiedDateText" : "FilterByPublishDateText";
        const { startDate, endDate, startAndEndDateDirty } = this.props;
        let label = Localization.get(filterByDateText);
        if (startAndEndDateDirty) {
            const fullStartDate = utils.formatDate(startDate);
            const fullEndDate = utils.formatDate(endDate);
            label = fullStartDate !== fullEndDate ? `${fullStartDate} - ${fullEndDate}` : `${fullStartDate}`;
        }
        return label;
    }
    
    toggleDropdownCalendar() {
        this.setState({
            DropdownCalendarIsActive : !this.state.DropdownCalendarIsActive
        });
    }
    getPageStatusLabel() {
        return (
            this.props.filterByPublishStatus ? 
            this.props.getFilterByPageStatusOptions().find(
                x => x.value === this.props.filterByPublishStatus.toLowerCase()
            ).label : 
            Localization.get("FilterbyPublishStatusText"));
    }

    onClear() {
        this.setState({
            DropdownCalendarIsActive : false,
            tags:[]
        });
        this.props.clearAdvancedSearch();
    }

    onChangeTags(evnt) {
        this.setState({
            tags: evnt
        },()=>this.props.updateSearchAdvancedTags(this.state.tags.join(",")));        
    }

    onTagClick() {
        this.node.getElementsByClassName("input-container")[0].childNodes[0].focus();
    }

    applyDateFilter() {
        this.toggleDropdownCalendar(); 
        this.props.onApplyChangesDropdownDayPicker();
    }

    clearDateFilter() {
        this.toggleDropdownCalendar();
        this.props.clearAdvancedSearchDateInterval();
    }

    render() {
        return (
            <div className="search-more-flyout" ref={node => this.node = node}>
                <GridCell columnSize={70} style={{ padding: "5px 5px 5px 10px" }}>
                    <h1>{Localization.get("lblGeneralFilters").toUpperCase()}</h1>
                </GridCell>
                <GridCell columnSize={30} style={{ paddingLeft: "10px" }}>
                    <h1>{Localization.get("lblTagFilters").toUpperCase()}</h1>
                </GridCell>
                <GridCell columnSize={70} style={{ padding: "5px" }}>
                    <GridCell columnSize={100} >
                        <GridCell columnSize={50} style={{ padding: "5px" }}>
                            <Dropdown
                                className="more-dropdown"
                                options={this.props.getFilterByPageTypeOptions()}
                                label={this.props.filterByPageType ? this.props.getFilterByPageTypeOptions().find(x => x.value === this.props.filterByPageType.toLowerCase()).label : Localization.get("FilterbyPageTypeText")}
                                value={this.props.filterByPageType !== "" && this.props.filterByPageType}
                                onSelect={(data) => { this.props.updateFilterByPageTypeOptions(data); }}
                                withBorder={true}
                            />
                        </GridCell>
                        <GridCell columnSize={50} style={{ padding: "5px 5px 5px 15px" }}>
                            <Dropdown
                                className="more-dropdown"
                                options={this.props.getFilterByPageStatusOptions()}
                                label={this.getPageStatusLabel()}
                                value={this.props.filterByPublishStatus !== "" && this.props.filterByPublishStatus}
                                onSelect={data=>this.props.updateFilterByPageStatusOptions(data)}
                                withBorder={true} />
                        </GridCell>
                    </GridCell>
                    <GridCell columnSize={100}>
                        <GridCell columnSize={50} style={{ padding: "5px" }}>
                            <DropdownDayPicker
                                onDayClick={this.props.onDayClick}
                                dropdownIsActive={this.state.DropdownCalendarIsActive}
                                applyChanges={this.applyDateFilter.bind(this)}
                                clearChanges={this.clearDateFilter.bind(this)}
                                startDate={this.props.startDate}
                                endDate={this.props.endDate}
                                toggleDropdownCalendar={this.toggleDropdownCalendar.bind(this)}
                                CalendarIcon={SvgIcons.CalendarIcon}
                                label={this.getDateLabel()}
                            />
                        </GridCell>
                        {!utils.isPlatform() &&
                            <GridCell columnSize={50} style={{ padding: "5px 5px 5px 15px" }}>
                                <Dropdown
                                    className="more-dropdown"
                                    options={this.props.getFilterByWorkflowOptions()}
                                    label={this.props.filterByWorkflow ? this.props.getFilterByWorkflowOptions().find(x => x.value === this.props.filterByWorkflow).label : Localization.get("FilterbyWorkflowText")}
                                    value={this.props.filterByWorkflow !== "" && this.props.filterByWorkflow}
                                    onSelect={data=>this.props.updateFilterByWorkflowOptions(data)}
                                    withBorder={true} />
                            </GridCell>
                        }
                    </GridCell>
                </GridCell>
                <GridCell columnSize={30} style={{ paddingLeft: "10px", paddingTop: "10px" }}>
                    <div onClick={this.onTagClick.bind(this)}>
                        <Tags style={{minHeight:"82px", width:"100%" }}
                            tags={this.state.tags}
                            onUpdateTags={this.onChangeTags.bind(this)} />
                    </div>
                </GridCell>
                <GridCell columnSize={100} style={{ textAlign: "center",height:"30%" }}>
                    <Button style={{ marginRight: "5px" }} onClick={this.onClear.bind(this)}>{Localization.get("Clear")}</Button>
                    <Button type="primary" onClick={()=>this.props.onSearch()}>{Localization.get("Apply")}</Button>
                </GridCell>
            </div>);
    }
}

SearchAdvancedDetails.propTypes = {
    onSearch : PropTypes.func.isRequired,
    workflowList : PropTypes.array,
    getWorkflowsList : PropTypes.func,
    getFilterByPageTypeOptions : PropTypes.func.isRequired,
    getFilterByPageStatusOptions : PropTypes.func.isRequired,
    getFilterByWorkflowOptions : PropTypes.func.isRequired,
    updateFilterByPageTypeOptions : PropTypes.func.isRequired,
    updateFilterByPageStatusOptions : PropTypes.func.isRequired,
    updateFilterByWorkflowOptions : PropTypes.func.isRequired,
    onApplyChangesDropdownDayPicker : PropTypes.func.isRequired,
    filterByPublishStatus: PropTypes.string,
    filterByPageType : PropTypes.string,
    onDayClick : PropTypes.func.isRequired,
    startDate : PropTypes.instanceOf(Date).isRequired,
    endDate : PropTypes.instanceOf(Date).isRequired,
    startAndEndDateDirty : PropTypes.bool.isRequired,
    tags : PropTypes.string.isRequired,
    filterByWorkflow : PropTypes.number,
    collapsed : PropTypes.bool.isRequired, 
    clearAdvancedSearch : PropTypes.func.isRequired,
    clearAdvancedSearchDateInterval : PropTypes.func.isRequired,
    updateSearchAdvancedTags : PropTypes.func.isRequired
};


export default SearchAdvancedDetails;