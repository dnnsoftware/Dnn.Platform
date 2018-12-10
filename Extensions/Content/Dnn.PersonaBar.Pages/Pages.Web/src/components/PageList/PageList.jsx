import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { GridCell, SearchBox } from "@dnnsoftware/dnn-react-common";
import PageHierarchy from "../PageHierarchy/PageHierarchy";
import {pageHierarchyActions as PageHierarchyActions} from "../../actions";
import styles from "./PageList.less";
import Localization from "../../localization";
import { bindActionCreators } from "redux";

class PageList extends Component {
    render() {
        return (
            <div className={styles.pageListBody + " dnn-persona-bar-page-body"}>
                <div className="search-container">      
                    <GridCell columnSize={65}>
                        <div>{this.props.toolbarComponents}</div>
                    </GridCell>
                    <GridCell columnSize={35} >
                        <div className="search-filter">                                
                            <SearchBox placeholder={Localization.get("Search")} 
                                onSearch={this.props.onSearchKeywordChanged} 
                                maxLength={50} />
                            <div className="clear"></div>
                        </div>
                    </GridCell>
                </div>
                <PageHierarchy
                    itemTemplate={this.props.itemTemplate}
                    dragItemTemplate={this.props.dragItemTemplate}
                    searchKeyword={this.props.searchKeyword} 
                    onPageSettings={this.props.onPageSettings}
                    selectedPage={this.props.selectedPage}
                    onSelectedPagePathChanged={this.props.onSelectedPagePathChanged}
                    createdPage={this.props.createdPage} />
            </div>
        );
    }
}

PageList.propTypes = {
    onPageSettings: PropTypes.func,
    selectedPage: PropTypes.object,
    onSelectedPagePathChanged: PropTypes.func.isRequired,
    onSearchKeywordChanged: PropTypes.func.isRequired,
    searchKeyword: PropTypes.string.isRequired,
    itemTemplate: PropTypes.string.isRequired,
    dragItemTemplate: PropTypes.string.isRequired,
    createdPage: PropTypes.object,
    toolbarComponents: PropTypes.array
};

function mapStateToProps(state) {
    return {
        searchKeyword: state.pageHierarchy.searchKeyword,
        itemTemplate: state.pageHierarchy.itemTemplate,
        dragItemTemplate: state.pageHierarchy.dragItemTemplate,
        createdPage: state.pageHierarchy.createdPage,
        toolbarComponents: state.extensions.toolbarComponents,
        selectedPage: state.pageHierarchy.selectedPage
    };
}

function mapDispatchToProps(dispatch) {
    return bindActionCreators({
        onSelectedPagePathChanged: PageHierarchyActions.changeSelectedPagePath,
        onSearchKeywordChanged: PageHierarchyActions.setSearchKeyword
    }, dispatch);
}

export default connect(mapStateToProps, mapDispatchToProps)(PageList);