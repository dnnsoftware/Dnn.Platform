export class PagePickerDataManager {

    constructor() {
        this.original_data = [];
        this.storage = {};
        this.select = 1;
        this.partialSelect = -1;
        this.unselect = 0;
    }

    reset() {
        this.original_data = [];
        this.storage = {};
    }

    /** INTERNAL METHODS **/

    _init() {
        this.original_data.forEach(tab => this._store(tab));
    }

    _flatten() {
        const storage = this.storage;
        for (let tab in storage) {
            const childrenTruthy = this._hasObjectChildren(tab);
            const Tab = storage[tab];

            const Left = () => {
                Tab.ChildTabs.forEach(childtab => this._store(childtab));
                Tab.ChildTabs = storage[tab].ChildTabs.map((childtab) => `${childtab.TabId}-${childtab.Name}`);
                this._flatten();
            };

            const Right = () => {
                const hasParent = this.hasParent(Tab.TabId);
                if (hasParent) {
                    this._store(Tab);
                    // const parent = this.getBy({
                    //     TabId: Tab.ParentTabId
                    // })[0];
                }
            };
            childrenTruthy ? Left() : Right();
        }
    }


    _hasObjectChildren(id) {
        const tab = this.storage[id];
        const conditional = (tab.HasChildren && Array.isArray(tab.ChildTabs) && typeof tab.ChildTabs[0] === "object");
        return conditional ? true : false;
    }

    _idExists(id) {
        const tab = this.storage[id];
        return typeof tab === "object" ? true : false;
    }

    _store(tab) {
        this.storage[`${tab.TabId}-${tab.Name}`] = tab;
    }

    _updateParentChildTabs() {
        const children = Object.keys(this.storage);
        const root_TabId = children[0];
        const rootTab = this.storage[root_TabId];
        let mem_cache = [];

        children
            .filter(id => id !== parseInt(root_TabId) )
            .sort((a, b) => a.split("-")[0] - b.split("-")[0])
            .forEach((id) => {
                const tab = this.storage[id];
                const parentTab = this.getBy({
                    TabId: tab.ParentTabId
                })[0];
                mem_cache.push(id);
                const Left = () => {
                    tab.ChildTabs.forEach((id) => parentTab.ChildTabs.push(id));
                };

                const Right = () => null;
                const condition = !!parentTab;
                condition ? Left() : Right();
            });

        rootTab.ChildTabs = rootTab.ChildTabs.concat(mem_cache);
        mem_cache = [];
        rootTab.ChildTabs = rootTab.ChildTabs.sort().filter((id, i, arr) => parseInt(id) !== parseInt(arr[i + 1]) );
    }

    _removeDuplicateChildTabIds() {
        Object.keys(this.storage).map(id => {
            let ChildTabs = this.storage[id].ChildTabs;
            ChildTabs = ChildTabs.sort().filter((id, i, arr) => parseInt(id) !== parseInt(arr[i + 1]) );
        });
    }

    _mapRootTabs(TabId, fn = (root) => root) {
        let root = null;
        let last_tab = null;

        const loop = (TabId) => {
            const tab = this.storage[TabId];
            last_tab = tab;
            const parentTabId = tab.ParentTabId || 0;
            let parent = this.getBy({
                TabId: parentTabId
            })[0];

            const Left = () => {
                const id = `${parent.TabId}-${parent.Name}`;
                root = parent;
                return this.hasParent(id) ? loop(id) : fn(root);
            };

            const Right = () => fn(tab);
            return !!parent && "ParentTabId" in parent ? Left() : Right();
        };

        return loop(TabId);
    }

    _setParentTabCheckedStates({
        selection
    }) {
        //console.log(selection)
        const hasAllChildTabsSelected = (arr) => {
            const condition = arr.reduce((a, b) => a + b) === arr.length;
            return condition;
        };

        const hasAllChildTabsUnselected = (arr) => {
            const filtered = arr.filter(v => v === this.unselect);
            const condition = filtered.length === arr.length;
            return condition;
        };

        const hasUncheckedPartialStates = (arr) => {
            const left = (parseInt( arr.indexOf(-1) ) !== -1) && (arr.filter(v => parseInt(v) === -1).length !== arr.length);
            const right = (parseInt( arr.indexOf(0) ) !== -1) && (arr.filter(v => parseInt(v) === 0).length !== arr.length);
            const condition = left || right;
            return condition;
        };

        const FullySelect = (parent) => {
            parent.CheckedState = this.select;
        };

        const PartiallySelect = (parent) => {
            parent.CheckedState = this.partialSelect;
        };

        const Unselect = (parent) => {
            parent.CheckedState = this.unselect;
        };


        Object.keys(this.storage)
            .filter(id => parseInt(id) !== parseInt(selection) )
            .filter(id => this.hasChildren(id))
            .forEach(id => {
                const ParentTab = this.storage[id];
                const ChildTabs = ParentTab.ChildTabs;
                const ChildCheckedStateArray = ChildTabs
                    .map(childTabId => this.storage[childTabId])
                    .map(childTab => childTab.CheckedState);

                switch (true) {
                    case hasAllChildTabsSelected(ChildCheckedStateArray):
                        this._mapRootTabs(id, FullySelect);
                        return;
                    case hasUncheckedPartialStates(ChildCheckedStateArray):
                        //console.log('inHasUncheckedPartialStates', id)
                        this._mapRootTabs(id, PartiallySelect);
                        return;
                    case hasAllChildTabsUnselected(ChildCheckedStateArray):
                        //console.log('inAllUnselected', id)
                        this._mapRootTabs(id, Unselect);
                        return;
                }
            });

    }

    _clearChildTabs() {
        for (let tab in this.storage) {
            this.storage[tab].ChildTabs = [];
        }
    }

    /** PUBLIC API METHODS **/
    flatten(tabs) {
        tabs = JSON.parse(JSON.stringify(tabs));
        this.original_data.push(tabs);
        this._init();
        this._flatten();
        this._updateParentChildTabs();
        //this._removeDuplicateChildTabIds();
        this._clearChildTabs();
        return this.storage;
    }

    hasParent(id) {
        const parent = this.getBy({
            TabId: id
        })[0];
        const conditional = (typeof parent === "object" && "ParentTabId" in parent && parseInt(parent.ParentTabId) !== 0) ? true : false;
        return conditional;
    }

    hasChildren(id) {
        const tab = this.storage[id];
        const conditional = (tab.HasChildren && Array.isArray(tab.ChildTabs) && tab.ChildTabs.length > 0);
        return conditional ? true : false;
    }

    append(tab) {
        this._flatten(tab);
    }

    getBy(paramObj) {
        const keys = Object.keys(paramObj);
        const tabs = Object.keys(this.storage)
            .filter((id) => {
                const bool_array = keys.map(key => {
                    const condition = (this.storage[id][key] === paramObj[key]);
                    return condition;
                });
                return bool_array.indexOf(false) === -1;
            })
            .map((id) => this.storage[id]);

        const Left = () => tabs;
        const Right = () => [];
        return tabs.length > 0 ? Left() : Right();
    }

    export() {
        return Object.assign({}, this.storage);
    }

    childrenOf({id}) {
        const Left = () => {
            const children = this.storage[id].ChildTabs;
            const child_tabs = children.map(id => this.storage[id]);
            return child_tabs;
        };

        const Right = () => {
            return [];
        };

        const conditional = (this._idExists(id) && this.hasChildren(id));
        return conditional ? Left() : Right();

    }

    replace(obj = false) {
        const Left = () => this.storage = obj;
        const Right = () => {
            throw (`ppdm.replace requires an object input, got ${typeof obj}`);
        };
        (typeof obj === "object") ? Left(): Right();
    }

    update({id}, updateObj) {
        const Right = () => {
            throw ("Both a unique TabId must exist and Update Object is required");
        };
        const Left = () => {
            Object.keys(updateObj).forEach((key) => this.storage[id][key] = updateObj[key]);
            this._setParentTabCheckedStates({
                selection: id
            });
        };
        (id && typeof updateObj === "object") ? Left(): Right();
    }

}
