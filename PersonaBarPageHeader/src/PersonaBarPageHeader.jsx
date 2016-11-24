import React, {PropTypes} from "react";
import TextOverflowWrapper from "dnn-text-overflow-wrapper";
import "./style.less";

const PersonaBarPageHeader = ({title, children, tooltip, titleMaxWidth}) => (
    <div className="dnn-persona-bar-page-header">
        <h3 title={tooltip}><TextOverflowWrapper text={title} maxWidth={titleMaxWidth}/></h3>
        {children}
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