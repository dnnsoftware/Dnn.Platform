import React, { Component} from "react";
import PropTypes from "prop-types";
import ReactDOM from "react-dom";
import Collapse from "react-collapse";
import Scrollbars from "react-custom-scrollbars";
import {ArrowDownIcon} from "dnn-svg-icons";
import scroll from "scroll";
import debounce from "lodash/debounce";
import "./style.less";

const DNN_DROPDOWN_MINHEIGHT = 100;

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

    toggleDropdown() {
        const {props} = this;
        if (props.enabled) {

            //This triggers re-render, showing scrollbar on open.
            if (!this.state.dropDownOpen) {
                this.dropdownSearch.focus();
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

    getDropdownHeight() {
        const {props} = this;
        const maxHeight = props.fixedHeight ? props.fixedHeight : DNN_DROPDOWN_MINHEIGHT;
        return this.dropDownListElement ? Math.min(this.dropDownListElement.offsetHeight, maxHeight) + 20 : 0;
    }

    componentWillMount() {
        const {props} = this;
        if (props.options && props.options.length > 0) {
            let fixedHeight = DNN_DROPDOWN_MINHEIGHT;
            this.setState({
                fixedHeight
            });
        }
    }

    componentWillReceiveProps(props) {
        if (props.options && props.options.length > 0) {
            let fixedHeight = DNN_DROPDOWN_MINHEIGHT;
            this.setState({
                fixedHeight
            });
        }

        if (props.isDropDownOpen !== this.props.isDropDownOpen) {
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
        if (!this._isMounted || !props.closeOnClick) {
            return;
        }

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

    /**
     * We have two types of Dropdown: small and large.
     * More informations about it here: https://dnntracker.atlassian.net/wiki/spaces/DP/pages/45940759/EVOQ+-+COLOR+AND+STYLE+GUIDE#EVOQ-COLORANDSTYLEGUIDE-SMALLDROPDOWN
     * PS: "small" is passed as a prop and used as a CSS class name.
     *
     * @returns {string}
     */
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
        return (props.prependWith ?
            <span className="dropdown-prepend"><strong>{props.prependWith}</strong> {label}</span> : label);
    }

    getIsMultiLineLabel() {
        return this.props.labelIsMultiLine ? "" : " no-wrap";
    }

    searchItems() {

        const { props } = this;

        let closestValueLength = 0, closestValue = null, itemIndex = 0;

        props.options.forEach((option, index) => {
            let regex = this.state.dropdownText.replace(/([.?*+^$[\]\\(){}|-])/g, "\\$1");

            const label = props.decode ? props.decode(option.label) : option.label;

            let stringToMatchBeginning = new RegExp("^" + regex, "gi");

            let labelToMatch = typeof label === "string" ? label : (option.searchableValue || "");

            if (labelToMatch.match(stringToMatchBeginning) && labelToMatch.match(stringToMatchBeginning).length > closestValueLength) {
                closestValueLength = labelToMatch.match(stringToMatchBeginning).length;
                closestValue = option.value;
                itemIndex = index;
            }
        });

        if (closestValue === null) {
            props.options.forEach((option, index) => {
                let regex = this.state.dropdownText.replace(/([.?*+^$[\]\\(){}|-])/g, "\\$1");

                let stringToMatchInBetween = new RegExp(regex, "gi");

                const label = props.decode ? props.decode(option.label) : option.label;

                let labelToMatch = typeof label === "string" ? label : (option.searchableValue || "");

                if (labelToMatch.match(stringToMatchInBetween) && labelToMatch.match(stringToMatchInBetween).length > closestValueLength) {
                    closestValueLength = labelToMatch.match(stringToMatchInBetween).length;
                    closestValue = option;
                    itemIndex = index;
                }
            });
        }

        this.setState({
            closestValue,
            currentIndex: itemIndex
        }, () => {
            setTimeout(() => {
                this.setState({
                    dropdownText: ""
                });
            }, 1500);
        });

        if (closestValue !== null) {
            this.scrollToSelectedItem(itemIndex);
        }
    }

    scrollToSelectedItem(itemIndex, eventKey) {

        const optionRef = this.selectedOptionElement ? this.selectedOptionElement : null;
        if (optionRef) {
            const domElement = ReactDOM.findDOMNode(optionRef);
            let offset = domElement.offsetTop;
            if (eventKey == "ArrowUp") {
                offset = domElement.offsetTop - domElement.clientHeight*2;
            }
            scroll.top(ReactDOM.findDOMNode(this.scrollBar).childNodes[0], offset);
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
        switch (event.key) {
            case  "Enter":
                if (this.state.closestValue && this.state.closestValue.value !== null) {
                    this.onSelect(this.state.closestValue);
                    this.dropdownSearch.blur();
                } else {
                    this.onSelect(this.state.selectedOption);
                }
                break;
            case "ArrowUp":
                this.onArrowUp(event.key);
                break;
            case "ArrowDown":
                this.onArrowDown(event.key);
                break;
        }

    }

    getCurrentIndex() {
        const maxIndex = this.optionItems ? this.optionItems.length : 0;
        const currentIndex = this.state.currentIndex != undefined ? this.state.currentIndex : -1;
        return currentIndex > -1 && currentIndex < maxIndex ? currentIndex : -1;
    }

    onArrowDown(eventKey) {
        const maxIndex = this.optionItems ? this.optionItems.length -1 : 0;
        let currentIndex = this.getCurrentIndex();
        const nextIndex = currentIndex < maxIndex ? currentIndex++ : currentIndex;
        const option = this.getOption(currentIndex);
        this.setState({currentIndex, selectedOption: option, closestValue: null, ignorePreselection: true});
        this.scrollToSelectedItem(nextIndex, eventKey);
    }

    onArrowUp(eventKey) {
        let currentIndex = this.getCurrentIndex();
        const nextIndex = currentIndex > 0 ? currentIndex-- : currentIndex;
        this.setState({currentIndex, selectedOption: this.getOption(currentIndex), closestValue: null, ignorePreselection: true});
        this.scrollToSelectedItem(nextIndex, eventKey);
    }

    initOptions() {
        const { props } = this;
        this.optionItems = [];
        const options = props.options && props.options.map((option, index) => {
            this.optionItems.push(option);
            return <li onClick={this.onSelect.bind(this, option)} key={index}
                       ref={this.isSelectedItem(index) ? this.addOptionRef.bind(this) : f => f}
                       className={this.getOptionClassName(option, index)}>{option.label}</li>;
        });
        return options;
    }

    getOptionClassName(option, index) {
        const {props, state} = this;
        const currentIndex = this.getCurrentIndex();
        const isCurrentIndex = index === currentIndex;
        const isPreselected = !this.state.ignorePreselection && props.value != null && (option.value === props.value && state.closestValue === null && currentIndex < 0);
        const isSearchResult = state.closestValue != null && (option.value === state.closestValue);
        const selected = currentIndex === -1 ? isPreselected : (isCurrentIndex || isSearchResult);
        return selected ? "dnn-dropdown-option selected" : "dnn-dropdown-option";
    }

    isSelectedItem(index) {
        return index == this.state.currentIndex;
    }

    addOptionRef(option) {
        if (option) {
            this.selectedOptionElement = option;
        }
    }

    getOption(index) {
        const options = this.optionItems;
        return options && options[index] !== undefined ? options[index] : null;
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        return (
            <div className={this.getClassName()} style={props.style}>
                <div className={"collapsible-label" + this.getIsMultiLineLabel()}
                     onClick={this.toggleDropdown.bind(this)} title={this.props.title}>
                    {this.getDropdownLabel()}
                </div>
                <input
                    type="text"
                    onChange={this.onDropdownSearch.bind(this)}
                    ref={(input) => this.dropdownSearch = input}
                    value={this.state.dropdownText}
                    onKeyDown={this.onKeyDown.bind(this)}
                    style={{
                        position: "absolute",
                        opacity: 0,
                        pointerEvents: "none",
                        width: 0,
                        height: 0,
                        padding: 0,
                        margin: 0
                    }}
                    aria-label="Search"
                />
                {props.withIcon && <div className="dropdown-icon" dangerouslySetInnerHTML={{__html: ArrowDownIcon}}
                                        onClick={this.toggleDropdown.bind(this)}></div>}
                <div className={"collapsible-content" + (state.dropDownOpen ? " open" : "")}>
                    <Collapse
                        isOpened={state.dropDownOpen}>
                        <div>
                            <Scrollbars
                                ref={(scrollbar) => this.scrollBar = scrollbar}
                                autoHide={this.props.autoHide}
                                autoHeight={true}
                                autoHeightMin={DNN_DROPDOWN_MINHEIGHT}
                                style={props.scrollAreaStyle}
                                onUpdate={this.props.onScrollUpdate}
                                renderTrackHorizontal={() => <div/>}>
                                <ul className="dnn-dropdown-options" ref={(ul) => this.dropDownListElement = ul}>
                                    {this.initOptions()}
                                </ul>
                            </Scrollbars>
                        </div>
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
    isDropDownOpen: PropTypes.bool,
    selectedIndex: PropTypes.number,
    onArrowKey: PropTypes.func,
    decode: PropTypes.func.isRequired // fn(labelObject):string
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
    isDropDownOpen: false,
    selectedIndex: -1,
    decode:(label) => label
};

export default Dropdown;