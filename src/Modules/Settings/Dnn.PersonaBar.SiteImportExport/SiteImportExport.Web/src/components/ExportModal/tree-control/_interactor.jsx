import React, { Component } from "react";
import { PropTypes } from "prop-types";
import { TreeControl } from "./_tree-control";
import { TreeControlDataManager } from "./helpers";
import { IconSelector } from "./icons";
import { global } from "./_global";
const styles = global.styles;

const floatLeft = styles.float();
const merge = styles.merge;
const tcdm = new TreeControlDataManager();

import "./styles.less";

export class TreeControlInteractor extends Component {

    constructor(props) {
        super();
        this.cached_ChildTabs;
        this.icon = IconSelector("arrow_bullet");
        this.PortalTabsParameters = props.PortalTabsParameters;

        this.ExportModalOnSelect = props.OnSelect;
        this.copy = {};

        this.fully_checked = 2;
        this.individually_checked = 1;
        this.unchecked = 0;

    }

    componentWillMount() {
        this.setState({ tabs: [], childrenSelected: true });
        this.init();
    }

    init() {

        const ExportInitialSelection = () => {
            const selection = this._generateSelectionObject(this.state.tabs);
            this.ExportModalOnSelect(selection);
        };

        this.props.getInitialPortalTabs(this.PortalTabsParameters, (response) => {
            this.PortalTabs = response.Results;
            this.flatTabs = tcdm.flatten(this.PortalTabs);
            this.copy = JSON.parse(JSON.stringify(this.PortalTabs));
            this.setState({ tabs: this.PortalTabs, flatTabs: this.flatTabs });
            ExportInitialSelection();
        });

    }

