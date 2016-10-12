import React, {PropTypes} from "react";
import GridCell from "dnn-grid-cell";
import "./style.less";

const SocialPanelBody = ({children, workSpaceTrayOutside, workSpaceTrayVisible, workSpaceTray, style, className}) => (
    <GridCell className={"dnn-social-panel-body" + (className ? " " + className : "")} style={style}>
        {(workSpaceTrayOutside && workSpaceTrayVisible) && 
            <GridCell className="dnn-workspace-tray">
                {workSpaceTray}
            </GridCell>
        }
        <GridCell className="socialpanelbody">
            <GridCell>
                <GridCell className="normalPanel">
                    <GridCell className="searchpanel">
                        {(!workSpaceTrayOutside && workSpaceTrayVisible) &&
                            <GridCell className="dnn-workspace-tray">
                                {workSpaceTray}
                            </GridCell>}
                        {children}
                    </GridCell>
                </GridCell>
            </GridCell>
        </GridCell>
    </GridCell>
);

SocialPanelBody.propTypes = {
    children: PropTypes.node,
    workSpaceTrayOutside: PropTypes.bool,
    workSpaceTrayVisible: PropTypes.bool,
    workSpaceTray: PropTypes.node,
    style: PropTypes.object,
    className: PropTypes.string
};
export default SocialPanelBody;