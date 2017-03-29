import React, {Component, PropTypes} from "react";
import Button from "dnn-button";

class TransitionModalContent extends Component {
    render() {
        return <div className="transition-modal-content">
            <h3>{this.props.header}</h3>
            <p>{this.props.message}</p>
            <div className="buttons-container">
                {this.props.showCancelButton && <Button
                    type="secondary"
                    onClick={this.props.onClose }>
                    {this.props.cancelButtonText || "Cancel"}
                </Button>}
                {this.props.showOkButton && <Button
                    type="primary"
                    onClick={this.props.onOk }>
                    {this.props.okButtonText || "Ok" }
                </Button>}
            </div>
        </div>;
    }
}

TransitionModalContent.propTypes = {
    onClose: PropTypes.func.isRequired,
    onOk: PropTypes.func,
    
    message: PropTypes.string,
    header: PropTypes.string,
    showCancelButton: PropTypes.bool,
    cancelButtonText:PropTypes.string,
    
    showOkButton: PropTypes.bool,
    okButtonText: PropTypes.string
};

export default TransitionModalContent;