import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
import DropDown from "dnn-dropdown";
import Localization from "localization";
import SearchBox from "dnn-search-box";
import GridCell from "dnn-grid-cell";

class FiltersBar extends Component {
    constructor(props) {
        super(props);
        this.state = {
            selectedJobFilter: {
                label: Localization.get("SortByDateNewest"),
                value: "newest"
            },
            searchText: ""
        };
    }

    onSelect(option) {
        let { selectedJobFilter } = this.state;
        if (option.value !== selectedJobFilter.value) {
            selectedJobFilter.label = option.label;
            selectedJobFilter.value = option.value;

            this.setState({
                selectedJobFilter
            }, () => { this.props.onFilterChanged(option); });
        }
    }

    BuildFiltersOptions() {
        const jobFilters = [
            { "Key": Localization.get("SortByDateNewest"), "Value": "newest" },
            { "Key": Localization.get("SortByDateOldest"), "Value": "oldest" },
            { "Key": Localization.get("SortByName"), "Value": "name" }
        ];
        let jobFiltersOptions = [];
        jobFiltersOptions = jobFilters.map((jobFilters) => {
            return { label: jobFilters.Key, value: jobFilters.Value };
        });
        return jobFiltersOptions;
    }

    render() {
        let jobFiltersOptions = this.BuildFiltersOptions();
        return <div className="jobs-filter-container">
            <GridCell columnSize={35} >
                {
                    jobFiltersOptions.length > 0 &&
                    <div className="job-filters-filter">
                        <DropDown style={{ width: "100%" }}
                            withBorder={false}
                            options={jobFiltersOptions}
                            onSelect={this.onSelect.bind(this)}
                            value={this.state.selectedJobFilter.value}
                            prependWith={Localization.get("ShowSortLabel")}
                        />
                        <div className="clear">
                        </div>
                    </div>
                }
            </GridCell>
            <GridCell columnSize={30} >
                <div>&nbsp; </div></GridCell>
            <GridCell columnSize={35} >
                <div className="search-filter">
                    <SearchBox placeholder={Localization.get("SearchPlaceHolder")} onSearch={this.props.onKeywordChanged} maxLength={50} iconStyle={{ right: 0 }} />
                    <div className="clear"></div>
                </div>
            </GridCell>
        </div>;
    }
}
FiltersBar.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onFilterChanged: PropTypes.func.isRequired,
    onKeywordChanged: PropTypes.func.isRequired
};
export default (FiltersBar);