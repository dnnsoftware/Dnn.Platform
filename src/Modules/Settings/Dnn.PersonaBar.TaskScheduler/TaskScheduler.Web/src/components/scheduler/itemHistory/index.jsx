import React, {PropTypes, Component} from "react";
import { connect } from "react-redux";
import Modal from "react-modal";
import Scrollbars from "react-custom-scrollbars";
import SingleLineInput from "dnn-single-line-input";
import MultiLineInput from "dnn-multi-line-input";
import Button from "dnn-button";
import InputGroup from "dnn-input-group";
import Select from "dnn-select";
import styles from "./style.less";
import History from "../../history";
import resx from "../../../resources";

const svgIcon = require(`!raw!./../../svg/x_thin.svg`);

const modalStyles = {
    overlay: {
        zIndex: "99999",
        backgroundColor: "rgba(0, 0, 0, 0.75)"
    },
    content: {
        top: "250px",
        left: "110px",
        position: "absolute",
        padding: "30px 0 30px 30px",
        borderRadius: 0,
        border: "none",
        width: "810px",
        height: "550px",
        backgroundColor: "#F2F2F2",
        userSelect: "none",
        WebkitUserSelect: "none",
        MozUserSelect: "none",
        MsUserSelect: "none"
    }
};

const closeIconStyles = {
    width: "16px",
    height: "16px",
    float: "right",
    cursor: "pointer",
    margin: "-20px 0px 0px 785px",
    position: "fixed"
};

class ItemHistoryPanel extends Component {
    constructor() {
        super();
    }

    onClose(event) {
        const {props} = this;
        props.onClose();
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <Modal
                fixedHeight={props.fixedHeight}
                isOpen={props.isOpened}
                style={modalStyles}>
                {props.fixedHeight &&
                    <Scrollbars width={props.collapsibleWidth || "100%"} height={props.collapsibleHeight || "100%"} style={props.scrollAreaStyle}>
                        <div>
                            <div style={closeIconStyles} dangerouslySetInnerHTML={{ __html: svgIcon }} onClick={this.onClose.bind(this) }/>
                            <div className="modepanel-content-wrapper" style={{ height: "calc(100% - 100px)" }}>
                                <History scheduleId={props.scheduleId} title={resx.get("HistoryModalTitle") + props.scheduleName}>
                                </History>
                            </div>
                        </div>
                    </Scrollbars>
                }
                {!props.fixedHeight && props.children}
            </Modal>
        );
    }
}

ItemHistoryPanel.PropTypes = {
    scheduleId: PropTypes.number,
    scheduleName: PropTypes.string,
    label: PropTypes.string,
    fixedHeight: PropTypes.number,
    collapsibleWidth: PropTypes.number,
    collapsibleHeight: PropTypes.number,
    keepCollapsedContent: PropTypes.bool,
    scrollAreaStyle: PropTypes.object,
    children: PropTypes.node,
    isOpened: PropTypes.bool,
    onClose: PropTypes.func.isRequired,
    schedulerMode: PropTypes.string.isRequired,
    schedulerDelay: PropTypes.number.isRequired,
    schedulerModeOptions: PropTypes.array.isRequired
};

export default ItemHistoryPanel;