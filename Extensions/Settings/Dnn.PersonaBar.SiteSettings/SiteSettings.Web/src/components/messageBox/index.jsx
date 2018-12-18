import React, { Component } from "react";
import PropTypes from "prop-types";
import Modal from "react-modal";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";

const modalStyles = {
    overlay: {
        zIndex: "9999",
        backgroundColor: "rgba(0, 0, 0, 0)"
    },
    content: {
        position: "fixed",
        top: "30%",
        left: "370px",
        right: "initial",
        bottom: "initial",
        borderRadius: 0,
        border: "none",
        width: "485px",
        padding: 0,
        userSelect: "none",
        WebkitUserSelect: "none",
        MozUserSelect: "none",
        MsUserSelect: "none",
        opacity: ".9"
    }
};

class MessageBox extends Component {
    constructor() {
        super();
    }

    onClose() {
        const {props} = this;
        props.onClose();
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props} = this;
        return (
            <Modal
                fixedHeight={props.fixedHeight}
                isOpen={props.isOpened}
                style={modalStyles}>
                <div className="dnn-message-box">
                    <div className="top-bar">
                        <div className="close-button" dangerouslySetInnerHTML={{ __html: SvgIcons.XThinIcon }} onClick={this.onClose.bind(this)} />
                    </div>
                    <div className="content">
                        <div className="message-icon" dangerouslySetInnerHTML={{ __html: SvgIcons.CheckMarkIcon }} />
                        <div className="message-text">{props.message}</div>
                        {props.link &&
                            <div className="message-link"><a target="_blank" rel="noopener noreferrer" href={props.link}>{props.link}</a></div>
                        }
                    </div>
                </div>
            </Modal>
        );
    }
}

MessageBox.propTypes = {
    text: PropTypes.string.isRequired,
    link: PropTypes.string,
    isOpened: PropTypes.bool,
    onClose: PropTypes.func.isRequired
};

MessageBox.defaultProps = {
    isOpened: false
};

export default MessageBox;