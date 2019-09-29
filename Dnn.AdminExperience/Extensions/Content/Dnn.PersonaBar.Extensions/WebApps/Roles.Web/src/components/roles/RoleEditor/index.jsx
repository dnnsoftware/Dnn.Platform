import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import util from "../../../utils";
import resx from "../../../resources";
import { Dropdown as Select, GridSystem as Grid, Switch, Button, SingleLineInputWithError, MultiLineInput, Label }  from "@dnnsoftware/dnn-react-common";
import RoleGroupEditor from "./RoleGroupEditor";
import {
    roles as RolesActions
} from "../../../actions";

class RolesEditor extends Component {
    constructor(props) {
        super(props);
        let roleDetails = Object.assign({}, props.roleDetails);
        this.state = {
            roleDetails: props.roleId !== -1 ? roleDetails : {
                id: -1,
                name: "",
                groupId: -1,
                description: "",
                securityMode: 0,
                status: 1,
                isPublic: false,
                autoAssign: false,
                isSystem: false
            },
            errors: {
                roleName: false
            },
            groupId: props.roleId !== -1 ? roleDetails.groupId : -1,
            assignToUsers: false,
            formModified: false,
            createGroupVisible: false
        };
        this.submitted = false;
    }

    getValue(selectKey) {
        const { state } = this;
        switch (selectKey) {
            case "RoleGroup":
                return state.roleDetails.groupId !== undefined ? state.roleDetails.groupId : -1;
            case "SecurityMode":
                return state.roleDetails.securityMode !== undefined ? state.roleDetails.securityMode : 0;
            case "Status":
                return state.roleDetails.status !== undefined ? state.roleDetails.status : 1;
            case "AutoAssignment":
                return state.roleDetails.autoAssign !== undefined ? state.roleDetails.autoAssign : false;
            case "Public":
                return state.roleDetails.isPublic !== undefined ? state.roleDetails.isPublic : false;
            default:
                break;
        }
    }

    onDropDownChange(key, option) {
        if (key === "groupId" && option.value === -3) {
            let { createGroupVisible } = this.state;
            createGroupVisible = true;
            this.setState({
                createGroupVisible
            });
        }
        this.performChange(key, option.value);
    }
    onTextChange(key, event) {
        this.performChange(key, event.target.value);
    }
    performChange(key, value) {
        if (key !== "assignToUsers") {
            let { roleDetails } = this.state;
            roleDetails[key] = value;
            this.setState({
                roleDetails
            });
        } else {
            let { assignToUsers } = this.state;
            assignToUsers = value;
            this.setState({
                assignToUsers
            });
        }
        let { state } = this;
        state.formModified = true;
        this.setState({
            state
        }, () => {
            this.validateForm();
        });
    }

    onSwitchToggle(key, status) {
        this.performChange(key, status);
    }
    addUpdateRoleDetails(event) {
        event.preventDefault();
        const { props, state } = this;
        this.submitted = true;
        if (!this.validateForm()) {
            return;
        }

        if (state.formModified) {
            let { roleDetails } = this.state;
            if (roleDetails.groupId === -3) {
                return;//Avoid saving while in create group window
            }

            let successMsg = resx.get("RoleAdded.Message");
            let errorMsg = resx.get("RoleAdded.Error");
            if (props.roleId > 0) {
                successMsg = resx.get("RoleUpdated.Message");
                errorMsg = resx.get("RoleUpdated.Error");
            }
            props.dispatch(RolesActions.saveRole(this.props.currentGroupId, state.assignToUsers, roleDetails, () => {
                util.utilities.notify(successMsg);
                props.Collapse(event);
            }, () => {
                util.utilities.notify(errorMsg);
            }));
        }
        else {
            props.Collapse(event);
        }
    }

