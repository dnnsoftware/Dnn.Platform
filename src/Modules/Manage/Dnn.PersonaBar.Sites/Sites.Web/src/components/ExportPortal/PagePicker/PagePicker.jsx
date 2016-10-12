import React, {PropTypes, Component} from "react";
import { connect } from "react-redux";
import Scrollbars from "react-custom-scrollbars";
import {ArrowDownIcon, ArrowRightIcon, CheckboxUncheckedIcon, CheckboxCheckedIcon, CheckboxPartialCheckedIcon} from "dnn-svg-icons";
import { portal as PortalActions } from "../../../actions";
import "./style.less";

class PagePicker extends Component {
    constructor(props) {
        super(props);
        this.state = {
            portalTabs: [],
            selectedPages: []
        };
        this.loaded = false;
    }
    componentWillMount() {
        this.initialize(this.props);
    }
    componentWillReceiveProps(newProps) {
        if (newProps.cultureCode !== this.props.cultureCode) {
            this.setState({ selectedPages: [] }, () => {
                this.initialize(newProps);
            });
        }
    }
    initialize(props) {
        let portalTabs = Object.assign([], JSON.parse(JSON.stringify(props.portalTabs)));
        this.setState({ portalTabs }, () => {
            this.setPages();
        });
    }
    getChildrenSelected(children) {
        let selectedCount = 0;
        for (let i = 0; i < children.children.length; i++) {
            if (children.children[i].CheckedState === "Checked") {
                selectedCount++;
            }
        }

        if (selectedCount === 0) {
            return "UnChecked";
        } else if (selectedCount > 0 && selectedCount !== children.children.length) {
            return "Partial";
        } else {
            return "Checked";
        }
    }

