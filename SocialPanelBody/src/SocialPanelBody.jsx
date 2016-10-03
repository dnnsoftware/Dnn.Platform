import React, {PropTypes} from "react";
import GridCell from "dnn-grid-cell";
import "./style.less";

const SocialPanelBody = ({children, workSpaceTrayOutside, workSpaceTrayVisible, workSpaceTray}) => (
    <GridCell className="dnn-social-panel-body">
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
    workSpaceTray: PropTypes.node
};
export default SocialPanelBody;