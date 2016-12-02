import React, { PropTypes, Component } from "react";
import DropDown from "dnn-dropdown";
import Label from "dnn-label";

import "./style.less";
import Service from "./Service";
function format() {
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/{(\d+)}/gi, function (value, index) {
        let argsIndex = parseInt(index) + 1;
        return methodsArgs[argsIndex];
    });
}
class RoleGroupFilter extends Component {
    constructor(props) {
        super(props);
        this.state = {
            roleGroups: [],
            selectedGroup: { id: -1, name: props.localization.globalGroupsText }
        };
    }

    componentWillMount() {
        const { props, state } = this;

        this.getRoleGroups();
    }

    closeDropdown() {
        /*This is done in order to keep the dropdown closed on click on edit/delete*/
        let {state} = this.refs["groupsDropdown"];
        state.dropDownOpen = true;
        this.refs["groupsDropdown"].setState({
            state
        });
    }

    getRoleGroups(callback) {
        const { props, state } = this;

        if (state.roleGroups.length > 0) {
            if (typeof callback === "function") {
                callback.call(this);
            }
        }

        let service = new Service(this.props.service);
        service.getRoleGroups((data) => {
            this.setState({
                roleGroups: [{ id: -2, name: props.localization.allGroupsText }, { id: -1, name: props.localization.globalGroupsText }].concat(data)
            }, () => {
                if (typeof callback === "function") {
                    callback.call(this);
                }
            });
        });
    }

    BuildRoleGroupsOptions() {
        const { props, state } = this;

        let roleGroupsOptions = state.roleGroups.map((group) => {
            return { label: group.name, value: group.id };
        });

        return roleGroupsOptions;
    }

    onSelect(group) {
        const {props, state} = this;

        let selectedGroup = { id: group.value, name: group.label };
        this.setState({ selectedGroup: selectedGroup }, function () {
            if (typeof props.onChange === "function") {
                props.onChange(this.state.selectedGroup);
            }
        });
    }

    getRoleGroupsDropDown() {
        const {props, state} = this;

        let roleGroupsOptions = this.BuildRoleGroupsOptions();

        let GroupsDropDown = <DropDown
            withBorder={false}
            options={roleGroupsOptions}
            value={state.selectedGroup.id}
            onSelect={this.onSelect.bind(this) }
            style={{ width: "100%" }}
            ref="groupsDropdown"
            prependWith={props.localization.filterByGroup}
            />;

        return GroupsDropDown;
    }


    render() {
        const {props, state} = this;

        return (
            <div className={"groups-filter" + (state.roleGroups.length === 0 ? "no-group" : "") }>
                {this.getRoleGroupsDropDown() }
                <div className="clear"></div>
            </div>
        );
    }
}

RoleGroupFilter.propTypes = {
    dispatch: PropTypes.func.isRequired,
    service: PropTypes.object,
    localization: PropTypes.object,
    onChange: PropTypes.func.isRequired
};

RoleGroupFilter.defaultProps = {
};

export default RoleGroupFilter;