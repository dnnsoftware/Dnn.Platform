import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import Button from "dnn-button";
import PersonaBarPageHeader from "dnn-persona-bar-page-header";
import PersonaBarPage from "dnn-persona-bar-page";
import RolesPanel from "./roles";
import {
    visiblePanelActions as VisiblePanelActions
} from "../actions";
import resx from "../resources";

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
        this.refs["rolesPanel"].getWrappedInstance().onAddRole();
    }

    render() {
        return (
            <div className="roles-app">
                <PersonaBarPage isOpen={true}>
                    <PersonaBarPageHeader title={resx.get("Roles")}>
                        <Button type="primary" size="large" onClick={this.onCreate.bind(this)}>{resx.get("Create")}</Button>
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


function mapStateToProps() {
    return {};
}


export default connect(mapStateToProps)(Root);