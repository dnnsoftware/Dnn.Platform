import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import resx from "../../resources";
import {
    roles as RolesActions
} from "../../actions";
import FiltersBar from "./FiltersBar";
import RoleRow from "./RoleRow";
import { GridCell }  from "@dnnsoftware/dnn-react-common";
import UsersInRole from "./UsersInRole";
import RoleEditor from "./RoleEditor";
import CollapsibleSwitcher from "../common/CollapsibleSwitcher";
import {
    createRoleGroupOptions
} from "./helpers/roles";

const tableFields = [
    { name: "RoleName", width: 40 },
    { name: "GroupName", width: 20 },
    { name: "Users", width: 15 },
    { name: "Auto", width: 15 },
    { name: "", width: 10 }
];

class RolesPanel extends Component {
    constructor() {
        super();

        this.state = {
            defaultRoleGroup: resx.get("AllGroups"),
            openId: "",
            renderIndex: -1,
            deleteAllowed: false,
            parameter: {
                groupId: -1,
                keyword: "",
                startIndex: 0,
                pageSize: 10,
                reload: true
            }
        };
    }

    componentDidMount() {
        const {props} = this;
        props.dispatch(RolesActions.getRoleGroupsList(false));
        this.refreshRolesList();
        let {deleteAllowed} = this.state;
        deleteAllowed = (props.rolesList !== undefined && props.rolesList.length === 0);
        this.setState({ deleteAllowed });
    }

    static getDerivedStateFromProps(props, state) {
        let {deleteAllowed} = state;
        deleteAllowed = (props.rolesList !== undefined && props.rolesList.length === 0);
        return { deleteAllowed };
    }

    refreshRolesList() {
        const {props, state} = this;
        props.dispatch(RolesActions.getRolesList(state.parameter, (data) => {
            let rolesList = Object.assign([], data.roles);
            this.setState({
                rolesList
            });
        }));
    }

    onPageChanged() {
        const {props} = this;
        let startIndex = props.rolesList.length;
        let {parameter} = this.state;
        parameter.startIndex = startIndex;
        parameter.reload = false;
        this.setState({
            parameter
        });
        this.refreshRolesList(startIndex);
    }

    onAddRole() {
        this.toggle(this.state.openId === "add" ? "" : "add", 1);
    }

    uncollapse(id, index) {
        setTimeout(() => {
            this.setState({
                openId: id,
                renderIndex: index
            });
        }, this.timeout);
    }
    collapse() {
        if (this.state.openId !== "") {
            this.setState({
                openId: "",
                renderIndex: -1
            });
        }
    }
    toggle(openId, index) {
        if (openId !== "") {
            this.uncollapse(openId, index);
        } else {
            this.collapse();
        }
    }
    onRoleGroupChanged(group) {
        let {parameter} = this.state;
        parameter.groupId = group.value;
        parameter.startIndex = 0;
        parameter.reload = true;
        this.setState({
            parameter
        });
        this.refreshRolesList();
    }

    onKeywordChanged(keyword) {
        let {parameter} = this.state;
        parameter.keyword = keyword;
        parameter.startIndex = 0;
        parameter.reload = true;
        this.setState({
            parameter
        });
        this.refreshRolesList();
    }
    GetGroupName(groupId) {
        const {props} = this;
        if (props.roleGroups.some(group => group.id === groupId)) {
            return props.roleGroups.filter(group => group.id === groupId)[0].name;
        }

        return groupId;
    }

