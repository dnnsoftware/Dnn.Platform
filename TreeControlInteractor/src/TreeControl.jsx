import React, { Component } from "react";
import { PropTypes } from "prop-types";
import {IconSelector} from "./icons/IconSelector";
import {global}  from "./global";

const styles = global.styles;
const floatLeft = styles.float();
const merge = styles.merge;

import "./styles.less";

export default class TreeControl extends Component {

    constructor(props) {
        super(props);

        const icon_type = props.icon_type;
        this.icon = IconSelector(icon_type);
        this.export = props.export;

        this.getDescendantPortalTabs = props.getDescendantPortalTabs;
    }

    _traverse(comparator) {
        let ChildTabs = this.props.tabs;
        const cachedChildTabs = [];
        cachedChildTabs.push(ChildTabs);
        const condition = cachedChildTabs.length > 0;

        const loop = () => {
            const childTab = cachedChildTabs.length ? cachedChildTabs.shift() : null;
            const left = () => childTab.forEach(tab => {
                Array.isArray(tab.ChildTabs) ? comparator(tab, this.props.tabs) : null;
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

    _mapToParentTabs(parent, fn) {
        const condition = (par) => Object.keys(par).length > 0;
        parent = this.props.findParent(parent);
        const loop = () => {
            parent ? fn(parent) : null;
            parent = parseInt(parent.TabId) !== -1 ? this.props.findParent(parent) : {};
            condition(parent) ? loop() : exit();
        };
        const exit = () => null;
        loop();
        return;

    }

    _mapToChildTabs(tab, fn) {
        let ChildTabs = tab.ChildTabs;
        const cachedChildTabs = [];
        cachedChildTabs.push(ChildTabs);
        const condition = cachedChildTabs.length > 0;

        const loop = () => {
            const childTab = cachedChildTabs.length ? cachedChildTabs.shift() : null;
            const left = () => childTab.forEach(tab => {
                Array.isArray(tab.ChildTabs) ? fn(tab) : null;
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

    setCheckedState(tab) {
        const set = () => tab.HasChildren ? this.selectParent(tab) : this.selectIndividual(tab);
        tab.CheckedState ? this.resetCheckedState(tab) : set();
    }

    mapParentCheckedState(parent) {
        const checkedArray = [];
        parent.ChildTabs.map(tab => tab.CheckedState !== this.props.unchecked ? checkedArray.push(true) : checkedArray.push(false));
        checkedArray.indexOf(false) !== -1 ? parent.CheckedState = this.props.individuallyChecked : parent.CheckedState = this.props.fullyChecked;
        this.props.updateTree(parent);
    }

    resetCheckedState(tab) {
        const unselectChildren = (childTab) => {
            childTab.CheckedState = this.props.unchecked;
            childTab.ChildrenSelected = false;
            this.props.updateTree(childTab);
        };

        const unselectIndividual = () => {
            tab.CheckedState = this.props.unchecked;
            tab.ChildrenSelected = false;
            this.props.updateTree(tab);
        };

        tab.HasChildren ? this._mapToChildTabs(tab, unselectChildren) : unselectIndividual();
        tab.CheckedState = this.props.unchecked;
        tab.ChildrenSelected = false;
        this.props.updateTree(tab);

        const parent = this.props.findParent(tab);
        this.setParentCheckedState(parent);
        this.props.reAlignTree();
    }

    setParentCheckedState(parent) {
        const ChildTabs = parent.ChildTabs || [];
        const length = ChildTabs.length;
        const checkedArray = [];

        const checkParent = () => {
            parent.ChildrenSelected = true;
            switch (true) {
                case checkedArray.filter(bool => !!bool).length === length:
                    parent.CheckedState = parent.CheckedState === this.props.individuallyChecked ? this.props.fullyChecked : parent.CheckedState;
                    this.props.updateTree(parent);
                    return;

                case checkedArray.indexOf(true) !== -1:
                    parent.CheckedState = parent.CheckedState === this.props.fullyChecked ? this.props.individuallyChecked : parent.CheckedState;
                    this.props.updateTree(parent);
                    return;
            }

            this.props.updateTree(parent);
        };

        const noChildrenSelected = () => {
            parent.ChildrenSelected = false;
            this.props.updateTree(parent);
        };

        ChildTabs.forEach(tab => tab.CheckedState ? checkedArray.push(true) : checkedArray.push(false));
        checkedArray.indexOf(true) !== -1 ? checkParent() : noChildrenSelected();
    }

    selectParent(tab) {
        const select = (tab) => {
            switch (true) {
                case tab.HasChildren === true:
                    tab.CheckedState = this.props.fullyChecked;
                    tab.ChildrenSelected = true;
                    this.props.updateTree(tab);
                    return;
                case tab.HasChildren === false:
                    tab.CheckedState = this.props.individuallyChecked;
                    tab.ChildrenSelected = false;
                    this.props.updateTree(tab);
                    return;
            }
        };
        tab.CheckedState = this.props.fullyChecked;
        tab.ChildrenSelected = true;
        this._mapToChildTabs(tab, select);
        this.props.updateTree(tab);
        const parent = this.props.findParent(tab);
        this.setParentCheckedState(parent);
        this.props.reAlignTree();

    }

    selectIndividual(tab) {
        tab.CheckedState = this.props.individuallyChecked;
        tab.ChildrenSelected = false;
        this.props.updateTree(tab);
        const parent = this.props.findParent(tab);
        this.setParentCheckedState(parent);
        this.props.reAlignTree();
    }


    expandParent(tab) {
        const condition = tab.HasChildren && tab.ChildTabs.length > 0;
        const left = () => tab.IsOpen = !tab.IsOpen;

        const right = () => {
            this.props.getDescendantPortalTabs(tab.TabId, () => {
                tab.IsOpen = !tab.IsOpen;
                this.props.updateTree(tab);

            });
        };
        condition ? left() : right();
        this.props.updateTree(tab);
    }

    render_icon(direction) {
        const width = styles.width(100);
        const animate = direction === "90deg" ? true : false;

        const render = (
            <div style={merge(width)}>
                <this.icon animate={animate} reset={false} direction={direction} />
            </div>
        );
        return render;
    }

    render_Bullet(tab) {
        const conditional = tab.HasChildren;
        const direction = tab.IsOpen && tab.ChildTabs.length ? "90deg" : "0deg";
        const render = (conditional ? this.render_icon(direction) : () => null);
        return render;
    }

    render_ListBullet(tab, fn) {
        const bullet = (() => {
            const width = styles.width(20, "px");
            const height = styles.height(20, "px");

            return (
                <div
                    onClick={() => fn()}
                    style={merge(floatLeft, width, height)}>
                    {this.render_Bullet.call(this,tab)}
                </div>);
        })();
        return bullet;
    }

    render_ListCheckbox(tab) {
        const checkbox = (() => {

            return (
                <div style={merge(floatLeft)}>
                    <input
                        type="checkbox"
                        onChange={() => this.setCheckedState.call(this, tab)}
                        checked={tab.CheckedState}
                    />
                    <label onClick={() => this.setCheckedState.call(this, tab)} ></label>
                </div>);
        })();
        return checkbox;
    }

    render_tabName(tab) {
        const render = (() => {
            const padding = styles.margin({ top: 10 });
            return (
                <span style={merge(padding)}>
                    {tab.Name}
                </span>
            );

        })();

        return render;
    }

    render_li(tabs) {
        const render = (() => {
            return tabs.map(tab => {
                const tabName = this.render_tabName(tab);
                const checkbox = this.render_ListCheckbox(tab);
                const bullet = this.render_ListBullet.call(this, tab, this.expandParent.bind(this, tab));
                const tree = this.render_tree(tab.ChildTabs);
                const anyChildrenSelected = (tab) => {
                    const ChildTabs = tab.ChildTabs;
                    const left = () => {
                        const trueCheckedState = [];
                        const AreChildrenChecked = (t) => {
                            const condition = t.CheckedState !== this.props.unchecked;
                            condition ? trueCheckedState.push(true) : trueCheckedState.push(false);
                        };

                        this._mapToChildTabs(tab, AreChildrenChecked);
                        const bool = trueCheckedState.indexOf(true) !== -1 ? true : false;
                        return bool;
                    };
                    const right = () => null;
                    return ChildTabs.length ? left() : right();
                };

                const li = () => (
                    <li key={tab.Name}>
                        {tab.HasChildren ? bullet : null}
                        {checkbox}
                        {tabName}
                        {(tab.HasChildren && anyChildrenSelected(tab)) || (tab.HasChildren && tab.CheckedState) ? <span>*</span> : <span></span>}
                        {tree}
                    </li>);
                const parent = this.props.findParent(tab);

                const show = parent.IsOpen || parseInt(tab.TabId) === -1 ? li() : null;
                return (
                    show
                );
            });
        })();
        return render;
    }

    render_tree(ChildTabs) {
        const render = (() => {
            return (
                <TreeControl
                    tabs={ChildTabs}
                    icon_type="arrow_bullet"
                    updateTree={this.props.updateTree.bind(this)}
                    reAlignTree={this.props.reAlignTree.bind(this)}
                    findParent={this.props.findParent.bind(this)}
                    fullyChecked={this.props.fullyChecked}
                    individuallyChecked={this.props.individuallyChecked}
                    unchecked={this.props.unchecked}

                    export={this.props.export}
                    PortalTabsParameters={this.props.PortalTabsParameters}
                    getDescendantPortalTabs={this.props.getDescendantPortalTabs}
                />
            );
        })();
        return render;
    }


    render() {
        const listStyle = styles.listStyle();
        const list_items = this.render_li(this.props.tabs);
        return (
            <ul className="page-picker" style={merge(listStyle)} >
                {list_items}
            </ul>
        );
    }
}

TreeControl.propTypes = {
    tabs: PropTypes.object.isRequired,
    icon_type: PropTypes.object.isRequired,
    export: PropTypes.func.isRequired,
    getDescendantPortalTabs: PropTypes.func.isRequired,
    PortalTabsParameters: PropTypes.object.isRequired,
    updateTree: PropTypes.func.isRequired,
    fullyChecked: PropTypes.number.isRequired,
    individuallyChecked: PropTypes.number.isRequired,
    unchecked: PropTypes.number.isRequired,
    reAlignTree: PropTypes.func.isRequired,
    findParent: PropTypes.func.isRequired
};
