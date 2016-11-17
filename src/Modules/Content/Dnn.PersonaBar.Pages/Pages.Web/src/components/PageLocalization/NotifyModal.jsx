import TransitionModal from "dnn-transition-modal";
import React, {Component, PropTypes} from "react";
import Notify from "./Notify";

import "./NotifyModal.less";


class NotifyModal extends Component {
    render() {
        return <TransitionModal onCloseModal={this.props.onClose}>
            <Notify 
                onSend={this.props.onSend}
                onUpdateMessage={this.props.onUpdateMessage}
                notifyMessage={this.props.notifyMessage}
            />
        </TransitionModal>;
    }
}

NotifyModal.propTypes = {
    onClose: PropTypes.func.isRequired,
    onSend: PropTypes.func.isRequired,
    onUpdateMessage: PropTypes.func.isRequired,
    notifyMessage: PropTypes.string.isRequired
};

export default NotifyModal;