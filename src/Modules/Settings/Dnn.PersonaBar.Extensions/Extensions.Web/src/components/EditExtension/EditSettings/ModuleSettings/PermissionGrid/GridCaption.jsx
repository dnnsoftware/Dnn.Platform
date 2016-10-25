import React, { PropTypes, Component } from "react";
import { connect } from "react-redux";

import { ExtensionActions } from "actions";

import GridCell from "dnn-grid-cell";
import Label from "dnn-label";
import RoleGroupFilter from "./RoleGroupFilter";
import Suggestion from "./Suggestion";

import Localization from "localization";

const nameColumnSpace = 15;
const actionColumnSpace = 10;

class GridCaption extends Component {

    constructor(){
        super();

        this.state = {
            roleGroupId: -1
        };
    }

    onRoleGroupChange(value){
        this.setState({roleGroupId: value.id});
    }

    render() {
        const { props, state } = this;

        let suggestionOptions = props.type === "role" ? {
                                    actionName: "GetSuggestionRoles",
                                    roleGroupId: state.roleGroupId,
                                    count: 10
                                } : {

                                };

        return <GridCell className="grid-caption">
                    <Label label={props.type === "role" ? props.localization.permissionsByRole : props.localization.permissionsByUser}></Label>
                    <GridCell columnSize="40" className="left">
                        <RoleGroupFilter 
                            service={props.service} 
                            localization={props.localization} 
                            value={state.roleGroupId} 
                            onChange={this.onRoleGroupChange.bind(this)}/>
                    </GridCell>
                    <GridCell columnSize="20" />
                    <GridCell columnSize="40" className="right">
                        <Suggestion 
                            service={props.service} 
                            localization={{
                                add: props.type === "role" ? props.localization.addRole : props.localization.addUser
                            }}  
                            options={suggestionOptions}
                             />
                    </GridCell>
                </GridCell>;
    }
}

GridCaption.propTypes = {
    localization: PropTypes.object.isRequired,
    service: PropTypes.object.isRequired,
    type: PropTypes.oneOf(["role", "user"])
};


export default GridCaption;