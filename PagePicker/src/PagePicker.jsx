import React, {PropTypes, Component} from "react";
import ReactDOM from "react-dom";
import Collapse from "react-collapse";
import Scrollbars from "react-custom-scrollbars";
import SearchBox from "dnn-search-box";
import {ArrowDownIcon, ArrowRightIcon, PageIcon} from "dnn-svg-icons";
import "./style.less";

function serializeQueryStringParameters(obj) {
    let s = [];
    for (let p in obj) {
        if (obj.hasOwnProperty(p)) {
            s.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
        }
    }
    return s.join("&");
}

class PagePicker extends Component {
    constructor() {
        super();
        this.state = {
            dropDownOpen: false,
            pageData: {},
            selectedPageId: "-1",
            searchPageData: {}
        };
        this.handleClick = this.handleClick.bind(this);
        this.loaded = false;

        String.prototype.replaceSpecialCharacters = function (specialCharacters) {
            let _this = this.split(" ").join("");
            for (let i = 0; i < specialCharacters.length; i++) {
                _this = _this.split(specialCharacters[i]).join("");
            }
            return _this;
        };

    }
    toggleDropdown() {
        const {props} = this;
        if (!this.loaded) {
            this.initialize();
        }
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
    initialize() {
        const {props} = this;
        props.serviceFramework.moduleRoot = props.moduleRoot;
        props.serviceFramework.controller = props.controller;

        props.serviceFramework.get("GetPages?" + serializeQueryStringParameters(props.apiParameters), {}, this.setPages.bind(this));
    }
    componentWillMount() {
        const {props} = this;
        this.setState({
            selectedPage: props.defaultLabel
        });
    }
    recursivelyFind(items, value, callback, findDescendants) {
        if (items && items.children) {
            for (let i = 0; i < items.children.length; i++) {
                if (items.children[i].data.value.replaceSpecialCharacters(["&", "?", ".", "'", "#", ":", "*"]) === value) {
                    if (findDescendants) {
                        this.getDescendants(items.children[i], callback);
                    }
                    return items.children[i];
                }
                let found = this.recursivelyFind(items.children[i], value, callback, findDescendants);
                if (found) return found;
            }
        }
    }

    findSelectedPage(path, index) {
        const { state } = this;
        if ((index < path.length - 1) && (path[index] !== state.selectedPage)) {
            this.recursivelyFind(state.pageData.Tree, path[index], () => {
                this.findSelectedPage(path, (index + 1));
            }, true);
        } else {
            const selectedItem = this.recursivelyFind(state.pageData.Tree, path[path.length - 1]);

            if (selectedItem) {
                this.setState({
                    selectedPageId: selectedItem.data.key,
                    selectedPage: selectedItem.data.value
                });
            }
        }
    }

    setPages(pageData) {
        this.loaded = true;
        this.setState({
            pageData
        }, () => {
            const {props} = this;
            let path = props.selectedPage.split("//").filter(page => {
                return page !== "";
            });
            let index = 0;
            this.findSelectedPage(path, index);
        });
    }
    componentDidMount() {
        const {props} = this;
        if (props.closeOnBlur) {
            document.addEventListener("click", this.handleClick);
        }
        this._isMounted = true;
    }

    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick);
        this._isMounted = false;
    }
    handleClick(event) {
        const {props} = this;
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted || !props.closeOnBlur) { return; }
        if (!ReactDOM.findDOMNode(this).contains(event.target)) {
            this.setState({
                dropDownOpen: false
            });
        }
    }
    getClassName() {
        const {props, state} = this;
        let className = "dnn-page-picker";

        className += (props.withBorder ? " with-border" : "");

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
        const { state } = this;
        let label = state.selectedPage;
        return label;
    }

    //page is passed by reference.
    getDescendants(page, callback, event) {
        if (event) {
            event.stopPropagation();
        }
        const { props } = this;

        let parentToAddChildrenTo = page;
        if (parentToAddChildrenTo.children && parentToAddChildrenTo.children.length > 0) {
            parentToAddChildrenTo.isOpen = !parentToAddChildrenTo.isOpen;

            //Add timeout to let render through - the dropdown will close on collapse of children if this is not here.
            setTimeout(() => {
                this.setState({});
            }, 0);
            return;
        }
        const apiParameters = Object.assign(props.apiParameters, { parentId: page.data.key });

        props.serviceFramework.moduleRoot = props.moduleRoot;
        props.serviceFramework.controller = props.controller;

        props.serviceFramework.get("GetPageDescendants?" + serializeQueryStringParameters(apiParameters), {},
            (data) => {
                parentToAddChildrenTo.isOpen = true;
                parentToAddChildrenTo.children = data.Items.map(child => {
                    return {
                        data: child,
                        children: []
                    };
                });
                this.setState({});
                if (callback) {
                    callback();
                }
            });
    }

    //page is passed by reference.
    openPage(page, event) {
        event.preventDefault();
        event.stopPropagation();
        page.isOpen = !page.isOpen;
        this.setState({});
    }

    onPageSelect(page) {
        const {props} = this;
        if (props.onPageSelect && page.data.selectable) {
            props.onPageSelect({
                tabId: parseInt(page.data.key),
                tabName: page.data.value
            });
        }
        this.setState({
            dropDownOpen: false,
            selectedPageId: parseInt(page.data.key),
            selectedPage: page.data.value
        });
    }

    getSelected(page) {
        const {state} = this;

        return "page-value" + ((parseInt(page.data.key) === parseInt(state.selectedPageId)) ? " selected" : "");
    }

    /* eslint-disable react/no-danger */
    getChildItems(children) {
        if (!children) {
            return [];
        }
        return children.map(page => {
            return <li className={"page-item" + (page.data.hasChildren ? " has-children" : "") + (page.isOpen ? " opened" : " closed") }>
                {(!page.isOpen && page.data.hasChildren) && <div className="arrow-icon" dangerouslySetInnerHTML={{ __html: ArrowRightIcon }} onClick={this.getDescendants.bind(this, page, null) }></div>}
                {(page.isOpen && page.data.hasChildren) && <div className="arrow-icon" dangerouslySetInnerHTML={{ __html: ArrowDownIcon }} onClick={this.getDescendants.bind(this, page, null) }></div>}
                <div className={this.getSelected(page) } onClick={this.onPageSelect.bind(this, page) } dangerouslySetInnerHTML={{ __html: (PageIcon + page.data.value) }}></div>
                {(page.children && page.children.length > 0) &&
                    <ul className={"child-pages" + (page.isOpen ? " opened" : "") }>
                        {this.getChildItems(page.children) }
                    </ul>
                }
            </li>;
        });
    }

    onSearch(value) {
        const {props} = this;

        if (value !== "") {
            const apiParameters = Object.assign(props.apiParameters, { searchText: value });

            props.serviceFramework.moduleRoot = props.moduleRoot;
            props.serviceFramework.controller = props.controller;

            props.serviceFramework.get("SearchPages?" + serializeQueryStringParameters(apiParameters), {}, (searchPageData) => {
                this.setState({
                    searchPageData,
                    searchMode: true
                });
            });
        } else {
            this.setState({
                searchMode: false
            });
        }
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;

        const children = state.searchMode ? Object.keys(state.searchPageData).length > 0 && this.getChildItems(state.searchPageData.Tree.children) : Object.keys(state.pageData).length > 0 && this.getChildItems(state.pageData.Tree.children);
        return (
            <div className={this.getClassName() } style={props.style} ref="dnnPagePicker">
                <div className="collapsible-label" onClick={this.toggleDropdown.bind(this) }>
                    {this.getDropdownLabel() }
                </div>
                {props.withIcon && <div className="dropdown-icon" dangerouslySetInnerHTML={{ __html: ArrowDownIcon }}></div>}
                <div className={"collapsible-content" + (state.dropDownOpen ? " open" : "") }>
                    <Collapse
                        className="page-picker-content"
                        ref="pagePickerContent"
                        keepCollapsedContent={props.keepCollapsedContent}
                        isOpened={state.dropDownOpen}>
                        <SearchBox
                            style={Object.assign({ display: "block" }, props.searchBoxStyle) }
                            placeholder={props.placeholderText}
                            className="page-picker-search"
                            onSearch={this.onSearch.bind(this) }
                            iconStyle={Object.assign({ right: 4 }, props.iconStyle) }
                            />
                        <Scrollbars style={props.scrollAreaStyle}>
                            <div className="pages-container">
                                <ul>
                                    {props.noneSpecified.visible && <li className="page-item">
                                        <div
                                            className={this.getSelected(props.noneSpecified) }
                                            onClick={this.onPageSelect.bind(this, props.noneSpecified) }
                                            style={Object.assign({ textIndent: 0 }, props.noneSpecifiedStyle) }>
                                            {props.noneSpecified.data.value}
                                        </div>
                                    </li>}
                                    {children}
                                </ul>
                            </div>
                        </Scrollbars>
                    </Collapse>
                </div>
            </div>
        );
    }
}

