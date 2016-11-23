import React, { PropTypes, Component } from "react";
import ReactDOM from "react-dom";
import Collapse from "react-collapse";
import Scrollbars from "react-custom-scrollbars";
import { ArrowDownIcon } from "dnn-svg-icons";
import "./style.less";

class Dropdown extends Component {
    constructor() {
        super();
        this.state = {
            dropDownOpen: false,
            fixedHeight: 0
        };
        this.handleClick = this.handleClick.bind(this);
        this.uniqueId = Date.now() * Math.random();
    }
    toggleDropdown() {
        const {props} = this;
        if (props.enabled) {
            this.setState({
                dropDownOpen: !this.state.dropDownOpen
            });
        }
        else {
            this.setState({
                dropDownOpen: false
            });
        }
    }
    getDropdownHeight(length, size) {
        const {props} = this;
        if (props.fixedHeight) {
            return props.fixedHeight;
        }
        let itemHeight = (size === "large" ? 38 : 28) * length;

        return itemHeight < 140 ? itemHeight + 20 : 160;
    }
    componentWillMount() {
        const {props} = this;
        if (props.options && props.options.length > 0) {
            let fixedHeight = this.getDropdownHeight(props.options.length, props.size);
            this.setState({
                fixedHeight
            });
        }
    }
    componentWillReceiveProps(props) {
        if (props.options && props.options.length > 0) {
            let fixedHeight = this.getDropdownHeight(props.options.length, props.size);
            this.setState({
                fixedHeight
            });
        }
    }
    componentDidMount() {
        const {props} = this;
        if (props.closeOnClick) {
            document.addEventListener("mousedown", this.handleClick);
        }
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("mousedown", this.handleClick);
        this._isMounted = false;
    }
    handleClick(event) {
        const {props} = this;
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted || !props.closeOnClick) { return; }

        if (!ReactDOM.findDOMNode(this).contains(event.target)) {
            this.setState({
                dropDownOpen: false
            });
        }
    }
    onSelect(option) {
        const {props} = this;
        if (props.enabled) {
            this.setState({
                dropDownOpen: false
            });
            if (props.onSelect) {
                props.onSelect(option);
            }
        }
    }
    getClassName() {
        const {props, state} = this;
        let className = "dnn-dropdown";

        className += (props.withBorder ? " with-border" : "");

        className += (" " + props.size);

        className += (" " + props.className);

        if (!props.enabled) {
            className += " disabled";
        }
        else {
            className += (state.dropDownOpen ? " open" : "");
        }
        return className;
    }

    getDropdownLabel() {
        const {props} = this;
        let label = props.label;
        if (props.value !== undefined && props.options !== undefined && props.options.length > 0) {
            const selectedValue = props.options.find((option) => {
                return option.value === props.value;
            });
            if (selectedValue && selectedValue.label) {
                label = selectedValue.label;
            }
        }
        return (props.prependWith ? props.prependWith + " " + label : label);
    }

    getIsMultiLineLabel(){
        return this.props.labelIsMultiLine ? "" : " no-wrap";
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        const options = props.options && props.options.map((option, index) => {
            return <li onClick={this.onSelect.bind(this, option)} key={this.uniqueId + "option-" + index} className={option.value === props.value ? "selected" : ""} >{option.label}</li>;
        });
        return (
            <div className={this.getClassName()} style={props.style}>
                <div className={"collapsible-label" + this.getIsMultiLineLabel()} onClick={this.toggleDropdown.bind(this)}>
                    {this.getDropdownLabel()}
                </div>
                {props.withIcon && <div className="dropdown-icon" dangerouslySetInnerHTML={{ __html: ArrowDownIcon }} onClick={this.toggleDropdown.bind(this)}></div>}
                <div className={"collapsible-content" + (state.dropDownOpen ? " open" : "")}>
                    <Collapse
                        fixedHeight={state.fixedHeight}
                        keepCollapsedContent={true}
                        isOpened={state.dropDownOpen}>
                        <Scrollbars width={props.collapsibleWidth || "100%"} height={props.collapsibleHeight || "100%"} style={props.scrollAreaStyle}>
                            <div>
                                <ul>
                                    {options}
                                </ul>
                            </div>
                        </Scrollbars>
                        {!props.fixedHeight &&
                            <ul>
                                {options}
                            </ul>
                        }
                    </Collapse>
                </div>
            </div>
        );
    }
}

Dropdown.propTypes = {
    label: PropTypes.string,
    fixedHeight: PropTypes.number,
    collapsibleWidth: PropTypes.number,
    collapsibleHeight: PropTypes.number,
    keepCollapsedContent: PropTypes.bool,
    className: PropTypes.string,
    scrollAreaStyle: PropTypes.object,
    options: PropTypes.array,
    onSelect: PropTypes.func,
    size: PropTypes.string,
    withBorder: PropTypes.bool,
    withIcon: PropTypes.bool,
    enabled: PropTypes.bool,
    value: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
    closeOnClick: PropTypes.bool,
    prependWith: PropTypes.string,
    labelIsMultiLine: PropTypes.bool
};

Dropdown.defaultProps = {
    label: "-- Select --",
    withIcon: true,
    withBorder: true,
    size: "small",
    closeOnClick: true,
    enabled: true,
    className: ""
};

export default Dropdown;