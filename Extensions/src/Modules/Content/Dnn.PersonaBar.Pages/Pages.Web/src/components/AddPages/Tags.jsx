import React, { Component, PropTypes } from "react";
import Localization from "../../localization";
import Label from "dnn-label";
import DnnTags from "dnn-tags";

class Tags extends Component {

    render() {
        const {props} = this;
        return <div className="input-group">
            <Label label={Localization.get("Tags")} />
            <DnnTags
                tags={props.tags}
                onUpdateTags={(tags) => props.onChangeTags(tags)} />
            <div style={{ clear: "both" }}></div>
        </div>;
    }
}

Tags.propTypes = {
    tags: PropTypes.array.isRequired,
    onChangeTags: PropTypes.func.isRequired
};

export default Tags;