import React, {Component} from "react";
import PropTypes from "prop-types";
import "./style.less";

class MenuItem extends Component {
    constructor() {
        super();
        this.state = { hover: false };
    }

    render() {
        return (
            <li className="dnn-in-context-menu menu-item" onMouseEnter={() => this.setState({ hover: true }) }
                onMouseLeave={() => this.setState({ hover: false }) }
                onClick={this.props.onMenuAction}>
                {this.props.children}
            </li>
        );
    }
}

MenuItem.propTypes = {
    onMenuAction: PropTypes.func.isRequired,
    children: PropTypes.node.isRequired
};

export default MenuItem;