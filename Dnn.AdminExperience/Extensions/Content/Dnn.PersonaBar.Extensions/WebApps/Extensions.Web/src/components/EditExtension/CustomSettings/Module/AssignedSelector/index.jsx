import React, { Component } from "react";
import PropTypes from "prop-types";
import { GridCell, SvgIcons } from "@dnnsoftware/dnn-react-common";
import { Scrollbars } from "react-custom-scrollbars";
import Localization from "localization";
import styles from "./style.less";
class AssignedSelector extends Component {
    constructor() {
        super();
    }
    getPortalList(list, type) {
        const { props } = this;
        return list.map((portal, index) => {
            return <li className={portal.selected ? "selected" : ""}
                onClick={props.onClickOnPortal.bind(this, index, type)}
                key={index}>
                {portal.name}
            </li>;
        });
    }
    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        const assignedPortals = this.getPortalList(props.assignedPortals, "assignedPortals");
        const unassignedPortals = this.getPortalList(props.unassignedPortals, "unassignedPortals");
        return (
            <GridCell className={styles.assignedSelector}>
                <GridCell columnSize={45} className="selector-box">
                    <h6>{Localization.get("EditModule_Unassigned.Label")}</h6>
                    <Scrollbars style={{ width: "100%", height: "100%", border: "1px solid #c8c8c8" }}>
                        <ul>
                            {unassignedPortals}
                        </ul>
                    </Scrollbars>
                </GridCell>
                <GridCell columnSize={10} className="selector-controls">
                    <div href="" className="move-item single-right" onClick={props.moveItemsRight.bind(this)} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowRightIcon }}></div>
                    <div href="" className="move-item single-left" onClick={props.moveItemsLeft.bind(this)} dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowLeftIcon }}></div>
                    <div href="" className="move-item double-right" onClick={props.moveAll.bind(this, "right")} dangerouslySetInnerHTML={{ __html: SvgIcons.DoubleArrowRightIcon }}></div>
                    <div href="" className="move-item double-left" onClick={props.moveAll.bind(this)} dangerouslySetInnerHTML={{ __html: SvgIcons.DoubleArrowLeftIcon }}></div>
                </GridCell>
                <GridCell columnSize={45} className="selector-box">
                    <h6>{Localization.get("EditModule_Assigned.Label")}</h6>
                    <Scrollbars style={{ width: "100%", height: "100%", border: "1px solid #c8c8c8" }}>
                        <ul>
                            {assignedPortals}
                        </ul>
                    </Scrollbars>
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

AssignedSelector.PropTypes = {
    assignedPortals: PropTypes.array,
    unassignedPortals: PropTypes.array
};

export default AssignedSelector;