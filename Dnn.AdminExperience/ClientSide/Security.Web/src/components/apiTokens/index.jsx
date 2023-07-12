import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { security as SecurityActions } from "../../actions";
import { Dropdown, Button, Pager } from "@dnnsoftware/dnn-react-common";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import "./style.less";
import util from "../../utils";
import resx from "../../resources";
import styles from "./style.less";
import ApiTokenRow from "./ApiTokenRow";
import ApiTokenDetails from "./ApiTokenDetails";
import CreateApiToken from "./CreateApiToken";


let pageSizeOptions = [];
let scopeOptions = [];
let filterOptions = [];
let isHost = false;

class ApiTokensPanelBody extends Component {
    constructor() {
        super();
        this.state = {
            portalList: [],
            currentPortal: "",
            currentPortalId: -2,
            currentScope: -2,
            apiTokens: [],
            apiTokenKeys: [],
            apiTokenKeysFiltered: [],
            filterByApiTokenKey: "",
            currentFilter: 0,
            pageIndex: 0,
            pageSize: 10,
            totalCount: 0,
            showNewApiToken: false,
        };
        isHost = util.settings.isHost;
    }

    componentWillMount() {
        const { props } = this;
        if (isHost) {
            props.dispatch(SecurityActions.getPortalList(util.settings.isHost, (dataPortal) => {
                let portalList = Object.assign([], dataPortal.Results);
                let currentPortalId = portalList[0].PortalID;
                let currentPortal = portalList[0].PortalName;
                this.setState({
                    portalList,
                    currentPortalId,
                    currentPortal
                });
            }));
        }
        props.dispatch(SecurityActions.getApiTokenKeys((data) => {
            this.setState({
                apiTokenKeys: data
            }, () => {
                this.resetApiFilterList();
            });
        }));
        if (props.apiTokenSettings) {
            this.setState({
                apiTokenSettings: props.apiTokenSettings
            });
        } else {
            props.dispatch(SecurityActions.getApiTokenSettings((data) => {
                let apiTokenSettings = Object.assign({}, data.Results.ApiTokenSettings);
                this.setState({
                    apiTokenSettings
                });
            }));
        }

        pageSizeOptions = [];
        pageSizeOptions.push({ "value": 10, "label": "10 entries per page" });
        pageSizeOptions.push({ "value": 25, "label": "25 entries per page" });
        pageSizeOptions.push({ "value": 50, "label": "50 entries per page" });
        pageSizeOptions.push({ "value": 100, "label": "100 entries per page" });
        pageSizeOptions.push({ "value": 250, "label": "250 entries per page" });

        scopeOptions = [];
        scopeOptions.push({ "value": -2, "label": resx.get("All") });
        scopeOptions.push({ "value": 0, "label": resx.get("User") });
        if (util.settings.isAdmin) {
            scopeOptions.push({ "value": 1, "label": resx.get("Portal") });
        }
        if (util.settings.isHost) {
            scopeOptions.push({ "value": 2, "label": resx.get("Host") });
        }

        filterOptions = [];
        filterOptions.push({ "value": 0, "label": resx.get("All") });
        filterOptions.push({ "value": 1, "label": resx.get("Active") });
        filterOptions.push({ "value": 2, "label": resx.get("Revoked") });
        filterOptions.push({ "value": 3, "label": resx.get("Expired") });
        filterOptions.push({ "value": 4, "label": resx.get("RevokedOrExpired") });

        this.getData();
    }

    getData() {
        const crtPid = this.state.currentPortalId;
        this.props.dispatch(SecurityActions.getApiTokens(crtPid < 0 ? -2 : crtPid, this.state.currentFilter, this.state.filterByApiTokenKey, this.state.currentScope, this.state.pageIndex, this.state.pageSize, (data) => {
            this.setState({
                apiTokens: data.Data,
                totalCount: data.TotalCount
            });
        }));
    }

    resetApiFilterList() {
        let newOptions = [];
        newOptions.push({ "value": "", "label": resx.get("All") });
        const f = this.state.currentScope == -2 ? this.state.apiTokenKeys : this.state.apiTokenKeys.filter((item) => {
            return item.Scope == this.state.currentScope;
        });
        f.forEach((item) => {
            if (newOptions.filter((option) => { return option.value == item.Key; }).length == 0)
                newOptions.push({ "value": item.Key, "label": item.Name });
        });
        this.setState({
            apiTokenKeysFiltered: newOptions,
            filterByApiTokenKey: ""
        }, () => {
            this.getData();
        });
    }

    onPageChange(currentPage, pageSize) {
        this.setState({
        });
    }

    renderHeader() {
        const tableFields = [
            { name: "Portal", width: 20 },
            { name: "Scope", width: 15 },
            { name: "CreatedOn", width: 10 },
            { name: "CreatedBy", width: 20 },
            { name: "ExpiresOn", width: 10 },
            { name: "ApiTokenStatus", width: 20 }
        ];

        let tableHeaders = tableFields.map((field, index) => {
            let className = "logHeader";
            return <div key={index} className={className} style={{ width: field.width.toString() + "%" }}>
                <span>{field.name != "" && resx.get(field.name + ".Header")}&nbsp;</span>
            </div>;
        });

        return <div className="logHeader-wrapper">{tableHeaders}</div>;
    }

