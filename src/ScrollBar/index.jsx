import React, { Component } from "react";
import PropTypes from "prop-types";

import ScrollArea from "react-scrollbar/dist/no-css";
import "./styles.less";

class ScrollBar extends Component {
    constructor(props) {
        super(props);

    }

    render() {
        return (
            <ScrollArea
                speed={0.8}
                className="area"
                contentClassName={this.props.contentClassName?this.props.contentClassName:""}
                contentStyle={this.props.contentStyle?this.props.contentStyle:""}>
                {this.props.children}
            </ScrollArea>
        );
    }
}


ScrollBar.propTypes = {
    children : PropTypes.object.isRequired,
    className : PropTypes.object,
    contentClassName : PropTypes.object,
    contentStyle: PropTypes.object
};

export default ScrollBar;