import React, {PropTypes} from "react";
import MainLabel from "./MainLabel";
import Statistic from "./statistic";
import GridCell from "dnn-grid-cell";
import styles from "./style.less";
/* eslint-disable react/no-danger */
const PortalListItem = ({portal, portalStatisticInfo, portalButtons}) => (
    <GridCell className={styles.portalListItem}>
        <GridCell className="portal-info-container">
            <GridCell className="portal-main-info" columnSize={35}>
                <MainLabel
                    label={portal.PortalName}
                    portalAliases={portal.PortalAliases}
                    />
                <GridCell className="icon-container">
                    {portalButtons}
                </GridCell>
            </GridCell>
            <GridCell className="portal-statistics-info" columnSize={65}>
                {portalStatisticInfo.map(statistic => {
                    return <Statistic label={statistic.label} value={statistic.value}/>;
                }) }
            </GridCell>
        </GridCell>
    </GridCell >
);

PortalListItem.propTypes = {
    portal: PropTypes.object,
    portalStatisticInfo: PropTypes.object,
    portalButtons: PropTypes.node
};
export default PortalListItem;