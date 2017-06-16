import React, { PropTypes } from "react";
import TextOverflowWrapperNew from "dnn-text-overflow-wrapper-new";

import "./style.less";

const hotspotStyles = {
    position: "absolute",
    zIndex: 10000,
    top: "50px",
    left: 0,
    wordWrap: "break-word",
    textOverflow: "wrap",
    height: "20px",
    width: "200px",
    marginLeft: "30px"
};

const PersonaBarPageHeader = ({ title, children, tooltip, titleMaxWidth }) => (
    <div className="dnn-persona-bar-page-header">
        <h3>{title}</h3>
        {title.length > 4 ? <TextOverflowWrapperNew text={title} hotspotStyles={hotspotStyles}  /> : null}
        <div style={{ marginTop: "-50px" }}>
            {children}
        </div>
    </div>
);

PersonaBarPageHeader.propTypes = {
    title: PropTypes.string,
    children: PropTypes.node,
    tooltip: PropTypes.string,
    titleMaxWidth: PropTypes.number
};

PersonaBarPageHeader.defaultProps = {
    titleMaxWidth: 500
};

export default PersonaBarPageHeader;