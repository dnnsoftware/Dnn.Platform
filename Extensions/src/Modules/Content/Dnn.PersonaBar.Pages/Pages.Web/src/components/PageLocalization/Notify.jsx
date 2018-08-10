import React, {Component, PropTypes} from "react";
import Localization from "../../localization";
import Button from "dnn-button";

class Notify extends Component {
    render() {
        return <div className="notify-translators">
            <h3>{Localization.get("NotifyModalHeader")}</h3>
            <textarea 
                placeholder={Localization.get("NotifyModalPlaceholder")}
                value={this.props.notifyMessage}
                onChange={this.props.onUpdateMessage}
                aria-label="Message"
                />
            <div className="buttons-container">
                <Button
                    type="secondary"
                    onClick={this.props.onClose }>
                    {Localization.get("Cancel") }
                </Button>
                <Button
                    type="primary"
                    onClick={this.props.onSend }>
                    {Localization.get("Send") }
                </Button>
            </div>
        </div>;
    }
}

Notify.propTypes = {
    onClose: PropTypes.func.isRequired,
    onSend: PropTypes.func.isRequired,
    onUpdateMessage: PropTypes.func.isRequired,
    notifyMessage: PropTypes.string.isRequired
};

export default Notify;