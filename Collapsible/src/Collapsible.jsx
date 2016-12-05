import React, {Component, PropTypes} from "react";
import ReactCollapse from "react-collapse";
import {findDOMNode} from "react-dom";
import scroll from "scroll";

const defaultDelay = 300;

const page = /Firefox/.test(navigator.userAgent) ?
  document.documentElement :
  document.body;

export default class Collapsible extends Component {

    scroll(height) {
        const {scrollDelay} = this.props;
        const delay = scrollDelay || scrollDelay === 0 ? scrollDelay : defaultDelay;
        if (!this.props.isOpened || !this.props.autoScroll ) {
            return;
        }
        setTimeout(()=> {
            const collapsible = findDOMNode(this.refs.collapsible);
            const collapsibleTop = collapsible.getBoundingClientRect().top;
            const collapsibleHeight = this.props.fixedHeight || height;
            const bodyTop = document.body.getBoundingClientRect().top;
            let bottom = collapsibleTop - bodyTop + collapsibleHeight + 40;
            
            if (bottom > window.innerHeight ) {
                bottom = bottom - window.innerHeight;
                if (bottom > window.scrollY) {
                    scroll.top(page, bottom);
                }
            }
        }, delay);
        if (typeof this.props.onHeightReady === "function") {
            this.props.onHeightReady(height);
        }
    }

    render() {
        const {isOpened, style, fixedHeight} = this.props;
        const className = this.props.className || "";
        return (
            <ReactCollapse 
                isOpened={isOpened}
                style={style}
                ref="collapsible"
                keepCollapsedContent={true}
                className={className}
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
    autoScroll: PropTypes.bool,
    scrollDelay: PropTypes.number,
    onHeightReady: PropTypes.func,
    children: PropTypes.node,
    className: PropTypes.string
};