    validateForm() {
        let valid = true;
        if (this.submitted) {
            let { roleDetails } = this.state;
            let { errors } = this.state;
            errors.roleName = false;
            if (roleDetails.name === "") {
                errors.roleName = true;
                valid = false;
            }
            this.setState({ errors });
        }
        return valid;
    }
    refreshRolesListIfRequired() {
        const { props, state } = this;
        let group = props.roleGroups.find(group => group.id === state.groupId);
        if (group !== undefined && group.rolesCount <= 1) {
            props.refreshRolesList();
        }
    }
    deleteRole(event) {
        let { roleDetails } = this.state;
        const { props } = this;
        if (props.roleId > 0) {
            util.utilities.confirm(resx.get("DeleteRole.Confirm"), resx.get("Delete"), resx.get("Cancel"), () => {
                props.dispatch(RolesActions.deleteRole(roleDetails, () => {
                    util.utilities.notify(resx.get("DeleteRole.Message"));
                    props.Collapse(event);
                }));
            });
        }
        else {
            util.utilities.notify(resx.get("DeleteInconsistency.Error"));
        }
    }
    getRoleGroupOptions() {
        let groupOptions = this.props.roleGroupOptions;
        groupOptions = groupOptions.filter(group => {
            return group.value !== -2;
        });
        groupOptions = [{
            label: <span className="do-not-close">{resx.get("lblNewGroup")}</span>, value: -3
        }].concat(groupOptions);
        return groupOptions;
    }
    CloseCreateGroup() {
        let { createGroupVisible } = this.state;
        createGroupVisible = false;
        this.setState({
            createGroupVisible
        });
    }
    onCancelCreateGroup() {
        this.CloseCreateGroup();
        this.onDropDownChange("groupId", { value: -1 });
    }
    onCreateGroup(group) {
        this.CloseCreateGroup();
        this.onDropDownChange("groupId", { value: group.id });
    }
    doNothing() {

    }
    /* eslint-disable react/no-danger */
    render() {
        let { state, props } = this;
        const columnOne = <div key="editor-container-columnOne" className="editor-container">
            <div className="editor-row divider">
                <SingleLineInputWithError
                    value={state.roleDetails.name}
                    enabled={!state.roleDetails.isSystem}
                    onChange={this.onTextChange.bind(this, "name")}
                    maxLength={50}
                    error={state.errors.roleName}
                    label={resx.get("RoleName")}
                    tooltipMessage={resx.get("RoleName.Help")}
                    errorMessage={resx.get("RoleName.Required")}
                    autoComplete="off"
                    inputStyle={{ marginBottom: 0 }}
                    tabIndex={1} />
            </div>
            <div className="editor-row divider">
                <Label
                    label={resx.get("Description")}
                    tooltipMessage={resx.get("Description.Help")}
                    tooltipPlace={"top"} />
                <MultiLineInput
                    value={state.roleDetails.description}
                    onChange={this.onTextChange.bind(this, "description")}
                    maxLength={500} />
            </div>
            <div className="editor-row divider">
                <Label
                    label={resx.get("statusListLabel")}
                    tooltipMessage={resx.get("statusListLabel.Help")}
                    tooltipPlace={"top"} />
                <Select
                    value={this.getValue("Status")}
                    enabled={!state.roleDetails.isSystem}
                    options={this.props.statusOptions}
                    style={{ width: 100 + "%", float: "left" }}
                    onSelect={this.onDropDownChange.bind(this, "status")}
                />
            </div>
            <div className="status-row">
                <div className="left">
                    <Label
                        labelType="inline"
                        label={resx.get("Public")}
                        tooltipMessage={resx.get("PublicRole.Help")}
                        tooltipPlace={"top"} />
                </div>
                <div className="right">
                    <Switch
                        onText={resx.get("SwitchOn")}
                        offText={resx.get("SwitchOff")}
                        readOnly={state.roleDetails.isSystem}
                        value={this.getValue("Public")}
                        onChange={this.onSwitchToggle.bind(this, "isPublic")} />
                </div>
            </div>
        </div>;

        const columnTwo = <div key="editor-container-columnTwo" className="editor-container right-column">
            <div className="editor-row divider">
                <Label
                    label={resx.get("plRoleGroups")}
                    tooltipMessage={resx.get("plRoleGroups.Help")}
                    tooltipPlace={"top"} />
                <Select
                    value={this.getValue("RoleGroup")}
                    enabled={!state.roleDetails.isSystem}
                    options={this.getRoleGroupOptions()}
                    style={{ width: 100 + "%", float: "left" }}
                    onSelect={this.onDropDownChange.bind(this, "groupId")} />
                <div className="new-group-container">
                    <RoleGroupEditor
                        visible={this.state.createGroupVisible}
                        onSave={this.onCreateGroup.bind(this)}
                        onCancel={this.onCancelCreateGroup.bind(this)}
                        onClick={this.doNothing.bind(this)}
                        showPopup={this.state.createGroupVisible}
                        group={{ id: -2, name: "", description: "" }}
                        title="New Group" />
                </div>
            </div>
            <div className="editor-row divider">
                <Label
                    label={resx.get("securityModeListLabel")}
                    tooltipMessage={resx.get("securityModeListLabel.Help")}
                    tooltipPlace={"top"} />
                <Select
                    value={this.getValue("SecurityMode")}
                    enabled={!state.roleDetails.isSystem}
                    options={this.props.securityModeOptions}
                    style={{ width: 100 + "%", float: "left" }}
                    onSelect={this.onDropDownChange.bind(this, "securityMode")}
                />
            </div>
            <div className="editor-row divider">
                <SingleLineInputWithError
                    value={state.roleDetails.rsvpCode}
                    enabled={!state.roleDetails.isSystem}
                    maxLength={50}
                    onChange={this.onTextChange.bind(this, "rsvpCode")}
                    label={resx.get("plRSVPCode")}
                    tooltipMessage={resx.get("plRSVPCode.Help")}
                    inputStyle={{ marginBottom: 5 }}
                    tabIndex={1} />
            </div>
            <div className="editor-row divider">
                <SingleLineInputWithError
                    value={state.roleDetails.rsvpCode &&
                        state.roleDetails.rsvpCode !== "" ?
                        props.rsvpLink + "&rsvp=" + state.roleDetails.rsvpCode : ""}
                    enabled={false}
                    onChange={this.onTextChange.bind(this, "rsvpLink")}
                    label={resx.get("plRSVPLink")}
                    tooltipMessage={resx.get("plRSVPLink.Help")}
                    inputStyle={{ marginBottom: 0 }}
                    tabIndex={1} />
            </div>
            <div className="status-row">
                <div className="left">
                    <Label
                        labelType="inline"
                        label={resx.get("AutoAssignment")}
                        tooltipMessage={resx.get("AutoAssignment.Help")}
                        tooltipPlace={"top"} />
                </div>
                <div className="right">
                    <Switch
                        onText={resx.get("SwitchOn")}
                        offText={resx.get("SwitchOff")}
                        readOnly={state.roleDetails.isSystem}
                        value={state.roleDetails.autoAssign}
                        onChange={this.onSwitchToggle.bind(this, "autoAssign")} />
                </div>
            </div>
            {state.roleDetails.autoAssign &&
                <div className="status-row">
                    <div className="left">
                        <Label
                            labelType="inline"
                            label={resx.get("AssignToExistUsers")}
                            tooltipMessage={resx.get("AssignToExistUsers.Help")}
                            tooltipPlace={"top"} />
                    </div>
                    <div className="right">
                        <Switch
                            onText={resx.get("SwitchOn")}
                            offText={resx.get("SwitchOff")}
                            readOnly={state.roleDetails.isSystem}
                            value={state.assignToUsers}
                            onChange={this.onSwitchToggle.bind(this, "assignToUsers")} />
                    </div>
                </div>
            }
        </div>;
        let children = [];
        children.push(columnOne);
        children.push(columnTwo);
        /* eslint-disable react/no-danger */
        return (
            <div className="role-details-editor">
                <Grid numberOfColumns={2}>{children}</Grid>
                <div className="buttons-box">
                    {
                        this.props.roleId > 0 && (!state.roleDetails.isSystem && state.roleDetails.id > -1) ?
                            <Button
                                type="secondary"
                                onClick={this.deleteRole.bind(this)}>
                                {resx.get("Delete")}
                            </Button>
                            : null
                    }
                    <Button
                        type="secondary"
                        onClick={this.props.Collapse.bind(this)}>
                        {resx.get("Cancel")}
                    </Button>
                    <Button
                        type="primary"
                        onClick={this.addUpdateRoleDetails.bind(this)}>
                        {resx.get("Save")}
                    </Button>
                </div>
            </div>
        );
    }
}
RolesEditor.propTypes = {
    dispatch: PropTypes.func.isRequired,
    roleId: PropTypes.number,
    roleDetails: PropTypes.object,
    roleGroupOptions: PropTypes.array,
    securityModeOptions: PropTypes.array,
    statusOptions: PropTypes.array,
    Collapse: PropTypes.func,
    refreshRolesList: PropTypes.func,
    roleGroups: PropTypes.array,
    currentGroupId: PropTypes.number,
    rsvpLink: PropTypes.string
};
function mapStateToProps(state) {
    return {
        roleGroups: state.roles.roleGroups,
        rsvpLink: state.roles.rsvpLink
    };
}

export default connect(mapStateToProps)(RolesEditor);