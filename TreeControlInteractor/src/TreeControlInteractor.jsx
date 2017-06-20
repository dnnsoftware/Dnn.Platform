import React, { Component } from "react";
import { PropTypes } from "prop-types";
import TreeControl from "./TreeControl";
import { IconSelector } from "./icons/IconSelector";
import { Scrollbars } from 'react-custom-scrollbars';

import "./styles.less";

export default class TreeControlInteractor extends Component {

    constructor(props) {
        super();
        this.cachedChildTabs;
        this.icon = IconSelector("arrow_bullet");
        this.PortalTabsParameters = props.PortalTabsParameters;
        this.fullyChecked = 2;
        this.individuallyChecked = 1;
        this.unchecked = 0;
        this.scrollbar = {};
    }

    componentWillMount() {
        this.setState({ tabs: [] });
        this.init();
    }

    componentDidUpdate() {
        if (this.scrollbar.getClientWidth() > 200) {
            this.scrollbar.scrollToRight();
        }

    }

    init() {

        const ExportInitialSelection = () => {
            this.export();
        };

        this.props.getInitialPortalTabs(this.PortalTabsParameters, (response) => {
            const tabs = [response.Results];
            tabs[0].CheckedState = 2;
            this.setState({ tabs: tabs }, () => {
                ExportInitialSelection();
            });

        });

    }

    requestDescendantTabs(ParentTabId, callback) {

        let descendantTabs = [];
        const captureDecendants = (tabs) => descendantTabs = tabs.map(tab => {
            !Array.isArray(tab.ChildTabs) ? tab.ChildTabs = [] : null;
            return tab;
        });

        const input = (tabs) => compose(tabs, captureDecendants);
        const compose = (tabs, ...fns) => fns.forEach(fn => fn(tabs));
        const appendDescendants = (parentTab) => {

            descendantTabs.forEach((tab) => {
                const condition = parseInt(tab.ParentTabId) === parseInt(parentTab.TabId);
                condition ? parentTab.ChildTabs.push(tab) : null;
            });
        };

        this.props.getDescendantPortalTabs(this.PortalTabsParameters, ParentTabId, (response) => {

            const setCheckedState = (tab) => {
                const left = () => {
                    const select = () => {
                        tab.ChildTabs = tab.ChildTabs.map(child => {
                            child.CheckedState = child.HasChildren ? this.fullyChecked : this.individuallyChecked;
                            return child;
                        });
                    };

                    const unselect = () => {
                        tab.ChildTabs = tab.ChildTabs.map(child => {
                            child.CheckedState = this.unchecked;
                            return child;
                        });
                    };

                    tab.CheckedState ? select() : unselect();
                };

                const right = () => null;
                parseInt(tab.TabId) === parseInt(ParentTabId) ? left() : right();
            };

            const tabs = response.Results;
            input(tabs);
            this.traverse(appendDescendants);
            this.traverse(setCheckedState);

            this.setState({ tabs: this.state.tabs });
            callback();
        });
    }


    generateSelectionObject(tab) {
        return {
            "TabId": parseInt(tab.TabId),
            "ParentTabId": parseInt(tab.ParentTabId),
            "CheckedState": tab.CheckedState,
            "Name": tab.Name
        };
    }


    traverse(comparator) {
        const copy = this.state.tabs;
        let ChildTabs = copy;
        const cachedChildTabs = [];
        cachedChildTabs.push(ChildTabs);
        const condition = cachedChildTabs.length > 0;

        const loop = () => {
            const childTab = cachedChildTabs.length ? cachedChildTabs.shift() : null;
            const left = () => childTab.forEach(tab => {
                Array.isArray(tab.ChildTabs) ? comparator(tab, copy) : null;
                Array.isArray(tab.ChildTabs) && tab.ChildTabs.length ? cachedChildTabs.push(tab.ChildTabs) : null;
                condition ? loop() : exit();
            });
            const right = () => null;
            childTab ? left() : right();
        };

        const exit = () => null;
        loop();
        return;
    }

    findParent(tabData) {
        let parent = {};
        const capture = (tab) => {
            parent = tab || {};
        };
        const find = (tab) => {
            const condition = parseInt(tab.TabId) === parseInt(tabData.ParentTabId);
            condition ? capture(tab) : null;
        };
        this.traverse(find);
        return parent;
    }

