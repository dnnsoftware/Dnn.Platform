import React, {Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import "./style.less";
import "react-widgets/lib/less/react-widgets.less";
import "./style.less";
import resx from "../../../resources";
import IconButton from "../../common/IconButton";
import { GridCell, SearchBox, Dropdown }  from "@dnnsoftware/dnn-react-common";
import RoleGroupEditor from "../RoleEditor/RoleGroupEditor";
import {
    roles as RolesActions
} from "../../../actions";
import util from "utils";

let canEdit = false;
class FiltersBar extends Component {
    constructor(props) {
        super(props);
        this.state = {
            selectedGroup: {
                label: resx.get("GlobalRolesGroup"),
                value: -1
            },
            showPopup: false
        };
        canEdit = util.settings.isHost || util.settings.isAdmin || util.settings.permissions.EDIT;
    }
    onDeleteGroup() {
        const {props} = this;
        this.closeDropDown();
        util.utilities.confirm(resx.get("DeleteRoleGroup.Confirm"), resx.get("Delete"), resx.get("Cancel"), () => {
            props.dispatch(RolesActions.deleteRoleGroup(this.getCurrentGroup(), () => {
                util.utilities.notify(resx.get("DeleteRoleGroup.Message"));
                let {selectedGroup} = this.state;
                selectedGroup.value = -1;
                selectedGroup.label = resx.get("GlobalRolesGroup");
                this.setState({
                    selectedGroup
                });
                this.props.onRoleGroupChanged(selectedGroup);
            }));
        }, () => {
        });
    }

    onSave(group) {
        this.updateSelectedGroup({ label: group.name, value: group.id });
        let {showPopup} = this.state;
        showPopup = !showPopup;
        this.setState({
            showPopup
        });
    }
    closeDropDown() {
        /*This is done in order to keep the dropdown closed on click on edit/delete*/
        let {state} = this.groupsDropdownRef;
        if (state) {
            state.dropDownOpen = false;
            this.groupsDropdownRef.setState({
                state
            });
        }
    }
    onSelect(option) {
        this.updateSelectedGroup(option);
        this.props.onRoleGroupChanged(option);
    }
    toggleEditGroup() {
        let {showPopup} = this.state;
        showPopup = !showPopup;
        this.setState({
            showPopup
        }, () => {
            this.closeDropDown();
        });
    }
    updateSelectedGroup(option) {
        let { label} = option;
        let { value} = option;
        let {selectedGroup} = this.state;
        selectedGroup.label = label;
        selectedGroup.value = value;
        this.setState({
            selectedGroup
        });
    }

    getCurrentLabel() {
        let {props} = this;
        let selectedGroup = Object.assign({}, this.state.selectedGroup);
        let value = selectedGroup.value;
        let label = selectedGroup.label;
        /*eslint-disable no-unused-vars*/
        let group = { id: value, name: label };
        if (props.roleGroups.some(group => group.id === this.state.selectedGroup.value)) {
            group = Object.assign({}, props.roleGroups.filter(group => group.id === value)[0]);
        }
        if (value > -1) {
            if (canEdit) {
                label = <div className="group-actions">{label}
                    <div className="role-group-actions">
                        <IconButton type="Edit" onClick={this.toggleEditGroup.bind(this) } />
                        {this.props.DeleteAllowed && <IconButton type="Trash" onClick={this.onDeleteGroup.bind(this) } />}
                    </div>
                </div>;
            }
        }
        return label;
    }
    getCurrentGroup() {
        let {props} = this;
        let selectedGroup = Object.assign({}, this.state.selectedGroup);
        let value = selectedGroup.value;
        let label = selectedGroup.label;
        let group = { id: value, name: label };
        if (props.roleGroups.some(group => group.id === this.state.selectedGroup.value)) {
            group = Object.assign({}, props.roleGroups.filter(group => group.id === value)[0]);
        }
        return group;
    }


    getRoleGroupsDropDown() {
        let label = this.getCurrentLabel();
        let roleGroupsOptions = this.BuildRoleGroupsOptions();
        let GroupsDropDown = <Dropdown  style={{ width: "100%" }}
            withBorder={false}
            options={roleGroupsOptions}
            label={label }
            onSelect={this.onSelect.bind(this) }
            ref={node => this.groupsDropdownRef = node}
        />;
        return <div className="groups-filter">{GroupsDropDown}<div className="clear"></div></div>;
    }

    BuildRoleGroupsOptions() {
        let {roleGroups} = this.props;
        let roleGroupsOptions = [];
        roleGroupsOptions = roleGroups.map((group) => {
            return { label: group.name, value: group.id };
        });
        return roleGroupsOptions;
    }

    render() {
        return <div className="filter-container">
            <GridCell columnSize={35} >
                {this.props.roleGroups.length > 0 && this.getRoleGroupsDropDown() }
                {
                    this.state.showPopup &&
                    <div className="edit-group-popup">
                        <RoleGroupEditor
                            onSave={this.onSave.bind(this) }
                            onClick={this.closeDropDown.bind(this) }
                            onCancel={this.toggleEditGroup.bind(this) }
                            title="Edit Group" group={this.getCurrentGroup() } />
                    </div>
                }
            </GridCell>
            <GridCell columnSize={30} >
                <div>&nbsp; </div></GridCell>
            <GridCell columnSize={35} >
                <div className="search-filter">
                    <SearchBox placeholder={resx.get("SearchPlaceHolder") } onSearch={this.props.onKeywordChanged.bind(this) } maxLength={50} />
                    <div className="clear"></div>
                </div>
            </GridCell>
        </div>;
    }
}
FiltersBar.propTypes = {
    dispatch: PropTypes.func.isRequired,
    onRoleGroupChanged: PropTypes.func.isRequired,
    onKeywordChanged: PropTypes.func.isRequired,
    roleGroups: PropTypes.array.isRequired,
    DeleteAllowed: PropTypes.bool
};
function mapStateToProps(state) {
    return {
        roleGroups: state.roles.roleGroups
    };
}
export default connect(mapStateToProps)(FiltersBar);