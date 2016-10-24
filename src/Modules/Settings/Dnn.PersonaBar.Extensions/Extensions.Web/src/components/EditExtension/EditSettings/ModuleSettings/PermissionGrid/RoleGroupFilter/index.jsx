import React, { PropTypes, Component } from "react";
import ReactDOM from "react-dom";
import DropDown from "dnn-dropdown";

import { ArrowDownIcon, ArrowRightIcon, CheckboxUncheckedIcon, CheckboxCheckedIcon, CheckboxPartialCheckedIcon, PagesIcon } from "dnn-svg-icons";
import "./style.less";
import { RoleGroupService } from "services";
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
            reload: false
        };
    }

    componentWillMount() {
        const { props, state } = this;

        this.getRoleGroups();
    }

    componentWillReceiveProps(newProps) {
        console.log(newProps);
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

        let service = new RoleGroupService(this.props.serviceFramework);
        service.getRoleGroups((data) => {
            this.setState({
                roleGroups: data
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

    }

    getRoleGroupsDropDown() {
        const {props, state} = this;

        let label = props.label;
        let roleGroupsOptions = this.BuildRoleGroupsOptions();

        let GroupsDropDown = <DropDown style={{ width: "100%" }}
            withBorder={false}
            options={roleGroupsOptions}
            label={label}
            onSelect={this.onSelect.bind(this)}
            ref="groupsDropdown"
            />;

        return GroupsDropDown;
    }


    render() {
        const {props, state} = this;

        return (
            <div className="groups-filter">{this.getRoleGroupsDropDown()}<div className="clear"></div></div>
        );
    }
}

RoleGroupFilter.propTypes = {
    dispatch: PropTypes.func.isRequired,
    serviceFramework: PropTypes.object,
    label: PropTypes.object,
    onChange: PropTypes.func.isRequired
};

RoleGroupFilter.defaultProps = {
};

export default RoleGroupFilter;