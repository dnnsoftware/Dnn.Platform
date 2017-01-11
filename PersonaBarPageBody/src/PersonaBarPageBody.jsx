import React, { PropTypes } from "react";
import GridCell from "dnn-grid-cell";
import BackToLink from "dnn-back-to-link";
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
    workSpaceTrayClassName,
    backToLinkProps
}) => {
    const outsideTrayVisible = workSpaceTrayOutside && workSpaceTray;
    const insideTrayVisible = !workSpaceTrayOutside && workSpaceTray;
    const _workSpaceTrayClassName = "dnn-workspace-tray" + getClassNameIfExists(workSpaceTrayClassName);
    const personaBarPageBodyClassName = "dnn-persona-bar-page-body" + getClassNameIfExists(className) + getClassNameIfExists((!backToLinkProps ? "" : backToLinkProps.text && "with-back-to-link"));
    return (
        <GridCell className={personaBarPageBodyClassName} style={style} >
            {backToLinkProps && backToLinkProps.text && <BackToLink {...backToLinkProps} />}
            {outsideTrayVisible && <GridCell className={_workSpaceTrayClassName}>{workSpaceTray}</GridCell>}
            <GridCell className="persona-bar-page-body">
                {insideTrayVisible && <GridCell className={_workSpaceTrayClassName}>{workSpaceTray}</GridCell>}
                {children}
            </GridCell>
        </GridCell >
    );
};

PersonaBarPageBody.propTypes = {
    children: PropTypes.node,
    workSpaceTrayOutside: PropTypes.bool,
    workSpaceTray: PropTypes.node,
    style: PropTypes.object,
    className: PropTypes.string,
    workSpaceTrayClassName: PropTypes.string,
    backToLinkProps: PropTypes.object
};
export default PersonaBarPageBody;