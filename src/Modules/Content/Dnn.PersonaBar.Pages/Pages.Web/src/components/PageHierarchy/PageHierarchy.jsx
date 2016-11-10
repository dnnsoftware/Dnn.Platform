import React, {Component, PropTypes} from "react";
import ReactDOM from "react-dom";
import {pageHierarchyManager} from "./pages.pageHierarchy";
import utils from "../../utils";
import "./css/pages-hierarchy.css";

class PageHierarchy extends Component {
    componentDidMount() {
        this.node = ReactDOM.findDOMNode(this);
        pageHierarchyManager.utility = utils.getUtilities();
        pageHierarchyManager.resx = pageHierarchyManager.utility.resx.Pages;
        pageHierarchyManager._viewModel = {};
        pageHierarchyManager.callPageSettings = this.callPageSettings.bind(this);
        pageHierarchyManager.init(this.node, this.initCallback.bind(this));
        pageHierarchyManager.setItemTemplate(this.props.itemTemplate);
        pageHierarchyManager.setSearchKeyword(this.props.searchKeyword);        
    }

    componentWillReceiveProps(nextProps) {
        const {itemTemplate, searchKeyword, createdPage} = this.props;
        if (itemTemplate !== nextProps.itemTemplate) {
            pageHierarchyManager.setItemTemplate(nextProps.itemTemplate);
        }    

        if (searchKeyword !== nextProps.searchKeyword) {
            pageHierarchyManager.setSearchKeyword(nextProps.searchKeyword);
        }

        if (createdPage !== nextProps.createdPage) {
            pageHierarchyManager.addPage(nextProps.createdPage);
        }    
    }
    
    initCallback() {
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
    searchKeyword: PropTypes.string.isRequired,
    onPageSettings: PropTypes.func.isRequired,
    createdPage: PropTypes.object
};

PageHierarchy.defaultProps = {
    itemTemplate: "pages-list-item-template"
};

export default PageHierarchy; 
