import React, { PropTypes } from "react";
import TextOverflowWrapper from "dnn-text-overflow-wrapper";
import "./style.less";

const hotspotStyles = {
    position: "absolute",
    zIndex: 10000,
    top: "50px",
    left: 0,
    wordWrap: "break-word",
    textOverflow: "wrap",
    height: "20px",
    width: "200px"
};

const tooltipStyles = {
    style: {
        position: "absolute",
        zIndex: 10000,
        top: 0,
        left: 0,
        wordWrap: "break-word",
        textOverflow: "ellipsis",
        maxWidth: "255px",
        pointerEvents: "auto",
        marginTop: "0px"
    },
    arrowStyle: {
    }
};


const PersonaBarPageHeader = ({ title, children, tooltip, titleMaxWidth }) => (
    <div className="dnn-persona-bar-page-header">
        <h3>{title}</h3>
        {title.length > 20 ? <TextOverflowWrapper text={title} hotspotStyles={hotspotStyles} tooltipStyles={tooltipStyles} /> : null}
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