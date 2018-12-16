import React, {Component } from "react";
import PropTypes from "prop-types";
import { GridCell, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./style.less";

export default class WarningBlock extends Component {
    
    render() {
        /* eslint-disable react/no-danger */
        const {props} = this;

        return <GridCell className="serversTabWarningInfo">
            <div dangerouslySetInnerHTML={{ __html: SvgIcons.ErrorStateIcon }} />
            <div className="dnn-label title"
                dangerouslySetInnerHTML={{ __html: props.label}} />
        </GridCell>;
    }
}

WarningBlock.propTypes = {
    label: PropTypes.string
};