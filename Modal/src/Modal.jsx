import React, {PropTypes} from "react";
import ReactModal from "react-modal";
import "./style.less";

const Modal = ({modalOpen, modalStyles, toggleModal, children}) => (
    <div className="dnn-modal">
        <ReactModal
            isOpen={modalOpen}
            onRequestClose={toggleModal}
            style={modalStyles}
            >
            {children}
        </ReactModal>
    </div>
);

Modal.propTypes = {
    modalOpen: PropTypes.bool,
    modalStyles: PropTypes.object,
    toggleModal: PropTypes.func,
    children: PropTypes.node
};
export default Modal;