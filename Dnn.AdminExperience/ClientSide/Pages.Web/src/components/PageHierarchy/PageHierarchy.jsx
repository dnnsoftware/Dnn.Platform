import React, {Component} from "react";
import PropTypes from "prop-types";
import {pageHierarchyManager} from "./pages.pageHierarchy";
import utils from "../../utils";
import "./css/pages-hierarchy.css";

class PageHierarchy extends Component {
    componentDidMount() {
        const {itemTemplate, dragItemTemplate, searchKeyword} = this.props;
        pageHierarchyManager.utility = utils.getUtilities();
        pageHierarchyManager.resx = pageHierarchyManager.utility.resx.Pages;
        pageHierarchyManager._viewModel = {};
        pageHierarchyManager.callPageSettings = this.callPageSettings.bind(this);
        pageHierarchyManager.init(this.node, this.initCallback.bind(this));
        pageHierarchyManager.setItemTemplate(itemTemplate);
        pageHierarchyManager.setDragItemTemplate(dragItemTemplate);
        pageHierarchyManager.setSearchKeyword(searchKeyword);   
        pageHierarchyManager.setCurrentTabIdAndSiteRoot(utils.getCurrentPageId(), utils.getSiteRoot());
        pageHierarchyManager.selectedPagePathChanged = p => this.props.onSelectedPagePathChanged([...p]);
    }

    componentDidUpdate(prevProps) {
        const {itemTemplate, dragItemTemplate, searchKeyword, selectedPage} = prevProps;
        if (itemTemplate && itemTemplate !== this.props.itemTemplate) {
            pageHierarchyManager.setItemTemplate(this.props.itemTemplate);
        }    

        if (dragItemTemplate && dragItemTemplate !== this.props.dragItemTemplate) {
            pageHierarchyManager.setDragItemTemplate(this.props.dragItemTemplate);
        }    

        if (searchKeyword && searchKeyword !== this.props.searchKeyword) {
            pageHierarchyManager.setSearchKeyword(this.props.searchKeyword);
        }

        if (selectedPage && selectedPage !== this.props.selectedPage) {
            pageHierarchyManager.selectPageFromBreadCrumbs(this.props.selectedPage);
        }
    }
    
    initCallback() {
        const {createdPage} = this.props;

        if (createdPage !== null) {
            pageHierarchyManager.addPage(createdPage);
        }
    }

    callPageSettings(action, params) {
        const pageId = params[0];
        this.props.onPageSettings(pageId);
    }
    
    render() {
        const html = require("raw-loader!./pages.html").default;
        return <div ref={node => this.node = node} dangerouslySetInnerHTML={{__html: html}} />; // eslint-disable-line react/no-danger
    }
} 

PageHierarchy.propTypes = {
    itemTemplate: PropTypes.string.isRequired,
    dragItemTemplate: PropTypes.string.isRequired,
    searchKeyword: PropTypes.string.isRequired,
    onPageSettings: PropTypes.func.isRequired,
    onSelectedPagePathChanged: PropTypes.func.isRequired,
    selectedPage: PropTypes.object,
    createdPage: PropTypes.object
};

PageHierarchy.defaultProps = {
    itemTemplate: "pages-list-item-template",
    dragItemTemplate: "pages-drag-item-template"
};

export default PageHierarchy; 
