import React from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import AssetsHeader from "../components/AssetsHeader";
import folderPanelActions from "../actions/folderPanelActions";
import ManageFolderTypesContainer from "../containers/ManageFolderTypesPanelContainer";
import AddFolderPanelContainer from "../containers/AddFolderPanelContainer";
import AddAssetPanelContainer from "../containers/AddAssetPanelContainer";
import TopBarContainer from "./TopBarContainer";
import Item from "../containers/ItemContainer";
import ItemDetailsContainer from "../containers/ItemDetailsContainer";
import ItemMoveContainer from "../containers/ItemMoveContainer";
import localizeService from "../services/localizeService.js";
import TransitionGroup from "react-transition-group/TransitionGroup";
import CSSTransition from "react-transition-group/CSSTransition";

class AssetsPanelContainer extends React.Component {
    constructor(props) {
        super(props);
        this.mainContainer = React.createRef();
    }
    componentWillMount() {
        window.addEventListener("scroll", this.handleScroll.bind(this));
    }

    getDetailsPosition(i) {
        const itemWidth = this.props.itemWidth;
        const container = this.mainContainer;
        const containerWidth = container.clientWidth;
        const itemsPerRow = Math.floor(containerWidth / itemWidth);
        let position = Math.floor(i / itemsPerRow) * itemsPerRow + itemsPerRow - 1;

        return position;
    }

    getDetailsPanel(showPanel, i) {
        return (
            <TransitionGroup key={"item-details-" + i}>
                <CSSTransition 
                    classNames="dnn-slide-in-out"
                    timeout={{enter:500, exit: 300}}
                    exit={!showPanel}>
                    {showPanel && <ItemDetailsContainer />}
                </CSSTransition>
            </TransitionGroup>
        );
    }

    getMovePanel(showPanel, i) {
        return (
            <TransitionGroup key={"item-move-" + i}>
                <CSSTransition
                    classNames="dnn-slide-in-out"
                    timeout={{enter:500, exit: 300}}
                    exit={!showPanel}>
                    {showPanel && <ItemMoveContainer />}
                </CSSTransition>
            </TransitionGroup>
        );
    }

    handleScroll() {
        const { folderPanelState, loadContent, searchFiles } = this.props;
        const { totalCount, loadedItems, search, loading } = folderPanelState;

        const bodyHeight = document.body.offsetHeight;
        const windowHeight = window.innerHeight;
        const totalScrolled = window.pageYOffset;
        const scrollMax = bodyHeight - windowHeight;

        if (loadedItems < totalCount && totalScrolled + 100 > scrollMax) {
            if (!loading) {
                this.setState({
                    loadingFlag: true,
                });

                if (search) {
                    searchFiles(folderPanelState, search, true);
                } else {
                    loadContent(folderPanelState);
                }
            }
        }
    }

    render() {
        const {
            items,
            itemEditing,
            itemMoving,
            search,
            itemContainerDisabled,
            loading,
        } = this.props;
        let propsPosition = -1;

        let result = [];
        for (let i = 0; i < items.length; i++) {
            const item = items[i];
            const { itemId, itemName, isFolder } = item;
            result.push(<Item key={itemId + itemName} item={item} />);

            if ( itemEditing &&
                (
                    (isFolder && itemEditing.folderId === itemId) ||
                    (!isFolder && itemEditing.fileId === itemId)
                )
            ) {
                propsPosition = this.getDetailsPosition(i);
            }

            if ( itemMoving &&
                (
                    (isFolder && itemMoving.folderId === itemId) ||
                    (!isFolder && itemMoving.fileId === itemId)
                )
            ) {
                propsPosition = this.getDetailsPosition(i);
            }

            let showPanel =
            i === propsPosition ||
            (propsPosition >= items.length && i === items.length - 1);
            if (itemEditing){
                result.push(this.getDetailsPanel(showPanel, i));
            }

            if (itemMoving) {
                result.push(this.getMovePanel(showPanel, i));
            }
        }

        return (
            <div id="Assets-panel" onClick={(e) => e.stopPropagation()}>
                <AssetsHeader />
                <div className="assets-body">
                    <TopBarContainer />
                    <div
                        ref={(c) => (this.mainContainer = c)}
                        className={"main-container" + (loading ? " loading" : "")}
                    >
                        <ManageFolderTypesContainer />
                        <AddFolderPanelContainer />
                        <AddAssetPanelContainer />
                        <div
                            className={
                                "item-container" +
                (search ? " rm_search" : "") +
                (!items.length ? " empty" : "") +
                (itemContainerDisabled ? " disabled" : "")
                            }
                        >
                            {result}
                            <div className="empty-label rm_search">
                                <span className="empty-title">
                                    {localizeService.getString("AssetsPanelNoSearchResults")}
                                </span>
                            </div>
                            <div className="empty-label rm-folder">
                                <span className="empty-title">
                                    {localizeService.getString("AssetsPanelEmpty_Title")}
                                </span>
                                <span className="empty-subtitle">
                                    {localizeService.getString("AssetsPanelEmpty_Subtitle")}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}

AssetsPanelContainer.propTypes = {
    folderPanelState: PropTypes.object,
    items: PropTypes.array,
    search: PropTypes.string,
    itemContainerDisabled: PropTypes.bool,
    loading: PropTypes.bool,
    itemEditing: PropTypes.object,
    itemMoving: PropTypes.object,
    itemWidth: PropTypes.number,
    loadContent: PropTypes.func,
    searchFiles: PropTypes.func,
};

function mapDispatchToProps(dispatch) {
    return {
        ...bindActionCreators(
            {
                loadContent: folderPanelActions.loadContent,
                searchFiles: folderPanelActions.searchFiles,
            },
            dispatch
        ),
    };
}

function mapStateToProps(state) {
    const folderPanelState = state.folderPanel;
    const addFolderPanelState = state.addFolderPanel;
    const addAssetPanelState = state.addAssetPanel;
    const itemDetailsState = state.itemDetails;

    return {
        folderPanelState,
        items: folderPanelState.items || [],
        search: folderPanelState.search,
        itemContainerDisabled:
      addFolderPanelState.expanded || addAssetPanelState.expanded,
        loading: folderPanelState.loading,
        itemEditing: itemDetailsState.itemEditing,
        itemMoving: itemDetailsState.itemMoving,
        itemWidth: folderPanelState.itemWidth,
    };
}

export default connect(
    mapStateToProps,
    mapDispatchToProps
)(AssetsPanelContainer);
