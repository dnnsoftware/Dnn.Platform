import React, {Component, PropTypes } from "react";
import "./style.less";
import { ErrorStateIcon } from "dnn-svg-icons";

export default class WarningBlock extends Component {
    
    render() {
        /* eslint-disable react/no-danger */
        const {props} = this;

        return <div className="serversTabWarningInfo">
            <div dangerouslySetInnerHTML={{ __html: ErrorStateIcon }} />
            <div className="dnn-label title"
                dangerouslySetInnerHTML={{ __html: props.label}} />
        </div>;
    }
}

WarningBlock.propTypes = {
    label: PropTypes.string
};