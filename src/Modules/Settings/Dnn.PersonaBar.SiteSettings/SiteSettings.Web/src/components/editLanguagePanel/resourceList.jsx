import React, { Component, PropTypes } from "react";
import resx from "resources";
import SingleLineInput from "dnn-single-line-input";
import DropdownWithError from "dnn-dropdown-with-error";
import Switch from "dnn-switch";
import Button from "dnn-button";
import RadioButtons from "dnn-radio-buttons";
import SearchBox from "dnn-search-box";
import GridCell from "dnn-grid-cell";
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
            resources: []
        };
    }
    componentWillMount() {
        this.setState({
            resources: this.props.list
        });
    }
    onChange(key, keyToChange, event) {
        let value = typeof (event) === "object" ? event.target.value : event;
        console.log(arguments);
        let resources = utilities.utilities.getObjectCopy(this.props.list);
        console.log(resources);
        resources[key][keyToChange] = value;

        this.props.onResxChange(resources);
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
                    {Object.keys(this.props.list).map((key, index) => {
                        const shouldBeHighlighted = (props.highlightPendingTranslations && this.props.list[key].First === this.props.list[key].Second);
                        return <GridCell className="resource-row">
                            <GridCell className="row-detail" columnSize={rowSizes[0]}>
                                <p>{key}</p>
                            </GridCell>
                            <GridCell className="row-detail" columnSize={rowSizes[1]}>
                                <SingleLineInput value={this.props.list[key].Second} enabled={false} />
                            </GridCell>
                            <GridCell className="row-detail" columnSize={rowSizes[2]}>
                                <SingleLineInput className={(shouldBeHighlighted ? "highlight" : "")} value={this.props.list[key].First} onChange={this.onChange.bind(this, key, "First")} />
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
