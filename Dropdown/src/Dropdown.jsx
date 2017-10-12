import React, {PropTypes, Component} from "react";
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

    // SCM-1115
    getDropdownHeight() {
        const {props} = this;
        const maxHeight = props.fixedHeight ? props.fixedHeight : DNN_DROPDOWN_MINHEIGHT;
        console.log(`dropDownListElement: ${this.dropDownListElement}`);
        return this.dropDownListElement ? Math.min(this.dropDownListElement.offsetHeight, maxHeight) + 20 : 0;
    }

    componentWillMount() {
        const {props} = this;
        if (props.options && props.options.length > 0) {
            let fixedHeight = DNN_DROPDOWN_MINHEIGHT; //this.getDropdownHeight();
            this.setState({
                fixedHeight
            });
        }
    }

    componentWillReceiveProps(props) {
        if (props.options && props.options.length > 0) {
            let fixedHeight = DNN_DROPDOWN_MINHEIGHT; //this.getDropdownHeight();
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
        this.updateScrollbar();
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("mousedown", this.handleClick);
        this._isMounted = false;
    }

    updateScrollbar() {
        if (this.scrollBar) {
            console.log(this.scrollBar);
        }
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
            // SCM-1115
            const optionValue = this.getOption(itemIndex);
            if (optionValue) {
                const bottom = optionValue.offsetTop - this.getDropdownHeight();
                scroll.top(ReactDOM.findDOMNode(this.scrollBar).childNodes[0], bottom);
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
                this.dropdownSearch.blur();
            } else {
                this.onSelect(this.state.selectedOption);
            }
        }
    }

    // SCM-1115
    initOptions(option, index) {
        const {props, state} = this;
        this.optionItems = [];
        const options = props.options && props.options.map((option, index) => {
            return <li onClick={this.onSelect.bind(this, option)} key={index}
                       ref={this.addOptionRef.bind(this)}
                       className={((option.value === props.value && state.closestValue === null) || option.value === (state.closestValue && state.closestValue.value)) ? "dnn-dropdown-option selected" : "dnn-dropdown-optionf"}>{option.label}</li>;
        });
        return options;
    }

    // SCM-1115
    addOptionRef(option) {
        //console.log(`OPTION REF: ${option}`);
        if (option) {
            this.optionItems = this.optionItems ? this.optionItems : [];
            this.optionItems.push(option);
        }
    }

    // SCM-1115
    getOption(index) {
        const options = this.optionItems;
        return options && options[index] !== undefined ? options[index] : null;
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        const options = this.initOptions();
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
                        keepCollapsedContent={true}
                        isOpened={state.dropDownOpen}>
                        <div>
                            <Scrollbars
                                ref={(scrollbar) => this.scrollBar = scrollbar}
                                autoHide={this.props.autoHide}
                                autoHeight={true}
                                autoHeightMin={DNN_DROPDOWN_MINHEIGHT}
                                style={props.scrollAreaStyle}
                                onUpdate={this.props.onScrollUpdate}
                                renderTrackHorizontal={props => <div /> }>
                                <ul className="dnn-dropdown-options" ref={(ul) => this.dropDownListElement = ul}>
                                    {options}
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
    onArrowKey: PropTypes.func
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
    selectedIndex: -1
};

export default Dropdown;