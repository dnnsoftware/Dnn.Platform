import React, {Component } from "react";
import PropTypes from "prop-types";
import { GridCell, SvgIcons } from "@dnnsoftware/dnn-react-common";
import "./style.less";

export default class WarningBlock extends Component {
    
    render() {
         
        const {props} = this;

        return <GridCell className="serversTabWarningInfo">
            <div><SvgIcons.ErrorStateIcon /></div>
            <div className="dnn-label title"
                dangerouslySetInnerHTML={{ __html: props.label}} />
        </GridCell>;
    }
}

WarningBlock.propTypes = {
    label: PropTypes.string
};