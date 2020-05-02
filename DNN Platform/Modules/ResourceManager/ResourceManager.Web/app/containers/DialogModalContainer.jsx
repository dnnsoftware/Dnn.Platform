import React from "react";
import PropTypes from "prop-types";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import dialogModalActions from "../actions/dialogModalActions";
import { Modal, Label } from "@dnnsoftware/dnn-react-common";

class DialogModalContainer extends React.Component {
    render() {
        const modalStyles = {
            overlay: {
                zIndex: "99999",
                backgroundColor: "rgba(0,0,0,0.6)"
            },
            content: {
                borderRadius: "7px",
                "background": "white",
                WebkitBoxShadow: "0 0 25px 0 rgba(0, 0, 0, 0.75)",
                boxShadow: "0 0 25px 0 rgba(0, 0, 0, 0.75)",
                textAlign: "center",
                "width": "300px",
                height: "178px",
                margin: "auto",
                padding: 0,
                border: "none",
                boxSizing: "border-box"
            }
        };

        const { dialogHeader, dialogMessage, close, yesFunction, noFunction } = this.props;

        function yesHandler() {
            close();
            yesFunction();
        }

        return (
            <Modal isOpen={!!dialogMessage} header={dialogHeader} onRequestClose={close} style={modalStyles} contentLabel="Modal" >
                <Label className="rm-dialog-modal-label" label={dialogMessage} />
                <div className="rm-form-buttons-container">
                    <button className="rm-common-button" type="button" role="secondary" onClick={noFunction} >No</button>
                    <button className="rm-common-button" type="button" role="primary" onClick={yesHandler} >Yes</button>
                </div>
            </Modal>
        );
    }
}

DialogModalContainer.propTypes = {
    dialogHeader: PropTypes.string,
    dialogMessage: PropTypes.string,
    close: PropTypes.func.isRequired,
    yesFunction: PropTypes.func,
    noFunction: PropTypes.func
};

function mapStateToProps(state) {
    const dialogModalState = state.dialogModal;
    return {
        dialogHeader: dialogModalState.dialogHeader,
        dialogMessage: dialogModalState.dialogMessage,
        yesFunction: dialogModalState.yesFunction,
        noFunction: dialogModalState.noFunction
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            close: dialogModalActions.close
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(DialogModalContainer);
