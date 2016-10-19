import React, {Component, PropTypes } from "react";
import { connect } from "react-redux";
import Tabs from "dnn-tabs";
import {
    PaginationActions
} from "actions";
import SocialPanelBody from "dnn-social-panel-body";
import Localization from "localization";
import SocialPanelHeader from "dnn-social-panel-header";
import GridCell from "dnn-grid-cell";
import Button from "dnn-button";
import SiteTheme from "./SiteTheme";
import ThemeList from "./ThemeList";
import RestoreTheme from "./RestoreTheme";
import "./style.less";

const radioButtonOptions = [
    {
        label: "Button 1",
        value: 0
    },
    {
        label: "Button 2",
        value: 1
    }
];

class Body extends Component {
    constructor() {
        super();
        this.handleSelect = this.handleSelect.bind(this);
        this.state = {};
    }

    handleSelect(index/*, last*/) {
        const {props} = this;
        props.dispatch(PaginationActions.loadTab(index));   //index acts as scopeTypeId
    }

    render() {
        const {props, state} = this;
        return (
            <GridCell className="themes-body">
                <SocialPanelHeader title={Localization.get("Themes") }>
                </SocialPanelHeader>
                <SocialPanelBody>
                    <SiteTheme />
                    <GridCell className="middle-actions">
                        <RestoreTheme />
                    </GridCell>
                    <ThemeList />
                </SocialPanelBody >

            </GridCell>
        );
    }
}

Body.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
    };
}

export default connect(mapStateToProps)(Body);