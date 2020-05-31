import React from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import messageModalActions from "../actions/messageModalActions";
import Modal from "../../../../../../Dnn.AdminExperience/ClientSide/Dnn.React.Common/src/Modal";


class MessageModalContainer extends React.Component {
    render() {
        const modalStyles = {
            overlay: {
                zIndex: "99999",
                "background": "none"
            },
            content: {
                borderRadius: "3px",
                color: "white",
                textAlign: "center",
                background: "#002e47",
                opacity: ".9",
                WebkitBoxShadow: "0 0 25px 0 rgba(0, 0, 0, 0.75)",
                width: "10%",
                minWidth: "250px",
                minHeight: "80px",
                maxHeight: "120px",
                margin: "auto",
                padding: 0,
                border: "none",
                boxSizing: "border-box"
            }
        };

        const { infoMessage, errorMessage, close } = this.props;
        const messageToShow = infoMessage || errorMessage;

        function setTimeOutToClose() {
            setTimeout(function () {
                close();
            }, 3000);
        }

        if (messageToShow)
        {
            setTimeOutToClose();
        }

        return (
            <Modal isOpen={!!messageToShow} onRequestClose={close} style={modalStyles} contentLabel="Modal" >
                <p>{messageToShow}</p>
            </Modal>
        );
    }
}

MessageModalContainer.propTypes = {
    infoMessage: PropTypes.string,
    errorMessage: PropTypes.string,
    close: PropTypes.func.isRequired
};

function mapStateToProps(state) {
    const messageModalState = state.messageModal;
    return {
        infoMessage: messageModalState.infoMessage,
        errorMessage: messageModalState.errorMessage
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            close: messageModalActions.close
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(MessageModalContainer);
