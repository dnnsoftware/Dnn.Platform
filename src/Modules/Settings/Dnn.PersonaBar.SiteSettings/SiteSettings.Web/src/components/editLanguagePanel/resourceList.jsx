import React, { Component, PropTypes } from "react";
import resx from "resources";
import SingleLineInput from "dnn-single-line-input";
import DropdownWithError from "dnn-dropdown-with-error";
import Switch from "dnn-switch";
import Button from "dnn-button";
import RadioButtons from "dnn-radio-buttons";
import SearchBox from "dnn-search-box";
import GridCell from "dnn-grid-cell";

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
            resources: []
        };
    }
    componentWillMount() {
        this.setState({
            resources: this.props.list
        });
    }
    onChange(key, index, event) {
        let value = typeof (event) === "object" ? event.target.value : event;

        let {resources} = this.state;
        resources[index][key] = value;
        this.setState({ resources });
    }
    /* eslint-disable react/no-danger */
    render() {
        const { props } = this;
        return (
            <GridCell className="resource-list">
                <GridCell className="resource-controls">
                    <GridCell columnSize={33}>
                        <p className="pending-translations">[Pending Translations: 35]</p>
                    </GridCell>
                    <GridCell columnSize={33}>
                        <p className="clear-pending-translations">[Clear Pending Translations]</p>
                    </GridCell>
                    <GridCell columnSize={33} className="search-box-container">
                        <SearchBox style={searchBoxStyle} iconStyle={searchIconStyle} />
                    </GridCell>
                </GridCell>
                <GridCell className="row-headers">
                    <GridCell className="row-header" columnSize={rowSizes[0]}>
                        {resx.get("ResourceName")}
                    </GridCell>
                    <GridCell className="row-header" columnSize={rowSizes[1]}>
                        {resx.get("DefaultValue")}
                    </GridCell>
                    <GridCell className="row-header" columnSize={rowSizes[2]}>
                        {resx.get("LocalizedValue")}
                    </GridCell>
                </GridCell>
                <GridCell className="resource-rows">
                    {this.state.resources.map((resource, index) => {
                        return <GridCell className="resource-row">
                            <GridCell className="row-detail" columnSize={rowSizes[0]}>
                                <p>{resource.resourceName}</p>
                            </GridCell>
                            <GridCell className="row-detail" columnSize={rowSizes[1]}>
                                <SingleLineInput value={resource.defaultValue} enabled={false} />
                            </GridCell>
                            <GridCell className="row-detail" columnSize={rowSizes[2]}>
                                <SingleLineInput value={resource.localizedValue} onChange={this.onChange.bind(this, "localizedValue", index)} />
                            </GridCell>
                        </GridCell>;
                    })}
                </GridCell>
            </GridCell>
        );
    }
}

ResourceList.propTypes = {
    list: PropTypes.array
};
export default ResourceList;
