import React, {Component, PropTypes} from "react";
import ReactCollapse from "react-collapse";
import {findDOMNode} from "react-dom";
import scroll from "scroll";

const page = /Firefox/.test(navigator.userAgent) ?
  document.documentElement :
  document.body;

export default class Collapsable extends Component {

    scroll(height) {
        if (!this.props.isOpened || this.props.scroll === false ) {
            return;
        }
        const collapsable = findDOMNode(this.refs.collapsible);
        const collapsableTop = collapsable.getBoundingClientRect().top;
        const parentHeight = collapsable.parentNode.getBoundingClientRect().height;
        const collapsableHeight = this.props.fixedHeight || height;
        const bodyTop = document.body.getBoundingClientRect().top;
        let bottom = collapsableTop - bodyTop + collapsableHeight  + parentHeight;
        
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

Collapsable.propTypes = {
    isOpened: PropTypes.bool,
    style: PropTypes.object,
    fixedHeight: PropTypes.number,
    scroll: PropTypes.bool,
    onHeightReady: PropTypes.func,
    children: PropTypes.node
};
