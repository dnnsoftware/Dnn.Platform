import React, {Component, PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import Localization from "localization";
import "./style.less";

class ExtensionHeader extends Component {
    render() {
        let columnSize = 100 / this.props.headers.length;
        return (
            <GridCell columnSize={100} className="header-row">
                {
                    this.props.headers.map((header) => {
                        return <GridCell columnSize={columnSize}>
                            <h6>{Localization.get(header) }</h6>
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