PagePicker.PropTypes = {
    //Selected page - format should be page names separated by //. 
    //Example: //Activity Feed//My Profile, My Profile will be the selected page.
    selectedPage: PropTypes.string,

    //React Collapse prop - set to false if you want to re-render the items every time.
    keepCollapsedContent: PropTypes.bool,

    //Class name to attach to parent
    className: PropTypes.string,

    //Style of scrollarea - page picker box, override if necessary.
    scrollAreaStyle: PropTypes.object,

    //Search box style
    searchBoxStyle: PropTypes.object,

    //Icon Style on search box
    iconStyle: PropTypes.object,

    //Style on the none specified object
    noneSpecifiedStyle: PropTypes.object,

    //Whether or not the dropdown toggle has a border.
    withBorder: PropTypes.bool,

    //Show the dropdown icon
    withIcon: PropTypes.bool,

    //Set to false to disable the picker (readonly)
    enabled: PropTypes.bool,

    //Close the picker on click outside.
    closeOnBlur: PropTypes.bool,

    //Service Framework - this is required to make this work.
    serviceFramework: PropTypes.object,

    //API Parameters - change parameters passed into API.
    apiParameters: PropTypes.object,

    //Placeholder text in searchbox.
    placeholderText: PropTypes.string,

    //None specified - default if none is selected
    noneSpecified: PropTypes.object,

    //Service Framework module root
    moduleRoot: PropTypes.string,

    //Service Framework controller
    controller: PropTypes.string,

    //Default label shown before pinging API.
    defaultLabel: PropTypes.string
};

PagePicker.defaultProps = {
    apiParameters: {
        sortOrder: 0,
        includeDisabled: true,
        includeAllTypes: true,
        includeActive: true,
        disabledNotSelectable: false,
        includeHostPages: false,
        roles: ""
    },
    noneSpecified: {
        data: {
            key: "-1",
            value: "<None Specified>",
            selectable: true
        },
        visible: true
    },
    selectedPage: "//< None Specified >",
    withIcon: true,
    withBorder: true,
    closeOnBlur: true,
    enabled: true,
    className: "",
    keepCollapsedContent: true,
    placeholderText: "Search pages..",
    style: {
        width: 300
    },
    scrollAreaStyle: {
        width: "100%",
        height: 300,
        marginTop: 14,
        border: "1px solid #c8c8c8"
    },
    moduleRoot: "InternalServices",
    controller: "ItemListService"
};

export default PagePicker;