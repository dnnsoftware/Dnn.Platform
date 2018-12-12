import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import ExtensionHeader from "../common/ExtensionHeader";
import ExtensionDetailRow from "../common/ExtensionDetailRow";
import "./style.less";

class ExtensionList extends Component {
    constructor() {
        super();
    }
    render() {
        const {props} = this;

        return (
            <GridCell style={{ padding: "5px 20px" }}>
                <ExtensionHeader />
                {props.packages.map((_package, index) => {
                    return <ExtensionDetailRow
                        _package={_package}
                        type={props.type}
                        onInstall={props.onInstall.bind(this)}
                        doingOperation={props.doingOperation}
                        onDeploy={props.onDeploy.bind(this, index)}
                        key={index} />;
                })}
            </GridCell>
        );
    }
}

ExtensionList.propTypes = {
    label: PropTypes.string,
    packages: PropTypes.array,
    collapsed: PropTypes.bool,
    onChange: PropTypes.func,
    type: PropTypes.string,
    onDownload: PropTypes.func,
    onInstall: PropTypes.func,
    onDeploy: PropTypes.func,
    doingOperation: PropTypes.bool
};


export default ExtensionList;
