import React, {Component, PropTypes} from "react";
import ReactDOM from "react-dom";

class Portal extends Component {
    componentDidMount() {
        let portal = this.props.portalId && document.getElementById(this.props.portalId);
        if (!portal) {
            portal = document.createElement('div');
            portal.id = this.props.portalId;
            document.body.appendChild(portal);
        }
        this.portalElement = portal;
        this.componentDidUpdate();
    }
    componentDidUpdate() {
        ReactDOM.render(<div className={`TransitionModalPortal ${this.props.className ? this.props.className : "" }`}>{this.props.children}</div>, this.portalElement);
    }
    componentWillUnmount() {
        document.body.removeChild(this.portalElement);
    }
    render() {
        return null;
    }
}

Portal.propTypes = {
    children: PropTypes.node.isRequired,
    portalId: PropTypes.string,
    className: PropTypes.string
};

export default Portal;