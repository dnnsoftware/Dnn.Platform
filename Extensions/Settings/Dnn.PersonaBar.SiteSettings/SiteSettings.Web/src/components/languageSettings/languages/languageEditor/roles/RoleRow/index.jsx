import React, { Component, PropTypes } from "react";
import "./style.less";
import GridCell from "dnn-grid-cell";
import Checkbox from "dnn-checkbox";
import { connect } from "react-redux";
import {
    languages as LanguagesActions
} from "actions";

class RoleRow extends Component {
    constructor() {
        super();
        this.state = {
            selected: false
        };
    }

    componentWillMount() {
        const {props} = this;

        this.setState({
            selected: props.selected
        });
    }

    componentWillReceiveProps(props) {
        this.setState({
            selected: props.selected
        });
    }

    onChange(role, event) {
        let {props} = this;
        this.setState({
            selected: event
        });
        props.dispatch(LanguagesActions.SelectLanguageRoles(props.rolesList, role, event));
        props.onSelectChange(role, event);
    }

    render() {
        const {props} = this;

        return (
            <div className={"collapsible-component-language-roles"}>
                <div className={"collapsible-header-language-roles "} >
                    <GridCell title={props.roleName} columnSize={60} >
                        {props.roleName}</GridCell>
                    <GridCell columnSize={40} >
                        <Checkbox
                            style={{ float: "left" }}
                            value={props.selected}
                            onChange={this.onChange.bind(this, props.roleName) } /></GridCell>
                </div>
            </div>
        );
    }
}

RoleRow.propTypes = {
    dispatch: PropTypes.func.isRequired,
    roleName: PropTypes.string,
    roleId: PropTypes.number,
    selected: PropTypes.bool,
    onSelectChange: PropTypes.func,
    id: PropTypes.string,
    rolesList: PropTypes.array
};

function mapStateToProps(state) {
    return {
        rolesList: state.languages.rolesList
    };
}

export default connect(mapStateToProps)(RoleRow);