import React, { Component } from "react";
import PropTypes from "prop-types";
import Collapse from "react-collapse";
import Button from "../Button";
import "./style.less";

export default class CollapsibleRow extends Component {
  constructor(props) {
    super(props);
    this.state = { collapsed: props.collapsed ? props.collapsed : true };
    this.handleClick = this.handleClick.bind(this);
  }

  componentDidMount() {
    const { props } = this;
    if (props.closeOnBlur) {
      document.addEventListener("click", this.handleClick);
      this._isMounted = true;
    }
  }

  componentDidUpdate(prevProps) {
    if (this.props.collapsed !== prevProps.collapsed) {
      this.setState({ collapsed: this.props.collapsed });
    }
  }

  componentWillUnmount() {
    document.removeEventListener("click", this.handleClick);
    this._isMounted = false;
  }

  handleClick(event) {
    const { props } = this;
    // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
    // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
    // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
    if (!this._isMounted || !props.closeOnBlur) {
      return;
    }

    if (!this.node.contains(event.target)) {
      this.collapse();
    }
  }
  uncollapse() {
    this.setState({
      collapsed: false,
    });
  }
  collapse() {
    this.setState({
      collapsed: true,
    });
  }
  onCancel() {
    this.collapse();
    if (this.props.onChange) {
      this.props.onChange(true);
    }
  }
  toggle() {
    if (this.props.onChange) {
      this.props.onChange(this.state.collapsed);
    }
    if (this.state.collapsed) {
      this.uncollapse();
    } else {
      this.collapse();
    }
  }
  render() {
    const { props } = this;
    return (
      <div
        className={
          "dnn-collapsible-component" +
          (props.className ? " " + props.className : "")
        }
      >
        <div className="collapsible-header" onClick={this.toggle.bind(this)}>
          {props.label}
          {!props.disabled && (
            <span
              className={
                "collapse-icon " + (this.state.collapsed ? "collapsed" : "")
              }
            ></span>
          )}
        </div>
        <Collapse
          ref={(node) => (this.node = node)}
          isOpened={!this.state.collapsed}
          theme={{
            collapse: "collapsible-row ReactCollapse--collapse",
            content: "ReactCollapse--content",
          }}
        >
          {props.children}
          {!props.buttonsAreHidden && (
            <div className="collapsible-footer">
              <Button type="secondary" onClick={this.onCancel.bind(this)}>
                {props.secondaryButtonText || "Cancel"}
              </Button>
              {props.extraFooterButtons}
            </div>
          )}
        </Collapse>
      </div>
    );
  }
}

CollapsibleRow.propTypes = {
  label: PropTypes.node,
  children: PropTypes.node,
  disabled: PropTypes.bool,
  className: PropTypes.string,
  closeOnBlur: PropTypes.bool,
  secondaryButtonText: PropTypes.string,
  extraFooterButtons: PropTypes.node,
  buttonsAreHidden: PropTypes.bool,
  collapsed: PropTypes.bool,
  onChange: PropTypes.func,
};

CollapsibleRow.defaultProps = {
  collapsed: true,
};
