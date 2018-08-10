import React, {Component, PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import "./style.less";

class ExtensionHeader extends Component {
    render() {
        return (
            <GridCell columnSize={100} className="header-row">
                {
                    this.props.headers.map((header) => {
                        return <GridCell columnSize={header.size} className={header.header ? "" : "empty"}>
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