import React, { Component } from "react";
import PropTypes from "prop-types";
import Localization from "../../localization";
import { Label, Tags } from "@dnnsoftware/dnn-react-common";

class PageTags extends Component {

    render() {
        const {props} = this;
        return <div className="input-group">
            <Label label={Localization.get("Tags")} />
            <Tags
                tags={props.tags}
                onUpdateTags={(tags) => props.onChangeTags(tags)} />
            <div style={{ clear: "both" }}></div>
        </div>;
    }
}

PageTags.propTypes = {
    tags: PropTypes.array.isRequired,
    onChangeTags: PropTypes.func.isRequired
};

export default PageTags;