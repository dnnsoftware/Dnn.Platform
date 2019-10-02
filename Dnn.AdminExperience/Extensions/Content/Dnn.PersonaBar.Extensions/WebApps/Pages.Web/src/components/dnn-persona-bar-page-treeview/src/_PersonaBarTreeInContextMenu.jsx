import React, { Component } from "react";
import { PropTypes } from "prop-types";
import Localization from "localization";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";
import Menu from "./InContextMenu/Menu";
import MenuItem from "./InContextMenu/MenuItem";
import cloneDeep from 'lodash/cloneDeep';
import securityService from "../../../services/securityService";

import "./styles.less";

export class PersonaBarTreeInContextMenu extends Component {

    constructor(props) {
        super(props);
        this.state = {showMenu:false};
    }

    componentDidMount() {
        let { props } = this;
        if (props.item === undefined) {
            this.setState({showMenu: false});
        } else {
            this.setState({showMenu: true});
        }
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

    getContextMenuItems(item) {
        const visibleMenus = [];

        const editable = item.canAddContentToPage && (item.pageType !== "tab" && item.pageType !== "url" && item.pageType !== "file");

        if(item.canAddPage) {
            visibleMenus.push({
                key: "Add",
                title: Localization.get("AddPage"),
                index: 10,
                icon: SvgIcons.TreeAddPage,
                onClick: this.onItemClick
            });
        }
        if(item.canViewPage) {
            visibleMenus.push({
                key: "View",
                title: Localization.get("View"),
                index: 20,
                icon: SvgIcons.EyeIcon,
                onClick: this.onItemClick
            });
        }

        if(editable) {
            visibleMenus.push({
                key: "Edit",
                title: Localization.get("Edit"),
                index: 30,
                icon: SvgIcons.TreeEdit,
                onClick: this.onItemClick
            });
        }

        if (item.canCopyPage) {
            visibleMenus.push({
                key: "Duplicate",
                title: Localization.get("Duplicate"),
                index: 40,
                icon: SvgIcons.TreeCopy,
                onClick: this.onItemClick
            });
        }
        return visibleMenus;
    }

    getAdditionalMenuItems() {
        if (this.props.pageInContextComponents && securityService.isSuperUser()) {
            let { onItemClick } = this;
            const additionalMenus = cloneDeep(this.props.pageInContextComponents || []);
            additionalMenus && additionalMenus.forEach((value, index, arr) => {
                arr[index].onClick = onItemClick;
            });
            return additionalMenus;
        }
        return [];
    }

    buildMenuItems(item) {
        const menuItems = this.getContextMenuItems(item);
        const additionalMenuItems = this.getAdditionalMenuItems();
        const visibleMenus = [...menuItems, ...additionalMenuItems];
        visibleMenus.sort((a, b) => {
            const column = "index";
            const aIdx = parseInt(a[column]);
            const bIdx = parseInt(b[column]);
            return aIdx - bIdx;
        });
        return visibleMenus;
    }

    renderActionable(item) {
        const visibleMenus = this.buildMenuItems(item);
        /*eslint-disable react/no-danger*/
        if (this.state.showMenu && visibleMenus.length) {
            return (<Menu>
                {
                    visibleMenus.map(menu => {
                        return <MenuItem key={menu.key} onMenuAction={menu.onClick.bind(this, menu.key, item, menu.OnClickAction)}>
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
                {this.renderActionable(item)}
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


