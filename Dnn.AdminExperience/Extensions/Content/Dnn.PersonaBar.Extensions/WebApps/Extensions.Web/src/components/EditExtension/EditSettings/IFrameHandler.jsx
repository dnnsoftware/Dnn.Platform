import React, { Component } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";

class IFrameHandler extends Component {
    render() {
        const {props} = this;

        return (
            <iframe src={props.extensionBeingEdited.siteSettingsLink.value} style={{ width: "100%", height: "750px" }} />
        );
    }
}

IFrameHandler.propTypes = {
    dispatch: PropTypes.func.isRequired,
    extensionBeingEdited: PropTypes.object
};

function mapStateToProps(state) {
    return {
        extensionBeingEdited: state.extension.extensionBeingEdited,
        extensionBeingEditedIndex: state.extension.extensionBeingEditedIndex
    };
}

export default connect(mapStateToProps)(IFrameHandler);