    renderHeader() {
        let tableHeaders = tableFields.map((field) => {
            return <GridCell key={field.name} columnSize={field.width} style={{ fontWeight: "bolder" }}>
                {
                    field.name !== "" ?
                        <span>{resx.get(field.name + ".Header")}</span>
                        : <span>&nbsp; </span>
                }
            </GridCell>;
        });
        return <div id="users-header-row" className="users-header-row">{tableHeaders}</div>;
    }
    renderedRolesList(roleGroupOptions, securityModeOptions, statusOptions) {
        if (this.props.rolesList.length > 0) {
            let validRolesList = this.props.rolesList.filter(logSetting => !!logSetting);
            let i = 0;

            return validRolesList.map((role, index) => {
                let id = "row-" + i++;
                let children = [
                    <UsersInRole key={"userInRole-" + id}
                        Collapse={this.collapse.bind(this)} roleDetails={role} />,
                    <RoleEditor
                        key={"roleeditor-" + id}
                        roleDetails={role}
                        roleGroupOptions={roleGroupOptions}
                        securityModeOptions={securityModeOptions}
                        statusOptions={statusOptions}
                        refreshRolesList={this.refreshRolesList.bind(this)}
                        roleId={role.id}
                        Collapse={this.collapse.bind(this)}
                        currentGroupId={this.state.parameter.groupId}
                    />];
                return (
                    <RoleRow
                        key={"role-" + index}
                        roleName={role.name}
                        groupName={this.GetGroupName(role.groupId)}
                        userCount={role.usersCount}
                        auto={role.autoAssign}
                        index={index}
                        closeOnClick={true}
                        openId={this.state.openId}
                        currentIndex={this.state.renderIndex}
                        OpenCollapseUsers={this.toggle.bind(this, id, 0)}
                        OpenCollapseEditRoles={this.toggle.bind(this, id, 1)}
                        Collapse={this.collapse.bind(this, id)}
                        roleIsApproved={role.status === 1}
                        id={id}>
                        <CollapsibleSwitcher renderIndex={this.state.renderIndex}>{children}</CollapsibleSwitcher>
                    </RoleRow>
                );
            });
        }
        else {
            return <div className="no-users-row">{resx.get("NoData")}</div>;
        }
    }
    render() {
        const {props, state} = this;
        let opened = (this.state.openId === "add");
        let roleGroupOptions = createRoleGroupOptions(this.props.roleGroups);
        const securityModeOptions = [
            { label: resx.get("SecurityRole"), value: 0 },
            { label: resx.get("SocialGroup"), value: 1 },
            { label: resx.get("Both"), value: 2 }
        ];
        const statusOptions = [
            { label: resx.get("Pending"), value: -1 },
            { label: resx.get("Disabled"), value: 0 },
            { label: resx.get("Approved"), value: 1 }
        ];
        let children = [
            <div key="" />,
            <RoleEditor
                key=""
                roleGroupOptions={roleGroupOptions}
                securityModeOptions={securityModeOptions}
                statusOptions={statusOptions}
                roleId={-1}
                Collapse={this.collapse.bind(this)}
                currentGroupId={this.state.parameter.groupId}
            />];
        return (
            props.roleGroups &&
            <div className="roles-list-container">
                <FiltersBar onRoleGroupChanged={this.onRoleGroupChanged.bind(this)}
                    roleGroups={this.props.roleGroups}
                    onKeywordChanged={this.onKeywordChanged.bind(this)}
                    DeleteAllowed={state.deleteAllowed}
                    />
                <div className="container">
                    {this.renderHeader()}
                    <div className="add-setting-editor">
                        <RoleRow
                            roleName={"-"}
                            groupName={"-"}
                            userCount={0}
                            auto={false}
                            index={"add"}
                            key={"role-add"}
                            closeOnClick={true}
                            openId={this.state.openId}
                            currentIndex={this.state.renderIndex}
                            OpenCollapseUsers={this.toggle.bind(this, "add", 1)}
                            OpenCollapseEditRoles={this.toggle.bind(this, "add", 1)}
                            Collapse={this.collapse.bind(this, "add")}
                            id={"add"}
                            addIsClosed={!opened}>
                            {opened && <CollapsibleSwitcher renderIndex={this.state.renderIndex}>{children}</CollapsibleSwitcher>}
                        </RoleRow>
                    </div>
                    {this.renderedRolesList(roleGroupOptions, securityModeOptions, statusOptions)}
                </div>
                {props.loadMore && <div className="loadMore">
                    <a href="#" onClick={this.onPageChanged.bind(this)}>
                        {resx.get("LoadMore")}
                    </a>
                </div>}
            </div>
        );
    }
}
RolesPanel.propTypes = {
    dispatch: PropTypes.func.isRequired,
    rolesList: PropTypes.array,
    roleGroups: PropTypes.array,
    loadMore: PropTypes.bool
};

function mapStateToProps(state) {
    return {
        roleGroups: state.roles.roleGroups,
        rolesList: state.roles.rolesList,
        loadMore: state.roles.loadMore
    };
}

export default connect(mapStateToProps, null, null, { withRef: true })(RolesPanel);