    setPages() {
        let {portalTabs} = this.state;
        if (portalTabs[0].ChildTabs !== undefined) {
            portalTabs[0].CheckedState = "UnChecked";
            portalTabs[0].isOpen = true;
            portalTabs[0].Processed = true;
            portalTabs[0].ParentTabId = undefined;
            portalTabs[0].ChildTabs = portalTabs[0].ChildTabs.map(tab => {
                tab.CheckedState = "UnChecked";
                tab.isOpen = false;
                tab.ChildTabs = tab.ChildTabs !== null ? tab.ChildTabs : [];
                return tab;
            });
        }
        this.setState({
            portalTabs
        });
        if (this.props.allSelected) {
            this.onPageSelect(portalTabs[0]);
        }
    }
    getClassName() {
        const {props, state} = this;
        let className = "dnn-page-picker-multi";

        className += (props.withBorder ? " with-border" : "");

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

    //page is passed by reference.
    getDescendants(page, callback, event) {
        if (event) {
            event.stopPropagation();
        }
        const { props } = this;

        let parentToAddChildrenTo = page;
        if (parentToAddChildrenTo.ChildTabs && parentToAddChildrenTo.ChildTabs.length > 0) {
            parentToAddChildrenTo.isOpen = !parentToAddChildrenTo.isOpen;

            //Add timeout to let render through - the dropdown will close on collapse of children if this is not here.
            setTimeout(() => {
                this.setState({});
            }, 0);
            return;
        }
        const portalTabsDescendantsParameters = {
            portalId: props.portalId,
            parentId: page.TabId,
            cultureCode: props.cultureCode,
            isMultiLanguage: props.isMultilanguage
        };
        props.dispatch(PortalActions.getTabsDescendants(portalTabsDescendantsParameters,
            (data) => {
                parentToAddChildrenTo.isOpen = true;
                parentToAddChildrenTo.ChildTabs = data.Results.map((child) => {
                    if (page.CheckedState) {
                        let selectedPages = Object.assign([], this.state.selectedPages);
                        selectedPages = this.UpdateSelectedData(child, page.CheckedState !== undefined ? page.CheckedState : "UnChecked", selectedPages);
                        this.setState({ selectedPages }, () => {
                            let {selectedPages} = this.state;
                            this.props.updateSelectionData(selectedPages);
                        });
                    }
                    return {
                        HasChildren: child.HasChildren,
                        ImageUrl: child.ImageUrl,
                        Name: child.Name,
                        ParentTabId: page.TabId,
                        TabId: child.TabId,
                        Tooltip: child.Tooltip,
                        CheckedState: page.CheckedState !== undefined ? page.CheckedState : "UnChecked",
                        ChildTabs: child.ChildTabs !== null ? child.ChildTabs : []
                    };
                });
                this.setState({}, () => { });
                if (callback) {
                    callback();
                }
            }));
    }

    onPageSelect(page) {
        page.CheckedState = page.CheckedState === "UnChecked" ? "Checked" : "UnChecked";
        let selectedPagesC = Object.assign([], this.state.selectedPages);
        selectedPagesC = this.CheckUnCheckAllChildren(page, page.CheckedState, selectedPagesC);
        this.setState({ selectedPages: selectedPagesC }, () => {
            this.DoRecursiveSelect(page, page.TabId, page.CheckedState, () => {
                let {selectedPages} = this.state;
                this.props.updateSelectionData(selectedPages);
            });
        });
    }
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
        //if ((checked === "Checked" || checked === "Partial") && selectedPages.indexOf(parseInt(TabId)) < 0) {
        if ((checked === "Checked" || checked === "Partial") && !selectedPages.some(page => page.TabId == Tab.TabId && page.CheckedState === checked)) {
            selectedPages = selectedPages.filter(page => {
                return page.TabId != Tab.TabId;
            });
            selectedPages = [{
                TabId: Tab.TabId,
                ParentTabId: Tab.ParentTabId !== undefined ? Tab.ParentTabId : -1,
                CheckedState: checked
            }].concat(selectedPages);

        } else if (checked === "UnChecked" && selectedPages.some(page => page.TabId == Tab.TabId)) {
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
            if (!page.ChildTabs.some(child => child.CheckedState === undefined || child.CheckedState === "UnChecked" || child.CheckedState === "Partial"))
                checked = "Checked";
            else if (page.ChildTabs.some(child => child.CheckedState !== undefined && child.CheckedState === "Checked" && child.CheckedState === "Partial"))
                checked = "UnChecked";
            else if (page.ChildTabs.some(child => child.CheckedState !== undefined && (child.CheckedState === "Partial" || child.CheckedState === "Checked")))
                checked = "Partial";
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
        if (currentTree.some(child => child.isOpen && child.TabId == parentId)) {
            parent = currentTree.find(child => child.isOpen && child.TabId == parentId);
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

    getSelected(page) {
        return "page-value" + (page.CheckedState ? " selected" : "");
    }

    getCheckboxIcon(selected) {
        switch (selected) {
            case "Checked":
                return CheckboxCheckedIcon;
            case "Partial":
                return CheckboxPartialCheckedIcon;
            case "UnChecked":
                return CheckboxUncheckedIcon;
            default:
                return CheckboxUncheckedIcon;
        }
    }

    /* eslint-disable react/no-danger */
    getChildItems(children) {
        if (!children) {
            return [];
        }
        return children.map((page) => {
            const checkboxIcon = this.getCheckboxIcon(page.CheckedState);
            return <li className={"page-item" + (page.HasChildren ? " has-children" : "") + (page.isOpen ? " opened" : " closed") }>
                {(!page.isOpen && page.HasChildren) && <div className="arrow-icon" dangerouslySetInnerHTML={{ __html: ArrowRightIcon }} onClick={this.getDescendants.bind(this, page, null) }></div>}
                {(page.isOpen && page.HasChildren) && <div className="arrow-icon" dangerouslySetInnerHTML={{ __html: ArrowDownIcon }} onClick={this.getDescendants.bind(this, page, null) }></div>}
                <div className={this.getSelected(page) } onClick={this.onPageSelect.bind(this, page) }
                    dangerouslySetInnerHTML={{ __html: (checkboxIcon + page.Name) }}></div>
                {page.ChildTabs !== null && page.ChildTabs.length > 0 &&
                    <ul className={"child-pages" + (page.isOpen ? " opened" : "") }>
                        {this.getChildItems(page.ChildTabs) }
                    </ul>
                }
            </li>;
        });
        //}
    }

    /* eslint-disable react/no-danger */
    render() {
        const {props, state} = this;
        if (state.portalTabs.length > 0 && state.portalTabs[0].Processed) {
            const children = state.portalTabs.length > 0 && state.portalTabs[0].Processed && this.getChildItems(state.portalTabs);
            return (
                <div className={this.getClassName() } style={props.style} ref="dnnPagePicker">
                    <div className="page-picker-content">
                        <Scrollbars style={props.scrollAreaStyle}>
                            <div className="pages-container">
                                <ul>
                                    {state.portalTabs.length > 0 && state.portalTabs[0].Processed && children}
                                    {state.portalTabs.length <= 0 || !state.portalTabs[0].Processed && props.noneSpecified}
                                </ul>
                            </div>
                        </Scrollbars>
                    </div>
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
    //Selected page - format should be page names separated by //. 
    //Example: //Activity Feed//My Profile, My Profile will be the selected page.
    selectedPage: PropTypes.string,

    //React Collapse prop - set to false if you want to re-render the items every time.
    keepCollapsedContent: PropTypes.bool,

    //Class name to attach to parent
    className: PropTypes.string,

    //Style of scrollarea - page picker box, override if necessary.
    scrollAreaStyle: PropTypes.object,

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

    portalTabs: PropTypes.array,

    portalId: PropTypes.number,

    cultureCode: PropTypes.string,

    isMultiLanguage: PropTypes.bool,

    updateSelectionData: PropTypes.func.isRequired,

    allSelected: PropTypes.bool
};

PagePicker.defaultProps = {
    portalTabs: [],
    noneSpecified: {
        data: {
            key: "-1",
            value: "<None Specified>"
        }
    },
    selectedPage: "//< None Specified >",
    withIcon: true,
    withBorder: true,
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
    }
};

function mapStateToProps(state) {
    return {
    };
}

export default connect(mapStateToProps)(PagePicker);