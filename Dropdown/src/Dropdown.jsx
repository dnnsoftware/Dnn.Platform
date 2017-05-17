import React, { PropTypes, Component } from "react";
import ReactDOM from "react-dom";
import Collapse from "react-collapse";
import Scrollbars from "react-custom-scrollbars";
import { ArrowDownIcon } from "dnn-svg-icons";
import scroll from "scroll";
import debounce from "lodash/debounce";
import "./style.less";

class Dropdown extends Component {
    constructor() {
        super();
        this.state = {
            dropDownOpen: false,
            fixedHeight: 0,
            dropdownText: "",
            closestValue: null,
            selectedOption: {}
        };
        this.handleClick = this.handleClick.bind(this);
        this.uniqueId = Date.now() * Math.random();
        this.debouncedSearch = debounce(this.searchItems, 500);
    }
    toggleDropdown(avoidFocusOnOpen) {
        const {props} = this;
        if (props.enabled) {

            //This triggers re-render, showing scrollbar on open.
            if (!this.state.dropDownOpen) {
                setTimeout(() => {
                    this.setState({});
                    if(!avoidFocusOnOpen) {
                        ReactDOM.findDOMNode(this.refs.dropdownSearch).focus();
                    }
                }, 250);
            } else {
                this.setState({
                    closestValue: null
                });
            }

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

        if(props.isDropDownOpen !== this.props.isDropDownOpen) {
            this.setState({dropDownOpen: !props.isDropDownOpen}, () => this.toggleDropdown(true));
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
                dropDownOpen: false,
                closestValue: null,
                dropdownText: ""
            });
        }
    }
    onSelect(option) {
        const {props} = this;
        if (props.enabled) {
            this.setState({
                dropDownOpen: false,
                closestValue: null,
                dropdownText: ""
            });
            if (props.onSelect) {
                this.setState({
                    selectedOption: option
                });
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
        return (props.prependWith ? <span className="dropdown-prepend"><strong>{props.prependWith}</strong> {label}</span> : label);
    }

    getIsMultiLineLabel() {
        return this.props.labelIsMultiLine ? "" : " no-wrap";
    }

    searchItems() {
        let closestValueLength = 0, closestValue = null, itemIndex = 0;

        this.props.options.forEach((option, index) => {
            let regex = this.state.dropdownText.replace(/([.?*+^$[\]\\(){}|-])/g, "\\$1");

            let stringToMatchBeginning = new RegExp("^" + regex, "gi");

            let labelToMatch = typeof option.label === "string" ? option.label : (option.searchableValue || "");

            if (labelToMatch.match(stringToMatchBeginning) && labelToMatch.match(stringToMatchBeginning).length > closestValueLength) {
                closestValueLength = labelToMatch.match(stringToMatchBeginning).length;
                closestValue = option;
                itemIndex = index;
            }
        });

        if (closestValue === null) {
            this.props.options.forEach((option, index) => {
                let regex = this.state.dropdownText.replace(/([.?*+^$[\]\\(){}|-])/g, "\\$1");

                let stringToMatchInBetween = new RegExp(regex, "gi");

                let labelToMatch = typeof option.label === "string" ? option.label : (option.searchableValue || "");

                if (labelToMatch.match(stringToMatchInBetween) && labelToMatch.match(stringToMatchInBetween).length > closestValueLength) {
                    closestValueLength = labelToMatch.match(stringToMatchInBetween).length;
                    closestValue = option;
                    itemIndex = index;
                }
            });
        }

        this.setState({
            closestValue
        }, () => {
            setTimeout(() => {
                this.setState({
                    dropdownText: ""
                });
            }, 1500);
        });

        if (closestValue !== null) {
            const optionValue = ReactDOM.findDOMNode(this.refs[this.uniqueId + "option-" + itemIndex]);
            if (optionValue) {
                const bottom = optionValue.offsetTop - 165;
                scroll.top(ReactDOM.findDOMNode(this.refs.dropdownScrollContainer).childNodes[0], bottom);
            }
        }
    }

    onDropdownSearch(event) {
        this.setState({
            dropdownText: event.target.value
        }, () => {
            this.debouncedSearch();
        });
    }

    onKeyDown(event) {
        if (event.key === "Enter") {
            if (this.state.closestValue && this.state.closestValue.value !== null) {
                this.onSelect(this.state.closestValue);
                ReactDOM.findDOMNode(this.refs.dropdownSearch).blur();
            } else {
                this.onSelect(this.state.selectedOption);
            }
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        const options = props.options && props.options.map((option, index) => {
            return <li onClick={this.onSelect.bind(this, option)} key={this.uniqueId + "option-" + index} ref={this.uniqueId + "option-" + index}
                className={((option.value === props.value && state.closestValue === null) || option.value === (state.closestValue && state.closestValue.value)) ? "selected" : ""}>{option.label}</li>;
        });
        return (
            <div className={this.getClassName()} style={props.style}>
                <div className={"collapsible-label" + this.getIsMultiLineLabel()} 
                    onClick={this.toggleDropdown.bind(this)} title={this.props.title}>
                    {this.getDropdownLabel()}
                </div>
                <input
                    type="text"
                    onChange={this.onDropdownSearch.bind(this)}
                    ref="dropdownSearch"
                    value={this.state.dropdownText}
                    onKeyDown={this.onKeyDown.bind(this)}
                    style={{ position: "absolute", opacity: 0, pointerEvents: "none", width: 0, height: 0, padding: 0, margin: 0 }}
                    aria-label="Search"
                    />
                {props.withIcon && <div className="dropdown-icon" dangerouslySetInnerHTML={{ __html: ArrowDownIcon }} onClick={this.toggleDropdown.bind(this)}></div>}
                <div className={"collapsible-content" + (state.dropDownOpen ? " open" : "")}>
                    <Collapse
                        fixedHeight={state.fixedHeight}
                        keepCollapsedContent={true}
                        isOpened={state.dropDownOpen}>
                        <Scrollbars
                            autoHide={this.props.autoHide}
                            style={props.scrollAreaStyle}
                            ref="dropdownScrollContainer"
                            onUpdate={this.props.onScrollUpdate}>
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
    autoHide: PropTypes.bool,
    value: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
    closeOnClick: PropTypes.bool,
    prependWith: PropTypes.string,
    labelIsMultiLine: PropTypes.bool,
    title: PropTypes.string,
    onScrollUpdate: PropTypes.func,
    isDropDownOpen: PropTypes.bool
};

Dropdown.defaultProps = {
    label: "-- Select --",
    withIcon: true,
    withBorder: true,
    size: "small",
    closeOnClick: true,
    enabled: true,
    autoHide: true,
    className: "",
    isDropDownOpen: false
};

export default Dropdown;