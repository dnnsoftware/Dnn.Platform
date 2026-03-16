import PropTypes from "prop-types";
import React from "react";
import { GridCell, TextOverflowWrapper } from "@dnnsoftware/dnn-react-common";

 
const MainLabel = ({label, portalAliases}) => (
    <GridCell className="portal-name-info">
        <TextOverflowWrapper text={label} maxWidth={220}/>
        <GridCell>
            {
                portalAliases.map((alias, index)=>{
                    const aliasKey = alias.url || alias.link || index;
                    return <TextOverflowWrapper key={"alias-" + aliasKey} href={alias.link} target="_blank" text={alias.url} isAnchor={true} maxWidth={220}/>;
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