    renderList() {
        return this.state.apiTokens.map((item, index) => {
            return <ApiTokenRow
                key={index}
                apiToken={item}
                onClose={() => { this.getData(); }}
                scopes={scopeOptions}>
                <ApiTokenDetails
                    apiToken={item}
                    apiTokenKeys={this.state.apiTokenKeys}
                />
            </ApiTokenRow>;
        });
    }

    renderPager() {
        const { state } = this;
        return (
            <div className="logPager">
                <Pager
                    showStartEndButtons={false}
                    showPageSizeOptions={true}
                    showPageInfo={false}
                    numericCounters={4}
                    pageSize={state.pageSize}
                    totalRecords={state.totalCount}
                    onPageChanged={this.onPageChange.bind(this)}
                    pageSizeDropDownWithoutBorder={true}
                    pageSizeOptionText={"{0} results per page"}
                    summaryText={"Showing {0}-{1} of {2} results"}
                    culture={util.utilities.getCulture()}
                />
            </div>
        );
    }

    render() {
        const { state } = this;
        if (state.apiTokenSettings && !state.apiTokenSettings.ApiTokensEnabled) {
            return (<div style={{ float: "left", width: "100%" }}>
                <div className="logContainer">
                    <div className="warningBox">
                        <div className="warningText">{resx.get("ApiTokensDisabled.Help")}</div>
                    </div>
                </div>
            </div>
            );
        }
        return (
            <div style={{ float: "left", width: "100%" }}>
                <div className="tokenCommandBox">
                    <Button
                        type="primary"
                        onClick={() => {
                            this.setState({
                                showAddApiToken: true
                            });
                        }}>
                        {resx.get("Add")}
                    </Button>
                </div>
                <div className="logContainer">
                    <div className="toolbar">
                        {isHost && state.portalList.length > 0 &&
                            <div className="security-filter-container">
                                <Dropdown
                                    value={state.currentPortalId}
                                    style={{ width: "100%" }}
                                    options={state.portalList.map((item) => {
                                        return { label: item.PortalName, value: item.PortalID };
                                    })}
                                    withBorder={false}
                                    onSelect={(value) => {
                                        let currentPortal = state.portalList.filter((item) => {
                                            return item.PortalID === value.value;
                                        })[0].PortalName;
                                        this.setState({
                                            currentPortalId: value.value,
                                            currentPortal
                                        }, () => {
                                            this.getData();
                                        });
                                    }}
                                />
                            </div>
                        }
                        {scopeOptions.length > 2 &&
                            <div className="security-filter-container">
                                <Dropdown
                                    value={state.currentScope}
                                    style={{ width: "100%" }}
                                    options={scopeOptions}
                                    withBorder={false}
                                    onSelect={(value) => {
                                        this.setState({
                                            currentScope: value.value
                                        }, () => {
                                            this.resetApiFilterList();
                                        });
                                    }}
                                />
                            </div>
                        }
                        <div className="security-filter-container">
                            <Dropdown
                                value={state.currentFilter}
                                style={{ width: "100%" }}
                                options={filterOptions}
                                withBorder={false}
                                onSelect={(value) => {
                                    this.setState({
                                        currentFilter: value.value
                                    }, () => {
                                        this.getData();
                                    });
                                }}
                            />
                        </div>
                        <div className="security-filter-container">
                            <Dropdown
                                value={state.filterByApiTokenKey}
                                style={{ width: "100%" }}
                                options={this.state.apiTokenKeysFiltered}
                                withBorder={false}
                                onSelect={(value) => {
                                    this.setState({
                                        filterByApiTokenKey: value.value
                                    }, () => {
                                        this.getData();
                                    });
                                }}
                            />
                        </div>
                    </div>
                    <div className="logContainerBox">
                        {this.state.showAddApiToken && <CreateApiToken
                            apiTokenKeys={this.state.apiTokenKeys}
                            scopeOptions={scopeOptions.slice(1)}
                            onCancel={() => {
                                this.setState({
                                    showAddApiToken: false
                                });
                            }}
                            onCreateApiToken={(scope, expiresOn, apiKeys) => {
                                this.setState({
                                    showAddApiToken: false
                                });
                                this.props.dispatch(SecurityActions.createApiToken(scope, expiresOn, apiKeys, (data) => {
                                    prompt(resx.get("ApiTokenCreated"), data);
                                    this.getData();
                                }));
                            }}
                        />}
                        {this.renderHeader()}
                        {this.renderList()}
                    </div>
                </div>
                {this.renderPager()}
            </div>
        );
    }
}

ApiTokensPanelBody.propTypes = {
    dispatch: PropTypes.func.isRequired,
    tabIndex: PropTypes.number,
    apiTokens: PropTypes.array,
    cultureCode: PropTypes.string,
    apiTokenSettings: PropTypes.object,
    apiTokenSettingsClientModified: PropTypes.bool,
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        apiTokens: state.security.apiTokens,
        apiTokenSettings: state.security.apiTokenSettings,
        apiTokenSettingsClientModified: state.security.apiTokenSettingsClientModified,
    };
}

export default connect(mapStateToProps)(ApiTokensPanelBody);
