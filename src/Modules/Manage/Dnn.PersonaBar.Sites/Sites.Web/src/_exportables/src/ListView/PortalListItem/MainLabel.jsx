import React, {PropTypes} from "react";
import GridCell from "dnn-grid-cell";

/* eslint-disable react/no-danger */
const MainLabel = ({label, portalAliases}) => (
    <GridCell className="portal-name-info">
        <label>{label}</label>
        <GridCell>
        {portalAliases.map((alias)=>{
            return <a href={alias.link} target="_blank">{alias.url}</a>;
        })}
        </GridCell>
    </GridCell>
);

MainLabel.propTypes = {
    label: PropTypes.string,
    portalAliases: PropTypes.node
};
export default MainLabel;