import React, { Component } from "react";
import { PropTypes } from "prop-types";
import { SvgIcons } from "@dnnsoftware/dnn-react-common";
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
        switch (true) {
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

    onMouseEnter() {
        this.setState({ showMenu: true });
    }

    onMouseLeave() {
        this.setState({ showMenu: false });
    }

    /* eslint-disable react/no-danger */
    renderMoreActions() {
        return <div className={this.state.showMenu ? "dots active" : "dots"}
            onMouseEnter={this.onMouseEnter.bind(this)}
            onMouseLeave={this.onMouseLeave.bind(this)}>
                <div dangerouslySetInnerHTML={{ __html: SvgIcons.MoreMenuIcon }} ></div>
                {this.state.showMenu && 
                    <PersonaBarTreeInContextMenu {...this.props} onClose={this.onMouseLeave.bind(this)} />
                }
        </div>;
    }

    render() {
        const { item } = this.props;
        /*eslint-disable react/no-danger*/
        return (
            <div id={`menu-item-${item.name} ${item.selected}`} 
            className="selection-arrow">
                {item.selected ? <div dangerouslySetInnerHTML={{ __html: SvgIcons.ArrowForward }} /> : <div></div>}
                {item.selected ? this.renderMoreActions() : <div></div>}                
            </div>
        );
    }
}

PersonaBarSelectionArrow.propTypes = {
    onAddPage: PropTypes.func.isRequired,
    onViewPage: PropTypes.func.isRequired,
    onViewEditPage: PropTypes.func.isRequired,
    onDuplicatePage: PropTypes.func.isRequired,
    CallCustomAction: PropTypes.func.isRequired,
    _traverse: PropTypes.func.isRequired,
    item: PropTypes.object.isRequired,
    pageInContextComponents: PropTypes.array.isRequired
};