import React, { Component, PropTypes } from "react";
import Localization from "../../localization";
import GridCell from "dnn-grid-cell";
import Dropdown from "dnn-dropdown";
import Button from "dnn-button";
import DropdownDayPicker from "../DropdownDayPicker/DropdownDayPicker";
import {CalendarIcon} from "dnn-svg-icons";
import utils from "../../utils";

export default class SearchAdvancedDetails extends Component {

    constructor(props) {
        super(props);
    }

    getDateLabel() {
        let filterByDateText = utils.isPlatform() ? "FilterByModifiedDateText" : "FilterByPublishDateText";
        const { startDate, endDate, startAndEndDateDirty } = this.props;
        let label = Localization.get(filterByDateText);
        if (startAndEndDateDirty) {
            const fullStartDate = `${startDate.getDate()}/${startDate.getMonth() + 1}/${startDate.getFullYear()}`;
            const fullEndDate = `${endDate.getDate()}/${endDate.getMonth() + 1}/${endDate.getFullYear()}`;
            label = fullStartDate !== fullEndDate ? `${fullStartDate} - ${fullEndDate}` : `${fullStartDate}`;
        }
        return label;
    }

    render() {
        return (
            <div className="search-more-flyout">
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
                                label={this.props.filterByPublishStatus ? this.props.getFilterByPageStatusOptions().find(x => x.value === this.props.filterByPublishStatus.toLowerCase()).label : Localization.get("FilterbyPublishStatusText")}
                                value={this.props.filterByPublishStatus !== "" && this.props.filterByPublishStatus}
                                onSelect={data=>this.props.updateFilterByPageStatusOptions(data)}
                                withBorder={true} />
                        </GridCell>
                    </GridCell>
                    <GridCell columnSize={100}>
                        <GridCell columnSize={50} style={{ padding: "5px" }}>
                            <DropdownDayPicker
                                onDayClick={this.props.onDayClick}
                                dropdownIsActive={this.props.DropdownCalendarIsActive}
                                applyChanges={() => this.props.onApplyChangesDropdownDayPicker()}
                                startDate={this.props.startDate}
                                endDate={this.props.endDate}
                                toggleDropdownCalendar={this.props.toggleDropdownCalendar.bind(this)}
                                CalendarIcon={CalendarIcon}
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
                    <textarea placeholder={Localization.get("TagsInstructions")} value={this.props.tags} onChange={(e) => this.props.generateTags(e)} onBlur={() => this.props.filterTags()}></textarea>
                </GridCell>
                <GridCell columnSize={100} style={{ textAlign: "right" }}>
                    <Button style={{ marginRight: "5px" }} onClick={() => this.setState({ DropdownCalendarIsActive: null, toggleSearchMoreFlyout: false })}>{Localization.get("Cancel")}</Button>
                    <Button type="primary" onClick={() => this.props.onSearch()}>{Localization.get("Save")}</Button>
                </GridCell>
            </div>);
    }
}

SearchAdvancedDetails.propTypes = {
    onSearch : PropTypes.func.isRequired,
    workflowList : PropTypes.array.isRequired,
    getWorkflowsList : PropTypes.func.isRequired,
    getFilterByPageTypeOptions : PropTypes.func.isRequired,
    getFilterByPageStatusOptions : PropTypes.func.isRequired,
    getFilterByWorkflowOptions : PropTypes.func.isRequired,
    updateFilterByPageTypeOptions : PropTypes.func.isRequired,
    updateFilterByPageStatusOptions : PropTypes.func.isRequired,
    updateFilterByWorkflowOptions : PropTypes.func.isRequired,
    generateTags : PropTypes.func.isRequired,
    filterTags : PropTypes.func.isRequired,
    onApplyChangesDropdownDayPicker : PropTypes.func.isRequired,
    filterByPublishStatus: PropTypes.bool.isRequired,
    filterByPageType : PropTypes.string.isRequired,
    onDayClick : PropTypes.func.isRequired,
    startDate : PropTypes.instanceOf(Date).isRequired,
    endDate : PropTypes.instanceOf(Date).isRequired,
    DropdownCalendarIsActive : PropTypes.bool.isRequired,
    toggleDropdownCalendar : PropTypes.func.isRequired,
    startAndEndDateDirty : PropTypes.bool.isRequired,
    tags : PropTypes.string.isRequired,
    filterByWorkflow : PropTypes.string.isRequired   

};
