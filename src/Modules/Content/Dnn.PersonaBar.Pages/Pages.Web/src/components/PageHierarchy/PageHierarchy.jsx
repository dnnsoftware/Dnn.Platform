import React, {Component, PropTypes} from "react";
import ReactDOM from "react-dom";
import {pageHierarchyManager} from "./pages.pageHierarchy";
import utils from "../../utils";
import "./css/pages-hierarchy.css";

class PageHierarchy extends Component {
    componentDidMount() {
        const {itemTemplate, dragItemTemplate, searchKeyword} = this.props;

        this.node = ReactDOM.findDOMNode(this);
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

    componentWillReceiveProps(nextProps) {
        const {itemTemplate, dragItemTemplate, searchKeyword, selectedPage} = this.props;
        if (itemTemplate !== nextProps.itemTemplate) {
            pageHierarchyManager.setItemTemplate(nextProps.itemTemplate);
        }    

        if (dragItemTemplate !== nextProps.dragItemTemplate) {
            pageHierarchyManager.setDragItemTemplate(nextProps.dragItemTemplate);
        }    

        if (searchKeyword !== nextProps.searchKeyword) {
            pageHierarchyManager.setSearchKeyword(nextProps.searchKeyword);
        }

        if (selectedPage !== nextProps.selectedPage) {
            pageHierarchyManager.selectPageFromBreadCrumbs(nextProps.selectedPage);
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
        const html = require("raw!./pages.html");
        return <div dangerouslySetInnerHTML={{__html: html}} />; // eslint-disable-line react/no-danger
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
