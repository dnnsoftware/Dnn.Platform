import React, {PropTypes} from "react";
import "./style.less";

const SocialPanelBody = ({children}) => (
    <div className="dnn-social-panel-body socialpanelbody">
        <div>
            <div className="normalPanel">
                <div className="searchpanel">
                    {children}
                </div>
            </div>
        </div>
    </div>
);

SocialPanelBody.propTypes = {
    children: PropTypes.node
};
export default SocialPanelBody;