    _requestDescendantTabs = (ParentTabId, callback) => {

        let descendantTabs = [];
        const mapToFlatTabs = (tabs) => tabs.map(tab => this.flatTabs[`${tab.TabId}-${tab.Name}`] = tab);
        const captureDecendants = (tabs) => descendantTabs = tabs.map(tab => {
            !Array.isArray(tab.ChildTabs) ? tab.ChildTabs = [] : null;
            return tab;
        });

        const input = (tabs) => compose(tabs, mapToFlatTabs, captureDecendants);
        const compose = (tabs, ...fns) => fns.forEach(fn => fn(tabs));
        const appendDescendants = (parentTab) => descendantTabs.forEach((tab) => parseInt(tab.ParentTabId) === parseInt(parentTab.TabId) ? parentTab.ChildTabs.push(tab) : null);

        this.props.getDescendantPortalTabs(this.PortalTabsParameters, ParentTabId, (response) => {
            const setCheckedState = (tab) => {
                const left = () => {
                    const select = () => {
                        tab.ChildTabs = tab.ChildTabs.map(child => {
                            child.CheckedState = child.HasChildren ? this.fully_checked : this.individually_checked;
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
            this._traverseChildTabs(appendDescendants);
            this._traverseChildTabs(setCheckedState);

            this.setState({ tabs: this.state.tabs });
            callback();
        });
    }


    _getRootTab(selection) {
        return Object.keys(selection)
            .map(key => selection[key])
            .filter(tab => tab.TabId === -1 && tab.ParentTabId === 0);
    }

    _generateSelectionObject(tab) {
        return {
            "TabId": tab.TabId,
            "ParentTabId": tab.ParentTabId,
            "CheckedState": tab.CheckedState,
            "Name": tab.Name
        };
    }


    _filterOutUnchecked(tabs) {
        return tabs.filter(tab => !!tab.CheckedState);
    }

    _filterChildrenOfAllSelected(tabs) {
        const ParentTabIds = tabs.filter(tab => tab.CheckedState === this.fully_checked).map(tab => tab.TabId);
        return tabs.filter(tab => ParentTabIds.indexOf(tab.ParentTabId) === -1);
    }

    _traverseChildTabs(comparator) {
        let ChildTabs = this.state.tabs.ChildTabs;
        const cached_childtabs = [];
        cached_childtabs.push(ChildTabs);
        const condition = (cached_childtabs.length > 0);
        const loop = () => {
            const childtab = cached_childtabs.length ? cached_childtabs.shift() : null;
            const left = () => childtab.forEach(tab => {
                Array.isArray(tab.ChildTabs) ? comparator(tab) : null;
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

    _isAnyAllSelected(tabs) {
        return tabs.filter(tab => tab.CheckedState === this.fully_checked).length ? true : false;
    }

    _mapSelection(selection, fn) {
        const mapped = [];
        for (let tab in selection) {
            const obj = fn(selection[tab]);
            mapped.push(obj);
        }
        return mapped;
    }


    export = (selection) => {
        console.log("the selection", selection);
        const onlyChildrenOrNoParents = (tabs) => tabs.filter(tab => parseInt(tab.CheckedState) === this.individually_checked && parseInt(tab.TabId) !== -1);
        const onlyParents = (tabs) => tabs.filter(tab => tab.CheckedState === this.fully_checked);
        const filterOutRoot = (tabs) => tabs.filter(tab => parseInt(tab.TabId) !== -1);

        let tabs = this._mapSelection(selection, this._generateSelectionObject);

        const Left = () => {

            setTimeout(() => { //Because setState is async
                const RootTab = this._generateSelectionObject(this.state.tabs);
                let tabs = this._mapSelection(selection, this._generateSelectionObject);
                let bool = false;
                tabs = this._filterOutUnchecked(tabs);
                tabs = filterOutRoot(tabs);

                let childrenTabs = onlyChildrenOrNoParents(tabs);
                let parentTabs = onlyParents(tabs);

                parentTabs = parentTabs.filter((tab, i, arr) => {
                    let bools = [];
                    const left = () => {
                        const ParentTabId = tab.ParentTabId;
                        const parentExists = arr.filter( t=> parseInt(t.TabId) === parseInt(ParentTabId) );
                        parentExists.length ? bools.push(true) : bools.push(false);
                        console.log(parentExists);
                        const condition =  bools.indexOf(true) === -1 ? true : false;
                        return condition;
                    };
                    const right = () => true;
                    bool = tab.CheckedState===this.fully_checked ? left() : right();
                    return bool;
                });



                childrenTabs = childrenTabs.filter((tab) => {
                    const ParentTabId = tab.ParentTabId;
                    const parent = tabs.filter(t => parseInt(t.TabId) === parseInt(ParentTabId))[0];
                    const parentExists = !!parent;
                    const falsey = () => false;
                    const isAllChildrenChecked = () => parent.CheckedState === this.fully_checked ? true : false;

                    const bool = parentExists ? isAllChildrenChecked() : falsey();
                    return !bool;
                });


                const ExportRootOnly = () => {
                    console.log("export root only");
                    const exports = [RootTab];
                    console.log(exports);
                    this.ExportModalOnSelect(exports);

                };
                const ExportSelection = () => {
                    console.log("export selection");
                    console.log("childrenTabs", childrenTabs);
                    console.log("parentTabs", parentTabs);

                    exports = [RootTab].concat(parentTabs, childrenTabs);
                    exports = exports.filter( tab => !!tab.CheckedState);
                    console.log(exports);
                    this.ExportModalOnSelect(exports);
                };
                RootTab.CheckedState === this.fully_checked ? ExportRootOnly() : ExportSelection();
            }, 1);

        };

        const Right = () => {

            setTimeout(() => {//Because setState is async
                console.log("in right exports");
                const RootTab = this._generateSelectionObject(this.state.tabs);
                let childrenTabs = onlyChildrenOrNoParents(tabs);
                let parentTabs = onlyParents(tabs);

                console.log("childrenTabs", childrenTabs);
                console.log("parentTabs", parentTabs);

                let exports = [RootTab].concat(childrenTabs);
                exports = exports.filter( tab => !!tab.CheckedState);
                console.log(exports);

                this.ExportModalOnSelect(exports);
            }, 1);
        };
        this._isAnyAllSelected(tabs) ? Left() : Right();
    }

    setMasterRootCheckedState = () => {
        console.log("set master root selected");

        let totalChildren = 0;
        let childrenSelected = 0;
        const setLength = (tab) => typeof tab === "object" ? totalChildren++ : null;
        const isAllSelected = (tab) => tab.CheckedState ? childrenSelected++ : null;

        this._traverseChildTabs(setLength);
        this._traverseChildTabs(isAllSelected);

        const left = () => this.setState({ childrenSelected: true });
        const right = () => this.setState({ childrenSelected: false });

        childrenSelected !== 0 ? left() : right();

        const update = JSON.parse(JSON.stringify(this.state.tabs));

        switch (true) {
            case childrenSelected === totalChildren:
                console.log("root all selected");
                update.CheckedState = this.fully_checked;
                this.setState({ tabs: update });

                return;
            case childrenSelected < totalChildren && childrenSelected !== 0:
                console.log("root some selected");
                update.CheckedState = update.CheckedState ? this.individually_checked : update.CheckedState;
                this.setState({ tabs: update });

                return;
            case childrenSelected === this.unchecked:
                console.log("root none selected");
                update.CheckedState = this.unchecked;
                this.setState({ tabs: update });
                return;
            default:

                return;
        }

    }

    getChildTabs = (ParentTabId, callback) => {
        this._requestDescendantTabs(ParentTabId, callback);
    }

    showChildTabs = () => {
        const update = Object.assign({}, this.state.tabs);
        update.IsOpen = !this.state.tabs.IsOpen;
        this.setState({ tabs: update });
    }

    selectAll = () => {
        const select = (tab) => {
            const parentSelect = () => {
                tab.CheckedState = this.fully_checked;
                tab.ChildrenSelected = true;
            };

            const singularCheck = () => {
                tab.CheckedState = this.individually_checked;
            };

            tab.HasChildren ? parentSelect() : singularCheck();
        };
        this._traverseChildTabs(select);
        const tabs = JSON.parse(JSON.stringify(this.state.tabs));
        tabs.IsOpen = true;

        const flatTabs = tcdm.flatten(tabs);

        this.setMasterRootCheckedState();
        this.setState({ tabs: tabs, flatTabs: flatTabs });
    }

    unselectAll = () => {
        const unselect = (tab) => {
            tab.CheckedState = this.unchecked;
            tab.ChildrenSelected = false;
        };

        this._traverseChildTabs(unselect);
        const tabs = JSON.parse(JSON.stringify(this.state.tabs));
        const flatTabs = tcdm.flatten(this.state.tabs);
        this.ExportModalOnSelect([]);
        this.setState({ tabs: tabs, flatTabs: flatTabs, childrenSelected: false });
    }

    setCheckedState = () => {
        const update = Object.assign({},this.state);
        update.tabs.CheckedState = this.state.tabs.CheckedState ? this.unchecked : this.fully_checked;
        update.flatTabs[`${this.state.tabs.TabId}-${this.state.tabs.Name}`].CheckedState = this.state.tabs.CheckedState;
        update.tabs.CheckedState === this.fully_checked ? this.selectAll() : this.setState({ tabs: update.tabs, flatTabs: update.flatTabs });

        const ExportRootTab = () => {
            console.log("export root");
            const RootTab = this._generateSelectionObject(this.state.tabs);
            const exports = [RootTab];
            console.log(exports);
            this.ExportModalOnSelect(exports);
        };

        this.state.tabs.CheckedState === this.fully_checked ? ExportRootTab() : this.unselectAll();
    };

    render_icon = (direction) => {
        const width = styles.width(100);
        const margin = styles.margin({ top: -2 });
        const render = (
            <div style={merge(width, margin)}>
                <this.icon animate={true} reset={false} direction={direction} />
            </div>
        );
        return render;
    }

    render_Bullet = () => {
        const direction = this.state.tabs.IsOpen && this.state.tabs.ChildTabs.length ? "90deg" : "0deg";
        const render = this.render_icon(direction);
        return render;
    }

    render_ListBullet = (tab, fn) => {
        const bullet = (() => {
            const width = styles.width(20, "px");
            const height = styles.height(20, "px");

            return (
                <div
                    onClick={() => fn()}
                    style={merge(floatLeft, width, height)}>
                    {this.render_Bullet(tab)}
                </div>);
        })();
        return bullet;
    }

    render_ListCheckbox = () => {
        const checkbox = (() => {
            const padding = styles.padding({ all: 0 });
            return (
                <div style={merge(floatLeft, padding)}>
                    <input
                        type="checkbox"
                        onChange={() => this.setCheckedState(this.state.tabs)}
                        checked={this.state.tabs.CheckedState}
                    />
                    <label onClick={() => this.setCheckedState(this.state.tabs)} ></label>
                </div>);
        })();
        return checkbox;
    }

    render_TreeControl = () => {
        const pagepicker = (() => {
            const condition = (this.state.tabs.IsOpen && this.state.tabs.ChildTabs.length);
            const picker = (
                <TreeControl
                    icon_type="arrow_bullet"
                    flatTabs={this.state.flatTabs}
                    tabs={this.state.tabs.ChildTabs}
                    export={this.export}
                    getChildTabs={this.getChildTabs.bind(this)}
                    setMasterRootCheckedState={this.setMasterRootCheckedState}
                    PortalTabsParameters={this.PortalTabsParameters}

                    rootContext={this}
                    getDescendantPortalTabs={this.props.getDescendantPortalTabs}
                />);

            return (
                <div>
                    {condition ? picker : null}
                </div>
            );

        })();

        return pagepicker;
    }

    render() {
        const listStyle = styles.listStyle();
        const textLeft = styles.textAlign("left");
        const ULPadding = styles.padding({ all: 3 });
        const spanPadLeft = styles.padding({ left: 5 });

        const checkbox = this.render_ListCheckbox(this.state.tabs);
        const bullet = this.render_ListBullet(this.state.tabs, () => this.showChildTabs(this.state.tabs));
        const treeControl = this.render_TreeControl();

        return (
            <ul className="page-picker" style={merge(listStyle, ULPadding)}>
                <li style={merge(textLeft, ULPadding)}>
                    {bullet}
                    {checkbox}
                    <span style={merge(spanPadLeft)}> {this.state.tabs.Name} </span>
                    {this.state.childrenSelected ? <span>*</span> : <span></span>}
                    {treeControl}
                </li>
            </ul>
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
