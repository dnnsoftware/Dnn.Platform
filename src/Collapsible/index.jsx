import React, { Component } from "react";
import PropTypes from "prop-types";
import ReactCollapse from "react-collapse";
import scroll from "scroll";

const defaultDelay = 300;

const page = /Firefox/.test(navigator.userAgent) ?
    document.documentElement :
    document.body;

export default class Collapsible extends Component {
    constructor(props) {
        super(props);
        this.collapsibleRef = React.createRef();
    }

    componentWillUnmount() {
        if (this.scrollTimeout) {
            clearTimeout(this.scrollTimeout);
            this.scrollTimeout = null;
        }
    }
    scroll(height, width) {
        const {scrollDelay} = this.props;
        const delay = scrollDelay || scrollDelay === 0 ? scrollDelay : defaultDelay;
        if (!this.props.isOpened || !this.props.autoScroll ) {
            return;
        }
        this.scrollTimeout = setTimeout(()=> {
            const collapsible = this.collapsibleRef.current;
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
                ref={this.collapsibleRef}
                className={className}
                onMeasure={this.scroll.bind(this)}
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
