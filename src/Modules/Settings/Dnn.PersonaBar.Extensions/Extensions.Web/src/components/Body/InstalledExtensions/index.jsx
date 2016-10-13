import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import {
    ExtensionActions,
    PaginationActions
} from "actions";
import Localization from "localization";
import ExtensionList from "./ExtensionList";
import GridCell from "dnn-grid-cell";
import DropdownWithError from "dnn-dropdown-with-error";
import utilities from "utils";
import "./style.less";

class InstalledExtensions extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
    }

    checkIfPackageTypesEmpty(props) {
        return !props.installedPackageTypes || props.installedPackageTypes.length === 0;
    }

    checkIfInstalledPackagesEmpty(props) {
        return !props.installedPackages || props.installedPackages.length === 0;
    }

    componentWillMount() {
        const {props} = this;
        if (this.checkIfPackageTypesEmpty(props)) {
            props.dispatch(ExtensionActions.getPackageTypes());
        }
    }

    componentWillReceiveProps(props) {
        if (!this.checkIfPackageTypesEmpty(props) && this.checkIfInstalledPackagesEmpty(props) && props.selectedInstalledPackageType === "") {
            props.dispatch(ExtensionActions.getInstalledPackages(props.installedPackageTypes[0].Type));
        }
    }

    handleSelect(index/*, last*/) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    onChange(key, event) {
        this.setState({
            [key]: event.target.value
        });
    }

    onFilterSelect(option) {
        const {props} = this;
        props.dispatch(ExtensionActions.getInstalledPackages(option.value));
    }

    onDelete(packageId, index) {
        const {props} = this;
        utilities.utilities.confirm("Are you sure you want to delete this package?", "Yes", "No", () => {
            props.dispatch(ExtensionActions.deletePackage(packageId, index));
        });
    }

    render() {
        const {props, state} = this;
        return (
            <GridCell className="extension-list">
                <GridCell className="collapse-section">
                    <DropdownWithError className="filter-dropdown" onSelect={this.onFilterSelect.bind(this)} options={props.installedPackageTypes && props.installedPackageTypes.map((_package) => {
                        return {
                            label: _package.Type.split("_").join("").split(/(?=[A-Z])/).join(" "),
                            value: _package.Type
                        };
                    })}
                        label={Localization.get("Showing.Label")}
                        value={props.selectedInstalledPackageType}
                        labelType="inline" />
                </GridCell>
                {(props.installedPackages && props.installedPackages.length > 0) &&
                    <ExtensionList
                        packages={props.installedPackages}
                        onEdit={props.onEdit.bind(this)}
                        onDelete={this.onDelete.bind(this)} />
                }
            </GridCell>
        );
    }
}

InstalledExtensions.propTypes = {
    dispatch: PropTypes.func.isRequired,
    installedPackages: PropTypes.array,
    installedPackageTypes: PropTypes.array,
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
    return {
        installedPackageTypes: state.extension.installedPackageTypes,
        installedPackages: state.extension.installedPackages,
        selectedInstalledPackageType: state.extension.selectedInstalledPackageType,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(InstalledExtensions);