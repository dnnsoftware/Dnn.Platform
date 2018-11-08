import React, {Component, PropTypes} from "react";
import "./style.less";

export default class TransitionModal extends Component {
    constructor() {
        super();
        this.state = {
            isOpen: false
        };
    }

    componentDidMount() {
        setTimeout(() => { this.setState({ isOpen: true }); }, 0);
    }

    onCloseModal() {
        if (this.props.onCloseModal) {
            setTimeout(this.props.onCloseModal, 400);
        }
    }

    onClose() {
        this.setState({ isOpen: false }, this.onCloseModal);
    }

    render() {
        const className = "transition-modal" + (this.state.isOpen ? " open" : "");
        const childrenWithProps = React.Children.map(this.props.children,
            (child) => {
                if (!child) {
                    return;
                }
                return React.cloneElement(child, {
                    onClose: this.onClose.bind(this)
                });
            }
        );
        return (
            <div className={className}>
                <div className="overlay"></div>
                <div className="modal-content">
                    <div className="close" onClick={this.onClose.bind(this) }>×</div>
                    {childrenWithProps}
                </div>
            </div>
        );
    }
}

TransitionModal.propTypes = {
    onCloseModal: PropTypes.func,
    children: PropTypes.node
};