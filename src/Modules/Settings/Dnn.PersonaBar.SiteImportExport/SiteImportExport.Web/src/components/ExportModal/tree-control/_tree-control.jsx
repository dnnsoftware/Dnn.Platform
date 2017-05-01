import React, { Component } from "react";
import { PropTypes } from "prop-types";
import { IconSelector } from "./icons";
import { global } from "./_global";


const styles = global.styles;
const floatLeft = styles.float();
const merge = styles.merge;
const inlineBlock = styles.display("inline-block");

import "./styles.less";

export class TreeControl extends Component {

    constructor(props) {
        super(props);

        const icon_type = props.icon_type;
        this.icon = IconSelector(icon_type);
        this.export = props.export;

        this.getDescendantPortalTabs = props.getDescendantPortalTabs;
    }

    _traverse = (comparator) => {
        let ChildTabs = this.props.tabs
        const cached_childtabs = []
        cached_childtabs.push(ChildTabs)
        const condition = cached_childtabs.length > 0

        const loop = () => {
            const childtab = cached_childtabs.length ? cached_childtabs.shift() : null
            const left = () => childtab.forEach(tab => {
                Array.isArray(tab.ChildTabs) ? comparator(tab, this.props.tabs) : null;
                Array.isArray(tab.ChildTabs) && tab.ChildTabs.length ? cached_childtabs.push(tab.ChildTabs) : null;
                condition ? loop() : exit()
            })
            const right = () => null;
            childtab ? left() : right()
        }

        const exit = () => null

        loop();
        return;
    }

    _mapToParentTabs = (parent, fn) => {
        const condition = (par) => Object.keys(par).length > 0
        parent = this.props.findParent(parent)
        const loop = () => {
            parent ? fn(parent) : null
            parent = parseInt(parent.TabId) !== -1 ? this.props.findParent(parent) : {}
            condition(parent) ? loop() : exit()
        }
        const exit = () => null
        loop()
        return

    }

    _mapToChildTabs = (tab, fn) => {
        let ChildTabs = tab.ChildTabs
        const cached_childtabs = []
        cached_childtabs.push(ChildTabs)
        const condition = cached_childtabs.length > 0

        const loop = () => {
            const childtab = cached_childtabs.length ? cached_childtabs.shift() : null
            const left = () => childtab.forEach(tab => {
                Array.isArray(tab.ChildTabs) ? fn(tab) : null
                Array.isArray(tab.ChildTabs) && tab.ChildTabs.length ? cached_childtabs.push(tab.ChildTabs) : null
                condition ? loop() : exit()
            })
            const right = () => null
            childtab ? left() : right()
        }
        const exit = () => null

        loop()
        return
    }

    setCheckedState = (tab) => {
        const set = () => tab.HasChildren ? this.selectParent(tab) : this.selectIndividual(tab)
        tab.CheckedState ? this.resetCheckedState(tab) : set()
    }

    mapParentCheckedState = (parent) => {
        const ChildTabs = parent.ChildTabs || []
        const length = ChildTabs.length
        const checkedArray = []
        const truthyChecked = parent.ChildTabs.map(tab => tab.CheckedState !== this.props.unchecked ? checkedArray.push(true) : checkedArray.push(false))
        checkedArray.indexOf(false) !== -1 ? parent.CheckedState = this.props.individuallyChecked : parent.CheckedState = this.props.fullyChecked
        this.props.updateTree(parent)
    }

    resetCheckedState = (tab) => {
        const unselectChildren = (childtab) => {
            childtab.CheckedState = this.props.unchecked
            childtab.ChildrenSelected = false;
            this.props.updateTree(childtab)
        }

        const unselectIndividual = () => {
            tab.CheckedState = this.props.unchecked
            tab.ChildrenSelected = false;
            this.props.updateTree(tab)
        }

        tab.HasChildren ? this._mapToChildTabs(tab, unselectChildren) : unselectIndividual()
        tab.CheckedState = this.props.unchecked
        tab.ChildrenSelected = false;
        this.props.updateTree(tab)

        const parent = this.props.findParent(tab)
        this.setParentCheckedState(parent)
        this.props.reAlignTree()
    }

    setParentCheckedState(parent) {
        const ChildTabs = parent.ChildTabs || []
        const length = ChildTabs.length
        const checkedArray = []

        const checkParent = () => {
            parent.ChildrenSelected = true
            switch (true) {
                case checkedArray.filter(bool => !!bool).length === length:
                    parent.CheckedState = this.props.fullyChecked
                    this.props.updateTree(parent)
                    return

                case checkedArray.indexOf(true) !== -1:
                    parent.CheckedState = parent.CheckedState==this.props.fullyChecked ?  this.props.individuallyChecked : parent.CheckedState;

                    this.props.updateTree(parent)
                    return
            }

            this.props.updateTree(parent)
        };

        const noChildrenSelected = () => {
            parent.CheckedState = this.props.individuallyChecked;
            parent.ChildrenSelected = false
            this.props.updateTree(parent)
        };

        ChildTabs.forEach(tab => tab.CheckedState ? checkedArray.push(true) : checkedArray.push(false))
        checkedArray.indexOf(true) !== -1 ? checkParent() : noChildrenSelected()
    }



