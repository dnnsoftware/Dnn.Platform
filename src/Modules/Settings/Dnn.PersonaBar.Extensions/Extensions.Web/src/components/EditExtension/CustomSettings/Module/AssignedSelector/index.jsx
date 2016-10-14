import React, { PropTypes, Component } from "react";
import GridCell from "dnn-grid-cell";
import { Scrollbars } from "react-custom-scrollbars";
import { ArrowLeftIcon, ArrowRightIcon } from "dnn-svg-icons";
import styles from "./style.less";

class AssignedSelector extends Component {
    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        return (
            <GridCell className={styles.assignedSelector}>
                <GridCell columnSize={45} className="selector-box">
                    <Scrollbars style={{ width: "100%", height: "100%" }}>
                        <ul>
                            <li className="selected">Website 01</li>
                            <li className="selected">Website 02</li>
                            <li>Website 03</li>
                        </ul>
                    </Scrollbars>
                </GridCell>
                <GridCell columnSize={10} className="selector-controls">
                    <a href="" dangerouslySetInnerHTML={{ __html: ArrowRightIcon }}></a>
                    <a href="" dangerouslySetInnerHTML={{ __html: ArrowLeftIcon }}></a>
                </GridCell>
                <GridCell columnSize={45} className="selector-box">
                    <Scrollbars style={{ width: "100%", height: "100%" }}>
                        <ul>
                            <li>Website 04</li>
                            <li>Website 05</li>
                            <li>Website 06</li>
                        </ul>
                    </Scrollbars>
                </GridCell>
            </GridCell>
        );
        // <p className="modal-pagination"> --1 of 2 -- </p>
    }
}

AssignedSelector.PropTypes = {
};

export default AssignedSelector;