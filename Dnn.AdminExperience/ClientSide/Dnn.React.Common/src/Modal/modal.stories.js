import React, { Component } from "react";
import Modal from "./index";

const storyStyle = {
    overlay: {
        zIndex: "99999",
        backgroundColor: "rgba(0,0,0,0.6)",
        position: "fixed",
        inset: 0,
        display: "flex",
        alignItems: "flex-start",
        justifyContent: "center",
        padding: "48px",
        boxSizing: "border-box",
    },
    content: {
        inset: "auto",
        padding: 0,
        borderRadius: 0,
        border: "none",
        backgroundColor: "#FFFFFF",
        position: "relative",
        userSelect: "none",
        boxSizing: "border-box",
        width: "760px",
        maxWidth: "calc(100vw - 96px)",
        height: "420px",
        maxHeight: "calc(100vh - 96px)",
    },
};

export default {
    component: Modal,
    args: {
        isOpen: true,
        header: "Example Modal",
        shouldCloseOnOverlayClick: true,
        dialogVerticalMargin: 25,
        dialogHorizontalMargin: 30,
        modalWidth: 861,
        modalTopMargin: 100,
        style: storyStyle,
        contentStyle: { padding: "25px 30px" },
        closeTimeoutMS: 0,
        children: "This is the modal body content.",
    },
    argTypes: {
        isOpen: { control: "boolean" },
        header: { control: "text" },
        shouldCloseOnOverlayClick: { control: "boolean" },
        dialogVerticalMargin: { control: "number" },
        dialogHorizontalMargin: { control: "number" },
        modalWidth: { control: "number" },
        modalHeight: { control: "number" },
        modalTopMargin: { control: "number" },
        closeTimeoutMS: { control: "number" },
        children: { control: "text" },
        style: { control: false },
        contentStyle: { control: false },
        headerChildren: { control: false },
        onRequestClose: { action: "onRequestClose" },
        onAfterOpen: { action: "onAfterOpen" },
    },
};

const renderModal = (args) => (
    <Modal {...args}>
        <p>{args.children}</p>
    </Modal>
);

export const WithHeader = {
    render: renderModal,
    args: {
        header: "Example Modal",
        children: "This is the modal body content.",
    },
};

export const WithoutHeader = {
    render: renderModal,
    args: {
        header: "",
        children: "This modal has no header. The close button is omitted and the scrollable area fills the full height.",
    },
};

export const WithHeaderChildren = {
    render: renderModal,
    args: {
        header: "Modal with Extra Header Controls",
        headerChildren: <button style={{ marginLeft: "auto" }}>Action</button>,
        children: "This modal has additional children rendered inside the header bar.",
    },
};

class ControlledModal extends Component {
    constructor(props) {
        super(props);
        this.state = { isOpen: props.isOpen };
        this.onOpen = this.onOpen.bind(this);
        this.onClose = this.onClose.bind(this);
    }

    componentDidUpdate(prevProps) {
        if (prevProps.isOpen !== this.props.isOpen) {
            this.setState({ isOpen: this.props.isOpen });
        }
    }

    onOpen() {
        this.setState({ isOpen: true });
    }

    onClose() {
        this.setState({ isOpen: false });
        if (this.props.onRequestClose) {
            this.props.onRequestClose();
        }
    }

    render() {
        const {
            buttonLabel,
            children,
            isOpen,
            onRequestClose,
            ...modalProps
        } = this.props;

        return (
            <div style={{ minHeight: "120px", padding: "24px" }}>
                <button onClick={this.onOpen}>{buttonLabel}</button>
                <Modal
                    {...modalProps}
                    isOpen={this.state.isOpen}
                    onRequestClose={this.onClose}
                >
                    <p>{children}</p>
                </Modal>
            </div>
        );
    }
}

export const Controlled = {
    render: (args) => <ControlledModal {...args} />,
    args: {
        isOpen: false,
        header: "Controlled Modal",
        buttonLabel: "Open Modal",
        children: "Click the X button or the overlay to close this modal.",
    },
    argTypes: {
        buttonLabel: { control: "text" },
    },
};
