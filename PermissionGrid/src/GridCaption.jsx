import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import Label from "dnn-label";
import RoleGroupFilter from "./RoleGroupFilter/RoleGroupFilter";
import Suggestion from "./Suggestion/Suggestion";

const nameColumnSpace = 15;
const actionColumnSpace = 10;

class GridCaption extends Component {

    constructor() {
        super();

        this.state = {
            roleGroupId: -1
        };
    }

    onRoleGroupChange(value) {
        const { props, state } = this;

        this.setState({ roleGroupId: value.id });
    }

    onSuggestionSelected(value) {
        const { props, state } = this;

        if (typeof props.onSuggestion === "function") {
            props.onSuggestion(value);
        }
    }

    render() {
        const { props, state } = this;

        let suggestionOptions = props.type === "role" ? {
            actionName: "GetSuggestionRoles",
            roleGroupId: state.roleGroupId,
            count: 10
        } : {
                actionName: "GetSuggestionUsers",
                count: 10
            };

        return <GridCell className="grid-caption">
            <Label className="title" label={props.type === "role" ? props.localization.permissionsByRole : props.localization.permissionsByUser}></Label>
            {props.type === "role" && <GridCell columnSize="40" className="left">
                <RoleGroupFilter
                    service={props.service}
                    localization={props.localization}
                    value={state.roleGroupId}
                    onChange={this.onRoleGroupChange.bind(this) }/>
            </GridCell>}
            <GridCell columnSize={props.type === "role" ? 20 : 60} />
            <GridCell columnSize="40" className="right">
                <Suggestion
                    service={props.service}
                    localization={{
                        add: props.type === "role" ? props.localization.addRole : props.localization.addUser,
                        placeHolder: props.type === "role" ? props.localization.addRolePlaceHolder : props.localization.addUserPlaceHolder
                    }}
                    options={suggestionOptions}
                    onSelect={this.onSuggestionSelected.bind(this) }
                    />
            </GridCell>
        </GridCell>;
    }
}

GridCaption.propTypes = {
    localization: PropTypes.object.isRequired,
    onSuggestion: PropTypes.func,
    service: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"])
};


export default GridCaption;