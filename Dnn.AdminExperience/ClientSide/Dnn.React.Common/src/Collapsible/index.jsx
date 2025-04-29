import React, { Component } from "react";
import PropTypes from "prop-types";
import ReactCollapse from "react-collapse";
import scroll from "scroll";
import "./style.less";

const defaultDelay = 300;

const page = /Firefox/.test(navigator.userAgent)
  ? document.documentElement
  : document.body;

export default class Collapsible extends Component {
  constructor(props) {
    super(props);
  }

  componentWillUnmount() {
    if (this.scrollTimeout) {
      clearTimeout(this.scrollTimeout);
      this.scrollTimeout = null;
    }
  }
  scroll(contentHeight) {
    const { scrollDelay } = this.props;
    const delay = scrollDelay || scrollDelay === 0 ? scrollDelay : defaultDelay;
    if (!this.props.isOpened || !this.props.autoScroll) {
      return;
    }
    this.scrollTimeout = setTimeout(() => {
      const collapsible = this.collapsibleRef;
      const collapsibleTop = collapsible.getBoundingClientRect().top;
      const bodyTop = document.body.getBoundingClientRect().top;
      let bottom = collapsibleTop - bodyTop + contentHeight + 40;

      if (bottom > window.innerHeight) {
        bottom = bottom - window.innerHeight;
        if (bottom > window.scrollY) {
          scroll.top(page, bottom);
        }
      }
    }, delay);
  }

  render() {
    const { isOpened } = this.props;
    const className = this.props.className || "";
    return (
      <div
        ref={(node) => (this.collapsibleRef = node)}
        style={{ height: "100%", width: "100%" }}
      >
        <ReactCollapse
          isOpened={isOpened}
          theme={{
            collapse: `${className} ReactCollapse--collapse`,
            content: "ReactCollapse--content",
          }}
          onRest={({ contentHeight }) => this.scroll(contentHeight)}
          onWork={({ contentHeight }) => this.scroll(contentHeight)}
        >
          {this.props.children}
        </ReactCollapse>
      </div>
    );
  }
}

Collapsible.propTypes = {
  isOpened: PropTypes.bool,
  autoScroll: PropTypes.bool,
  scrollDelay: PropTypes.number,
  children: PropTypes.node,
  className: PropTypes.string,
};
