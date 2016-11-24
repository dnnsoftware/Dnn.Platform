import React, {PropTypes} from "react";
import "./style.less";

const SocialPanelHeader = ({title, children, tooltip}) => (
    <div className="dnn-social-panel-header socialpanelheader">
        <h3 title={tooltip}>{title}</h3>
        {children}
    </div>
);

SocialPanelHeader.propTypes = {
    title: PropTypes.string,
    children: PropTypes.node,
    tooltip: PropTypes.string
};
export default SocialPanelHeader;