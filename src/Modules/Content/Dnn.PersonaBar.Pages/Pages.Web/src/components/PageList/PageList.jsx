import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import SocialPanelBody from "dnn-social-panel-body";
import GridCell from "dnn-grid-cell";
import SearchBox from "dnn-search-box";
import PageHierarchy from "../PageHierarchy/PageHierarchy";
import {pageHierarchyActions as PageHierarchyActions} from "../../actions";
import styles from "./PageList.less";
import Localization from "../../localization";

class PageList extends Component {
    onSearchKeywordChanged(value) {
        this.props.dispatch(PageHierarchyActions.setSearchKeyword(value));
    }

    render() {
        return (
            <SocialPanelBody className={styles.pageListBody}>                  
                <div className="search-container">      
                    <GridCell columnSize={65}>
                        <div>{this.props.toolbarComponents}</div>
                    </GridCell>
                    <GridCell columnSize={35} >
                        <div className="search-filter">                                
                            <SearchBox placeholder={Localization.get("Search")} 
                                onSearch={this.onSearchKeywordChanged.bind(this)} maxLength={50} />                                
                            <div className="clear"></div>
                        </div>
                    </GridCell>
                </div>
                <PageHierarchy
                    itemTemplate={this.props.itemTemplate}
                    searchKeyword={this.props.searchKeyword} 
                    onPageSettings={this.props.onPageSettings}
                    createdPage={this.props.createdPage} />
            </SocialPanelBody>
        );
    }
}

PageList.propTypes = {
    onPageSettings: PropTypes.func,
    dispatch: PropTypes.func.isRequired,
    searchKeyword: PropTypes.string.isRequired,
    itemTemplate: PropTypes.string.isRequired,
    createdPage: PropTypes.object,
    toolbarComponents: PropTypes.array
};

function mapStateToProps(state) {
    return {
        searchKeyword: state.pageHierarchy.searchKeyword,
        itemTemplate: state.pageHierarchy.itemTemplate,
        createdPage: state.pageHierarchy.createdPage,
        toolbarComponents: state.extensions.toolbarComponents
    };
}

export default connect(mapStateToProps)(PageList);