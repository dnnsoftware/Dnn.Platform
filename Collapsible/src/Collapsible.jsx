import React, {Component, PropTypes} from "react";
import ReactCollapse from "react-collapse";
import {findDOMNode} from "react-dom";
import scroll from "scroll";

const page = /Firefox/.test(navigator.userAgent) ?
  document.documentElement :
  document.body;

export default class Collapsible extends Component {

    scroll(height) {
        if (!this.props.isOpened || this.props.scroll === false ) {
            return;
        }
        const collapsible = findDOMNode(this.refs.collapsible);
        const collapsibleTop = collapsible.getBoundingClientRect().top;
        const parentHeight = collapsible.parentNode.getBoundingClientRect().height;
        const collapsibleHeight = this.props.fixedHeight || height;
        const bodyTop = document.body.getBoundingClientRect().top;
        let bottom = collapsibleTop - bodyTop + collapsibleHeight  + parentHeight;
        
        if (bottom > window.innerHeight ) {
            bottom = bottom - window.innerHeight;
            if (bottom > window.scrollY) {
                scroll.top(page, bottom);
            }
        }
        if (typeof this.props.onHeightReady === "function") {
            this.props.onHeightReady(height);
        }

    }

    render() {
        const {isOpened, style, fixedHeight} = this.props;
        return (
            <ReactCollapse 
                isOpened={isOpened}
                style={style}
                ref="collapsible"
                keepCollapsedContent={true}
                onHeightReady={this.scroll.bind(this)}
                fixedHeight={fixedHeight}>
                {this.props.children}
            </ReactCollapse>
        );
    }
}

Collapsible.propTypes = {
    isOpened: PropTypes.bool,
    style: PropTypes.object,
    fixedHeight: PropTypes.number,
    scroll: PropTypes.bool,
    onHeightReady: PropTypes.func,
    children: PropTypes.node
};
