import React, {PropTypes, Component} from "react";
import ReactDOM from "react-dom";
import Collapse from "react-collapse";
import Scrollbars from "react-custom-scrollbars";
import SearchBox from "dnn-search-box";
import {ArrowDownIcon, ArrowRightIcon, CheckboxUncheckedIcon, CheckboxCheckedIcon, CheckboxPartialCheckedIcon, PagesIcon} from "dnn-svg-icons";
import "./style.less";
import Service from "./Service";
function format() {
    let format = arguments[0];
    let methodsArgs = arguments;
    return format.replace(/{(\d+)}/gi, function (value, index) {
        let argsIndex = parseInt(index) + 1;
        return methodsArgs[argsIndex];
    });
}
class PagePicker extends Component {
    constructor(props) {
        super(props);
        this.state = {
            dropDownOpen: false,
            portalTabs: [],
            selectedPages: [],
            totalCount: 0,
            selectedPage: props.defaultLabel
        };
        this.loaded = false;
        this.handleClick = this.handleClick.bind(this);
    }
    componentWillMount() {
        this._isMounted = false;
        if (!this.props.IsMultiSelect) {
            this.setDefaultPage(this.props);
        }
        if (!this.props.IsInDropDown) {
            this.initialize(this.props);
        }
        this.setState({
            selectedPage: this.props.defaultLabel
        });
    }

    componentWillReceiveProps(newProps) {
        if (newProps.Reload && !newProps.IsInDropDown && this._isMounted) {
            if (!newProps.IsMultiSelect) {
                this.setDefaultPage(newProps);
            }
            this.setState({ selectedPages: [], dropDownOpen: false, portalTabs: [] }, () => {
                this.initialize(newProps);
            });
        }
        else if (newProps.ResetSelected && !this.props.IsMultiSelect && newProps.IsInDropDown && this._isMounted) {
            this.setDefaultPage(newProps);
        }
    }