    selectParent = (tab) => {
        const select = (tab) => {
            switch (true) {
                case tab.HasChildren === true:
                    tab.CheckedState = this.props.fullyChecked
                    tab.ChildrenSelected = true;
                    this.props.updateTree(tab)
                    return
                case tab.HasChildren === false:
                    tab.CheckedState = this.props.individuallyChecked
                    tab.ChildrenSelected = false
                    this.props.updateTree(tab)
                    return
            }
        };
        tab.CheckedState = this.props.fullyChecked
        tab.ChildrenSelected = true
        this._mapToChildTabs(tab, select)
        this.props.updateTree(tab)
        const parent = this.props.findParent(tab)
        this.setParentCheckedState(parent)
        this.props.reAlignTree()

    }

    selectIndividual = (tab) => {
        tab.CheckedState = this.props.individuallyChecked
        tab.ChildrenSelected = false
        this.props.updateTree(tab)
        const parent = this.props.findParent(tab)
        this.setParentCheckedState(parent)
        this.props.reAlignTree()
    }


    expandParent = (tab) => {
        const condition = tab.HasChildren && tab.ChildTabs.length > 0
        const left = () => tab.IsOpen = !tab.IsOpen

        const right = () => {
            this.props.getDescendantPortalTabs(tab.TabId, () => {
                 tab.IsOpen = !tab.IsOpen
                 this.props.updateTree(tab)

            });

        }
        condition ? left() : right()
        this.props.updateTree(tab)
    }



    render_icon = (direction) => {
        const width = styles.width(100);
        const animate = direction === "90deg" ? true : false;

        const render = (
            <div style={merge(width)}>
                <this.icon animate={animate} reset={false} direction={direction} />
            </div>
        );
        return render;
    }

    render_Bullet = (tab) => {
        const conditional = tab.HasChildren;
        const direction = tab.IsOpen && tab.ChildTabs.length ? "90deg" : "0deg";
        const render = (conditional ? this.render_icon(direction) : () => null);
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

    render_ListCheckbox = (tab) => {
        const checkbox = (() => {

            return (
                <div style={merge(floatLeft)}>
                    <input
                        type="checkbox"
                        onChange={() => this.setCheckedState(tab)}
                        checked={tab.CheckedState}
                    />
                    <label onClick={() => this.setCheckedState(tab)} ></label>
                </div>);
        })();
        return checkbox;
    }

    render_tabName = (tab) => {
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

    render_li = (tabs) => {
        const render = (() => {
            return tabs.map(tab => {
                const listStyle = styles.listStyle();
                const textLeft = styles.textAlign("left");
                const ULPadding = styles.padding({ all: 3 });
                const spanPadLeft = styles.padding({ left: 5 });

                const tabName = this.render_tabName(tab);
                const checkbox = this.render_ListCheckbox(tab);
                const bullet = this.render_ListBullet(tab, this.expandParent.bind(this, tab));
                const tree = this.render_tree(tab.ChildTabs);
                const anyChildrenSelected = (tab) => {
                    const ChildTabs = tab.ChildTabs;
                    const left = () => {
                        const truthyCheckedState = [];
                        const AreChildrenChecked = (t) => {
                            const condition = t.CheckedState !== this.props.unchecked;
                            condition ? truthyCheckedState.push(true) : truthyCheckedState.push(false)
                        }

                        this._mapToChildTabs(tab, AreChildrenChecked)
                        const bool = truthyCheckedState.indexOf(true) !== -1 ? true : false
                        return bool
                    }

                    const right = () => console.log('in right')
                    return ChildTabs.length ? left() : right()
                }

                const li = () => (
                    <li key={tab.Name}>
                        <div>
                            {tab.HasChildren ? bullet : null}
                            {checkbox}
                            {tabName}
                            {tab.CheckedState && tab.HasChildren && anyChildrenSelected(tab) ? <span>*</span> : <span></span>}
                        </div>
                        {tree}
                    </li>)
                const parent = this.props.findParent(tab)

                const show = parent.IsOpen || parseInt(tab.TabId) === -1 ? li() : null
                return (
                    show
                )
            });
        })();
        return render
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
            )
        })();
        return render
    }


    render() {
        const listStyle = styles.listStyle();
        const textLeft = styles.textAlign("left");
        const ULPadding = styles.padding({ all: 'inherit' });
        const spanPadLeft = styles.padding({ left: 5 });

        const list_items = this.render_li(this.props.tabs);

        return (
            <ul className="page-picker" style={merge(listStyle, ULPadding)} >
                {list_items}
            </ul>
        )
    }
}

TreeControl.propTypes = {
    flatTabs: PropTypes.object.isRequired,
    tabs: PropTypes.object.isRequired,
    setMasterRootCheckedState: PropTypes.func.isRequired,
    rootContext: PropTypes.object.isRequired,
    root: PropTypes.object.isRequired,
    icon_type: PropTypes.object.isRequired,
    export: PropTypes.func.isRequired,
    getChildTabs: PropTypes.func.isRequired,
    getDescendantPortalTabs: PropTypes.func.isRequired
};
