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
    width: "400px",
    marginLeft: "30px"
};


const PersonaBarPageHeader = ({ title, children, tooltip, titleMaxWidth, titleCharLimit }) => {

    titleCharLimit = titleCharLimit ? titleCharLimit : 20;

    const renderTitle = () => {
        switch (true) {
            case title.length > titleCharLimit:
                return (
                    <span className="title">
                        <h3>{`${title.substr(0, titleCharLimit)}...`}</h3>
                        <TextOverflowWrapperNew text={title} hotspotStyles={hotspotStyles} />
                    </span>
                );

            default:
                return (
                    <span className="title">
                        <h3>{title}</h3>
                    </span>
                );
        }
    };

    return (
        <div className="dnn-persona-bar-page-header">
            {renderTitle()}
            <div className="children">
                {children}
            </div>
        </div>
    );
};

PersonaBarPageHeader.propTypes = {
    title: PropTypes.string,
    titleCharLimit: PropTypes.number,
    children: PropTypes.node,
    tooltip: PropTypes.string,
    titleMaxWidth: PropTypes.number
};

PersonaBarPageHeader.defaultProps = {
    titleMaxWidth: 500
};

export default PersonaBarPageHeader;