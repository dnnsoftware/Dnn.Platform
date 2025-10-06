import React, {Component } from "react";
import PropTypes from "prop-types";
import { GridCell, SvgIcons } from "@dnnsoftware/dnn-react-common";
import Html from "../Html";
import "./style.less";

export default class WarningBlock extends Component {
    
    render() {
         
        const {props} = this;

        return <GridCell className="serversTabWarningInfo">
            <div><SvgIcons.ErrorStateIcon /></div>
            <div className="dnn-label title"><Html html={props.label} /></div>
        </GridCell>;
    }
}

WarningBlock.propTypes = {
    label: PropTypes.string
};