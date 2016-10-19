import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Tabs from "dnn-tabs";
import {
    pagination as PaginationActions,
    users as UserActions
} from "../../actions";
import Localization from "localization";
import Button from "dnn-button";
import GridCell from "dnn-grid-cell";
import SocialPanelHeader from "dnn-social-panel-header";
import SocialPanelBody from "dnn-social-panel-body";
import Dropdown from "dnn-dropdown";
import SearchBox from "dnn-search-box";
import UserTable from "./UserTable";
import CreateUserBox from "./CreateUserBox";
import Collapse from "react-collapse";
import FiltersBar from "./FiltersBar";
import Pager from "dnn-pager";
import "./style.less";
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
            selectedRadioButton: 0,
            value: "Test Me!",
            textAreaValue: "Multi line!",
            singleLineValue: "Single line!",
            userFilters: [],
            searchParameters
        };
    }
    componentWillMount() {
        this.props.dispatch(UserActions.getUserFilters((data) => {
            let userFilters = Object.assign([], JSON.parse(JSON.stringify(data.Results)));
            this.setState({
                userFilters
            });
        }));
    }

    onRadioButtonChange(value) {
        alert(value);
    }

    onEnter(key) {
        const { state } = this;
        alert("You pressed enter! My value is: " + state[key]);
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
        this.props.dispatch(UserActions.getUsers(searchParameters));
        this.setState({ searchParameters });
    }

    onPageChanged(currentPage, pageSize) {
        let {searchParameters} = this.state;
        searchParameters.pageIndex = currentPage;
        searchParameters.pageSize = pageSize;
        this.props.dispatch(UserActions.getUsers(searchParameters));
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

    render() {
        const {props, state} = this;
        const panelBodyMargin = state.createBoxVisible ? "without-margin" : "";

        return (
            <GridCell>
                <SocialPanelHeader title={Localization.get("nav_Users") }>
                    <Button type="primary" size="large" onClick={this.toggleCreateBox.bind(this) }>{Localization.get("btn_CreateUser") }</Button>
                </SocialPanelHeader>
                <SocialPanelBody workSpaceTrayVisible={true} workSpaceTrayOutside={true} workSpaceTray={this.getWorkSpaceTray() } className={panelBodyMargin}>
                    <UserTable ref="userTable"/>
                    {
                        state.searchParameters.filter === 0 && <div className="users-paging">
                            <Pager pageSizeDropDownWithoutBorder={true} showSummary={true} showPageInfo={false}
                                pageSizeOptionText={"{0} users per page"}
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
    tabIndex: PropTypes.number,
    totalUsers: PropTypes.number
};

function mapStateToProps(state) {
    return {
        tabIndex: state.pagination.tabIndex,
        totalUsers: state.users.totalUsers
    };
}


export default connect(mapStateToProps)(Body);