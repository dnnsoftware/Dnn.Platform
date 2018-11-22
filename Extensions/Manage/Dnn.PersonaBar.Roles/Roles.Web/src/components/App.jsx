import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Button, PersonaBarPageHeader, PersonaBarPage}  from "@dnnsoftware/dnn-react-common";
import RolesPanel from "./roles";
import {
    visiblePanelActions as VisiblePanelActions
} from "../actions";
import resx from "../resources";
import util from "utils";
let canEdit = false;
class Root extends Component {
    constructor() {
        super();
        canEdit = util.settings.isHost || util.settings.isAdmin || util.settings.permissions.EDIT;
    }

    navigateMap(page, index, event) {
        event.preventDefault();
        const {props} = this;
        props.dispatch(VisiblePanelActions.selectPanel(page, index));
    }

    onCreate() {
        this.rolesPanelref.getWrappedInstance().onAddRole();
    }

    render() {
        return (
            <div className="roles-app">
                <PersonaBarPage isOpen={true}>
                    <PersonaBarPageHeader title={resx.get("Roles") }>
                        {canEdit &&
                            <Button type="primary" size="large" onClick={this.onCreate.bind(this) }>{resx.get("Create") }</Button>
                        }
                    </PersonaBarPageHeader>
                    <RolesPanel ref={node => this.rolesPanelref = node} />
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