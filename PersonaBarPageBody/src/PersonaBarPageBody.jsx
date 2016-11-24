import React, {PropTypes} from "react";
import GridCell from "dnn-grid-cell";
import "./style.less";

function getClassNameIfExists(value) {
    return value
        ? " " + value
        : "";
}

const PersonaBarPageBody = ({
    children,
    workSpaceTrayOutside,
    workSpaceTray,
    style,
    className,
    workSpaceTrayClassName
}) => {
    const outsideTrayVisible = workSpaceTrayOutside && workSpaceTray;
    const insideTrayVisible = !workSpaceTrayOutside && workSpaceTray;
    const _workSpaceTrayClassName = "dnn-workspace-tray" + getClassNameIfExists(workSpaceTrayClassName);
    const personaBarPageBodyClassName = "dnn-persona-bar-page-body" + getClassNameIfExists(className);
    return (
        <GridCell className={personaBarPageBodyClassName} style={style}>
            {outsideTrayVisible && <GridCell className={_workSpaceTrayClassName}>{workSpaceTray}</GridCell>}
            <GridCell className="persona-bar-page-body">
                {insideTrayVisible && <GridCell className={_workSpaceTrayClassName}>{workSpaceTray}</GridCell>}
                {children}
            </GridCell>
        </GridCell>
    );
};

PersonaBarPageBody.propTypes = {
    children: PropTypes.node,
    workSpaceTrayOutside: PropTypes.bool,
    workSpaceTray: PropTypes.node,
    style: PropTypes.object,
    className: PropTypes.string,
    workSpaceTrayClassName: PropTypes.string
};
export default PersonaBarPageBody;
