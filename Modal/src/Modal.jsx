import React, {Component, PropTypes} from "react";
import ReactModal from "react-modal";
import {Scrollbars} from "react-custom-scrollbars";
import { XThinIcon } from "dnn-svg-icons";
import "./style.less";


class Modal extends Component {
    getScrollbarStyle(props) {
        return {
            width: "100%",
            height: props.header ? "calc(100% - 55px)" : "100%",
            boxSizing: "border-box",
            padding: "25px 30px"
        };
    }
    getModalStyles(props) {
        let modalWidth = props.modalWidth;
        let modalTopMargin = props.modalTopMargin;
        if (document.getElementsByClassName("socialpanel") && document.getElementsByClassName("socialpanel").length > 0 && !props.modalWidth) {
            modalWidth = document.getElementsByClassName("socialpanel")[0].offsetWidth;
        }
        if (document.getElementsByClassName("dnn-persona-bar-page-header") && document.getElementsByClassName("dnn-persona-bar-page-header").length > 0 && !props.modalHeight) {
            modalTopMargin = document.getElementsByClassName("dnn-persona-bar-page-header")[0].offsetHeight;
        }
        return (props.style || {
            overlay: {
                zIndex: "99999",
                backgroundColor: "rgba(0,0,0,0.6)"
            },
            content: {
                top: modalTopMargin + props.dialogVerticalMargin,
                left: props.dialogHorizontalMargin + 85,
                padding: 0,
                borderRadius: 0,
                border: "none",
                width: modalWidth - props.dialogHorizontalMargin * 2,
                height: props.modalHeight || "60%",
                backgroundColor: "#FFFFFF",
                position: "absolute",
                userSelect: "none",
                WebkitUserSelect: "none",
                MozUserSelect: "none",
                MsUserSelect: "none",
                boxSizing: "border-box"
            }
        });
    }
    /*eslint-disable react/no-danger*/
    render() {
        const {props} = this;
        const modalStyles = this.getModalStyles(props);
        const scrollBarStyle = this.getScrollbarStyle(props);
        return (
            <ReactModal
                isOpen={props.isOpen}
                onRequestClose={props.onRequestClose}
                onAfterOpen={props.onAfterOpen}
                closeTimeoutMS={props.closeTimeoutMS}
                shouldCloseOnOverlayClick={props.shouldCloseOnOverlayClick}
                style={modalStyles}>
                {props.header &&
                    <div className="modal-header">
                        <h3>{props.header}</h3>
                        {props.headerChildren}
                        <div
                            className="close-modal-button"
                            dangerouslySetInnerHTML={{ __html: XThinIcon }}
                            onClick={props.onRequestClose}>
                        </div>
                    </div>
                }
                <Scrollbars style={scrollBarStyle}>
                    <div style={props.contentStyle}>
                        {props.children}
                    </div>
                </Scrollbars>
            </ReactModal>
        );
    }
}
Modal.propTypes = {
    isOpen: PropTypes.bool,
    style: PropTypes.object,
    onRequestClose: PropTypes.func,
    children: PropTypes.node,
    dialogVerticalMargin: PropTypes.number,
    dialogHorizontalMargin: PropTypes.number,
    modalWidth: PropTypes.number,
    modalHeight: PropTypes.number,
    modalTopMargin: PropTypes.number,
    header: PropTypes.string,
    headerChildren: PropTypes.node,
    contentStyle: PropTypes.object,
    onAfterOpen: PropTypes.func,
    closeTimeoutMS: PropTypes.number,
    shouldCloseOnOverlayClick: PropTypes.bool
};
Modal.defaultProps = {
    modalWidth: 861,
    modalTopMargin: 100,
    dialogVerticalMargin: 25,
    dialogHorizontalMargin: 30,
    contentStyle: { padding: "25px 30px" },
    shouldCloseOnOverlayClick: true
};
export default Modal;