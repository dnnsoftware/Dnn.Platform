import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import {
    ExtensionActions,
    PaginationActions,
    VisiblePanelActions
} from "actions";
import Localization from "localization";
import ExtensionList from "./ExtensionList";
import GridCell from "dnn-grid-cell";
import DropdownWithError from "dnn-dropdown-with-error";
import "./style.less";

class InstalledExtensions extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
        this.state = {
            loading: false
        };
    }

    checkIfPackageTypesEmpty(props) {
        return !props.installedPackageTypes || props.installedPackageTypes.length === 0;
    }

    checkIfInstalledPackagesEmpty(props) {
        return !props.installedPackages || props.installedPackages.length === 0;
    }

    UNSAFE_scomponentWillMount() {
        const {props} = this;
        if (this.checkIfPackageTypesEmpty(props)) {
            props.dispatch(ExtensionActions.getPackageTypes());
        }
    }

    UNSAFE_componentWillReceiveProps(props) {
        if (!this.checkIfPackageTypesEmpty(props) && this.checkIfInstalledPackagesEmpty(props) && props.selectedInstalledPackageType === "") {
            this.setState({loading: true}, () => {
                props.dispatch(ExtensionActions.getInstalledPackages(props.installedPackageTypes[0].Type, ()=> {
                    this.setState({loading: false});
                }));
            });
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
        this.setState({loading: true}, () => {
            props.dispatch(ExtensionActions.getInstalledPackages(option.value, () => {
                this.setState({loading: false});
            }));
        });
    }

    onDelete(_package, index) {
        const {props} = this;
        props.dispatch(ExtensionActions.setPackageBeingDeleted(Object.assign({ packageType: props.selectedInstalledPackageType }, _package), index, () => {
            props.dispatch(VisiblePanelActions.selectPanel(6));
        }));
    }

    renderLoading() {
        /* eslint-disable react/no-danger */
        return <div className="loading-extensions">
            <h2>{Localization.get("Loading")}</h2>
            <p>{Localization.get("Loading.Tooltip")}</p>
            <div dangerouslySetInnerHTML={{ __html: require("!raw-loader!./../../../img/fetching.svg") }} />
        </div>;
    }

    render() {
        const {props, state} = this;
        return (
            <GridCell className="extension-list">
                <GridCell className="collapse-section filter-section">
                    <DropdownWithError className="filter-dropdown" onSelect={this.onFilterSelect.bind(this)} options={props.installedPackageTypes && props.installedPackageTypes.map((_package) => {
                        return {
                            label: _package.DisplayName,
                            value: _package.Type
                        };
                    })}
                    label={Localization.get("Showing.Label")}
                    value={props.selectedInstalledPackageType}
                    labelType="inline" />
                </GridCell>
                {state.loading && this.renderLoading()}
                {(props.installedPackages && props.installedPackages.length > 0 && !state.loading) &&
                    <ExtensionList
                        packages={props.installedPackages}
                        isHost={props.isHost}
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
    isHost: PropTypes.bool,
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
