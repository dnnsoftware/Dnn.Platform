import React, { Component } from "react";
import { PropTypes } from "prop-types";
import Localization from "localization";
import { TreeAddPage, TreeAnalytics, TreeCopy, TreeEdit, EyeIcon } from "dnn-svg-icons";
import Menu from "./InContextMenu/Menu";
import MenuItem from "./InContextMenu/MenuItem";
import ReactDOM from "react-dom";
import cloneDeep from 'lodash.clonedeep';
import "./styles.less";

export class PersonaBarTreeInContextMenu extends Component {
    constructor(props) {
        super(props);
        this.showMenu = false;
        this.handleClick = this.handleClick.bind(this);
    }

    handleClick(event) {
        if (!ReactDOM.findDOMNode(this).contains(event.target) && (typeof event.target.className !== "string" || (typeof event.target.className === "string" && event.target.className.indexOf("menu-item") === -1))) {
            this.props.onClose();
        }
    }
    componentWillMount() {
        document.addEventListener("click", this.handleClick, false);
        let { props } = this;
        if (props.item === undefined)// || props.item.id !== props.pageId) 
        {
            this.showMenu = false;
        }
        else {
            this.showMenu = true;
        }
    }
    componentWillUnmount() {
        document.removeEventListener("click", this.handleClick, false);
    }
    // componentWillReceiveProps(newProps) {
    //     if (newProps.item === undefined && newProps.item.id !== newProps.pageId) {
    //         this.showMenu = false;
    //     }
    //     else {
    //         this.showMenu = true;
    //     }
    // }

    render_default(item) {
        return (
            <div className="in-context-menu"></div>
        );
    }
    onItemClick(key, item, customAction) {
        switch (key) {
            case "Add":
                this.props.onAddPage(item);
                this.props.onClose();
                break;
            case "View":
                this.props.onViewPage(item);
                this.props.onClose();
                break;
            case "Edit":
                this.props.onViewEditPage(item);
                this.props.onClose();
                break;
            case "Duplicate":
                this.props.onDuplicatePage(item);
                this.props.onClose();
                break;
            default:
                if (typeof customAction === "function")
                    this.props.CallCustomAction(customAction);
                this.props.onClose();
                break;
        }
    }
    sort(items, column, order) {
        order = order === undefined ? "asc" : order;
        items = items.sort(function (a, b) {
            if (a[column] > b[column]) //sort string descending
                return order === "asc" ? 1 : -1;
            if (a[column] < b[column])
                return order === "asc" ? -1 : 1;
            return 0;//default return value (no sorting)
        });
        return items;
    }
    clone(source) {
        let that = source;
        let temp = function temporary() { return that.apply(this, arguments); };
        for (let key in that) {
            if (this.hasOwnProperty(key)) {
                temp[key] = this[key];
            }
        }
        return temp;
    }

    render_actionable(item) {
        let visibleMenus = [];

        item.canAddPage ? visibleMenus.push({ key: "Add", title: Localization.get("AddPage"), index: 10, icon: TreeAddPage, onClick: this.onItemClick }) : null;
        item.canViewPage ? visibleMenus.push({ key: "View", title: Localization.get("View"), index: 20, icon: EyeIcon, onClick: this.onItemClick }) : null;
        item.canAddContentToPage ? visibleMenus.push({ key: "Edit", title: Localization.get("Edit"), index: 30, icon: TreeEdit, onClick: this.onItemClick }) : null;
        item.canCopyPage ? visibleMenus.push({ key: "Duplicate", title: Localization.get("Duplicate"), index: 40, icon: TreeCopy, onClick: this.onItemClick }) : null;

        if (this.props.pageInContextComponents) {
            let { onItemClick } = this;
            let additionalMenus = cloneDeep(this.props.pageInContextComponents || []);
            additionalMenus && additionalMenus.map(item => {
                item.onClick = onItemClick;
            });
            visibleMenus = visibleMenus.concat(additionalMenus && additionalMenus || []);
        }
        visibleMenus = this.sort(visibleMenus, "index");
        /*eslint-disable react/no-danger*/

        if (this.showMenu && visibleMenus.length) {
            return (<Menu>
                {
                    visibleMenus.map(menu => {
                        return <MenuItem onMenuAction={menu.onClick.bind(this, menu.key, item, menu.OnClickAction)}>
                            <div className="icon" dangerouslySetInnerHTML={{ __html: menu.icon }} />
                            <div className="label">{menu.title}</div>
                        </MenuItem>;
                    })
                }
            </Menu>);
        }
        else {
            return <div />;
        }
    }

    render() {
        const { item } = this.props;
        return (
            <span>
                {this.render_actionable(item)}
            </span>
        );
    }

}

PersonaBarTreeInContextMenu.propTypes = {
    onViewPage: PropTypes.func.isRequired,
    onViewEditPage: PropTypes.func.isRequired,
    onAddPage: PropTypes.func.isRequired,
    onDuplicatePage: PropTypes.func.isRequired,
    CallCustomAction: PropTypes.func.isRequired,
    item: PropTypes.object.isRequired,
    pageInContextComponents: PropTypes.array.isRequired,
    onClose: PropTypes.func.isRequired
};


