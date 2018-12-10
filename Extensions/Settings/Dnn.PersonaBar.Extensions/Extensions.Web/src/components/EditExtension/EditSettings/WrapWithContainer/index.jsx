import React, { } from "react";
import PropTypes from "prop-types";
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