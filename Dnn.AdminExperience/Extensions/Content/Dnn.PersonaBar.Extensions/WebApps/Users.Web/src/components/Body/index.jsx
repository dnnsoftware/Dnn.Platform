import PropTypes from "prop-types";
import React, { Component } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import UserTable from "../../_exportables/src/components/UserTable";
import FiltersBar from "../../_exportables/src/components/FiltersBar";
import "./style.less";
import {CommonUsersActions } from "dnn-users-common-actions";
import appSettings from "utils/applicationSettings";
import utilities from "utils";
import { Button, GridCell, PersonaBarPageHeader, PersonaBarPageBody, Pager } from "@dnnsoftware/dnn-react-common";

const searchParameters = {
    searchText: "",
    filter: 0,
    pageIndex: 0,
    pageSize: 10,
    sortColumn: "",
    sortAscending: false,
    resetIndex: false
};
class Body extends Component {
    constructor() {
        super();
        this.userTable = React.createRef();
        this.state = {
            userFilters: [],
            searchParameters
        };
    }
    componentDidMount() {
        this.props.dispatch(CommonUsersActions.getUserFilters((data) => {
            let userFilters = Object.assign([], JSON.parse(JSON.stringify(data)));
            this.setState({
                userFilters
            });
        }));
    }

    onFilterChange(option, searchText) {
        let {searchParameters} = this.state;
        searchParameters.searchText = searchText;
        searchParameters.filter = option.value;
        searchParameters.pageIndex = 0;
        searchParameters.resetIndex = true;
        this.props.dispatch(CommonUsersActions.getUsers(searchParameters));
        this.setState({ searchParameters }, () => {
            let {searchParameters} = this.state;
            searchParameters.resetIndex = false;
            this.setState({ searchParameters });
        });
    }

    onPageChanged(currentPage, pageSize) {
        let {searchParameters} = this.state;
        searchParameters.pageIndex = currentPage;
        searchParameters.pageSize = pageSize;
        this.props.dispatch(CommonUsersActions.getUsers(searchParameters));
        this.setState({ searchParameters });
    }

    getWorkSpaceTray() {
        return this.state.userFilters.length > 0 &&
            <GridCell className="users-workspace-tray">
                <FiltersBar
                    onChange={this.onFilterChange.bind(this) }
                    userFilters={this.state.userFilters}
                />
            </GridCell>;
    }

    onRemoveDeletedUsers() {
        utilities.confirm(
            Localization.get("RemoveDeleted.Confirm"),
            Localization.get("Yes"),
            Localization.get("No"),
            () => {
                this.props.dispatch(CommonUsersActions.removeDeletedUsers(() => {
                    let {searchParameters} = this.state;
                    this.props.dispatch(CommonUsersActions.getUsers(searchParameters));
                    utilities.notify(Localization.get("RemoveDeleted.Done"));
                }));
            }
        );
    }

    toggleCreateBox() {
        this.userTable.wrappedInstance.onAddUser();
    }
    canAddUser()
    {
        return appSettings.applicationSettings.settings.isAdmin || appSettings.applicationSettings.settings.permissions.ADD_USER;
    }
    render() {
        const {props, state} = this;
        const panelBodyMargin = state.createBoxVisible ? "without-margin" : "";
        return (
            <GridCell>
                <PersonaBarPageHeader title={Localization.get("nav_Users") }>
                    {
                        this.canAddUser() &&  
                    <Button type="primary" size="large" onClick={this.toggleCreateBox.bind(this) } title={Localization.get("btnCreateUser")}>
                        {Localization.get("btnCreateUser") }
                    </Button>
                    }
                    {
                        appSettings.applicationSettings.settings.isAdmin &&  
                    <Button type="secondary" size="large" onClick={() => {this.onRemoveDeletedUsers()}} title={Localization.get("RemoveDeleted.Btn")}>
                        {Localization.get("RemoveDeleted.Btn") }
                    </Button>
                    }
                </PersonaBarPageHeader>
                <PersonaBarPageBody workSpaceTrayVisible={true} workSpaceTrayOutside={true} workSpaceTray={this.getWorkSpaceTray() } className={panelBodyMargin}>
                    <UserTable ref={(node) => this.userTable = node} appSettings={appSettings} filter={state.searchParameters.filter}/>
                    {
                        <div className="users-paging">
                            <Pager pageSizeDropDownWithoutBorder={true} 
                                showSummary={true} 
                                showPageInfo={false}
                                pageSizeOptionText={Localization.get("usersPageSizeOptionText")}
                                summaryText={Localization.get("usersSummaryText")}
                                pageSize={this.state.searchParameters.pageSize}
                                totalRecords={props.totalUsers}
                                onPageChanged={this.onPageChanged.bind(this) }
                                resetIndex={this.state.searchParameters.resetIndex}
                                culture={utilities.getCulture() }
                            />
                        </div>
                    }
                </PersonaBarPageBody >
            </GridCell>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired,
    totalUsers: PropTypes.number
};

function mapStateToProps(state) {
    return {
        totalUsers: state.users.totalUsers
    };
}


export default connect(mapStateToProps)(Body);