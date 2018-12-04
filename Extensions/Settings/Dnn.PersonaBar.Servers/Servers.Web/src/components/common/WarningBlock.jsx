import React, {Component } from "react";
import PropTypes from "prop-types";
import GridCell from "dnn-grid-cell";
import "./style.less";
import { ErrorStateIcon } from "dnn-svg-icons";

export default class WarningBlock extends Component {
    
    render() {
        /* eslint-disable react/no-danger */
        const {props} = this;

        return <GridCell className="serversTabWarningInfo">
            <div dangerouslySetInnerHTML={{ __html: ErrorStateIcon }} />
            <div className="dnn-label title"
                dangerouslySetInnerHTML={{ __html: props.label}} />
        </GridCell>;
    }
}

WarningBlock.propTypes = {
    label: PropTypes.string
};