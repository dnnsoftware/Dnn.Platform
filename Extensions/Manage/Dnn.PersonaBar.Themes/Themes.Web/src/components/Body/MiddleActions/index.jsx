import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import Localization from "localization";
import { GridCell, SearchBox, Dropdown } from "@dnnsoftware/dnn-react-common";
import utils from "utils";
import RestoreTheme from "./RestoreTheme";
import "./style.less";

let canEdit = false;
class MiddleActions extends Component {
    constructor() {
        super();
        this.state = {
            selectedThemeFilter: {
                label: Localization.get("ThemeLevelAll"),
                value: 7
            }
        };
        canEdit = utils.params.settings.isHost || utils.params.settings.isAdmin || (utils.params.settings.permissions && utils.params.settings.permissions.EDIT === true);
    }

    onKeywordChanged(value) {
        const { props } = this;

        props.onSearch.call(this, value);
    }

    buildFiltersOptions() {
        const levelFilters = [
            { "Key": Localization.get("ThemeLevelAll"), "Value": 7 },
            { "Key": Localization.get("ThemeLevelGlobal"), "Value": 4 },
            { "Key": Localization.get("ThemeLevelSite"), "Value": 2 }
        ];
        let themeFilterOptions = [];
        themeFilterOptions = levelFilters.map((levelFilter) => {
            return { label: levelFilter.Key, value: levelFilter.Value };
        });
        return themeFilterOptions;
    }

    onSelect(option) {
        let { selectedThemeFilter } = this.state;
        if (option.value !== selectedThemeFilter.value) {
            selectedThemeFilter.label = option.label;
            selectedThemeFilter.value = option.value;

            this.setState({
                selectedThemeFilter
            }, () => { this.props.onFilterChanged(option.value); });
        }
    }

    render() {
        let themeFilterOptions = this.buildFiltersOptions();
        return (
            <GridCell className="middle-actions">
                <GridCell columnSize={40}>
                    {canEdit &&
                        <RestoreTheme />
                    }
                </GridCell>
                <GridCell columnSize={30}>
                    <div className="theme-level-filter">
                        <Dropdown style={{ width: "100%" }}
                            withBorder={false}
                            options={themeFilterOptions}
                            onSelect={this.onSelect.bind(this)}
                            value={this.state.selectedThemeFilter.value}
                            prependWith={Localization.get("ShowFilterLabel")}
                        />
                        <div className="clear">
                        </div>
                    </div>
                </GridCell>
                <GridCell columnSize={30}>
                    <div className="search-filter">
                        {
                            <SearchBox placeholder={Localization.get("SearchPlaceHolder")} onSearch={this.onKeywordChanged.bind(this)} maxLength={50} iconStyle={{ right: 0 }} />
                        }
                        <div className="clear"></div>
                    </div>
                </GridCell>
            </GridCell>
        );
    }
}

MiddleActions.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onSearch: PropTypes.func.isRequired,
    onFilterChanged: PropTypes.func.isRequired
};

function mapStateToProps() {
    return {};
}

export default connect(mapStateToProps)(MiddleActions);