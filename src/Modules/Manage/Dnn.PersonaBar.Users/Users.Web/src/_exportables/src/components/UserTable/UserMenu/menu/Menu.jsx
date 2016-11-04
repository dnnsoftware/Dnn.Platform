import React, {Component, PropTypes} from "react";
import "./style.less";

class Menu extends Component {
    render() {
        return (
            <ul className="dnn-user-menu menu">
                {this.props.children}
            </ul>
        );
    }
}

Menu.propTypes = {
    children: PropTypes.node
};

export default Menu;