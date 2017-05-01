import React, { Component } from "react";
import { PropTypes } from "prop-types";
import { TreeControl } from "./_tree-control";
import { TreeControlDataManager } from "./helpers";
import { IconSelector } from "./icons";
import { global } from "./_global";

import { PortalTabs } from "./mocks";

const styles = global.styles;
const floatLeft = styles.float();
const merge = styles.merge;


import "./styles.less";

export class TreeControlInteractor extends Component {

    constructor(props) {
        super();
        this.cached_ChildTabs;
        this.icon = IconSelector("arrow_bullet");
        this.PortalTabsParameters = props.PortalTabsParameters;

        this.ExportModalOnSelect = props.OnSelect;

        this.fullyChecked = 2;
        this.individuallyChecked = 1;
        this.unchecked = 0;

    }

    componentWillMount() {
        this.setState({ tabs: PortalTabs });
        this.init();
    }

    init() {

        const ExportInitialSelection = () => {
            const selection = this._generateSelectionObject(this.state.tabs);
            this.ExportModalOnSelect(selection);
        };

        this.props.getInitialPortalTabs(this.PortalTabsParameters, (response) => {
            const tabs = [response.Results];
            this.setState({ tabs: tabs });
        });

    }



    requestDescendantTabs = (ParentTabId, callback) => {

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


    traverse = (comparator) => {
        const copy = this.state.tabs;
        let ChildTabs = copy;
        const cached_childtabs = [];
        cached_childtabs.push(ChildTabs);
        const condition = cached_childtabs.length > 0;

        const loop = () => {
            const childtab = cached_childtabs.length ? cached_childtabs.shift() : null;
            const left = () => childtab.forEach(tab => {
                Array.isArray(tab.ChildTabs) ? comparator(tab, copy) : null;
                Array.isArray(tab.ChildTabs) && tab.ChildTabs.length ? cached_childtabs.push(tab.ChildTabs) : null;
                condition ? loop() : exit();
            });
            const right = () => null;
            childtab ? left() : right();
        };

        const exit = () => null;
        loop();
        return;
    }

    findParent = (tabdata) => {
        let parent = {}
        const capture = (tab) => {
            parent = tab || {}
        }
        const find = (tab, copy) => {
            const condition = parseInt(tab.TabId) === parseInt(tabdata.ParentTabId);
            condition ? capture(tab) : null;
        };
        this.traverse(find);
        return parent;
    }

    updateTree = (tabdata) => {
        let updateTab = null;
        let newState = null;
        const capture = (tab, copy) => {
            tab = JSON.parse(JSON.stringify(tabdata));
            newState = copy;
        };
        const find = (tab, copy) => {
            parseInt(tab.TabId) === parseInt(tabdata.TabId) ? capture(tab, copy) : null;
        };
        this.traverse(find);
        this.setState({ tabs: newState });
        this.export(this.state.tabs);
    }

    reAlignTree = () => {

        let iterationsArray = []
        const iterations = (t) => t.ChildTabs.length ? iterationsArray.push(...t.ChildTabs) : null
        this.traverse(iterations)

        const realign = (tab) => {
            iterationsArray = [];
            let sum = 0;
            let newState = null;
            const tabsWithChildren = [];
            const tabsWithoutChildren = [];
            const ChildTabs = tab.ChildTabs;

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

            sum = sum

            switch (true) {
                case sum === expect && tab.HasChildren:
                    tab.CheckedState = tab.CheckedState === this.individuallyChecked ? this.fullyChecked : tab.CheckedState;

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

        iterationsArray.forEach(iter => this.traverse(realign));
    }


    export = (selection) => {

        let tabs = []
        let children = []
        let parents = []
        const filterOutZeros = (tabs) => tabs.filter(tab => !!tab.CheckedState);
        const filterOnlyParents = (tabs) => tabs.filter(tab => tab.CheckedState === this.fullyChecked);
        const filterOnlyChildren = (tabs) => tabs.filter(tab => tab.CheckedState === this.individuallyChecked);

        const filterOutIfAllSelected = (children) => children.filter(tab => {
            let exists = false;
            parents.forEach(t => t.TabId === tab.ParentTabId ? exists = t : null);
            return exists === false;
        });



        const generateSelections = (tab) => tabs.push(this.generateSelectionObject(tab));
        this.traverse(generateSelections);
        tabs = filterOutZeros(tabs);
        parents = filterOnlyParents(tabs);
        children = filterOnlyChildren(tabs);
        children = filterOutIfAllSelected(children);

        const exports = parents.concat(children)

        console.log(exports);


    }


    render() {
        return (
            <TreeControl
                icon_type="arrow_bullet"
                tabs={this.state.tabs}
                export={this.export}
                PortalTabsParameters={this.PortalTabsParameters}
                getDescendantPortalTabs={this.requestDescendantTabs}
                fullyChecked={this.fullyChecked}
                individuallyChecked={this.individuallyChecked}
                unchecked={this.unchecked}
                updateTree={this.updateTree}
                reAlignTree={this.reAlignTree}
                findParent={this.findParent}

            />
        );

    }
}

TreeControlInteractor.propTypes = {
    PortalTabsParameters: PropTypes.object.isRequired,
    serviceFramework: PropTypes.object.isRequired,
    controller: PropTypes.string.isRequired,
    moduleRoot: PropTypes.string.isrequired,
    OnSelect: PropTypes.func.isRequired,
    PortalTabParamters: PropTypes.object.isRequired,
    getInitialPortalTabs: PropTypes.func.isRequired,
    getDescendantPortalTabs: PropTypes.func.isRequired
};
