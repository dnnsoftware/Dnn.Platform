import React, { Component, PropTypes } from "react";
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
                label: Localization.get("JobTypeAll"),
                value: null
            },
            searchText: ""
        };
    }

    onSelect(option) {
        let { label } = option;
        let { value } = option;
        let { selectedJobFilter } = this.state;
        if (value !== selectedJobFilter.value) {
            selectedJobFilter.label = label;
            selectedJobFilter.value = value;

            this.setState({
                selectedJobFilter
            }, () => { this.props.onFilterChanged(option); });
        }
    }

    BuildFiltersOptions() {
        const jobFilters = [
            { "Key": Localization.get("JobTypeAll"), "Value": null },
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
                        <DropDown style={{ width: "100%" }}
                            withBorder={false}
                            options={jobFiltersOptions}
                            label={this.state.selectedJobFilter.label}
                            onSelect={this.props.onFilterChanged}
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
    dispatch: PropTypes.func.isRequired,
    onFilterChanged: PropTypes.func.isRequired,
    onKeywordChanged: PropTypes.func.isRequired
};
export default (FiltersBar);