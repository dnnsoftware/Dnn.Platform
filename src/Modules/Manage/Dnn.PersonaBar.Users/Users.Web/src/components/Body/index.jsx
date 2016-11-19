import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Localization from "localization";
import Button from "dnn-button";
import GridCell from "dnn-grid-cell";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import { UserTable, FiltersBar } from "dnn-users-common-components";
import Pager from "dnn-pager";
import "./style.less";
import {CommonUsersActions } from "dnn-users-common-actions";
import appSettings from "utils/applicationSettings";

const searchParameters = {
    searchText: "",
    filter: 0,
    pageIndex: 0,
    pageSize: 10,
    sortColumn: "",
    sortAscending: false
};
class Body extends Component {
    constructor() {
        super();
        this.state = {
            userFilters: [],
            searchParameters
        };
    }
    componentWillMount() {
        this.props.dispatch(CommonUsersActions.getUserFilters((data) => {
            let userFilters = Object.assign([], JSON.parse(JSON.stringify(data)));
            this.setState({
                userFilters
            });
        }));
    }

    onChange(key, event) {
        this.setState({
            [key]: event.target.value
        });
    }

    onFilterChange(option, searchText) {
        let {searchParameters} = this.state;
        searchParameters.searchText = searchText;
        searchParameters.filter = option.value;
        searchParameters.pageIndex = 0;
        this.props.dispatch(CommonUsersActions.getUsers(searchParameters));
        this.setState({ searchParameters });
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

    toggleCreateBox() {
        this.refs["userTable"].getWrappedInstance().onAddUser();
    }
    canAddUser()
    {
        return appSettings.applicationSettings.settings.permissions.ADD_USER;
    }
    render() {
        const {props, state} = this;
        const panelBodyMargin = state.createBoxVisible ? "without-margin" : "";
        return (
            <GridCell>
                <SocialPanelHeader title={Localization.get("nav_Users") }>
                 {
                    this.canAddUser() &&  
                    <Button type="primary" size="large" onClick={this.toggleCreateBox.bind(this) } title={Localization.get("btnCreateUser")}>
                        {Localization.get("btnCreateUser") }
                    </Button>
                }
                </SocialPanelHeader>
                <SocialPanelBody workSpaceTrayVisible={true} workSpaceTrayOutside={true} workSpaceTray={this.getWorkSpaceTray() } className={panelBodyMargin}>
                    <UserTable ref="userTable" appSettings={appSettings}/>
                    {
                        (state.searchParameters.filter === 0 || state.searchParameters.filter === 3) && <div className="users-paging">
                            <Pager pageSizeDropDownWithoutBorder={true} 
                                showSummary={true} 
                                showPageInfo={false}
                                pageSizeOptionText={Localization.get("usersPageSizeOptionText")}
                                summaryText={Localization.get("usersSummaryText")}
                                pageSize={this.state.searchParameters.pageSize}
                                totalRecords={props.totalUsers}
                                onPageChanged={this.onPageChanged.bind(this) }
                                />
                        </div>
                    }
                </SocialPanelBody >
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