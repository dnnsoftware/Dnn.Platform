import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { ExtensionActions, VisiblePanelActions, InstallationActions } from "actions";
import Localization from "localization";
import { GridCell, DropdownWithError } from "@dnnsoftware/dnn-react-common";
import ExtensionList from "./ExtensionList";
import "./style.less";

class AvailableExtensions extends Component {
    constructor() {
        super();
        this.state = {
            doingOperation: false,
            loading: false
        };
    }
    checkIfAvailablePackageTypesEmpty(props) {
        return !props.availablePackageTypes || props.availablePackageTypes.length === 0;
    }

    checkIfAvailablePackagesEmpty(props) {
        return !props.availablePackages || props.availablePackages.length === 0;
    }
    UNSAFE_componentWillMount() {
        const { props } = this;

        if (!this.checkIfAvailablePackageTypesEmpty(props) && this.checkIfAvailablePackagesEmpty(props) && props.selectedAvailablePackageType === "") {
            this.setState({loading: true}, () => {
                props.dispatch(ExtensionActions.getAvailablePackages(props.availablePackageTypes[0].Type, () => {
                    this.setState({loading: false});
                }));
            });
        }
    }
    UNSAFE_componentWillReceiveProps(props) {
        if (props.availablePackages && props.availablePackages.length > 0) {
            this.setState({loading: false});
        }
    }

    onChange(key, event) {
        this.setState({
            [key]: event.target.value
        });
    }

    onSelectChange(option) {
        this.setState({
            selectValue: option.value
        });
    }
    onFilterSelect(option) {
        const {props} = this;
        this.setState({loading: true}, () => {
            props.dispatch(ExtensionActions.getAvailablePackages(option.value, () => {
                this.setState({loading: false});
            }));
        });
    }

    onInstall(name, event) {
        if (event) {
            event.preventDefault();
        }
        const {props} = this;
        this.setDoingOperation(true);
        props.dispatch(ExtensionActions.parseAvailablePackage(name, props.selectedAvailablePackageType, () => {
            this.setDoingOperation(false);
            props.dispatch(InstallationActions.setInstallingAvailablePackage(name, props.selectedAvailablePackageType, () => {
                props.dispatch(InstallationActions.navigateWizard(1, () => {
                    props.dispatch(VisiblePanelActions.selectPanel(3));
                }));
            }));
        }));
    }

    setDoingOperation(doingOperation) {
        this.setState({
            doingOperation
        });
    }

    onDeploy(index, _package, event) {
        if (event) {
            event.preventDefault();
        }
        const {props} = this;

        this.setDoingOperation(true);
        props.dispatch(ExtensionActions.deployAvailablePackage(_package, index, (data) => {
            props.dispatch(ExtensionActions.getAvailablePackages(props.selectedAvailablePackageType, () => {
                let packageToInstall = this.props.availablePackages.find((_package)=>{
                    return _package.name === data.name;
                });
                this.onInstall(packageToInstall.fileName);
            }));
        }));
    }

    renderLoading() {
        /* eslint-disable react/no-danger */
        return <div className="loading-extensions">
            <h2>{Localization.get("Loading")}</h2>
            <p>{Localization.get("Loading.Tooltip")}</p>
            <div dangerouslySetInnerHTML={{ __html: require("!raw-loader!./../../../img/fetching.svg").default }} />
        </div>;
    }

    render() {
        const {props, state} = this;
        return (
            <GridCell className="extension-list">
                <GridCell className="collapse-section filter-section">
                    <DropdownWithError className="filter-dropdown" onSelect={this.onFilterSelect.bind(this)} options={props.availablePackageTypes && props.availablePackageTypes.map((_package) => {
                        return {
                            label: _package.DisplayName,
                            value: _package.Type
                        };
                    })}
                    label={Localization.get("Showing.Label")}
                    value={props.selectedAvailablePackageType}
                    labelType="inline" />
                </GridCell>
                {state.loading && this.renderLoading()}
                {(props.availablePackages && props.availablePackages.length > 0 && !state.loading) &&
                    <ExtensionList
                        packages={props.availablePackages}
                        doingOperation={this.state.doingOperation}
                        type={props.selectedAvailablePackageType}
                        onInstall={this.onInstall.bind(this)}
                        onDeploy={this.onDeploy.bind(this)} />
                }
            </GridCell>
        );
    }
}

AvailableExtensions.propTypes = {
    dispatch: PropTypes.func.isRequired,
    availablePackages: PropTypes.array,
    tabIndex: PropTypes.number
};

function mapStateToProps(state) {
    return {
        availablePackageTypes: state.extension.availablePackageTypes,
        availablePackages: state.extension.availablePackages,
        selectedAvailablePackageType: state.extension.selectedAvailablePackageType,
        tabIndex: state.pagination.tabIndex
    };
}

export default connect(mapStateToProps)(AvailableExtensions);