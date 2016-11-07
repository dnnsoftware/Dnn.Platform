import React, { Component, PropTypes } from "react";
import { connect } from "react-redux";
import SocialPanelBody from "dnn-social-panel-body";
import GridCell from "dnn-grid-cell";
import LanguageInfoView from "./languageInfoView";
import { visiblePanel as VisiblePanelActions } from "actions";
import resx from "resources";
import ResourceList from "./resourceList";
import "./style.less";

const ModeOptions = [
    {
        label: "Global",
        value: "Host"
    },
    {
        label: "My Website E90-534-6",
        value: "My Website"
    }
];

class EditLanguagePanel extends Component {
    constructor() {
        super();
        this.state = {
            testResources: [
                {
                    resourceName: "//AboutUs.String",
                    defaultValue: "About Us",
                    localizedValue: "About Us"
                },

                {
                    resourceName: "//AboutUs.String",
                    defaultValue: "About Us",
                    localizedValue: "About Us"
                },

                {
                    resourceName: "//AboutUs.String",
                    defaultValue: "About Us",
                    localizedValue: "About Us"
                },

                {
                    resourceName: "//AboutUs.String",
                    defaultValue: "About Us",
                    localizedValue: "About Us"
                }
            ]
        };
    }
    backToSiteSettings() {
        this.props.dispatch(VisiblePanelActions.selectPanel(0));
    }
    /* eslint-disable react/no-danger */
    render() {
        const { props } = this;
        const { languageBeingEdited } = props;
        return (
            <SocialPanelBody
                className="edit-language-panel"
                workSpaceTrayOutside={true}
                workSpaceTray={<div className="siteSettings-back dnn-grid-cell" onClick={this.backToSiteSettings.bind(this)}>{resx.get("BackToSiteSettings")}</div>}
                workSpaceTrayVisible={true}>
                <LanguageInfoView languageBeingEdited={languageBeingEdited} ModeOptions={ModeOptions} />
                <ResourceList list={this.state.testResources} />
            </SocialPanelBody>
        );
    }
}

EditLanguagePanel.propTypes = {
    dispatch: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    return {
        languageBeingEdited: state.languageEditor.languageBeingEdited
    };
}
export default connect(mapStateToProps)(EditLanguagePanel);
