import React, { Component } from "react";
import PropTypes from "prop-types";
import ReactDOM from "react-dom";
import {Scrollbars} from "react-custom-scrollbars";
import { XThinIcon } from "../SvgIcons";
import "./style.less";


class Modal extends Component {
    componentDidMount() {
        if (this.props.isOpen) {
            this.handleModalOpened();
        }
    }

    componentDidUpdate(prevProps) {
        if (!prevProps.isOpen && this.props.isOpen) {
            this.handleModalOpened();
        } else if (prevProps.isOpen && !this.props.isOpen) {
            this.handleModalClosed();
        }
    }

    componentWillUnmount() {
        this.handleModalClosed();
    }

    handleModalOpened() {
        if (document && document.body) {
            document.body.classList.add("ReactModal__Body--open");
        }
        if (this.props.onAfterOpen) {
            this.props.onAfterOpen();
        }
    }

    handleModalClosed() {
        if (document && document.body) {
            document.body.classList.remove("ReactModal__Body--open");
        }
    }

    onOverlayMouseDown() {
        if (this.props.shouldCloseOnOverlayClick && this.props.onRequestClose) {
            this.props.onRequestClose();
        }
    }

    onContentMouseDown(event) {
        event.stopPropagation();
    }

    onPortalKeyDown(event) {
        if (event.key === "Escape" && this.props.onRequestClose) {
            this.props.onRequestClose();
        }
    }

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
        const defaultStyles = {
            overlay: {
                position: "fixed",
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                zIndex: "99999",
                backgroundColor: "rgba(0,0,0,0.6)"
            },
            content: {
                top: modalTopMargin + props.dialogVerticalMargin,
                left: props.dialogHorizontalMargin + 85,
                right: "auto",
                bottom: "auto",
                padding: 0,
                borderRadius: 0,
                border: "none",
                width: modalWidth - props.dialogHorizontalMargin * 2,
                height: props.modalHeight || "60%",
                backgroundColor: "#FFFFFF",
                position: "absolute",
                overflow: "auto",
                WebkitOverflowScrolling: "touch",
                outline: "none",
                userSelect: "none",
                WebkitUserSelect: "none",
                MozUserSelect: "none",
                MsUserSelect: "none",
                boxSizing: "border-box"
            }
        };

        const customOverlay = props.style && props.style.overlay ? props.style.overlay : {};
        const customContent = props.style && props.style.content ? props.style.content : {};

        return {
            overlay: {
                ...defaultStyles.overlay,
                ...customOverlay
            },
            content: {
                ...defaultStyles.content,
                ...customContent
            }
        };
    }
     
    render() {
        const {props} = this;
        if (!props.isOpen || !document || !document.body) {
            return null;
        }

        const modalStyles = this.getModalStyles(props);
        const scrollBarStyle = this.getScrollbarStyle(props);
        return ReactDOM.createPortal(
            <div className="ReactModalPortal" onKeyDown={this.onPortalKeyDown.bind(this)}>
                <div
                    className="ReactModal__Overlay"
                    style={modalStyles.overlay}
                    onMouseDown={this.onOverlayMouseDown.bind(this)}>
                    <div
                        className="ReactModal__Content"
                        style={modalStyles.content}
                        role="dialog"
                        aria-modal="true"
                        onMouseDown={this.onContentMouseDown.bind(this)}>
                        {props.header &&
                            <div className="modal-header">
                                <h3>{props.header}</h3>
                                {props.headerChildren}
                                <div className="close-modal-button" onClick={props.onRequestClose}>
                                    <XThinIcon />
                                </div>
                            </div>
                        }
                        <Scrollbars style={scrollBarStyle}>
                            <div style={props.contentStyle}>
                                {props.children}
                            </div>
                        </Scrollbars>
                    </div>
                </div>
            </div>,
            document.body
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