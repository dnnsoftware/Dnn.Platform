import React, {PropTypes} from "react";
import GridCell from "dnn-grid-cell";
import TextOverflowWrapper from "dnn-text-overflow-wrapper";

/* eslint-disable react/no-danger */
const MainLabel = ({label, portalAliases}) => (
    <GridCell className="portal-name-info">
        <TextOverflowWrapper text={label} maxWidth={220}/>
        <GridCell>
        {portalAliases.map((alias)=>{
            return <TextOverflowWrapper href={alias.link} target="_blank" text={alias.url} isAnchor={true} maxWidth={220}/>;
        })}
        </GridCell>
    </GridCell>
);

MainLabel.propTypes = {
    label: PropTypes.string,
    portalAliases: PropTypes.node
};
export default MainLabel;