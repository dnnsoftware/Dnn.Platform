import { Component } from "react";
import PropTypes from "prop-types";
import "./style.less";

class CollapsibleSwitcher extends Component {
    constructor() {
        super();
    }
    render() {
        const {props} = this;
        return props.renderIndex >= 0 && props.children[props.renderIndex];
    }
}
CollapsibleSwitcher.propTypes = {
    renderIndex: PropTypes.number,
    children: PropTypes.array
};
CollapsibleSwitcher.defaultProps = {
    renderIndex: 0
};

export default (CollapsibleSwitcher);