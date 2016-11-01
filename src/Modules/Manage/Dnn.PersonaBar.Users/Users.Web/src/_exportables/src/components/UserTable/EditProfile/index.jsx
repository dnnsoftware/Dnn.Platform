import React, {Component, PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import GridSystem from "dnn-grid-system";
import Localization from "localization";
import "./style.less";

class EditProfile extends Component {
    render() {
        return (
            <GridCell columnSize={100} className="edit-profile">
                Hello World
            </GridCell>
        );
    }
}

EditProfile.propTypes = {
};


export default EditProfile;