    updateTree(tabData) {
        let newState = null;
        const capture = (tab, copy) => {
            tab = JSON.parse(JSON.stringify(tabData));
            newState = copy;
        };
        const find = (tab, copy) => {
            parseInt(tab.TabId) === parseInt(tabData.TabId) ? capture(tab, copy) : null;
        };
        this.traverse(find);
        this.setState({ tabs: newState });
        this.export(this.state.tabs);
    }

    reAlignTree() {

        let iterationsArray = [];
        const iterations = (t) => t.ChildTabs.length ? iterationsArray.push(...t.ChildTabs) : null;
        this.traverse(iterations);

        const realign = (tab) => {
            iterationsArray = [];
            let sum = 0;
            const tabsWithChildren = [];
            const tabsWithoutChildren = [];
            tab.ChildTabs.forEach((t) => {
                t.HasChildren ? tabsWithChildren.push(t) : tabsWithoutChildren.push(t);
            });
            const expect = tabsWithoutChildren.length + tabsWithChildren.length * this.fullyChecked;
            tabsWithoutChildren.forEach(t => {
                t.CheckedState === this.individuallyChecked ? sum += 1 : null;
            });

            tabsWithChildren.forEach(t => {
                switch (true) {
                    case t.CheckedState === this.fullyChecked:
                        sum += 2;
                        return;
                    case t.CheckedState === this.individuallyChecked:
                        sum += 1;
                        return;
                    default:
                        return;

                }
            });

            switch (true) {
                case sum === expect && tab.HasChildren:
                    tab.CheckedState = tab.CheckedState ? this.fullyChecked : tab.CheckedState;

                    break;
                case sum !== 0 && sum === expect && !tab.HasChildren:
                    tab.CheckedState = this.individuallyChecked;

                    break;
                case sum !== 0 && sum < expect:
                    tab.CheckedState = tab.CheckedState === this.fullyChecked ? this.individuallyChecked : tab.CheckedState;
                    break;
                default:
                    break;
            }

            this.updateTree(tab);
        };

        iterationsArray.forEach(() => this.traverse(realign));
    }


    export() {

        let tabs = [];
        let children = [];
        let parents = [];
        const filterOutZeros = (tabs) => tabs.filter(tab => !!tab.CheckedState);
        const filterOnlyParents = (tabs) => tabs.filter(tab => tab.CheckedState === this.fullyChecked);
        const filterOnlyChildren = (tabs) => tabs.filter(tab => tab.CheckedState === this.individuallyChecked);
        const filterOutIfAllSelected = (arr1, arr2) => arr1.filter(tab => {
            let exists = false;
            arr2.forEach(t => t.TabId === tab.ParentTabId ? exists = t : null);
            return exists === false;
        });

        const generateSelections = (tab) => tabs.push(this.generateSelectionObject(tab));
        this.traverse(generateSelections);
        tabs = filterOutZeros(tabs);
        parents = filterOnlyParents(tabs);
        children = filterOnlyChildren(tabs);
        children = filterOutIfAllSelected(children, parents);
        parents = filterOutIfAllSelected(parents, parents);
        const exports = parents.concat(children);
        this.props.OnSelect(exports);

    }


    render() {
        return (
            <Scrollbars
                autoHeight
                autoHeightMin={405}
                autoHeightMax={405}
                style={{ width: 340, maxWidth: "100%" }}
                ref={(scrollbar) => { this.scrollbar = scrollbar; }}
            >

                <TreeControl
                    characterLimit={this.props.characterLimit}
                    selectedColor={this.props.selectedColor}
                    icon_type="arrow_bullet"
                    tabs={this.state.tabs}
                    export={this.export}
                    PortalTabsParameters={this.PortalTabsParameters}
                    getDescendantPortalTabs={this.requestDescendantTabs.bind(this)}
                    fullyChecked={this.fullyChecked}
                    individuallyChecked={this.individuallyChecked}
                    unchecked={this.unchecked}
                    updateTree={this.updateTree.bind(this)}
                    reAlignTree={this.reAlignTree.bind(this)}
                    findParent={this.findParent.bind(this)}
                />
            </Scrollbars>
        );

    }
}

TreeControlInteractor.propTypes = {
    selectedColor: PropTypes.string.isRequired,
    characterLimit: PropTypes.number.isRequired,
    PortalTabsParameters: PropTypes.object.isRequired,
    OnSelect: PropTypes.func.isRequired,
    PortalTabParameters: PropTypes.object.isRequired,
    getInitialPortalTabs: PropTypes.func.isRequired,
    getDescendantPortalTabs: PropTypes.func.isRequired
};
