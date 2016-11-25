import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import Button from "dnn-button";
import PersonaBarPageHeader from "dnn-persona-bar-page-header";
import PersonaBarPage from "dnn-persona-bar-page";
import RolesPanel from "./roles";
import {
    roles as RolesActions, visiblePanelActions as VisiblePanelActions
} from "../actions";
import resx from "../resources";
require("es6-object-assign").polyfill();
require("array.prototype.find").shim();
require("array.prototype.findindex").shim();

class Root extends Component {
    constructor() {
        super();
    }

    navigateMap(page, index, event) {
        event.preventDefault();
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page, index));
    }

    onCreate() {
        const {props, state} = this;

        this.refs["rolesPanel"].getWrappedInstance().onAddRole();
    }

    render() {
        const {props} = this;
        return (
            <div className="roles-app">
                <PersonaBarPage isOpen={true}>
                    <PersonaBarPageHeader title={resx.get("Roles") }>
                        <Button type="primary" size="large" onClick={this.onCreate.bind(this) }>{resx.get("Create") }</Button>
                    </PersonaBarPageHeader>
                    <RolesPanel ref="rolesPanel" />
                </PersonaBarPage>
            </div>
        );
    }
}

Root.PropTypes = {
    dispatch: PropTypes.func.isRequired,
    selectedPage: PropTypes.number,
    selectedPageVisibleIndex: PropTypes.number
};


function mapStateToProps(state) {
    return {
    };
}


export default connect(mapStateToProps)(Root);