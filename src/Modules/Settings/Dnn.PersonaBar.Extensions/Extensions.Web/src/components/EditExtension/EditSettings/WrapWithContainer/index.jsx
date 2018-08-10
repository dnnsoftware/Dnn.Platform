import React, { PropTypes } from "react";
import GridCell from "dnn-grid-cell";
const WrapWithContainer = ({children}) => (
    <GridCell className="extension-form">
        {children}
    </GridCell>
);

WrapWithContainer.propTypes = {
    children: PropTypes.node
};

export default WrapWithContainer;