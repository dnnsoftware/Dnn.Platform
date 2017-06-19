import React, { Component } from "react";
import { PropTypes } from "prop-types";
import { IconSelector } from "./icons/IconSelector";
import { global } from "./global";
import TextOverflowWrapperNew from "dnn-text-overflow-wrapper-new";

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
        this.textOverflowRefs = [];
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
        const animate = direction === "90deg" ? true : false;
        return <this.icon animate={animate} shouldAnimate={true} reset={false} direction={direction} />;
    }

    render_Bullet(tab) {
        const conditional = tab.HasChildren;
        const direction = tab.IsOpen && tab.ChildTabs.length ? "90deg" : "0deg";
        const render = (conditional ? this.render_icon(direction) : () => null);
        return render;
    }

    render_ListBullet(tab, fn) {
        const bullet = (() => {
            return (
                <div
                    className="arrow-bullet"
                    onClick={() => fn()}>
                    {this.render_Bullet.call(this, tab)}
                </div>);
        })();
        return bullet;
    }

    render_ListCheckbox(tab) {
        const checkbox = (() => {
            const position = { position: "absolute" };
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

            const renderCheckBoxClassName = () => {
                switch (true) {
                    case !!tab.HasChildren === true && !!tab.CheckedState === true:
                        return "treecontrol-label-parent";

                    case !!tab.HasChildren === true && !!tab.CheckedState === false && anyChildrenSelected(tab):
                        return "treecontrol-label-parent-unselected";

                    default:
                        return "treecontrol-label-normal";
                }
            };

            return (
                <div style={merge(floatLeft, position)}>
                    <input
                        style={{ border: "3px solid black" }}
                        type="checkbox"
                        onChange={() => this.setCheckedState.call(this, tab)}
                        checked={tab.CheckedState} />
                    <label
                        className={renderCheckBoxClassName()}
                        onClick={() => this.setCheckedState.call(this, tab)} >
                    </label>
                </div>);
        })();
        return checkbox;
    }

    render_tabName(tab) {
        const charLimit = 15;
        const name = tab.Name.length > charLimit ? `${tab.Name.substr(0, charLimit)}...` : tab.Name;
        const render = (() => {
            const margin = styles.margin({ top: 0 });
            const padding = styles.padding({ left: 20 });
            const nowrap = { whiteSpace: "nowrap" };
            const bold = { fontWeight: 400 };
            const color = () => {
                return tab.CheckedState ? { color: this.props.selectedColor } : {};
            };
            const clr = color();

            const renderName = () => {
                switch (true) {
                    case name.length > charLimit:
                        return (
                            <span>
                                { name }
                                <TextOverflowWrapperNew
                                    text= { tab.Name }
                                    border = { true}
                                    className = "page-picker-tooltip-styles" />
                            </span>
                        );
                    default:
                        return (
                            <span>
                                {name}
                                <div></div>
                            </span>
                        );

            }
};

return (
    <span style={merge(margin, padding, nowrap, clr, bold)}>
        {renderName()}
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
                <li
                    key={tab.Name}>
                    {tab.HasChildren ? bullet : null}
                    {checkbox}
                    {tabName}
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
                selectedColor={this.props.selectedColor}
                characterLimit={this.props.characterLimit}
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
    selectedColor: PropTypes.string.isRequired,
    characterLimit: PropTypes.number.isRequired,
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
