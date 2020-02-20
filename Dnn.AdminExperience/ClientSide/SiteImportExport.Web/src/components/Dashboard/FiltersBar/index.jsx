import React, { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";
import Localization from "localization";
import { Dropdown, SearchBox, GridCell } from "@dnnsoftware/dnn-react-common";

class FiltersBar extends Component {
    constructor(props) {
        super(props);
        this.state = {
            selectedJobFilter: {
                label: Localization.get("JobTypeAll"),
                value: -1
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
            { "Key": Localization.get("JobTypeAll"), "Value": -1 },
            { "Key": Localization.get("JobTypeImport"), "Value": 1 },
            { "Key": Localization.get("JobTypeExport"), "Value": 0 }
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
                        <Dropdown style={{ width: "100%" }}
                            withBorder={false}
                            options={jobFiltersOptions}
                            onSelect={this.onSelect.bind(this)}
                            value={this.state.selectedJobFilter.value}
                            prependWith={Localization.get("ShowFilterLabel")}
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
    onFilterChanged: PropTypes.func.isRequired,
    onKeywordChanged: PropTypes.func.isRequired
};
export default (FiltersBar);