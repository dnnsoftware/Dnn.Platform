import React, { Component } from "react";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";
import Localization from "../../localization";
import PropTypes from "prop-types";

class SearchPageInput extends Component {
    constructor(props) {
        super(props);
        this.state = {
            searchTerm : ""
        };
    }

    onSearchFieldChange(e) {
        const currentSearchTerm = this.state.searchTerm;
        const inSearch = this.state.inSearch;
        this.setState({ searchTerm: e.target.value, filtersUpdated: true }, () => {
            const { searchTerm } = this.state;
            switch (true) {
                case searchTerm.length > 3:
                    this.props.onSearch(searchTerm);
                    return;
                case currentSearchTerm.length > 0 && searchTerm.length === 0 && inSearch:
                    this.props.onSearch();
                    return;
            }
        });
    }

    render() {
        /* eslint-disable react/no-danger */
        return (
            <div className="search-container">
                {this.props.inSearch ?
                    <div className="dnn-back-to-link" onClick={() => this.props.clearSearch()}>
                        <div className="dnn-back-to-arrow" dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowBack }} /> <span>{Localization.get("BackToPages").toUpperCase()}</span>
                    </div> : null
                }

                <div className="search-box">
                    <div className="search-input">
                        <input
                            type="text"
                            value={this.state.searchTerm}
                            onChange={this.onSearchFieldChange.bind(this)}
                            onKeyPress={(e) => { e.key === "Enter" ? this.props.onSearch(this.state.searchTerm) : null; }}
                            placeholder="Search" />
                    </div>
                    {this.state.searchTerm ?
                        <div
                            className="btn clear-search"
                            style={{ fill: "#444" }}
                            dangerouslySetInnerHTML={{ __html: SvgIcons.XIcon }}
                            onClick={() => this.setState({ searchTerm: "", filtersUpdated: true }, () => this.props.onSearch(""))}
                        />

                        : <div className="btn clear-search" />}
                    <div
                        className="btn search-btn"
                        dangerouslySetInnerHTML={{ __html: SvgIcons.PagesSearchIcon }}
                        onClick={()=>this.props.onSearch(this.state.searchTerm)}
                    >
                    </div>
                </div>
            </div>
        );
    }
}


SearchPageInput.propTypes = {
    inSearch: PropTypes.bool.isRequired,
    onSearch: PropTypes.func.isRequired,
    clearSearch: PropTypes.func.isRequired
};

export default SearchPageInput;