    componentDidMount() {
        this._isMounted = true;
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

    initialize(props, callback) {
        this.getPortalTabs(props.cultureCode, () => {
            this.setPages(() => {
                if (callback)
                    callback();
            });
        });
    }

    getPortalTabs(cultureCode, callback) {
        const { props } = this;
        let service = new Service(this.props.serviceFramework, this.props.moduleRoot, this.props.controller);
        const portalTabsParameters = !props.IsMultiSelect ? Object.assign(props.PortalTabsParameters, { selectedTabId: props.selectedTabId }) : Object.assign(props.PortalTabsParameters);
        service.getPortalTabs(portalTabsParameters, (data) => {
            this.setState({
                portalTabs: [data.Results]
            }, () => {
                if (callback) {
                    callback();
                }
            });
        });
    }

    setPages(callback) {
        const {props} = this;
        this.loaded = true;
        let {portalTabs} = this.state;
        if (!props.IsMultiSelect) {
            portalTabs[0].Name = props.noneSpecifiedText;
        }
        if (portalTabs[0].ChildTabs !== undefined) {
            portalTabs[0].IsOpen = true;
            portalTabs[0].Processed = true;
            portalTabs[0].ParentTabId = undefined;
            portalTabs[0].ChildTabs = portalTabs[0].ChildTabs.map(tab => {
                tab.ChildTabs = tab.ChildTabs !== null ? tab.ChildTabs : [];
                return tab;
            });
        }
        this.setState({
            portalTabs
        }, () => {
            this.SetOpenNodesCount(this.state.portalTabs[0], this.state.portalTabs[0].TabId, 0);
            if (props.IsMultiSelect && props.allSelected) {
                this.onPageSelect(portalTabs[0]);
            }
            if (typeof callback === "function")
                callback();
        });
    }

    getClassName() {
        const {props} = this;
        let className = "dnn-page-picker";
        if (props.IsInDropDown) {
            className += (props.withBorder ? " with-border" : "");
        }

        className += (" " + props.className);

        if (!props.enabled) {
            className += " disabled";
        }
        return className;
    }

    getDropdownLabel() {
        const { state } = this;
        let label = state.selectedPage;
        return label;
    }

    SetOpenNodesCount(root, lastId, count) {
        if (root.IsOpen && root.ChildTabs !== null && root.ChildTabs.length > 0) {
            root.ChildTabs.map((child) => {
                count++;
                if (child.IsOpen && child.ChildTabs !== null && child.ChildTabs.length > 0) {
                    count = this.SetOpenNodesCount(child, lastId, count);
                }
                return count;
            });
        }
        if (root.TabId === lastId) {
            this.setState({
                totalCount: count
            });
        }
        return count;
    }

    onPageSelect(page, keepOpen) {
        keepOpen = keepOpen === undefined || (typeof keepOpen) !== "boolean" ? false : keepOpen;
        const {props} = this;
        if (props.IsMultiSelect) {
            page.CheckedState = page.CheckedState === 1 ? 0 : 1;
            let selectedPagesC = Object.assign([], this.state.selectedPages);
            selectedPagesC = this.CheckUnCheckAllChildren(page, page.CheckedState, selectedPagesC);
            this.setState({ selectedPages: selectedPagesC }, () => {
                this.DoRecursiveSelect(page, page.TabId, page.CheckedState, () => {
                    let {selectedPages} = this.state;
                    this.props.OnSelect(selectedPages);
                });
            });
        } else {
            let {portalTabs} = this.state;
            this.UnCheckAllExcept(Object.assign([], portalTabs[0]), page, (data) => {
                if (props.selectedTabId === -1) {
                    data.CheckedState = 0;
                } else {
                    data.CheckedState = 1;
                }
                portalTabs[0] = data;
                page.CheckedState = 0;
                this.setState({ portalTabs }, () => {
                    if (page.TabId !== props.selectedTabId) {
                        this.props.OnSelect(page.TabId, page.Name);
                    }
                    if (props.IsInDropDown) {
                        this.setState({
                            selectedPage: page.Name,
                            dropDownOpen: keepOpen
                        });
                    }
                });
            });
        }
    }
    //page is passed by reference.
    getDescendants(page, callback, event) {
        if (event) {
            event.stopPropagation();
        }
        const { props } = this;
        let parentToAddChildrenTo = page;
        if (parentToAddChildrenTo.ChildTabs && parentToAddChildrenTo.ChildTabs.length > 0) {
            parentToAddChildrenTo.IsOpen = !parentToAddChildrenTo.IsOpen;

            //Add timeout to let render through - the dropdown will close on collapse of children if this is not here.
            setTimeout(() => {
                this.setState({}, () => {
                    this.SetOpenNodesCount(this.state.portalTabs[0], this.state.portalTabs[0].TabId, 0);
                });
            }, 0);

            return;
        }
        const portalTabsDescendantsParameters = Object.assign(props.PortalTabsParameters, { parentId: page.TabId });

        let service = new Service(this.props.serviceFramework, this.props.moduleRoot, this.props.controller);
        service.getTabsDescendants(portalTabsDescendantsParameters,
            (data) => {
                parentToAddChildrenTo.IsOpen = true;
                parentToAddChildrenTo.ChildTabs = data.Results.map((child) => {
                    if (page.CheckedState === 0) {
                        let selectedPages = Object.assign([], this.state.selectedPages);
                        selectedPages = this.UpdateSelectedData(child, page.CheckedState !== undefined ? page.CheckedState : 1, selectedPages);
                        if (props.IsMultiSelect && page.CheckedState === 0) {
                            this.setState({ selectedPages }, () => {
                                let {selectedPages} = this.state;
                                this.props.OnSelect(selectedPages);
                            });
                        }
                    }
                    return {
                        HasChildren: child.HasChildren,
                        ImageUrl: child.ImageUrl,
                        Name: child.Name,
                        ParentTabId: page.TabId,
                        TabId: child.TabId,
                        Tooltip: child.Tooltip,
                        CheckedState: this.props.IsMultiSelect ? (page.CheckedState !== undefined ? page.CheckedState : 1) : 1,
                        ChildTabs: child.ChildTabs !== null ? child.ChildTabs : [],
                        Selectable: child.Selectable
                    };
                });
                this.setState({}, () => {
                    this.SetOpenNodesCount(this.state.portalTabs[0], this.state.portalTabs[0].TabId, 0);
                });
                if (callback) {
                    callback();
                }
            });
    }

    getSelected(page) {
        let className = "page-value";
        if (page.Selectable)
            className += (page.CheckedState !== 1 ? " selected" : "");
        else
            className += " non-selectable";
        return className;
    }

    /* eslint-disable react/no-danger */
    getChildItems(children) {
        if (!children) {
            return [];
        }
        return children.map((page) => {
            const {props} = this;
            const checkboxIcon = this.getCheckboxIcon(page.CheckedState);
            const pageIcon = PagesIcon;
            const textClass = props.IsMultiSelect && props.ShowIcon ? " text-with-page-icon" : (!props.IsMultiSelect && !props.ShowIcon ? "no-icon" : "");
            const pageClass = props.IsMultiSelect && props.ShowIcon ? " page-icon" : "";
            return <li className={"page-item" + (page.HasChildren ? " has-children" : "") + (page.IsOpen ? " opened" : " closed") }>
                {(!page.IsOpen && page.HasChildren) && <div className="arrow-icon" dangerouslySetInnerHTML={{ __html: ArrowRightIcon }} onClick={this.getDescendants.bind(this, page, null) }></div>}
                {(page.IsOpen && page.HasChildren) && <div className="arrow-icon" dangerouslySetInnerHTML={{ __html: ArrowDownIcon }} onClick={this.getDescendants.bind(this, page, null) }></div>}
                <div className={this.getSelected(page) } onClick={page.Selectable ? this.onPageSelect.bind(this, page) : void (0) }>
                    { props.ShowIcon &&
                        <div className={pageClass } dangerouslySetInnerHTML={{ __html: pageIcon }}></div>
                    }
                    {props.IsMultiSelect && <div  dangerouslySetInnerHTML={{ __html: checkboxIcon }}></div>}
                    <div className={textClass }>{page.Name}</div>
                </div>
                {page.ChildTabs !== null && page.ChildTabs.length > 0 &&
                    <ul className={"child-pages" + (page.IsOpen ? " opened" : "") }>
                        {this.getChildItems(page.ChildTabs) }
                    </ul>
                }
            </li>;
        });
    }

    toggleDropdown() {
        const {props} = this;
        if (!this.loaded) {
            this.initialize(props, () => {
                if (props.selectedTabId === -1) {
                    this.setDefaultPage(props, true);
                }
            });
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

    getCountText() {
        return format(this.props.CountText, this.state.totalCount);
    }

    findTabById(tabId, tabs, tab) {
        if (tab === null) {
            for (let i = 0; i < tabs.length; i++) {
                let child = tabs[i];
                if (tab === null) {
                    if (child.TabId == tabId) {
                        tab = child;
                    }
                    else if (child.HasChildren && child.ChildTabs !== null && child.ChildTabs.length > 0) {
                        tab = this.findTabById(tabId, child.ChildTabs, tab);
                    }
                } else {
                    break;
                }
            }
            // tabs.forEach(child => {
            //     if (tab === null) {
            //         if (child.TabId == tabId) {
            //             tab = child;
            //         }
            //         else if (child.HasChildren && child.ChildTabs !== null && child.ChildTabs.length > 0) {
            //             tab = this.findTabById(tabId, child.ChildTabs, tab);
            //         }
            //     }
            // });
        }
        return tab;
    }
    //Single Selection Methods
    //Set the dropdown label to the default selected page name.
    setDefaultPage(props, keepOpen) {
        if (props.IsInDropDown && !props.IsMultiSelect) {
            let {portalTabs} = this.state;
            if (portalTabs !== undefined && portalTabs.length > 0) {
                let page = null;
                if (props.selectedTabId === -1) {
                    page = portalTabs[0];
                } else {
                    page = this.findTabById(props.selectedTabId, portalTabs[0].ChildTabs, null);
                }
                if (page !== undefined && page !== null) {
                    this.onPageSelect(page, keepOpen);
                }
            }
            else if (props.selectedTabId >= 0) {
                let service = new Service(this.props.serviceFramework, this.props.moduleRoot, this.props.controller);
                service.getPortalTab({
                    portalId: props.PortalTabsParameters.portalId,
                    tabId: props.selectedTabId,
                    cultureCode: props.PortalTabsParameters.cultureCode
                }, (tab) => {
                    let {selectedPage} = this.state;
                    selectedPage = tab.Results.Name;
                    this.setState({ selectedPage });
                });
            }
        }
    }

    handleClick(event) {
        const {props} = this;
        // Note: this workaround is needed in IE. The remove event listener in the componentWillUnmount is called
        // before the handleClick handler is called, but in spite of that, the handleClick is executed. To avoid
        // the "findDOMNode was called on an unmounted component." error we need to check if the component is mounted before execute this code
        if (!this._isMounted || !props.closeOnBlur || !props.IsInDropDown) { return; }
        if (!ReactDOM.findDOMNode(this).contains(event.target) && (typeof event.target.className === "string" && event.target.className.indexOf("do-not-close") === -1)) {
            this.setState({
                dropDownOpen: false
            });
        }
    }

    UnCheckAllExcept(root, page, callback) {
        if (page.CheckedState !== 0) {
            if (root.HasChildren && root.ChildTabs !== null && root.ChildTabs.length > 0) {
                root.ChildTabs = root.ChildTabs.map(child => {
                    if (child.TabId !== page.TabId) {
                        child.CheckedState = 1;
                    }
                    if (child.HasChildren && child.ChildTabs !== null && child.ChildTabs.length > 0) {
                        child = this.UnCheckAllExcept(child, page);
                    }
                    return child;
                });
            }
            if (callback !== undefined) {
                callback(root);
            }
        }
        if (callback !== undefined) {
            callback(root);
        }
        return root;
    }

    onSearch(value) {
        const {props} = this;

        if (value !== "") {
            const apiParameters = Object.assign(props.PortalTabsParameters, { searchText: value });
            let service = new Service(this.props.serviceFramework, this.props.moduleRoot, this.props.controller);

            service.searchPortalTabs(apiParameters, (data) => {
                this.setState({
                    portalTabs: [data.Results]
                }, () => {
                    this.setPages();
                });
            });
        } else {
            this.initialize(props);
        }
    }
    //Single Selection Methods Ends

    //Multi Selection Methods
    CheckUnCheckAllChildren(root, checked, selectedPages) {
        selectedPages = this.UpdateSelectedData(root, checked, selectedPages);
        root.CheckedState = checked;
        this.setState({});
        if (root.HasChildren && root.ChildTabs.length > 0) {
            root.ChildTabs.forEach(child => {
                selectedPages = this.CheckUnCheckAllChildren(child, checked, selectedPages);
            });
        }
        return selectedPages;
    }

    UpdateSelectedData(Tab, checked, selectedPages) {
        //if ((checked === 0 || checked === 2) && selectedPages.indexOf(parseInt(TabId)) < 0) {
        if ((checked === 0 || checked === 2) && !selectedPages.some(page => page.TabId == Tab.TabId && page.CheckedState === checked)) {
            selectedPages = selectedPages.filter(page => {
                return page.TabId != Tab.TabId;
            });
            selectedPages = [{
                TabId: Tab.TabId,
                ParentTabId: Tab.ParentTabId !== undefined ? Tab.ParentTabId : -1,
                CheckedState: checked
            }].concat(selectedPages);

        } else if (checked === 1 && selectedPages.some(page => page.TabId == Tab.TabId)) {
            selectedPages = selectedPages.filter(page => {
                return page.TabId != Tab.TabId;
            });
        }
        return selectedPages;
    }

    DoRecursiveSelect(page, mainKey, originalChecked, callback) {
        let {portalTabs} = this.state;
        let checked = originalChecked;
        if (mainKey != page.TabId && page.HasChildren && page.ChildTabs.length > 0) {
            if (!page.ChildTabs.some(child => child.CheckedState === undefined || child.CheckedState === 1 || child.CheckedState === 2))
                checked = 0;
            else if (page.ChildTabs.some(child => child.CheckedState !== undefined && child.CheckedState === 0 && child.CheckedState === 2))
                checked = 1;
            else if (page.ChildTabs.some(child => child.CheckedState !== undefined && (child.CheckedState === 2 || child.CheckedState === 0)))
                checked = 2;
        }
        let selectedPages = Object.assign([], this.state.selectedPages);
        selectedPages = this.UpdateSelectedData(page, checked, selectedPages);
        this.setState({ selectedPages }, () => {
            page.CheckedState = checked;
            this.setState({});
            let parentId = page.ParentTabId;
            if (parentId !== undefined) {
                let children = portalTabs;
                let parent = this.RecursiveFindParent(parentId, children);
                if (parent !== undefined && parent !== null) {
                    this.setState({});
                    if (parent.HasChildren && parent.ChildTabs.length > 0) {
                        this.DoRecursiveSelect(parent, mainKey, originalChecked, callback);
                    } else {
                        return;
                    }
                }
            } else {
                let selectedPages = Object.assign([], this.state.selectedPages);
                selectedPages = this.UpdateSelectedData(this.state.portalTabs[0], checked, selectedPages);
                this.setState({ selectedPages });
                if (callback)
                    callback();
            }
        });
        if (callback)
            callback();
    }

    RecursiveFindParent(parentId, currentTree) {
        let parent = null;
        if (currentTree.some(child => child.IsOpen && child.TabId == parentId)) {
            parent = currentTree.find(child => child.IsOpen && child.TabId == parentId);
        } else if (parent === undefined || parent === null) {
            for (let child of currentTree) {
                if (child.HasChildren && child.ChildTabs.length > 0) {
                    parent = this.RecursiveFindParent(parentId, child.ChildTabs);
                    if (parent !== undefined && parent !== null) {
                        break;
                    }
                }
            }
        }
        return parent;
    }

    getCheckboxIcon(selected) {
        switch (selected) {
            case 0:
                return CheckboxCheckedIcon;
            case 2:
                return CheckboxPartialCheckedIcon;
            case 1:
                return CheckboxUncheckedIcon;
            default:
                return CheckboxUncheckedIcon;
        }
    }

    //Multi Selection Methods Ends
    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        // const children = state.searchMode ? Object.keys(state.searchPageData).length > 0 &&
        //     this.getChildItems(state.searchPageData.Tree.children) : Object.keys(state.pageData).length > 0 &&
        //     this.getChildItems(state.pageData.Tree.children);
        if (this.loaded || props.IsInDropDown) {
            let picker = <div/>;
            if (state.portalTabs.length > 0 && state.portalTabs[0].Processed) {
                let children = state.portalTabs.length > 0 && state.portalTabs[0].Processed && this.getChildItems(state.portalTabs);
                picker =
                    <Scrollbars style={props.scrollAreaStyle}>
                        <div className="pages-container">
                            <ul>
                                {state.portalTabs.length > 0 && state.portalTabs[0].Processed && children}
                                {state.portalTabs.length <= 0 || !state.portalTabs[0].Processed && props.noneSpecified}
                            </ul>
                        </div>
                    </Scrollbars>;
            }
            return (
                <div className={this.getClassName() } style={props.style} ref="dnnPagePicker">
                    {
                        props.IsInDropDown &&
                        <div className="collapsible-label" onClick={this.toggleDropdown.bind(this) }>
                            {this.getDropdownLabel() }
                        </div>
                    }
                    {props.IsInDropDown && props.withIcon &&
                        <div className="dropdown-icon" dangerouslySetInnerHTML={{ __html: ArrowDownIcon }} onClick={this.toggleDropdown.bind(this) }></div>
                    }
                    {props.IsInDropDown &&
                        <div className={"collapsible-content" + (state.dropDownOpen ? " open" : "") } style={props.style}>
                            <Collapse
                                className="page-picker-content"
                                ref="pagePickerContent"
                                keepCollapsedContent={props.keepCollapsedContent}
                                isOpened={state.dropDownOpen}>
                                {!props.IsMultiSelect && props.SearchEnabled &&
                                    <SearchBox
                                        style={Object.assign({ display: "block" }, props.searchBoxStyle) }
                                        placeholder={props.placeholderText}
                                        className="page-picker-search"
                                        onSearch={this.onSearch.bind(this) }
                                        iconStyle={Object.assign({ right: 4 }, props.iconStyle) }
                                        />
                                }
                                {picker}
                                {props.ShowCount && <div className="count" dangerouslySetInnerHTML={{ __html: this.getCountText() }}></div>}
                            </Collapse>
                        </div>
                    }
                    {
                        !props.IsInDropDown && <div className="page-picker-content">{ picker }
                            {props.ShowCount && <div className="count" dangerouslySetInnerHTML={{ __html: this.getCountText() }}></div>}
                        </div>
                    }

                </div>
            );
        }
        else {
            return (<div/>);
        }
    }
}

PagePicker.propTypes = {
    dispatch: PropTypes.func.isRequired,

    //React Collapse prop - set to false if you want to re-render the items every time.
    keepCollapsedContent: PropTypes.bool,

    //Class name to attach to parent
    className: PropTypes.string,

    //Style of scrollarea - page picker box, override if necessary.
    scrollAreaStyle: PropTypes.object,

    //Whether or not the dropdown toggle has a border.
    withBorder: PropTypes.bool,

    //Show the dropdown icon
    withIcon: PropTypes.bool,

    //Set to false to disable the picker (readonly)
    enabled: PropTypes.bool,

    //Close the picker on click outside.
    closeOnBlur: PropTypes.bool,

    //Placeholder text in searchbox.
    placeholderText: PropTypes.string,

    //None specified Text- default if none is selected
    noneSpecifiedText: PropTypes.string,

    //Default label shown before pinging API.
    defaultLabel: PropTypes.string,

    //callback method on selection of a tab. 
    //In case of "Single" selection mode it will send back currently selected tabid and tabname.
    //In case of "Multi" selection mode it will send back array currently selected tabs. 
    OnSelect: PropTypes.func.isRequired,

    //Send as true if you all the tabs to be pre selected.  This will work only in case of IsMultiSelect=true
    allSelected: PropTypes.bool,

    //The parameters to list the portal tabs. Default is : {portalId: 0,cultureCode: "",isMultiLanguage: false,excludeAdminTabs: false,disabledNotSelectable: false,roles: "1;-1",sortOrder: 0}
    PortalTabsParameters: PropTypes.object,

    //Id of the tab which should be selected by default. Default is -1 
    //This will work only in case of IsMultiSelect=false.
    selectedTabId: PropTypes.number,

    //Tells the component to reload. Use this if circumstances have changed at UI end and need the page picker to reload with new props. e.g. cultureCode code changed.
    Reload: PropTypes.bool,

    //Reset the selected page to the props one.  This will work only in case of IsMultiSelect=false and IsInDropDown=true
    ResetSelected: PropTypes.bool,

    //Selection mode of the tree. Default is "Single"
    IsMultiSelect: PropTypes.bool,

    //Default: true, Shows the tree in the dropdown mode.
    IsInDropDown: PropTypes.bool,

    ShowIcon: PropTypes.bool,

    //Default: true, Enables the search of the tabs. This will work only in case of IsMultiSelect=false
    SearchEnabled: PropTypes.bool,

    //If true, show the count of total tabs in the tree.
    ShowCount: PropTypes.bool,

    //The text to show in the count. Default is "<strong>{0}</strong> Results". Please include {0} at the place you want to show the count.
    CountText: PropTypes.string,

    //Service Framework module root
    moduleRoot: PropTypes.string,

    //Service Framework controller
    controller: PropTypes.string,

    serviceFramework: PropTypes.object
};

PagePicker.defaultProps = {
    defaultLabel: "-- Select --",
    IsMultiSelect: false,
    SearchEnabled: true,
    ShowCount: true,
    CountText: "<strong>{0}</strong> Results",
    ShowIcon: true,
    IsInDropDown: true,
    Reload: false,
    ResetSelected: false,
    noneSpecifiedText: "< None Specified >",
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
        minHeight: 300,
        marginTop: 0,
        border: "1px solid #c8c8c8"
    },
    moduleRoot: "PersonaBar",
    controller: "Tabs",
    PortalTabsParameters: {
        portalId: -2,
        cultureCode: "",
        isMultiLanguage: false,
        excludeAdminTabs: false,
        disabledNotSelectable: false,
        roles: "1;-1",
        sortOrder: 0
    },
    selectedTabId: -1
};


export default PagePicker;