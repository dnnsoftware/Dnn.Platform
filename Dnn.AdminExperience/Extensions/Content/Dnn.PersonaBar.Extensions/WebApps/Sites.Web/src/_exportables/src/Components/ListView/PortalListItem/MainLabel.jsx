import PropTypes from 'prop-types';
import React from "react";
import { GridCell, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";

/* eslint-disable react/no-danger */
const MainLabel = ({label, portalAliases}) => (
    <GridCell className="portal-name-info">
        <TextOverflowWrapper text={label} maxWidth={220}/>
        <GridCell>
        {
            portalAliases.map((alias)=>{
                return <TextOverflowWrapper key={"alias-" + label} href={alias.link} target="_blank" text={alias.url} isAnchor={true} maxWidth={220}/>;
            })
        }
        </GridCell>
    </GridCell>
);

MainLabel.propTypes = {
    label: PropTypes.string,
    portalAliases: PropTypes.node
};
export default MainLabel;