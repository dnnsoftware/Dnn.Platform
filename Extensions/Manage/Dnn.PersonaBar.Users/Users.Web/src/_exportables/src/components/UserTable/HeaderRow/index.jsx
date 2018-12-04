import PropTypes from 'prop-types';
import React, { Component } from "react";
import { GridCell } from "@dnnsoftware/dnn-react-common";
import "./style.less";

class ExtensionHeader extends Component {
    render() {
        return (
            <GridCell columnSize={100} className="header-row">
                {
                    this.props.headers.map((header, index) => {
                        return <GridCell key={`header-row-grid-cell-${index}`} columnSize={header.size} className={header.header ? "" : "empty"}>
                            <h6>{header.header || "Default"}</h6>
                        </GridCell>;
                    })
                }
            </GridCell>
        );
    }
}

ExtensionHeader.propTypes = {
    headers: PropTypes.array.isRequired
};


export default ExtensionHeader;