import React, {Component, PropTypes} from "react";
import { connect } from "react-redux";
import SocialPanelBody from "dnn-social-panel-body";
import GridCell from "dnn-grid-cell";
import SearchBox from "dnn-search-box";
import PageHierarchy from "../PageHierarchy/PageHierarchy";
import {pageHierarchyActions as PageHierarchyActions} from "../../actions";
import "./PageList.less";
import localization from "../../localization";

class PageList extends Component {
    onSearchKeywordChanged(value) {
        this.props.dispatch(PageHierarchyActions.setSearchKeyword(value));
    }

    render() {
        return (
            <SocialPanelBody>                        
                <GridCell columnSize={65} >
                    <div>&nbsp; </div></GridCell>
                <GridCell columnSize={35} >
                    <div className="search-filter">                                
                        <SearchBox placeholder={localization.get("Search")} 
                            onSearch={this.onSearchKeywordChanged.bind(this)} maxLength={50} />                                
                        <div className="clear"></div>
                    </div>
                </GridCell>

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
    createdPage: PropTypes.object
};

function mapStateToProps(state) {
    return {
        searchKeyword: state.pageHierarchy.searchKeyword,
        itemTemplate: state.pageHierarchy.itemTemplate,
        createdPage: state.pageHierarchy.createdPage
    };
}

export default connect(mapStateToProps)(PageList);