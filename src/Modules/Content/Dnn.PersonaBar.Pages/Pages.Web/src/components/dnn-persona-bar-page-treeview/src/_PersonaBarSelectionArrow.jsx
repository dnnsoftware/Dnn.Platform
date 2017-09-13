import React, { Component } from "react";
import GridCell from "dnn-grid-cell";
import { PropTypes } from "prop-types";
import { ArrowForward, MoreMenuIcon } from "dnn-svg-icons";
import { PersonaBarTreeInContextMenu } from "./_PersonaBarTreeInContextMenu";
import "./styles.less";

export default class PersonaBarSelectionArrow extends Component {

    constructor() {
        super();
        this.state = {
            showInContext: false,
            showMenu: false
        };
    }

    hasAtLeastOnePermission(item){
        switch(true){
            case item.canViewPage:
            case item.canManagePage:
            case item.canEditPage:
            case item.canAddContentToPage:
            case item.canCopyPage:
                return true;
            default:
                return false;
        }
    }

    toggleInContext(li) {
        const { _traverse } = this.props;
        let updateReduxStore, pageList = null;
        _traverse((item, list, updateStore) => {
            item.showInContextMenu = item.id === li.id ? !item.showInContextMenu : delete item.showInContextMenu;
            updateReduxStore = updateStore;
            pageList = list;
        });
        updateReduxStore(pageList);
    }
    toggleInContextMenu() {
        const show = !this.state.showMenu;
        this.setState({ showMenu: show });
    }
    /* eslint-disable react/no-danger */
    render() {
        const { item } = this.props;
        /*eslint-disable react/no-danger*/
        return (
            <div id={`menu-item-${item.name}`} className="selection-arrow">
                {item.selected ? <div dangerouslySetInnerHTML={{ __html: ArrowForward }} /> : <div></div>}
                {item.selected && this.hasAtLeastOnePermission(item)? <div className="dots" dangerouslySetInnerHTML={{ __html: MoreMenuIcon }} onClick={() => this.toggleInContextMenu()} /> : <div></div>}
                {this.state.showMenu && <PersonaBarTreeInContextMenu {...this.props} onClose={this.toggleInContextMenu.bind(this)} />}
            </div>

        );
    }
}

PersonaBarSelectionArrow.propTypes = {
    onAddPage: PropTypes.func.isRequired,
    onViewPage: PropTypes.func.isRequired,
    onViewEditPage: PropTypes.func.isRequired,
    onDuplicatePage: PropTypes.func.isRequired,
    _traverse: PropTypes.func.isRequired,
    item: PropTypes.object.isRequired,
    pageInContextComponents: PropTypes.array.isRequired
};