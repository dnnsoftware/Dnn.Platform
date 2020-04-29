import React, { PropTypes } from "react";
import { bindActionCreators } from "redux";
import { connect } from "react-redux";
import Breadcrumb from "../components/Breadcrumb";
import folderPanelActions from "../actions/folderPanelActions";
import localizeService from "../services/localizeService";

class BreadcrumbsContainer extends React.Component {
    render() {
        const { breadcrumbs, loadContent, folderPanelState } = this.props;
        const search = folderPanelState.search;

        let result = [];
        let folderId;

        if (breadcrumbs[0]) {
            folderId = breadcrumbs[0].folderId;
        }
        result.push(<Breadcrumb key="HOME" name="HOME" onClick={loadContent.bind(this, folderPanelState, folderId)} />);
        for (let i = 1; i<breadcrumbs.length; i++) 
        {
            folderId = breadcrumbs[i].folderId;
            result.push(">");
            result.push(<Breadcrumb key={folderId} name={breadcrumbs[i].folderName} onClick={loadContent.bind(this, folderPanelState, folderId)} />);
        }

        if (search) {
            result.push(">");
            result.push(<Breadcrumb key="search" name={localizeService.getString("Search") + " '"  + search + "'"} />);
        }

        return (
            <div className="breadcrumbs-container">
                { result }
            </div>
        );
    }
}

BreadcrumbsContainer.propTypes = {
    breadcrumbs: PropTypes.array,
    loadContent: PropTypes.func,
    folderPanelState: PropTypes.object
};

function mapStateToProps(state) {
    const breadcrumbsState = state.breadcrumbs;
    const folderPanelState = state.folderPanel;

    return {
        breadcrumbs: breadcrumbsState.breadcrumbs || [],
        folderPanelState
    };
}

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators({
            loadContent: folderPanelActions.loadContent
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(BreadcrumbsContainer);