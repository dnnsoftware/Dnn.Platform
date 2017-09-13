import React, { Component } from "react";
import { PropTypes } from "prop-types";
import Localization from "localization";
import { TreeAddPage, TreeAnalytics, TreeCopy, TreeEdit, EyeIcon } from "dnn-svg-icons";
import Menu from "./InContextMenu/Menu";
import MenuItem from "./InContextMenu/MenuItem";
import ReactDOM from "react-dom";
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
    onItemClick(key, item) {
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
    render_actionable(item) {
        let visibleMenus = [
            { key: "Add", title: Localization.get("AddPage"), index: 10, icon: TreeAddPage, onClick: this.onItemClick },
            { key: "View", title: Localization.get("View"), index: 20, icon: EyeIcon, onClick: this.onItemClick },
            { key: "Edit", title: Localization.get("Edit"), index: 30, icon: TreeEdit, onClick: this.onItemClick },
            { key: "Duplicate", title: Localization.get("Duplicate"), index: 40, icon: TreeCopy, onClick: this.onItemClick }
        ];

        console.log(item);

        if (this.props.pageInContextComponents) {
            visibleMenus = visibleMenus.concat(this.props.pageInContextComponents && this.props.pageInContextComponents || []);
        }
        visibleMenus = this.sort(visibleMenus, "index");
        /*eslint-disable react/no-danger*/

        if (this.showMenu) {
            return (<Menu>
                {
                    visibleMenus.map(menu => {
                        return <MenuItem onMenuAction={menu.onClick.bind(this, menu.key, item)}>
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
    item: PropTypes.object.isRequired,
    pageInContextComponents: PropTypes.array.isRequired,
    onClose: PropTypes.func.isRequired
};


