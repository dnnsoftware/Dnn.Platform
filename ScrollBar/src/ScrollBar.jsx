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
                    contentClassName="content">
                {this.props.children}
            </ScrollArea>
        );
    }
}


ScrollBar.propTypes = {
    children : PropTypes.object.isRequired
};

export default ScrollBar;