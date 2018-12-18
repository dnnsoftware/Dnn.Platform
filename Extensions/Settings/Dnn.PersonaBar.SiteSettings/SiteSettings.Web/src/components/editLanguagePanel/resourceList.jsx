import React, { Component } from "react";
import PropTypes from "prop-types";
import resx from "resources";
import { SearchBox, GridCell } from "@dnnsoftware/dnn-react-common";
import ResourceEditor from "./resourceEditor";
import utilities from "utils";

const rowSizes = [30, 35, 35];

const searchBoxStyle = {
    display: "block",
    width: "100%",
    height: 30,
    float: "right"
};
const searchIconStyle = {
    width: 19,
    height: 19,
    right: 10
};

class ResourceList extends Component {
    constructor() {
        super();
        this.state = {
            resources: [],
            searchText: ""
        };
    }
    componentDIdMount() {
        this.setState({
            resources: this.props.list
        });
    }
    onChange(key, keyToChange, event) {
        let value = typeof (event) === "object" ? event.target.value : event;
        let resources = utilities.utilities.getObjectCopy(this.props.list);
        resources[key][keyToChange] = value;

        this.props.onResxChange(resources);
    }
    filterListFromSearchText(list) {
        return list.filter((key) => {
            return key.toLowerCase().indexOf(this.state.searchText.toLowerCase()) > -1;
        });
    }
    onSearch(searchText) {
        this.setState({
            searchText
        });
    }

    /* eslint-disable react/no-danger */
    render() {
        const { props } = this;
        const pendingTranslations = Object.keys(this.props.list).filter((key) => {
            return this.props.list[key].First === this.props.list[key].Second;
        });
        const searchedList = this.filterListFromSearchText(Object.keys(this.props.list));
        return (
            <GridCell className="resource-list">
                <GridCell className="resource-controls">
                    <GridCell columnSize={33}>
                        <p className="pending-translations">[Pending Translations: {pendingTranslations.length}]</p>
                    </GridCell>
                    <GridCell columnSize={33}>

                    </GridCell>
                    <GridCell columnSize={33} className="search-box-container">
                        <SearchBox style={searchBoxStyle} iconStyle={searchIconStyle} onSearch={this.onSearch.bind(this) }/>
                    </GridCell>
                </GridCell>
                <GridCell className="row-headers">
                    <GridCell className="row-header" columnSize={rowSizes[0]}>
                        {resx.get("ResourceName") }
                    </GridCell>
                    <GridCell className="row-header" columnSize={rowSizes[1]}>
                        {resx.get("DefaultValue") }
                    </GridCell>
                    <GridCell className="row-header" columnSize={rowSizes[2]}>
                        {resx.get("LocalizedValue") }
                    </GridCell>
                </GridCell>
                <GridCell className="resource-rows">
                    {searchedList.map((key, i) => {
                        const shouldBeHighlighted = props.highlightPendingTranslations 
                            && this.props.list[key].First === this.props.list[key].Second;

                        return <GridCell className="resource-row" key={i}>
                            <GridCell className="row-detail" columnSize={rowSizes[0]}>
                                <div className="key-name">{key}</div>
                            </GridCell>
                            <GridCell className="row-detail" columnSize={rowSizes[1]}>
                                <ResourceEditor 
                                    value={this.props.list[key].Second} 
                                    enabled={false} />
                            </GridCell>
                            <GridCell className="row-detail" columnSize={rowSizes[2]}>
                                <ResourceEditor 
                                    className={(shouldBeHighlighted ? "highlight" : "")} 
                                    value={this.props.list[key].First} 
                                    onChange={this.onChange.bind(this, key, "First") } />
                            </GridCell>
                        </GridCell>;
                    }) }
                </GridCell>
            </GridCell>
        );
    }
}

ResourceList.propTypes = {
    list: PropTypes.array,
    onResxChange: PropTypes.func
};
export default ResourceList;
